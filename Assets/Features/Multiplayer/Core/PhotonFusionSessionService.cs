using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Player.Core;

namespace ProjectZombie.Features.Multiplayer.Core
{
    /// <summary>
    /// Quản lý vòng đời NetworkRunner và phiên kết nối mạng Photon Fusion 2.0 (Host Mode / Relay).
    /// Đơn trách nhiệm (SRP): Chỉ điều phối việc Khởi tạo (StartGame), Ngắt kết nối (Shutdown),
    /// và kết nối các Sub-services (NetworkLobbySync, FusionNetworkInputCollector, NetworkPlayerSpawner).
    /// </summary>
    public class PhotonFusionSessionService : MonoBehaviour, INetworkSessionService, INetworkRunnerCallbacks
    {
        [Header("Runner Settings")]
        [SerializeField] private NetworkRunner _runnerPrefab;

        private NetworkRunner _activeRunner;
        private NetworkLobbySync _lobbySync;
        private FusionNetworkInputCollector _inputCollector;
        private NetworkPlayerSpawner _playerSpawner;
        private bool _isHost;
        private bool _isStarting = false;

        public NetworkRoomInfo CurrentRoom => _lobbySync != null ? _lobbySync.CurrentRoom : null;
        public bool IsHost => _isHost;
        public bool IsInRoom => _activeRunner != null && _activeRunner.IsRunning && CurrentRoom != null;

        public event Action<NetworkRoomInfo> OnRoomUpdated;
        public event Action OnMatchStarted;
        public event Action OnMatchEnded;
        public event Action<string> OnConnectionError;

