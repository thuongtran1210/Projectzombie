using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using ProjectZombie.Core.Services.Data;

namespace ProjectZombie.Features.Spawners
{
    /// <summary>
    /// Component điều phối trung gian nạp tài nguyên Addressables bất đồng bộ
    /// và khởi tạo Object Pool trước khi vào trận đấu (Preload Phase).
    /// </summary>
    public class WavePreloader : MonoBehaviour
    {
        private IGameDataService _gameDataService;
        private readonly List<string> _loadedAddresses = new List<string>();
        private CancellationTokenSource _cts;

        public void Construct(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService;
        }

        private void Awake()
        {
            _cts = new CancellationTokenSource();
        }

        /// <summary>
        /// Nạp bất đồng bộ toàn bộ Prefab quái có trong LevelTimelineConfig và đưa vào Object Pool.
        /// </summary>
        public async Task PreloadTimelineAssetsAsync(LevelTimelineConfig timelineConfig, CancellationToken cancellationToken = default)
        {
            if (timelineConfig == null || timelineConfig.events == null) return;

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
            var ct = linkedCts.Token;

            // Dùng HashSet để tránh load lặp lại nếu nhiều event dùng chung 1 loại quái
            var processedKeys = new HashSet<string>();

            foreach (var evt in timelineConfig.events)
            {
                if (ct.IsCancellationRequested) break;

                string poolKey = evt.GetPoolKey();
                if (string.IsNullOrEmpty(poolKey) || processedKeys.Contains(poolKey)) continue;

                processedKeys.Add(poolKey);

                GameObject enemyPrefab = null;

                // 1. Nạp tập trung qua GameDataService (RAM Cache -> Addressables -> Fallback)
                if (!string.IsNullOrEmpty(evt.enemyAddress))
                {
                    try
                    {
                        if (_gameDataService != null)
                            enemyPrefab = await _gameDataService.GetAsync<GameObject>(evt.enemyAddress);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[WavePreloader] Asset load failed for '{evt.enemyAddress}': {ex.Message}");
                    }
                }

                // 2. Fallback dùng Direct Reference nếu chưa gán Addressable Address
                if (enemyPrefab == null && evt.spawnPrefab != null)
                {
                    enemyPrefab = evt.spawnPrefab;
                }

                if (ct.IsCancellationRequested) break;

                // 3. Đưa Prefab vào EnemyPoolManager và gán lại cho Event
                if (enemyPrefab != null)
                {
                    evt.spawnPrefab = enemyPrefab;

                    if (EnemyPoolManager.Instance != null)
                    {
                        int preloadAmount = evt.eventType == TimelineEventType.BurstWave ? Mathf.Min(evt.spawnCount, 25) : 15;
                        EnemyPoolManager.Instance.PrewarmPool(enemyPrefab, preloadAmount, poolKey);

                        if (!string.IsNullOrEmpty(evt.enemyAddress) && !_loadedAddresses.Contains(evt.enemyAddress))
                        {
                            _loadedAddresses.Add(evt.enemyAddress);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Giải phóng bộ nhớ RAM đối với các Addressable Assets khi kết thúc màn chơi.
        /// </summary>
        public void ReleasePreloadedAssets()
        {
            if (_gameDataService != null)
            {
                foreach (var address in _loadedAddresses)
                {
                    _gameDataService.ReleaseAsset(address);
                }
            }
            _loadedAddresses.Clear();
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            ReleasePreloadedAssets();
        }
    }
}
