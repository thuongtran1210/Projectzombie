using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace ProjectZombie.Features.Collectibles
{
    /// <summary>
    /// Manager singleton quản lý Object Pooling cho Coin Drops (Cổ Tiền rớt từ Enemy).
    /// Hỗ trợ cơ chế Nén & Gộp đồng tiền khi số lượng trên sàn vượt quá ngưỡng cho phép,
    /// bảo đảm duy trì 60 FPS ổn định trên thiết bị di động.
    /// </summary>
    public class CoinPoolManager : MonoBehaviour
    {
        public static CoinPoolManager Instance { get; private set; }

        [Header("Default Prefab")]
        [SerializeField] private GameObject defaultCoinPrefab;

        [Header("Performance & Limits")]
        [Tooltip("Số lượng đồng xu tối đa đồng thời trên mặt đất")]
        [SerializeField] private int maxGroundCoins = 100;

        private Dictionary<GameObject, IObjectPool<GameObject>> _prefabToPoolMap = new Dictionary<GameObject, IObjectPool<GameObject>>();
        private List<CoinDrop> _activeCoins = new List<CoinDrop>(120);

        public int ActiveCoinCount => _activeCoins.Count;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void RegisterActiveCoin(CoinDrop coin)
        {
            if (coin != null && !_activeCoins.Contains(coin))
            {
                _activeCoins.Add(coin);
            }
        }

        public void UnregisterActiveCoin(CoinDrop coin)
        {
            if (coin != null)
            {
                _activeCoins.Remove(coin);
            }
        }

        public IObjectPool<GameObject> GetOrCreatePool(GameObject prefab)
        {
            if (prefab == null) return null;

            if (_prefabToPoolMap.TryGetValue(prefab, out var existingPool))
            {
                return existingPool;
            }

            IObjectPool<GameObject> pool = null;
            pool = new ObjectPool<GameObject>(
                createFunc: () => {
                    GameObject obj = Instantiate(prefab);
                    var config = obj.GetComponent<CoinPoolConfig>();
                    if (config == null) config = obj.AddComponent<CoinPoolConfig>();
                    config.Pool = pool;
                    return obj;
                },
                actionOnGet: (obj) => obj.SetActive(true),
                actionOnRelease: (obj) => obj.SetActive(false),
                actionOnDestroy: (obj) => Destroy(obj),
                collectionCheck: false,
                defaultCapacity: 50,
                maxSize: 500
            );

            _prefabToPoolMap[prefab] = pool;
            return pool;
        }

        /// <summary>
        /// Spawn đồng Coin tại vị trí chỉ định.
        /// </summary>
        public GameObject SpawnCoin(GameObject prefab, Vector3 position, int amount)
        {
            GameObject targetPrefab = prefab != null ? prefab : defaultCoinPrefab;
            if (targetPrefab == null) return null;

            if (_activeCoins.Count >= maxGroundCoins)
            {
                CompressDistantCoins();
            }

            var pool = GetOrCreatePool(targetPrefab);
            if (pool != null)
            {
                GameObject coinObj = pool.Get();
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
        private void CompressDistantCoins()
        {
            if (_activeCoins.Count < 2) return;

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
