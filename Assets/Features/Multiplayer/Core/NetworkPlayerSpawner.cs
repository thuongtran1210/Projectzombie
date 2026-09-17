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

            // Dọn dẹp thực thể nhân vật Offline/Singleplayer đứng ở Sảnh để nhường chỗ cho nhân vật mạng
            GameplayBootstrapper.Instance?.DespawnActivePlayer();

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

        private void SpawnPlayerCharacter(PlayerRef player)
        {
            if (Runner == null || !Runner.IsServer) return;

            if (_spawnedCharacters.TryGetValue(player, out var existing) && existing != null)
            {
                return;
            }

            Vector3 spawnPos = GetSpawnPosition(player.PlayerId);
            Quaternion spawnRot = Quaternion.identity;

            NetworkObject playerObject = null;
            NetworkObject prefabToSpawn = null;

            // 1. Ưu tiên lấy đúng tướng đã chọn trong RunLoadoutState cho người chơi cục bộ (Host)
            if (player == Runner.LocalPlayer)
            {
                if (RunLoadoutState.SelectedCharacter != null && RunLoadoutState.SelectedCharacter.playerPrefab != null)
                {
                    prefabToSpawn = RunLoadoutState.SelectedCharacter.playerPrefab.GetComponent<NetworkObject>();
                }
            }
            else
            {
                // Đối với người chơi khác (Client): Tìm tướng họ đã chọn trong dữ liệu phòng (Room Info)
                if (ServiceContext.TryGet<INetworkSessionService>(out var session) && session.CurrentRoom != null)
                {
                    string pid = player.PlayerId.ToString();
                    var pData = session.CurrentRoom.Players.Find(p => p.PlayerId == pid);
                    if (pData != null && !string.IsNullOrEmpty(pData.SelectedCharacterId))
                    {
                        var loaded = Resources.Load<GameObject>($"Players/{pData.SelectedCharacterId}");
                        if (loaded != null) prefabToSpawn = loaded.GetComponent<NetworkObject>();
                    }
                }
            }

            // 2. Nếu có gán _networkPlayerPrefab qua Inspector
            if (prefabToSpawn == null && _networkPlayerPrefab.IsValid)
            {
                playerObject = Runner.Spawn(_networkPlayerPrefab, spawnPos, spawnRot, player);
            }
            else
            {
                // 3. Fallback tìm theo tên tướng đã chọn trong Resources/Players
                if (prefabToSpawn == null)
                {
                    string heroName = RunLoadoutState.SelectedCharacter?.characterName;
                    if (!string.IsNullOrEmpty(heroName))
                    {
                        var loaded = Resources.Load<GameObject>($"Players/{heroName}");
                        if (loaded != null) prefabToSpawn = loaded.GetComponent<NetworkObject>();
                    }
                }

                // 4. Fallback cuối cùng
                if (prefabToSpawn == null)
                {
                    var fallbackPrefab = Resources.Load<GameObject>("Players/Thanh Dong")
                                         ?? Resources.Load<GameObject>("Players/Dao Si")
                                         ?? Resources.Load<GameObject>("Players/Thu Sinh");
                    if (fallbackPrefab != null)
                    {
                        prefabToSpawn = fallbackPrefab.GetComponent<NetworkObject>();
                    }
                }

                if (prefabToSpawn != null)
                {
                    playerObject = Runner.Spawn(prefabToSpawn, spawnPos, spawnRot, player);
                }
            }

            if (playerObject != null)
            {
                _spawnedCharacters[player] = playerObject;
                Debug.Log($"<color=#00FF88>[NetworkPlayerSpawner]</color> Host đã spawn nhân vật '{playerObject.name}' cho PlayerRef #{player.PlayerId} tại {spawnPos}");
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
