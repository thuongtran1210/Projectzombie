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
    /// Quản lý vòng đời NetworkRunner, đồng bộ phòng chơi 2 chiều (Reliable Data RPCs), danh sách người chơi và nạp Input qua mạng.
    /// </summary>
    public class PhotonFusionSessionService : MonoBehaviour, INetworkSessionService, INetworkRunnerCallbacks
    {
        private const byte MSG_MATCH_START = 1;
        private const byte MSG_ROOM_STATE_SYNC = 2;
        private const byte MSG_CLIENT_PROFILE_SUBMIT = 3;
        private const byte MSG_CLIENT_READY_SUBMIT = 4;

        [Header("Runner Settings")]
        [SerializeField] private NetworkRunner _runnerPrefab;

        private NetworkRunner _activeRunner;
        private NetworkRoomInfo _currentRoom;
        private bool _isHost;
        private bool _isStarting = false;
        private float _lastPingSyncTime;

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

        private void Update()
        {
            if (_activeRunner == null || !_activeRunner.IsRunning || _currentRoom == null || _currentRoom.IsGameStarted) return;

            // Định kỳ mỗi 1.5 giây cập nhật lại chỉ số Ping (RTT) thực tế từ Photon
            if (Time.time - _lastPingSyncTime > 1.5f)
            {
                _lastPingSyncTime = Time.time;
                if (_isHost)
                {
                    BroadcastRoomState();
                }
                else
                {
                    if (_currentRoom.LocalPlayer != null)
                    {
                        _currentRoom.LocalPlayer.PingMs = GetLivePing(_activeRunner.LocalPlayer);
                        OnRoomUpdated?.Invoke(_currentRoom);
                    }
                }
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
                string heroName = RunLoadoutState.SelectedCharacter != null ? RunLoadoutState.SelectedCharacter.characterName : "Đạo Sĩ";
                _currentRoom = new NetworkRoomInfo
                {
                    RoomCode = code,
                    RoomName = $"Phòng Host [{code}]",
                    MaxPlayers = maxPlayers,
                    IsGameStarted = false
                };

                var localData = new NetworkPlayerData
                {
                    PlayerId = _activeRunner.LocalPlayer.PlayerId.ToString(),
                    DisplayName = "Chủ Phòng (Bạn)",
                    SelectedCharacterId = heroName,
                    IsHost = true,
                    IsReady = true,
                    PingMs = GetLivePing(_activeRunner.LocalPlayer)
                };

                _currentRoom.Players.Add(localData);
                _currentRoom.LocalPlayer = localData;

                // Chuyển đổi PlayerRegistry sang chế độ Multiplayer
                ProjectZombie.Core.Architecture.ServiceContext.Register<IPlayerRegistry>(new MultiplayerPlayerRegistry());

                // Dọn dẹp thực thể nhân vật Offline/Singleplayer trong Scene
                GameplayBootstrapper.Instance?.DespawnActivePlayer();

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
                string heroName = RunLoadoutState.SelectedCharacter != null ? RunLoadoutState.SelectedCharacter.characterName : "Thư Sinh";
                var localData = new NetworkPlayerData
                {
                    PlayerId = _activeRunner.LocalPlayer.PlayerId.ToString(),
                    DisplayName = "Hiệp Khách",
                    SelectedCharacterId = heroName,
                    IsHost = false,
                    IsReady = false,
                    PingMs = GetLivePing(_activeRunner.LocalPlayer)
                };

                _currentRoom = new NetworkRoomInfo
                {
                    RoomCode = formattedCode,
                    RoomName = $"Phòng Trận [{formattedCode}]",
                    MaxPlayers = 4,
                    IsGameStarted = false
                };

                _currentRoom.Players.Add(localData);
                _currentRoom.LocalPlayer = localData;

                // Gửi thông tin hồ sơ của Client lên Host để đồng bộ
                SendClientProfileToHost(localData);

                // Chuyển đổi PlayerRegistry sang chế độ Multiplayer
                ProjectZombie.Core.Architecture.ServiceContext.Register<IPlayerRegistry>(new MultiplayerPlayerRegistry());

                // Dọn dẹp thực thể nhân vật Offline/Singleplayer trong Scene
                GameplayBootstrapper.Instance?.DespawnActivePlayer();

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
                if (_activeRunner.TryGetComponent<NetworkPlayerSpawner>(out var spawner))
                {
                    spawner.StopMatch();
                }

                await _activeRunner.Shutdown();
                if (_activeRunner != null)
                {
                    Destroy(_activeRunner.gameObject);
                    _activeRunner = null;
                }
            }

            _currentRoom = null;
            _isHost = false;

            // Khôi phục nhân vật Offline/Singleplayer cho Sảnh nếu quay về Menu chính
            GameplayBootstrapper.Instance?.SpawnPlayerForActiveHero();

            OnRoomUpdated?.Invoke(null);
        }

        public void SetLocalPlayerReady(bool isReady)
        {
            if (_currentRoom == null || _currentRoom.LocalPlayer == null) return;

            _currentRoom.LocalPlayer.IsReady = isReady;

            if (_isHost)
            {
                BroadcastRoomState();
            }
            else if (_activeRunner != null && _activeRunner.IsRunning)
            {
                // Gửi tín hiệu Ready cập nhật lên Host
                byte[] payload = new byte[] { MSG_CLIENT_READY_SUBMIT, (byte)(isReady ? 1 : 0) };
                var key = ReliableKey.FromInts(MSG_CLIENT_READY_SUBMIT, 0, 0, 0);
                foreach (var p in _activeRunner.ActivePlayers)
                {
                    if (p != _activeRunner.LocalPlayer)
                    {
                        _activeRunner.SendReliableDataToPlayer(p, key, payload);
                        break;
                    }
                }
            }

            OnRoomUpdated?.Invoke(_currentRoom);
        }

        public async Task StartGameMatchAsync()
        {
            if (_activeRunner == null || !_isHost) return;

            if (_currentRoom != null) _currentRoom.IsGameStarted = true;

            // 1. Host kích hoạt Spawner để spawn nhân vật cho tất cả người chơi trong phòng
            if (_activeRunner.TryGetComponent<NetworkPlayerSpawner>(out var spawner))
            {
                spawner.StartMatch();
            }

            // 2. Gửi tín hiệu Reliable bắt đầu trận đấu tới tất cả các Client khác
            byte[] startSignal = new byte[] { MSG_MATCH_START };
            var msgKey = ReliableKey.FromInts(MSG_MATCH_START, 0, 0, 0);
            foreach (var player in _activeRunner.ActivePlayers)
            {
                if (player != _activeRunner.LocalPlayer)
                {
                    _activeRunner.SendReliableDataToPlayer(player, msgKey, startSignal);
                }
            }

            await Task.Delay(50);
            OnMatchStarted?.Invoke();
        }

        // =========================================================================
        // NETWORK DATA BROADCAST & SYNC
        // =========================================================================

        private void BroadcastRoomState()
        {
            if (_activeRunner == null || !_isHost || _currentRoom == null) return;

            // Cập nhật chỉ số Ping cho từng người chơi trước khi phát sóng
            foreach (var p in _currentRoom.Players)
            {
                if (int.TryParse(p.PlayerId, out int pid))
                {
                    var pRef = PlayerRef.FromIndex(pid);
                    p.PingMs = GetLivePing(pRef);
                }
            }

            string json = JsonUtility.ToJson(_currentRoom);
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
            byte[] payload = new byte[1 + jsonBytes.Length];
            payload[0] = MSG_ROOM_STATE_SYNC;
            Buffer.BlockCopy(jsonBytes, 0, payload, 1, jsonBytes.Length);

            var key = ReliableKey.FromInts(MSG_ROOM_STATE_SYNC, 0, 0, 0);
            foreach (var player in _activeRunner.ActivePlayers)
            {
                if (player != _activeRunner.LocalPlayer)
                {
                    _activeRunner.SendReliableDataToPlayer(player, key, payload);
                }
            }

            OnRoomUpdated?.Invoke(_currentRoom);
        }

        private void SendClientProfileToHost(NetworkPlayerData profile)
        {
            if (_activeRunner == null || _isHost) return;

            string json = JsonUtility.ToJson(profile);
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
            byte[] payload = new byte[1 + jsonBytes.Length];
            payload[0] = MSG_CLIENT_PROFILE_SUBMIT;
            Buffer.BlockCopy(jsonBytes, 0, payload, 1, jsonBytes.Length);

            var key = ReliableKey.FromInts(MSG_CLIENT_PROFILE_SUBMIT, 0, 0, 0);
            foreach (var p in _activeRunner.ActivePlayers)
            {
                if (p != _activeRunner.LocalPlayer)
                {
                    _activeRunner.SendReliableDataToPlayer(p, key, payload);
                    break;
                }
            }
        }

        private int GetLivePing(PlayerRef player)
        {
            if (_activeRunner == null) return 15;
            double rtt = _activeRunner.GetPlayerRtt(player);
            if (rtt <= 0) return _isHost ? 8 : 25;
            return Mathf.Clamp(Mathf.RoundToInt((float)(rtt * 1000.0)), 1, 999);
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

            if (_activeRunner.GetComponent<NetworkPlayerSpawner>() == null)
            {
                _activeRunner.gameObject.AddComponent<NetworkPlayerSpawner>();
            }
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
            if (runner.IsServer && runner.TryGetComponent<NetworkPlayerSpawner>(out var spawner))
            {
                spawner.PlayerJoined(player);
            }

            if (_currentRoom == null) return;

            bool isLocal = player == runner.LocalPlayer;
            if (_isHost && !isLocal)
            {
                string pid = player.PlayerId.ToString();
                var existing = _currentRoom.Players.Find(p => p.PlayerId == pid);
                if (existing == null)
                {
                    var remotePlayer = new NetworkPlayerData
                    {
                        PlayerId = pid,
                        DisplayName = $"Hiệp Khách #{player.PlayerId}",
                        SelectedCharacterId = "Đang chọn...",
                        IsHost = false,
                        IsReady = false,
                        PingMs = GetLivePing(player)
                    };
                    _currentRoom.Players.Add(remotePlayer);
                }

                BroadcastRoomState();
            }
            else if (!isLocal)
            {
                // Client cũng chủ động gửi lại Profile khi thấy Host
                if (_currentRoom.LocalPlayer != null)
                {
                    SendClientProfileToHost(_currentRoom.LocalPlayer);
                }
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer && runner.TryGetComponent<NetworkPlayerSpawner>(out var spawner))
            {
                spawner.PlayerLeft(player);
            }

            if (_currentRoom == null) return;

            string pid = player.PlayerId.ToString();
            _currentRoom.Players.RemoveAll(p => p.PlayerId == pid);

            if (_isHost)
            {
                BroadcastRoomState();
            }
            else
            {
                OnRoomUpdated?.Invoke(_currentRoom);
            }
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var inputData = new NetworkInputData();
            Vector2 moveDir = Vector2.zero;
            Vector2 aimDir = Vector2.zero;
            NetworkInputButtons buttons = NetworkInputButtons.None;

            // 1. Đọc từ PlayerInputReader của Local Player
            if (PlayerProvider.HasPlayer && PlayerProvider.PlayerGameObject != null && PlayerProvider.PlayerGameObject.TryGetComponent<PlayerInputReader>(out var inputReader))
            {
                moveDir = inputReader.MovementInput;
                buttons = inputReader.ConsumePendingButtons(out aimDir);
            }

            // 2. Ưu tiên ghi đè moveDir từ Mobile Virtual Joystick nếu có thao tác chạm thực tế
            if (ProjectZombie.Features.UI.DynamicVirtualJoystick.Instance != null && ProjectZombie.Features.UI.DynamicVirtualJoystick.Instance.InputVector.sqrMagnitude > 0.001f)
            {
                moveDir = ProjectZombie.Features.UI.DynamicVirtualJoystick.Instance.InputVector;
            }

            // 3. Fallback: Đọc từ New Input System Keyboard khi chạy trong Unity Editor hoặc Standalone
            if (moveDir.sqrMagnitude < 0.001f)
            {
#if ENABLE_INPUT_SYSTEM
                if (UnityEngine.InputSystem.Keyboard.current != null)
                {
                    var kb = UnityEngine.InputSystem.Keyboard.current;
                    float h = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
                    float v = (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1f : 0f);
                    if (h != 0 || v != 0)
                    {
                        moveDir = new Vector2(h, v);
                    }
                }
#endif
            }

            inputData.MoveDirection = moveDir.sqrMagnitude > 1f ? moveDir.normalized : moveDir;
            inputData.AimDirection = aimDir;
            inputData.Buttons = buttons;
            input.Set(inputData);
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            if (runner != null && runner.TryGetComponent<NetworkPlayerSpawner>(out var spawner))
            {
                spawner.StopMatch();
            }

            _currentRoom = null;
            _isHost = false;
            OnRoomUpdated?.Invoke(null);
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            if (!_isHost && _currentRoom != null && _currentRoom.LocalPlayer != null)
            {
                SendClientProfileToHost(_currentRoom.LocalPlayer);
            }
        }

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

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef sender, ReliableKey key, ArraySegment<byte> data)
        {
            if (data.Count == 0 || data.Array == null) return;

            byte msgType = data.Array[data.Offset];

            if (msgType == MSG_MATCH_START)
            {
                Debug.Log("<color=#00FF88>[PhotonFusionSessionService]</color> Client nhận tín hiệu bắt đầu trận đấu từ Host!");
                if (_currentRoom != null) _currentRoom.IsGameStarted = true;
                OnMatchStarted?.Invoke();
            }
            else if (msgType == MSG_ROOM_STATE_SYNC)
            {
                if (data.Count > 1)
                {
                    string json = System.Text.Encoding.UTF8.GetString(data.Array, data.Offset + 1, data.Count - 1);
                    var syncedRoom = JsonUtility.FromJson<NetworkRoomInfo>(json);
                    if (syncedRoom != null)
                    {
                        string localPid = runner.LocalPlayer.PlayerId.ToString();
                        foreach (var p in syncedRoom.Players)
                        {
                            if (p.PlayerId == localPid)
                            {
                                p.DisplayName = p.IsHost ? "Chủ Phòng (Bạn)" : $"{p.DisplayName} (Bạn)";
                                p.PingMs = GetLivePing(runner.LocalPlayer);
                                syncedRoom.LocalPlayer = p;
                            }
                        }
                        _currentRoom = syncedRoom;
                        OnRoomUpdated?.Invoke(_currentRoom);
                    }
                }
            }
            else if (msgType == MSG_CLIENT_PROFILE_SUBMIT)
            {
                if (_isHost && data.Count > 1 && _currentRoom != null)
                {
                    string json = System.Text.Encoding.UTF8.GetString(data.Array, data.Offset + 1, data.Count - 1);
                    var clientProfile = JsonUtility.FromJson<NetworkPlayerData>(json);
                    if (clientProfile != null)
                    {
                        string senderPid = sender.PlayerId.ToString();
                        var existing = _currentRoom.Players.Find(p => p.PlayerId == senderPid);
                        if (existing != null)
                        {
                            existing.DisplayName = !string.IsNullOrEmpty(clientProfile.DisplayName) ? clientProfile.DisplayName : $"Hiệp Khách #{sender.PlayerId}";
                            existing.SelectedCharacterId = clientProfile.SelectedCharacterId;
                            existing.IsReady = clientProfile.IsReady;
                        }
                        else
                        {
                            clientProfile.PlayerId = senderPid;
                            clientProfile.IsHost = false;
                            _currentRoom.Players.Add(clientProfile);
                        }

                        BroadcastRoomState();
                    }
                }
            }
            else if (msgType == MSG_CLIENT_READY_SUBMIT)
            {
                if (_isHost && data.Count >= 2 && _currentRoom != null)
                {
                    bool isReady = data.Array[data.Offset + 1] == 1;
                    string senderPid = sender.PlayerId.ToString();
                    var existing = _currentRoom.Players.Find(p => p.PlayerId == senderPid);
                    if (existing != null)
                    {
                        existing.IsReady = isReady;
                        BroadcastRoomState();
                    }
                }
            }
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    }
}
