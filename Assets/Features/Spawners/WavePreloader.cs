using System.Collections.Generic;
using System.Threading;
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
        private CancellationTokenSource _cts;

        private IAssetProvider AssetProvider => _assetProvider ??= AddressableAssetManager.Instance;

        public void Construct(IAssetProvider assetProvider)
        {
            _assetProvider = assetProvider;
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

                // 1. Nạp từ Addressables nếu có địa chỉ Address qua AssetProvider chuẩn
                if (!string.IsNullOrEmpty(evt.enemyAddress))
                {
                    try
                    {
                        enemyPrefab = await AssetProvider.LoadAssetAsync<GameObject>(evt.enemyAddress, ct);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[WavePreloader] Addressables load failed for '{evt.enemyAddress}': {ex.Message}. Thử fallback sang Resources...");
                    }

                    // 1.1 Fallback nạp từ Resources nếu Addressables chưa build
                    if (enemyPrefab == null)
                    {
                        enemyPrefab = Resources.Load<GameObject>($"Enemies/{evt.enemyAddress}") ??
                                      Resources.Load<GameObject>(evt.enemyAddress);
                    }

#if UNITY_EDITOR
                    // 1.2 Fallback Editor AssetDatabase
                    if (enemyPrefab == null)
                    {
                        enemyPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Prefabs/Characters/Enemies/{evt.enemyAddress}.prefab");
                    }
#endif
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
            var provider = AssetProvider;
            if (provider != null)
            {
                foreach (var address in _loadedAddresses)
                {
                    provider.ReleaseAsset(address);
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
