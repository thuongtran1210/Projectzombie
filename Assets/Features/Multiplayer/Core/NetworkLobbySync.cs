using System;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Multiplayer.Core
{
    /// <summary>
    /// Đồng bộ hóa dữ liệu trạng thái phòng chờ (Lobby Synchronizer) giữa Host và các Client.
    /// Đơn trách nhiệm: Chỉ quản lý Room State, Player Slots, Ready Toggles, và Live Ping qua mạng.
    /// </summary>
    public class NetworkLobbySync : MonoBehaviour, INetworkRunnerCallbacks
    {
        private NetworkRunner _runner;
        private NetworkRoomInfo _currentRoom;
        private bool _isHost;
        private float _lastPingSyncTime;

        public NetworkRoomInfo CurrentRoom => _currentRoom;
        public bool IsHost => _isHost;

        public event Action<NetworkRoomInfo> OnRoomUpdated;
        public event Action OnMatchStarted;

        private void Update()
        {
            if (_runner == null || !_runner.IsRunning || _currentRoom == null || _currentRoom.IsGameStarted) return;

            // Định kỳ mỗi 1.5 giây cập nhật lại chỉ số Ping (RTT)
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
                        _currentRoom.LocalPlayer.PingMs = GetLivePing(_runner.LocalPlayer);
                        OnRoomUpdated?.Invoke(_currentRoom);
                    }
                }
            }
        }

        public void Initialize(NetworkRunner runner, bool isHost, string roomCode, int maxPlayers)
        {
            _runner = runner;
            _isHost = isHost;

            string heroName = RunLoadoutState.SelectedCharacter != null ? RunLoadoutState.SelectedCharacter.characterName : (isHost ? "Đạo Sĩ" : "Thư Sinh");
            _currentRoom = new NetworkRoomInfo
            {
                RoomCode = roomCode,
                RoomName = isHost ? $"Phòng Host [{roomCode}]" : $"Phòng Trận [{roomCode}]",
                MaxPlayers = maxPlayers,
                IsGameStarted = false
            };

            var localData = new NetworkPlayerData
            {
                PlayerId = _runner.LocalPlayer.PlayerId.ToString(),
                DisplayName = isHost ? "Chủ Phòng (Bạn)" : "Hiệp Khách",
                SelectedCharacterId = heroName,
                IsHost = isHost,
                IsReady = isHost, // Host mặc định luôn Ready
                PingMs = GetLivePing(_runner.LocalPlayer)
            };

            _currentRoom.Players.Add(localData);
            _currentRoom.LocalPlayer = localData;

            if (!isHost)
            {
                SendClientProfileToHost(localData);
            }

            OnRoomUpdated?.Invoke(_currentRoom);
        }

        public void ResetRoom()
        {
            _currentRoom = null;
            _isHost = false;
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
            else if (_runner != null && _runner.IsRunning)
            {
                byte[] payload = LobbyMessageProtocol.EncodeClientReady(isReady);
                var key = ReliableKey.FromInts(LobbyMessageProtocol.MSG_CLIENT_READY_SUBMIT, 0, 0, 0);
                foreach (var p in _runner.ActivePlayers)
                {
                    if (p != _runner.LocalPlayer)
                    {
                        _runner.SendReliableDataToPlayer(p, key, payload);
                        break;
                    }
                }
            }

            OnRoomUpdated?.Invoke(_currentRoom);
        }

        public void NotifyMatchStarted()
        {
            if (_currentRoom != null) _currentRoom.IsGameStarted = true;

            if (_isHost && _runner != null)
            {
                byte[] startSignal = LobbyMessageProtocol.EncodeMatchStart();
                var msgKey = ReliableKey.FromInts(LobbyMessageProtocol.MSG_MATCH_START, 0, 0, 0);
                foreach (var player in _runner.ActivePlayers)
                {
                    if (player != _runner.LocalPlayer)
                    {
                        _runner.SendReliableDataToPlayer(player, msgKey, startSignal);
                    }
                }
            }

            OnMatchStarted?.Invoke();
        }

        public void BroadcastRoomState()
        {
            if (_runner == null || !_isHost || _currentRoom == null) return;

            // Cập nhật chỉ số Ping cho từng người chơi trước khi phát sóng
            foreach (var p in _currentRoom.Players)
            {
                if (int.TryParse(p.PlayerId, out int pid))
                {
                    var pRef = PlayerRef.FromIndex(pid);
                    p.PingMs = GetLivePing(pRef);
                }
            }

            byte[] payload = LobbyMessageProtocol.EncodeRoomState(_currentRoom);
            var key = ReliableKey.FromInts(LobbyMessageProtocol.MSG_ROOM_STATE_SYNC, 0, 0, 0);
            foreach (var player in _runner.ActivePlayers)
            {
                if (player != _runner.LocalPlayer)
                {
                    _runner.SendReliableDataToPlayer(player, key, payload);
                }
            }

            OnRoomUpdated?.Invoke(_currentRoom);
        }

        private void SendClientProfileToHost(NetworkPlayerData profile)
        {
            if (_runner == null || _isHost) return;

            byte[] payload = LobbyMessageProtocol.EncodeClientProfile(profile);
            var key = ReliableKey.FromInts(LobbyMessageProtocol.MSG_CLIENT_PROFILE_SUBMIT, 0, 0, 0);
            foreach (var p in _runner.ActivePlayers)
            {
                if (p != _runner.LocalPlayer)
                {
                    _runner.SendReliableDataToPlayer(p, key, payload);
                    break;
                }
            }
        }

        private int GetLivePing(PlayerRef player)
        {
            if (_runner == null) return 15;
            double rtt = _runner.GetPlayerRtt(player);
            if (rtt <= 0) return _isHost ? 8 : 25;
            return Mathf.Clamp(Mathf.RoundToInt((float)(rtt * 1000.0)), 1, 999);
        }

        // =========================================================================
        // PHOTON FUSION CALLBACKS FOR LOBBY
        // =========================================================================

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
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
                if (_currentRoom.LocalPlayer != null)
                {
                    SendClientProfileToHost(_currentRoom.LocalPlayer);
                }
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
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

        public void OnConnectedToServer(NetworkRunner runner)
        {
            if (!_isHost && _currentRoom != null && _currentRoom.LocalPlayer != null)
            {
                SendClientProfileToHost(_currentRoom.LocalPlayer);
            }
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef sender, ReliableKey key, ArraySegment<byte> data)
        {
            if (data.Count == 0 || data.Array == null) return;

            byte msgType = data.Array[data.Offset];

            if (msgType == LobbyMessageProtocol.MSG_MATCH_START)
            {
                Debug.Log("<color=#00FF88>[NetworkLobbySync]</color> Client nhận tín hiệu bắt đầu trận đấu từ Host!");
                NotifyMatchStarted();
            }
            else if (msgType == LobbyMessageProtocol.MSG_ROOM_STATE_SYNC)
            {
                var syncedRoom = LobbyMessageProtocol.DecodeRoomState(data);
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
            else if (msgType == LobbyMessageProtocol.MSG_CLIENT_PROFILE_SUBMIT)
            {
                if (_isHost && _currentRoom != null)
                {
                    var clientProfile = LobbyMessageProtocol.DecodeClientProfile(data);
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
            else if (msgType == LobbyMessageProtocol.MSG_CLIENT_READY_SUBMIT)
            {
                if (_isHost && _currentRoom != null)
                {
                    bool isReady = LobbyMessageProtocol.DecodeClientReady(data);
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

        // Dummy runner callbacks not used by lobby sync
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    }
}
