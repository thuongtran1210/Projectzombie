using System.Collections.Generic;
using UnityEngine;
using Fusion;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Player.Core;
using ProjectZombie.Features.Player.Input;
using ProjectZombie.Core.Architecture;

namespace ProjectZombie.Features.Multiplayer.Core
{
    /// <summary>
    /// Điều phối việc sinh (Spawn) và cấu hình nhân vật cho các người chơi trong phòng mạng Photon Fusion.
    /// Tự động phân biệt giữa Local Player (máy này) và Remote Player (máy khác).
    /// </summary>
    public class NetworkPlayerSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
    {
        [Header("Spawn Configuration")]
        [Tooltip("Prefab nhân vật mạng (Bắt buộc có NetworkObject & NetworkTransform)")]
        [SerializeField] private NetworkPrefabRef _networkPlayerPrefab;

        [Tooltip("Các điểm xuất hiện của người chơi trên bản đồ")]
        [SerializeField] private Transform[] _spawnPoints;

        private readonly Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

        /// <summary>
        /// Trạng thái trận đấu đã thực sự bắt đầu hay chưa (false = Đang trong Sảnh Chờ Lobby).
        /// </summary>
        public bool IsMatchActive { get; private set; } = false;

        /// <summary>
        /// Host kích hoạt bắt đầu trận đấu: Spawn nhân vật cho toàn bộ người chơi đã kết nối trong phòng.
        /// </summary>
        public void StartMatch()
        {
            IsMatchActive = true;

            // Dọn dẹp thực thể nhân vật Offline/Singleplayer để nhường chỗ cho nhân vật mạng
            PlayerProvider.ClearPlayer();

            SpawnAllActivePlayers();
        }

        /// <summary>
        /// Kết thúc trận đấu hoặc rời phòng: Dọn dẹp toàn bộ nhân vật mạng.
        /// </summary>
        public void StopMatch()
        {
            IsMatchActive = false;
            DespawnAllPlayers();
        }

        /// <summary>
        /// Host sinh thực thể nhân vật cho tất cả người chơi hiện có trong phiên.
        /// </summary>
        public void SpawnAllActivePlayers()
        {
            if (Runner == null || !Runner.IsServer) return;

            foreach (var player in Runner.ActivePlayers)
            {
                SpawnPlayerCharacter(player);
            }
        }

        public void PlayerJoined(PlayerRef player)
        {
            // CHỈ spawn nhân vật khi trận đấu đã bắt đầu (IsMatchActive == true, ví dụ Late Join).
            // Khi đang ở trong Sảnh Chờ (Lobby), tuyệt đối không spawn nhân vật vào map để giữ nguyên giao diện phòng chờ.
            if (Runner != null && Runner.IsServer && IsMatchActive)
            {
                SpawnPlayerCharacter(player);
            }
        }

        [Header("Character Database Reference")]
        [Tooltip("Database toàn bộ Anh Hùng để tra cứu Prefab mạng (Single Source of Truth)")]
        [SerializeField] private CharacterDatabaseSO _characterDatabase;

        private ICharacterPrefabProvider _prefabProvider;

        public void SetPrefabProvider(ICharacterPrefabProvider provider)
        {
            _prefabProvider = provider;
        }

        private ICharacterPrefabProvider GetPrefabProvider()
        {
            if (_prefabProvider == null)
            {
                _prefabProvider = new CharacterDatabasePrefabProvider(_characterDatabase);
            }
            return _prefabProvider;
        }

