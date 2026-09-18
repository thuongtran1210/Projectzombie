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

        private NetworkRunner _runner;

        /// <summary>
        /// Gán thủ công NetworkRunner từ SessionService để không phụ thuộc vào chu kỳ AddGlobal.
        /// </summary>
        public void Initialize(NetworkRunner runner)
        {
            _runner = runner;
        }

        /// <summary>
        /// Lấy NetworkRunner khả dụng (Ưu tiên runner tiêm thủ công, fallback base.Runner).
        /// </summary>
        public NetworkRunner ActiveRunner => _runner != null ? _runner : Runner;

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

            // Đảm bảo RunLoadoutState đã được nạp dữ liệu tướng
            RunLoadoutState.EnsureInitialized();

            // Dọn dẹp thực thể nhân vật Offline/Singleplayer để nhường chỗ cho nhân vật mạng
            PlayerProvider.ClearPlayer();

            // Dọn dẹp toàn bộ nhân vật mạng cũ còn sót lại từ lượt chơi trước (nếu có)
            DespawnAllPlayers();

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
            var runner = ActiveRunner;
            if (runner == null || !runner.IsServer)
            {
                Debug.LogWarning($"<color=#FF9900>[NetworkPlayerSpawner]</color> Bỏ qua spawn: Runner={(runner != null ? runner.name : "NULL")}, IsServer={(runner != null && runner.IsServer)}");
                return;
            }

            Debug.Log($"<color=#00FF88>[NetworkPlayerSpawner]</color> Bắt đầu spawn nhân vật mạng cho các người chơi trong phòng...");
            foreach (var player in runner.ActivePlayers)
            {
                SpawnPlayerCharacter(player);
            }
        }

        public void PlayerJoined(PlayerRef player)
        {
            // CHỈ spawn nhân vật khi trận đấu đã bắt đầu (IsMatchActive == true, ví dụ Late Join).
            // Khi đang ở trong Sảnh Chờ (Lobby), tuyệt đối không spawn nhân vật vào map để giữ nguyên giao diện phòng chờ.
            var runner = ActiveRunner;
            if (runner != null && runner.IsServer && IsMatchActive)
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
            var runner = ActiveRunner;
            if (runner == null || !runner.IsServer) return;

            if (_spawnedCharacters.TryGetValue(player, out var existing) && existing != null)
            {
                return;
            }

            RunLoadoutState.EnsureInitialized();

            Vector3 spawnPos = GetSpawnPosition(player.PlayerId);
            Quaternion spawnRot = Quaternion.identity;
            NetworkObject prefabToSpawn = null;
            var provider = GetPrefabProvider();

            // 1. Tìm đúng tướng đã chọn qua ICharacterPrefabProvider (DIP & OCP)
            if (player == runner.LocalPlayer)
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
                    var playerObj = runner.Spawn(_networkPlayerPrefab, spawnPos, spawnRot, player);
                    if (playerObj != null)
                    {
                        _spawnedCharacters[player] = playerObj;
                        Debug.Log($"<color=#00FF88>[NetworkPlayerSpawner]</color> Đã spawn nhân vật fallback cho PlayerRef #{player.PlayerId} tại {spawnPos}");
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
                    var playerObj = runner.Spawn(prefabToSpawn, spawnPos, spawnRot, player);
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
            var runner = ActiveRunner;
            if (runner != null && runner.IsServer)
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

                        runner.Despawn(playerObj);
                    }
                    _spawnedCharacters.Remove(player);
                }
            }
        }

        public void DespawnAllPlayers()
        {
            var runner = ActiveRunner;
            if (runner != null && runner.IsServer)
            {
                foreach (var kvp in _spawnedCharacters)
                {
                    if (kvp.Value != null)
                    {
                        if (ServiceContext.TryGet<IPlayerRegistry>(out var registry))
                        {
                            var ctx = registry.GetPlayerById(kvp.Key.PlayerId);
                            if (ctx != null) registry.Unregister(ctx);
                        }

                        runner.Despawn(kvp.Value);
                    }
                }
            }
            _spawnedCharacters.Clear();

            // Đảm bảo dọn sạch PlayerRegistry nếu còn sót lại các PlayerContext cũ
            if (ServiceContext.TryGet<IPlayerRegistry>(out var pRegistry))
            {
                pRegistry.Clear();
            }
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

            // Xếp so le nhẹ nếu không có mốc spawn cố định
            float offsetX = ((playerId % 2 == 0) ? 1.0f : -1.0f) * ((playerId / 2) + 1) * 0.6f;
            float offsetY = (playerId > 2 ? -0.6f : 0f);
            return new Vector3(offsetX, offsetY, 0f);
        }
    }
}
