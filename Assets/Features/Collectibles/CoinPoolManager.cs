using UnityEngine;
using UnityEngine.Pool;

namespace ProjectZombie.Features.Collectibles
{
    /// <summary>
    /// Manager singleton quản lý Object Pooling cho Coin Drops (Cổ Tiền rớt từ Enemy).
    /// Kế thừa từ CollectiblePoolBase, hỗ trợ cơ chế Nén & Gộp đồng tiền khi số lượng trên sàn vượt quá ngưỡng cho phép.
    /// </summary>
    public class CoinPoolManager : CollectiblePoolBase<CoinPoolManager, CoinDrop>
    {
        [Header("Default Prefab")]
        [SerializeField] private GameObject defaultCoinPrefab;

        // Backward-compatibility aliases
        public int ActiveCoinCount => ActiveItemCount;
        public void RegisterActiveCoin(CoinDrop coin) => RegisterActiveItem(coin);
        public void UnregisterActiveCoin(CoinDrop coin) => UnregisterActiveItem(coin);

        protected override void AttachPoolConfig(GameObject obj, IObjectPool<GameObject> pool)
        {
            var config = obj.GetComponent<CoinPoolConfig>() ?? obj.AddComponent<CoinPoolConfig>();
            config.Pool = pool;
        }

        /// <summary>
        /// Spawn đồng Coin tại vị trí chỉ định.
        /// </summary>
        public GameObject SpawnCoin(GameObject prefab, Vector3 position, int amount)
        {
            GameObject targetPrefab = prefab != null ? prefab : defaultCoinPrefab;
            if (targetPrefab == null) return null;

            if (ActiveItems.Count >= maxGroundItems)
            {
                CompressDistantItems();
            }

            var pool = GetOrCreatePool(targetPrefab);
            if (pool != null)
            {
                GameObject coinObj = null;
                while (pool.CountInactive > 0)
                {
                    coinObj = pool.Get();
                    if (coinObj != null) break;
                }

                if (coinObj == null)
                {
                    coinObj = Instantiate(targetPrefab, transform);
                    AttachPoolConfig(coinObj, pool);
                }

                coinObj.transform.position = position;
                coinObj.transform.rotation = Quaternion.identity;

                var coinDrop = coinObj.GetComponent<CoinDrop>();
                if (coinDrop != null)
                {
                    coinDrop.SetCoinValue(amount);
                }

                return coinObj;
            }

            GameObject fallbackObj = Instantiate(targetPrefab, position, Quaternion.identity);
            var fallbackCoin = fallbackObj.GetComponent<CoinDrop>();
            if (fallbackCoin != null) fallbackCoin.SetCoinValue(amount);
            return fallbackObj;
        }

        public GameObject SpawnCoin(Vector3 position, int amount)
        {
            return SpawnCoin(defaultCoinPrefab, position, amount);
        }

        /// <summary>
        /// Gộp 2 đồng tiền ở xa người chơi nhất thành 1 đồng giá trị cao hơn.
        /// </summary>
        protected override void CompressDistantItems()
        {
            if (ActiveItems.Count < 2) return;

            Vector3 playerPos = Vector3.zero;
            if (Player.PlayerController.Instance != null)
            {
                playerPos = Player.PlayerController.Instance.transform.position;
            }

            CoinDrop furthest1 = null;
            CoinDrop furthest2 = null;
            float maxDistSq1 = -1f;
            float maxDistSq2 = -1f;

            for (int i = 0; i < _activeCoins.Count; i++)
            {
                var coin = _activeCoins[i];
                if (coin == null || !coin.IsIdle) continue;

                float distSq = (coin.transform.position - playerPos).sqrMagnitude;

                if (distSq > maxDistSq1)
                {
                    maxDistSq2 = maxDistSq1;
                    furthest2 = furthest1;

                    maxDistSq1 = distSq;
                    furthest1 = coin;
                }
                else if (distSq > maxDistSq2)
                {
                    maxDistSq2 = distSq;
                    furthest2 = coin;
                }
            }

            if (furthest1 != null && furthest2 != null && furthest1 != furthest2)
            {
                furthest1.MergeCoin(furthest2.CoinValue);

                if (furthest2.TryGetComponent<CoinPoolConfig>(out var poolConfig))
                {
                    poolConfig.ReturnToPool();
                }
                else
                {
                    Destroy(furthest2.gameObject);
                }
            }
        }

        /// <summary>
        /// Hút toàn bộ đồng xu trên sàn đấu về phía người chơi.
        /// </summary>
        public void CollectAllActiveCoins(Transform player)
        {
            if (player == null) return;

            for (int i = _activeCoins.Count - 1; i >= 0; i--)
            {
                var coin = _activeCoins[i];
                if (coin != null && coin.IsActiveOnGround)
                {
                    coin.StartMagnetEffect(player);
                }
            }
        }
    }
}
