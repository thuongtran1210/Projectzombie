using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ProjectZombie.Features.Multiplayer.Core
{
    /// <summary>
    /// Triển khai giả lập INetworkSessionService để phục vụ kiểm thử phòng chơi và UI trong Unity Editor.
    /// Cho phép mô phỏng Host, Client, Room Code 6 ký tự, và người chơi ảo tham gia phòng.
    /// </summary>
    public class MockNetworkSessionService : INetworkSessionService
    {
        private NetworkRoomInfo _currentRoom;
        private bool _isHost;
        private readonly System.Random _random = new System.Random();

        public NetworkRoomInfo CurrentRoom => _currentRoom;
        public bool IsHost => _isHost;
        public bool IsInRoom => _currentRoom != null;

        public event Action<NetworkRoomInfo> OnRoomUpdated;
        public event Action OnMatchStarted;
        public event Action OnMatchEnded;
        public event Action<string> OnConnectionError;

        public async Task<bool> CreateHostSessionAsync(string roomCode = null, int maxPlayers = 4)
        {
            await Task.Delay(100); // Giả lập độ trễ mạng ngắn

            string code = string.IsNullOrEmpty(roomCode) ? GenerateRandomRoomCode() : roomCode.ToUpper();

            _isHost = true;
            _currentRoom = new NetworkRoomInfo
            {
                RoomCode = code,
                RoomName = $"Phòng Của Chủ Phòng [{code}]",
                MaxPlayers = maxPlayers,
                IsGameStarted = false
            };

            var hostPlayer = new NetworkPlayerData
            {
                PlayerId = "player_host_01",
                DisplayName = "Chủ Phòng (Bạn)",
                SelectedCharacterId = "hero_daosi",
                SelectedRelicId = "relic_chuong_dong",
                IsHost = true,
                IsReady = true,
                PingMs = 15
            };

            _currentRoom.Players.Add(hostPlayer);
            _currentRoom.LocalPlayer = hostPlayer;

            OnRoomUpdated?.Invoke(_currentRoom);
            return true;
        }

        public async Task<bool> JoinSessionAsync(string roomCode)
        {
            await Task.Delay(150);

            if (string.IsNullOrWhiteSpace(roomCode) || roomCode.Length < 4)
            {
                OnConnectionError?.Invoke("Mã phòng không hợp lệ. Vui lòng kiểm tra lại 6 ký tự.");
                return false;
            }

            string formattedCode = roomCode.Trim().ToUpper();

            _isHost = false;
            _currentRoom = new NetworkRoomInfo
            {
                RoomCode = formattedCode,
                RoomName = $"Phòng Trận [{formattedCode}]",
                MaxPlayers = 4,
                IsGameStarted = false
            };

            var hostPlayer = new NetworkPlayerData
            {
                PlayerId = "player_host_remote",
                DisplayName = "Đội Trưởng",
                SelectedCharacterId = "hero_daosi",
                SelectedRelicId = "relic_chuong_dong",
                IsHost = true,
                IsReady = true,
                PingMs = 28
            };

            var localClient = new NetworkPlayerData
            {
                PlayerId = "player_client_local",
                DisplayName = "Hiệp Khách (Bạn)",
                SelectedCharacterId = "hero_kiemkhach",
                SelectedRelicId = "relic_chieu_hon",
                IsHost = false,
                IsReady = false,
                PingMs = 32
            };

            _currentRoom.Players.Add(hostPlayer);
            _currentRoom.Players.Add(localClient);
            _currentRoom.LocalPlayer = localClient;

            OnRoomUpdated?.Invoke(_currentRoom);
            return true;
        }

        public async Task LeaveSessionAsync()
        {
            await Task.Delay(50);
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
            if (_currentRoom == null) return;

            if (!_isHost)
            {
                OnConnectionError?.Invoke("Chỉ có Chủ Phòng (Host) mới có quyền Bắt Đầu Trận Đấu.");
                return;
            }

            await Task.Delay(200);
            _currentRoom.IsGameStarted = true;
            OnMatchStarted?.Invoke();
        }

        /// <summary>
        /// Phương thức hỗ trợ Editor/Debug: Giả lập một người chơi khác tham gia phòng.
        /// </summary>
        public void SimulateRemotePlayerJoined(string displayName, string characterId)
        {
            if (_currentRoom == null) return;

            var newPlayer = new NetworkPlayerData
            {
                PlayerId = $"player_mock_{_currentRoom.Players.Count + 1}",
                DisplayName = displayName,
                SelectedCharacterId = characterId,
                IsHost = false,
                IsReady = true,
                PingMs = _random.Next(20, 60)
            };

            _currentRoom.Players.Add(newPlayer);
            OnRoomUpdated?.Invoke(_currentRoom);
        }

        private string GenerateRandomRoomCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            char[] codeChars = new char[6];
            for (int i = 0; i < 6; i++)
            {
                codeChars[i] = chars[_random.Next(chars.Length)];
            }
            return new string(codeChars);
        }
    }
}
