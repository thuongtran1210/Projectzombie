using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Player.Core;
using ProjectZombie.Features.Player.Input;

namespace ProjectZombie.Features.Multiplayer.Core
{
    /// <summary>
    /// Triển khai kết nối mạng thực tế qua Photon Fusion 2.0 (Host Mode / Relay).
    /// Quản lý vòng đời NetworkRunner, đồng bộ phòng chơi, danh sách người chơi và nạp Input qua mạng.
    /// </summary>
    public class PhotonFusionSessionService : MonoBehaviour, INetworkSessionService, INetworkRunnerCallbacks
    {
        [Header("Runner Settings")]
        [SerializeField] private NetworkRunner _runnerPrefab;

        private NetworkRunner _activeRunner;
        private NetworkRoomInfo _currentRoom;
        private bool _isHost;
        private bool _isStarting = false;

        public NetworkRoomInfo CurrentRoom => _currentRoom;
        public bool IsHost => _isHost;
        public bool IsInRoom => _activeRunner != null && _activeRunner.IsRunning && _currentRoom != null;

        public event Action<NetworkRoomInfo> OnRoomUpdated;
        public event Action OnMatchStarted;
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

            var sceneManager = _activeRunner.GetComponent<NetworkSceneManagerDefault>();
            if (sceneManager == null)
            {
                sceneManager = _activeRunner.gameObject.AddComponent<NetworkSceneManagerDefault>();
            }

            var startGameArgs = new StartGameArgs
            {
                GameMode = GameMode.Host,
                SessionName = code,
                PlayerCount = maxPlayers,
                SceneManager = sceneManager,
                Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex)
            };

            var startResult = await _activeRunner.StartGame(startGameArgs);
            _isStarting = false;

            if (startResult.Ok)
            {
                _currentRoom = new NetworkRoomInfo
                {
                    RoomCode = code,
                    RoomName = $"Phòng Host [{code}]",
                    MaxPlayers = maxPlayers,
                    IsGameStarted = false
                };

                var localData = new NetworkPlayerData
                {
                    PlayerId = _activeRunner.LocalPlayer.ToString(),
                    DisplayName = "Chủ Phòng (Bạn)",
                    SelectedCharacterId = RunLoadoutState.SelectedCharacter != null ? RunLoadoutState.SelectedCharacter.characterName : "Đạo Sĩ",
                    IsHost = true,
                    IsReady = true,
                    PingMs = 15
                };

                _currentRoom.Players.Add(localData);
                _currentRoom.LocalPlayer = localData;

                // Chuyển đổi PlayerRegistry sang chế độ Multiplayer
                ProjectZombie.Core.Architecture.ServiceContext.Register<IPlayerRegistry>(new MultiplayerPlayerRegistry());

                OnRoomUpdated?.Invoke(_currentRoom);
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

            var sceneManager = _activeRunner.GetComponent<NetworkSceneManagerDefault>();
            if (sceneManager == null)
            {
                sceneManager = _activeRunner.gameObject.AddComponent<NetworkSceneManagerDefault>();
            }

            var startGameArgs = new StartGameArgs
            {
                GameMode = GameMode.Client,
                SessionName = formattedCode,
                SceneManager = sceneManager
            };

            var startResult = await _activeRunner.StartGame(startGameArgs);
            _isStarting = false;

            if (startResult.Ok)
            {
                _currentRoom = new NetworkRoomInfo
                {
                    RoomCode = formattedCode,
                    RoomName = $"Phòng Trận [{formattedCode}]",
                    MaxPlayers = 4,
                    IsGameStarted = false
                };

                var localData = new NetworkPlayerData
                {
                    PlayerId = _activeRunner.LocalPlayer.ToString(),
                    DisplayName = "Hiệp Khách (Bạn)",
                    SelectedCharacterId = RunLoadoutState.SelectedCharacter != null ? RunLoadoutState.SelectedCharacter.characterName : "Kiếm Khách",
                    IsHost = false,
                    IsReady = false,
                    PingMs = 30
                };

                _currentRoom.Players.Add(localData);
                _currentRoom.LocalPlayer = localData;

                // Chuyển đổi PlayerRegistry sang chế độ Multiplayer
                ProjectZombie.Core.Architecture.ServiceContext.Register<IPlayerRegistry>(new MultiplayerPlayerRegistry());

                OnRoomUpdated?.Invoke(_currentRoom);
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
            if (_activeRunner != null)
            {
                await _activeRunner.Shutdown();
                if (_activeRunner != null)
                {
                    Destroy(_activeRunner.gameObject);
                    _activeRunner = null;
                }
            }

            _currentRoom = null;
            _isHost = false;
            OnRoomUpdated?.Invoke(null);
        }

        public void SetLocalPlayerReady(bool isReady)
        {
            if (_currentRoom == null || _currentRoom.LocalPlayer == null) return;

            _currentRoom.LocalPlayer.IsReady = isReady;
            OnRoomUpdated?.Invoke(_currentRoom);
        }

        public async Task StartGameMatchAsync()
        {
            if (_activeRunner == null || !_isHost) return;

            await Task.Delay(100);
            if (_currentRoom != null) _currentRoom.IsGameStarted = true;
            OnMatchStarted?.Invoke();
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

            _activeRunner.AddCallbacks(this);
            _activeRunner.ProvideInput = true;
        }

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
        // PHOTON FUSION RUNNER CALLBACKS
        // =========================================================================

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (_currentRoom == null) return;

            bool isLocal = player == runner.LocalPlayer;
            if (!isLocal)
            {
                var remotePlayer = new NetworkPlayerData
                {
                    PlayerId = player.ToString(),
                    DisplayName = $"Hiệp Khách #{player.PlayerId}",
                    SelectedCharacterId = "hero_daosi",
                    IsHost = false,
                    IsReady = true,
                    PingMs = 35
                };
                _currentRoom.Players.Add(remotePlayer);
                OnRoomUpdated?.Invoke(_currentRoom);
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (_currentRoom == null) return;

            string pid = player.ToString();
            _currentRoom.Players.RemoveAll(p => p.PlayerId == pid);
            OnRoomUpdated?.Invoke(_currentRoom);
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var inputData = new NetworkInputData();

            // Đọc Input từ PlayerInputReader của Local Player
            if (PlayerProvider.HasPlayer && PlayerProvider.PlayerGameObject.TryGetComponent<PlayerInputReader>(out var inputReader))
            {
                inputData.MoveDirection = inputReader.MovementInput;
            }

            input.Set(inputData);
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            _currentRoom = null;
            _isHost = false;
            OnRoomUpdated?.Invoke(null);
        }

        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            OnConnectionError?.Invoke($"Mất kết nối tới máy chủ Photon: {reason}");
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            OnConnectionError?.Invoke($"Kết nối phòng thất bại: {reason}");
        }

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