        private void SpawnPlayerCharacter(PlayerRef player)
        {
            if (Runner == null || !Runner.IsServer) return;

            if (_spawnedCharacters.TryGetValue(player, out var existing) && existing != null)
            {
                return;
            }

            Vector3 spawnPos = GetSpawnPosition(player.PlayerId);
            Quaternion spawnRot = Quaternion.identity;
            NetworkObject prefabToSpawn = null;
            var provider = GetPrefabProvider();

            // 1. Tìm đúng tướng đã chọn qua ICharacterPrefabProvider (DIP & OCP)
            if (player == Runner.LocalPlayer)
            {
                string localHeroId = RunLoadoutState.SelectedCharacter?.characterId ?? RunLoadoutState.SelectedCharacter?.characterName;
                if (!string.IsNullOrEmpty(localHeroId) && provider != null)
                {
                    prefabToSpawn = provider.GetNetworkCharacterPrefab(localHeroId);
                }
            }
            else
            {
                // Đối với khách (Remote Client): Tìm theo mã tướng gửi qua hồ sơ mạng
                if (ServiceContext.TryGet<INetworkSessionService>(out var session) && session.CurrentRoom != null)
                {
                    string pid = player.PlayerId.ToString();
                    var pData = session.CurrentRoom.Players.Find(p => p.PlayerId == pid);
                    if (pData != null && !string.IsNullOrEmpty(pData.SelectedCharacterId) && provider != null)
                    {
                        prefabToSpawn = provider.GetNetworkCharacterPrefab(pData.SelectedCharacterId);
                    }
                }
            }

            // 2. Fallback 1: Lấy tướng mặc định từ Provider
            if (prefabToSpawn == null && provider != null)
            {
                prefabToSpawn = provider.GetDefaultNetworkCharacterPrefab();
            }

            // 3. Fallback 2: Sử dụng _networkPlayerPrefab được cấu hình qua Inspector
            if (prefabToSpawn == null && _networkPlayerPrefab.IsValid)
            {
                try
                {
                    var playerObj = Runner.Spawn(_networkPlayerPrefab, spawnPos, spawnRot, player);
                    if (playerObj != null)
                    {
                        _spawnedCharacters[player] = playerObj;
                        Debug.Log($"<color=#00FF88>[NetworkPlayerSpawner]</color> Đã spawn nhân vật cho PlayerRef #{player.PlayerId} tại {spawnPos}");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[NetworkPlayerSpawner] Lỗi spawn fallback: {ex.Message}");
                }
                return;
            }

            if (prefabToSpawn != null)
            {
                try
                {
                    var playerObj = Runner.Spawn(prefabToSpawn, spawnPos, spawnRot, player);
                    if (playerObj != null)
                    {
                        _spawnedCharacters[player] = playerObj;
                        Debug.Log($"<color=#00FF88>[NetworkPlayerSpawner]</color> Host đã spawn nhân vật '{playerObj.name}' cho PlayerRef #{player.PlayerId} tại {spawnPos}");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[NetworkPlayerSpawner] Không thể spawn nhân vật '{prefabToSpawn.name}': {ex.Message}");
                }
            }
            else
            {
                Debug.LogError($"[NetworkPlayerSpawner] Không tìm thấy Prefab hợp lệ cho PlayerRef #{player.PlayerId}!");
            }
        }

        public void PlayerLeft(PlayerRef player)
        {
            if (Runner != null && Runner.IsServer)
            {
                if (_spawnedCharacters.TryGetValue(player, out var playerObj))
                {
                    if (playerObj != null)
                    {
                        if (ServiceContext.TryGet<IPlayerRegistry>(out var registry))
                        {
                            var ctx = registry.GetPlayerById(player.PlayerId);
                            if (ctx != null) registry.Unregister(ctx);
                        }

                        Runner.Despawn(playerObj);
                    }
                    _spawnedCharacters.Remove(player);
                }
            }
        }

        public void DespawnAllPlayers()
        {
            if (Runner != null && Runner.IsServer)
            {
                foreach (var kvp in _spawnedCharacters)
                {
                    if (kvp.Value != null)
                    {
                        Runner.Despawn(kvp.Value);
                    }
                }
            }
            _spawnedCharacters.Clear();
        }

        private Vector3 GetSpawnPosition(int playerId)
        {
            if (_spawnPoints != null && _spawnPoints.Length > 0)
            {
                int index = Mathf.Abs(playerId) % _spawnPoints.Length;
                if (_spawnPoints[index] != null)
                {
                    return _spawnPoints[index].position;
                }
            }
            return Vector3.zero;
        }
    }
}
