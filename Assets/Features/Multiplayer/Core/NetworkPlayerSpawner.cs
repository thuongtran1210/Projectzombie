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
            if (_networkPlayerPrefab.IsValid)
            {
                playerObject = Runner.Spawn(_networkPlayerPrefab, spawnPos, spawnRot, player);
            }
            else
            {
                var fallbackPrefab = Resources.Load<GameObject>("Players/Dao Si")
                                     ?? Resources.Load<GameObject>("Players/DaoSi")
                                     ?? Resources.Load<GameObject>("Players/Thu Sinh");
                if (fallbackPrefab != null && fallbackPrefab.TryGetComponent<NetworkObject>(out var netObj))
                {
                    playerObject = Runner.Spawn(netObj, spawnPos, spawnRot, player);
                }
            }

            if (playerObject != null)
            {
                _spawnedCharacters[player] = playerObject;
                Debug.Log($"<color=#00FF88>[NetworkPlayerSpawner]</color> Host đã spawn nhân vật cho PlayerRef #{player.PlayerId} tại {spawnPos}");
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