        private void Awake()
        {
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        public async Task<bool> CreateHostSessionAsync(string roomCode = null, int maxPlayers = 4)
        {
            if (_isStarting) return false;
            _isStarting = true;

            string code = string.IsNullOrEmpty(roomCode) ? GenerateRandomRoomCode() : roomCode.ToUpper();
            _isHost = true;

            EnsureRunnerInstance();

            var startGameArgs = new StartGameArgs
            {
                GameMode = GameMode.Host,
                SessionName = code,
                PlayerCount = maxPlayers
            };

            var startResult = await _activeRunner.StartGame(startGameArgs);
            _isStarting = false;

            if (startResult.Ok)
            {
                // Khởi tạo trạng thái phòng qua NetworkLobbySync
                _lobbySync.Initialize(_activeRunner, isHost: true, roomCode: code, maxPlayers: maxPlayers);

                // Chuyển đổi PlayerRegistry sang chế độ Multiplayer
                ProjectZombie.Core.Architecture.ServiceContext.Register<IPlayerRegistry>(new MultiplayerPlayerRegistry());

                return true;
            }
            else
            {
                OnConnectionError?.Invoke($"Khởi tạo phòng Host thất bại: {startResult.ShutdownReason}");
                return false;
            }
        }

        public async Task<bool> JoinSessionAsync(string roomCode)
        {
            if (_isStarting) return false;
            if (string.IsNullOrWhiteSpace(roomCode))
            {
                OnConnectionError?.Invoke("Mã phòng không được để trống!");
                return false;
            }

            _isStarting = true;
            string formattedCode = roomCode.Trim().ToUpper();
            _isHost = false;

            EnsureRunnerInstance();

            var startGameArgs = new StartGameArgs
            {
                GameMode = GameMode.Client,
                SessionName = formattedCode
            };

            var startResult = await _activeRunner.StartGame(startGameArgs);
            _isStarting = false;

            if (startResult.Ok)
            {
                // Khởi tạo trạng thái phòng qua NetworkLobbySync
                _lobbySync.Initialize(_activeRunner, isHost: false, roomCode: formattedCode, maxPlayers: 4);

                // Chuyển đổi PlayerRegistry sang chế độ Multiplayer
                ProjectZombie.Core.Architecture.ServiceContext.Register<IPlayerRegistry>(new MultiplayerPlayerRegistry());

                return true;
            }
            else
            {
                OnConnectionError?.Invoke($"Không thể tham gia phòng '{formattedCode}': {startResult.ShutdownReason}");
                return false;
            }
        }

        public async Task LeaveSessionAsync()
        {
            if (_playerSpawner != null)
            {
                _playerSpawner.StopMatch();
            }

            if (_lobbySync != null)
            {
                _lobbySync.ResetRoom();
            }

            if (_activeRunner != null)
            {
                var runnerToShutdown = _activeRunner;
                _activeRunner = null;
                try
                {
                    await runnerToShutdown.Shutdown();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[PhotonFusionSessionService] Shutdown warning: {ex.Message}");
                }

                if (runnerToShutdown != null)
                {
                    Destroy(runnerToShutdown.gameObject);
                }
            }

            _isHost = false;

            // Khôi phục PlayerRegistry về SinglePlayer
            ProjectZombie.Core.Architecture.ServiceContext.Register<IPlayerRegistry>(new SinglePlayerRegistry());

            // Khôi phục nhân vật Offline/Singleplayer cho Sảnh nếu chưa có
            if (GameplayBootstrapper.Instance != null && !PlayerProvider.HasPlayer)
            {
                GameplayBootstrapper.Instance.SpawnPlayerForActiveHero();
            }

            OnMatchEnded?.Invoke();
            OnRoomUpdated?.Invoke(null);
        }

        public void SetLocalPlayerReady(bool isReady)
        {
            if (_lobbySync != null)
            {
                _lobbySync.SetLocalPlayerReady(isReady);
            }
        }

        public async Task StartGameMatchAsync()
        {
            if (_activeRunner == null || !_isHost) return;

            // 1. Dọn dẹp nhân vật Offline trước khi vào trận Co-op
            GameplayBootstrapper.Instance?.DespawnActivePlayer();

            // 2. Kích hoạt Spawner sinh nhân vật cho tất cả người chơi trong phòng
            if (_playerSpawner != null)
            {
                _playerSpawner.StartMatch();
            }

            // 3. Đồng bộ tín hiệu Match Start qua LobbySync
            if (_lobbySync != null)
            {
                _lobbySync.NotifyMatchStarted();
            }

            await Task.Delay(50);
        }

        private void EnsureRunnerInstance()
        {
            if (_activeRunner != null) return;

            if (_runnerPrefab != null)
            {
                _activeRunner = Instantiate(_runnerPrefab);
            }
            else
            {
                var go = new GameObject("[Fusion_NetworkRunner]");
                _activeRunner = go.AddComponent<NetworkRunner>();
            }

            // Gắn và đăng ký các Sub-Components chuyên biệt (Đơn trách nhiệm)
            _lobbySync = _activeRunner.GetComponent<NetworkLobbySync>() 
                         ?? _activeRunner.gameObject.AddComponent<NetworkLobbySync>();
            _inputCollector = _activeRunner.GetComponent<FusionNetworkInputCollector>() 
                              ?? _activeRunner.gameObject.AddComponent<FusionNetworkInputCollector>();
            _playerSpawner = _activeRunner.GetComponent<NetworkPlayerSpawner>() 
                             ?? _activeRunner.gameObject.AddComponent<NetworkPlayerSpawner>();

            // Kết nối các Event từ LobbySync
            _lobbySync.OnRoomUpdated -= ForwardRoomUpdated;
            _lobbySync.OnRoomUpdated += ForwardRoomUpdated;
            _lobbySync.OnMatchStarted -= ForwardMatchStarted;
            _lobbySync.OnMatchStarted += ForwardMatchStarted;

            // Đăng ký Callbacks vào NetworkRunner
            _activeRunner.AddCallbacks(this);
            _activeRunner.AddCallbacks(_lobbySync);
            _activeRunner.AddCallbacks(_inputCollector);
            _activeRunner.ProvideInput = true;
        }

        private void ForwardRoomUpdated(NetworkRoomInfo room) => OnRoomUpdated?.Invoke(room);
        private void ForwardMatchStarted() => OnMatchStarted?.Invoke();

        private string GenerateRandomRoomCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var rng = new System.Random();
            char[] code = new char[6];
            for (int i = 0; i < 6; i++)
            {
                code[i] = chars[rng.Next(chars.Length)];
            }
            return new string(code);
        }

        // =========================================================================
        // NETWORK RUNNER CALLBACKS (Chỉ xử lý kết nối chung & Forward Spawner)
        // =========================================================================

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer && _playerSpawner != null)
            {
                _playerSpawner.PlayerJoined(player);
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer && _playerSpawner != null)
            {
                _playerSpawner.PlayerLeft(player);
            }
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            if (_playerSpawner != null)
            {
                _playerSpawner.StopMatch();
            }

            if (_lobbySync != null)
            {
                _lobbySync.ResetRoom();
            }

            _isHost = false;

            if (_activeRunner != null)
            {
                _activeRunner = null;
                OnRoomUpdated?.Invoke(null);
            }
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            OnConnectionError?.Invoke($"Mất kết nối tới máy chủ Photon: {reason}");
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            OnConnectionError?.Invoke($"Kết nối phòng thất bại: {reason}");
        }

        // Unused Callbacks
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    }
}
