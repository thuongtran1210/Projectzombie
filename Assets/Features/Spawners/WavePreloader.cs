using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using ProjectZombie.Core.Services.Addressables;

namespace ProjectZombie.Features.Spawners
{
    /// <summary>
    /// Component điều phối trung gian nạp tài nguyên Addressables bất đồng bộ
    /// và khởi tạo Object Pool trước khi vào trận đấu (Preload Phase).
    /// </summary>
    public class WavePreloader : MonoBehaviour
    {
        private IAssetProvider _assetProvider;
        private readonly List<string> _loadedAddresses = new List<string>();

        public void Construct(IAssetProvider assetProvider)
        {
            _assetProvider = assetProvider;
        }

        /// <summary>
        /// Nạp bất đồng bộ toàn bộ Prefab quái có trong LevelTimelineConfig và đưa vào Object Pool.
        /// </summary>
        public async Task PreloadTimelineAssetsAsync(LevelTimelineConfig timelineConfig)
        {
            if (timelineConfig == null || timelineConfig.events == null) return;

            // Dùng HashSet để tránh load lặp lại nếu nhiều event dùng chung 1 loại quái
            var processedKeys = new HashSet<string>();

            foreach (var evt in timelineConfig.events)
            {
                string poolKey = evt.GetPoolKey();
                if (string.IsNullOrEmpty(poolKey) || processedKeys.Contains(poolKey)) continue;

                processedKeys.Add(poolKey);

                GameObject enemyPrefab = null;

                // 1. Nạp từ Addressables nếu có địa chỉ Address
                if (!string.IsNullOrEmpty(evt.enemyAddress))
                {
                    if (_assetProvider != null)
                    {
                        enemyPrefab = await _assetProvider.LoadAssetAsync<GameObject>(evt.enemyAddress);
                    }
                    else
                    {
                        var handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(evt.enemyAddress);
                        await handle.Task;
                        if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                        {
                            enemyPrefab = handle.Result;
                        }
                    }
                }
                // 2. Fallback dùng Direct Reference nếu chưa gán Addressable Address
                else if (evt.spawnPrefab != null)
                {
                    enemyPrefab = evt.spawnPrefab;
                }

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
            if (_assetProvider != null)
            {
                foreach (var address in _loadedAddresses)
                {
                    _assetProvider.ReleaseAsset(address);
                }
            }
            _loadedAddresses.Clear();
        }

        private void OnDestroy()
        {
            ReleasePreloadedAssets();
        }
    }
}
