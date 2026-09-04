using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using ProjectZombie.Core.Pooling;

namespace ProjectZombie.Features.Collectibles
{
    /// <summary>
    /// Lớp cơ sở Generic quản lý Object Pooling cho tất cả các loại vật phẩm thu thập (Collectibles)
    /// như ExpGem, CoinDrop, MagnetItem...
    /// Tích hợp sẵn thuật toán nén gộp (Compression), IObjectPool<GameObject> và quản lý active items.
    /// </summary>
    /// <typeparam name="TManager">Kiểu kế thừa Singleton Manager con</typeparam>
    /// <typeparam name="TItem">Kiểu Component Item kế thừa MonoBehaviour, ICollectible, IPoolable</typeparam>
    public abstract class CollectiblePoolBase<TManager, TItem> : MonoBehaviour 
        where TManager : CollectiblePoolBase<TManager, TItem>
        where TItem : MonoBehaviour, ICollectible, IPoolable
    {
        private static TManager _instance;
        public static TManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<TManager>();
                    if (_instance == null)
                    {
                        Transform parent = PoolHierarchyManager.Instance != null 
                            ? PoolHierarchyManager.Instance.GetCategoryRoot(PoolHierarchyManager.PoolCategory.Collectibles) 
                            : null;

                        GameObject go = new GameObject($"[{typeof(TManager).Name}]");
                        if (parent != null)
                        {
                            go.transform.SetParent(parent);
                        }
                        _instance = go.AddComponent<TManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Performance & Limits")]
        [Tooltip("Số lượng vật phẩm tối đa đồng thời trên mặt đất trước khi kích hoạt nén gộp")]
        [SerializeField] protected int maxGroundItems = 120;

        protected readonly Dictionary<GameObject, IObjectPool<GameObject>> PrefabToPoolMap = new();
        protected readonly List<TItem> ActiveItems = new(150);

        public int ActiveItemCount => ActiveItems.Count;

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = (TManager)this;
                if (transform.parent == null && PoolHierarchyManager.Instance != null)
                {
                    transform.SetParent(PoolHierarchyManager.Instance.GetCategoryRoot(PoolHierarchyManager.PoolCategory.Collectibles));
                }
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                ClearPools();
            }
        }

        public void RegisterActiveItem(TItem item)
        {
            if (item != null && !ActiveItems.Contains(item))
            {
                ActiveItems.Add(item);
            }
        }

        public void UnregisterActiveItem(TItem item)
        {
            if (item != null)
            {
                ActiveItems.Remove(item);
            }
        }

        public virtual void ClearPools()
        {
            foreach (var pool in PrefabToPoolMap.Values)
            {
                pool.Clear();
            }
            PrefabToPoolMap.Clear();
            ActiveItems.Clear();
        }

        public virtual IObjectPool<GameObject> GetOrCreatePool(GameObject prefab)
        {
            if (prefab == null) return null;

            if (PrefabToPoolMap.TryGetValue(prefab, out var existingPool))
            {
                return existingPool;
            }

            IObjectPool<GameObject> pool = null;
            pool = new ObjectPool<GameObject>(
                createFunc: () => {
                    GameObject obj = Instantiate(prefab, transform);
                    AttachPoolConfig(obj, pool);
                    return obj;
                },
                actionOnGet: (obj) => {
                    if (obj != null)
                    {
                        obj.SetActive(true);
                        if (obj.TryGetComponent<IPoolable>(out var poolable))
                        {
                            poolable.OnSpawn();
                        }
                    }
                },
                actionOnRelease: (obj) => {
                    if (obj != null)
                    {
                        if (obj.TryGetComponent<IPoolable>(out var poolable))
                        {
                            poolable.OnDespawn();
                        }
                        obj.SetActive(false);
                    }
                },
                actionOnDestroy: (obj) => {
                    if (obj != null) Destroy(obj);
                },
                collectionCheck: false,
                defaultCapacity: 50,
                maxSize: 500
            );

            PrefabToPoolMap[prefab] = pool;
            return pool;
        }

        /// <summary>
        /// Gán Config callback để return về đúng pool instance
        /// </summary>
        protected abstract void AttachPoolConfig(GameObject obj, IObjectPool<GameObject> pool);

        /// <summary>
        /// Thuật toán nén 2 vật phẩm ở xa người chơi nhất khi số lượng trên sàn vượt maxGroundItems.
        /// </summary>
        protected abstract void CompressDistantItems();
    }
}
