using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace ProjectZombie.Features.Collectibles
{
    /// <summary>
    /// Manager singleton quản lý Object Pooling cho Exp Gems (Kinh nghiệm rớt từ Enemy).
    /// Hỗ trợ cơ chế Nén & Gộp hạt (Gem Compression) khi số lượng trên sân vượt quá ngưỡng cho phép,
    /// bảo đảm duy trì 60 FPS ổn định trên di động.
    /// </summary>
    public class ExpGemPoolManager : MonoBehaviour
    {
        public static ExpGemPoolManager Instance { get; private set; }

        [Header("Performance & Limits")]
        [Tooltip("Số lượng hạt kinh nghiệm tối đa đồng thời trên mặt đất")]
        [SerializeField] private int maxGroundGems = 150;

        private Dictionary<GameObject, IObjectPool<GameObject>> _prefabToPoolMap = new Dictionary<GameObject, IObjectPool<GameObject>>();
        private List<ExpGem> _activeGems = new List<ExpGem>(200);

        public int ActiveGemCount => _activeGems.Count;

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

        public void RegisterActiveGem(ExpGem gem)
        {
            if (gem != null && !_activeGems.Contains(gem))
            {
                _activeGems.Add(gem);
            }
        }

        public void UnregisterActiveGem(ExpGem gem)
        {
            if (gem != null)
            {
                _activeGems.Remove(gem);
            }
        }

        public void ClearPools()
        {
            foreach (var pool in _prefabToPoolMap.Values)
            {
                pool.Clear();
            }
            _prefabToPoolMap.Clear();
            _activeGems.Clear();
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
                    var config = obj.GetComponent<ExpGemPoolConfig>();
                    if (config == null) config = obj.AddComponent<ExpGemPoolConfig>();
                    config.Pool = pool;
                    return obj;
                },
                actionOnGet: (obj) => {
                    if (obj != null) obj.SetActive(true);
                },
                actionOnRelease: (obj) => {
                    if (obj != null) obj.SetActive(false);
                },
                actionOnDestroy: (obj) => {
                    if (obj != null) Destroy(obj);
                },
                collectionCheck: false,
                defaultCapacity: 100,
                maxSize: 1000
            );

            _prefabToPoolMap[prefab] = pool;
            return pool;
        }

        /// <summary>
        /// Spawn viên ExpGem từ Pool tại vị trí chỉ định.
        /// Tự động kích hoạt nén gộp hạt nếu số lượng vượt quá ngưỡng cho phép.
        /// </summary>
        public GameObject SpawnGem(GameObject prefab, Vector3 position, float expAmount)
        {
            if (prefab == null) return null;

            // Kiểm tra và nén hạt ở xa nếu vượt ngưỡng tối đa
            if (_activeGems.Count >= maxGroundGems)
            {
                CompressDistantGems();
            }

            var pool = GetOrCreatePool(prefab);
            if (pool != null)
            {
                GameObject gemObj = null;
                while (pool.CountInactive > 0)
                {
                    gemObj = pool.Get();
                    if (gemObj != null) break;
                }

                if (gemObj == null)
                {
                    gemObj = Instantiate(prefab);
                    var config = gemObj.GetComponent<ExpGemPoolConfig>() ?? gemObj.AddComponent<ExpGemPoolConfig>();
                    config.Pool = pool;
                }

                gemObj.transform.position = position;
                gemObj.transform.rotation = Quaternion.identity;

                var expGem = gemObj.GetComponent<ExpGem>();
                if (expGem != null)
                {
                    expGem.SetExpAmount(expAmount);
                }

                return gemObj;
            }

            // Fallback nếu không tạo được pool
            GameObject fallbackObj = Instantiate(prefab, position, Quaternion.identity);
            var fallbackGem = fallbackObj.GetComponent<ExpGem>();
            if (fallbackGem != null) fallbackGem.SetExpAmount(expAmount);
            return fallbackObj;
        }

        /// <summary>
        /// Thuật toán nén gộp 2 hạt Exp ở xa người chơi nhất thành 1 hạt cấp cao hơn.
        /// </summary>
        private void CompressDistantGems()
        {
            if (_activeGems.Count < 2) return;

            Vector3 playerPos = Vector3.zero;
            if (Player.PlayerController.Instance != null)
            {
                playerPos = Player.PlayerController.Instance.transform.position;
            }

            ExpGem furthest1 = null;
            ExpGem furthest2 = null;
            float maxDistSq1 = -1f;
            float maxDistSq2 = -1f;

            for (int i = 0; i < _activeGems.Count; i++)
            {
                var gem = _activeGems[i];
                if (gem == null || !gem.IsIdle) continue;

                float distSq = (gem.transform.position - playerPos).sqrMagnitude;

                if (distSq > maxDistSq1)
                {
                    maxDistSq2 = maxDistSq1;
                    furthest2 = furthest1;

                    maxDistSq1 = distSq;
                    furthest1 = gem;
                }
                else if (distSq > maxDistSq2)
                {
                    maxDistSq2 = distSq;
                    furthest2 = gem;
                }
            }

            // Gộp furthest2 vào furthest1
            if (furthest1 != null && furthest2 != null && furthest1 != furthest2)
            {
                furthest1.MergeExp(furthest2.ExpAmount);

                // Thu hồi furthest2 về Pool
                if (furthest2.TryGetComponent<ExpGemPoolConfig>(out var poolConfig))
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
        /// Hút toàn bộ hạt kinh nghiệm trên mặt đất về phía người chơi (dành cho vật phẩm Toàn Bản Đồ Magnet).
        /// </summary>
        public void CollectAllActiveGems(Transform player)
        {
            if (player == null) return;

            for (int i = _activeGems.Count - 1; i >= 0; i--)
            {
                var gem = _activeGems[i];
                if (gem != null && gem.IsActiveOnGround)
                {
                    gem.StartMagnetEffect(player);
                }
            }
        }
    }
}
