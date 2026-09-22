using UnityEngine;
using ProjectZombie.Features.Collectibles.PowerUps.Config;
using ProjectZombie.Features.Spawners.Core;
using ProjectZombie.Features.Spawners.Spatial;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Collectibles.PowerUps
{
    /// <summary>
    /// Component điều phối việc kích hoạt ngẫu nhiên sự kiện sinh vật phẩm bổ trợ trên bản đồ.
    /// Tuân thủ nguyên tắc Single Responsibility (S) và Dependency Inversion (D):
    /// - Không chứa logic hiệu ứng vật phẩm.
    /// - Tìm vị trí an toàn thông qua ISpawnPositionLocator.
    /// - Cấp phát và thu hồi thông qua PowerUpPoolManager (0 GC).
    /// </summary>
    public class MapPowerUpSpawner : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Cấu hình quy tắc sinh vật phẩm ngẫu nhiên")]
        [SerializeField] private MapPowerUpConfigSO _config;

        private ISpawnPositionLocator _spawnLocator;
        private float _nextSpawnTime;

        public void Construct(ISpawnPositionLocator spawnLocator)
        {
            _spawnLocator = spawnLocator;
        }

        private void Start()
        {
            if (_spawnLocator == null)
            {
                // Mặc định sử dụng CameraAwareSpawnLocator của dự án để đảm bảo vị trí hợp lệ trên sàn đấu
                _spawnLocator = new CameraAwareSpawnLocator(null, Camera.main, cameraPadding: 2.0f);
            }

            ScheduleNextSpawn();
        }

        private void Update()
        {
            if (!GameStateManager.IsPlaying) return;
            if (_config == null || _config.entries.Count == 0) return;

            if (Time.time >= _nextSpawnTime)
            {
                TrySpawnRandomPowerUp();
                ScheduleNextSpawn();
            }
        }

        private void TrySpawnRandomPowerUp()
        {
            // 1. Kiểm tra giới hạn số lượng vật phẩm đang tồn tại trên sàn
            if (PowerUpPoolManager.Instance != null &&
                PowerUpPoolManager.Instance.ActiveItemCount >= _config.maxActiveItemsOnGround)
            {
                return;
            }

            // 2. Chọn ngẫu nhiên loại vật phẩm
            var selectedEntry = _config.SelectRandomEntry();
            if (selectedEntry == null || selectedEntry.itemPrefab == null) return;

            // 3. Tìm vị trí người chơi hiện tại
            Transform playerTf = PlayerProvider.PlayerTransform;
            if (playerTf == null) return;

            // 4. Tìm vị trí spawn hợp lệ xung quanh Player (tránh tường, chướng ngại vật)
            Vector3 spawnPos = _spawnLocator != null
                ? _spawnLocator.GetSpawnPosition(playerTf, _config.minSpawnRadius, _config.maxSpawnRadius)
                : playerTf.position + (Vector3)(Random.insideUnitCircle.normalized * _config.minSpawnRadius);

            // 5. Cấp phát từ Pool Manager (0 GC Allocation)
            if (PowerUpPoolManager.Instance != null)
            {
                PowerUpPoolManager.Instance.SpawnItem(selectedEntry.itemPrefab, spawnPos, selectedEntry.lifeTime);
            }
        }

        private void ScheduleNextSpawn()
        {
            if (_config == null)
            {
                _nextSpawnTime = Time.time + 45f;
                return;
            }

            float interval = Random.Range(_config.minIntervalSeconds, _config.maxIntervalSeconds);
            _nextSpawnTime = Time.time + interval;
        }
    }
}
