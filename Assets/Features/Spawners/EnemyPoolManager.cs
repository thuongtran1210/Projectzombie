using UnityEngine;
using System.Collections.Generic;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Core.Pooling;

namespace ProjectZombie.Features.Spawners
{
    public class EnemyPoolManager : MonoBehaviour
    {
        private static EnemyPoolManager _instance;
        public static EnemyPoolManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<EnemyPoolManager>();
                    if (_instance == null)
                    {
                        Transform parent = PoolHierarchyManager.Instance != null 
                            ? PoolHierarchyManager.Instance.GetCategoryRoot(PoolHierarchyManager.PoolCategory.Enemies) 
                            : null;

                        GameObject go = new GameObject("[EnemyPoolManager]");
                        if (parent != null)
                        {
                            go.transform.SetParent(parent);
                        }
                        _instance = go.AddComponent<EnemyPoolManager>();
                    }
                }
                return _instance;
            }
        }

        private Dictionary<GameObject, UnityEngine.Pool.ObjectPool<GameObject>> _prefabToPoolMap = new Dictionary<GameObject, UnityEngine.Pool.ObjectPool<GameObject>>();
        private Dictionary<string, UnityEngine.Pool.ObjectPool<GameObject>> _keyToPoolMap = new Dictionary<string, UnityEngine.Pool.ObjectPool<GameObject>>();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                if (transform.parent == null && PoolHierarchyManager.Instance != null)
                {
                    transform.SetParent(PoolHierarchyManager.Instance.GetCategoryRoot(PoolHierarchyManager.PoolCategory.Enemies));
                }
                _prefabToPoolMap.Clear();
                _keyToPoolMap.Clear();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                _prefabToPoolMap.Clear();
                _keyToPoolMap.Clear();
            }
        }

        public UnityEngine.Pool.ObjectPool<GameObject> GetOrCreatePool(GameObject prefab, string addressKey = null)
        {
            if (prefab == null) return null;

            string key = !string.IsNullOrEmpty(addressKey) ? addressKey : prefab.name;
            if (_keyToPoolMap.TryGetValue(key, out var existingPoolByKey) && existingPoolByKey != null)
            {
                return existingPoolByKey;
            }

            if (_prefabToPoolMap.TryGetValue(prefab, out var existingPool) && existingPool != null)
            {
                _keyToPoolMap[key] = existingPool;
                return existingPool;
            }

            UnityEngine.Pool.ObjectPool<GameObject> pool = null;
            Transform parentTransform = PoolHierarchyManager.Instance != null 
                ? PoolHierarchyManager.Instance.GetCategoryRoot(PoolHierarchyManager.PoolCategory.Enemies) 
                : transform;

            pool = new UnityEngine.Pool.ObjectPool<GameObject>(
                createFunc: () => {
                    if (prefab == null) return null;
                    GameObject obj = Instantiate(prefab, parentTransform);
                    var config = obj.GetComponent<EnemyPoolConfig>();
                    if (config == null) config = obj.AddComponent<EnemyPoolConfig>();
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
                defaultCapacity: 50,
                maxSize: 500
            );

            _prefabToPoolMap[prefab] = pool;
            _keyToPoolMap[key] = pool;
            return pool;
        }

        public void PrewarmPool(GameObject prefab, int count, string addressKey = null)
        {
            if (prefab == null) return;
            var pool = GetOrCreatePool(prefab, addressKey);
            if (pool == null) return;
            
            // Giảm số lượng prewarm đồng bộ tối thiểu (ví dụ 6 con) để không làm đơ Game Loop
            int immediateCount = Mathf.Min(count, 6);
            var tempObjects = new List<GameObject>(immediateCount);
            for (int i = 0; i < immediateCount; i++)
            {
                GameObject obj = null;
                try { obj = pool.Get(); } catch { }
                if (obj != null) tempObjects.Add(obj);
            }
            foreach (var obj in tempObjects)
            {
                if (obj != null) pool.Release(obj);
            }

            // Số lượng còn lại phân bổ qua Coroutine để giữ vững 60 FPS
            int remaining = count - immediateCount;
            if (remaining > 0 && gameObject.activeInHierarchy)
            {
                StartCoroutine(RoutinePrewarmSlice(pool, remaining));
            }
        }

        private System.Collections.IEnumerator RoutinePrewarmSlice(UnityEngine.Pool.ObjectPool<GameObject> pool, int remainingCount, int itemsPerFrame = 3)
        {
            var tempObjects = new List<GameObject>(itemsPerFrame);
            while (remainingCount > 0)
            {
                yield return null; // Chờ sang frame tiếp theo
                int batch = Mathf.Min(remainingCount, itemsPerFrame);
                tempObjects.Clear();
                for (int i = 0; i < batch; i++)
                {
                    GameObject obj = null;
                    try { obj = pool.Get(); } catch { }
                    if (obj != null) tempObjects.Add(obj);
                }
                foreach (var obj in tempObjects)
                {
                    if (obj != null) pool.Release(obj);
                }
                remainingCount -= batch;
            }
        }

        public GameObject SpawnEnemy(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;

            var pool = GetOrCreatePool(prefab);
            if (pool != null)
            {
                GameObject enemy = null;
                int retryCount = 0;
                while (enemy == null && retryCount < 3)
                {
                    retryCount++;
                    try
                    {
                        enemy = pool.Get();
                    }
                    catch (System.Exception)
                    {
                        enemy = null;
                    }

                    if (enemy == null)
                    {
                        enemy = Instantiate(prefab);
                        var config = enemy.GetComponent<EnemyPoolConfig>();
                        if (config == null) config = enemy.AddComponent<EnemyPoolConfig>();
                        config.Pool = pool;
                        break;
                    }
                }

                if (enemy != null)
                {
                    enemy.transform.position = position;
                    enemy.transform.rotation = rotation;
                    if (!enemy.activeSelf) enemy.SetActive(true);

                    // Reset health nếu enemy tái sử dụng
                    var health = enemy.GetComponent<ProjectZombie.Features.Shared.HealthSystem>();
                    var enemyComp = enemy.GetComponent<Enemy>();
                    if (health != null && enemyComp != null && enemyComp.Config != null)
                    {
                        health.SetMaxHealth(enemyComp.Config.maxHealth);
                    }
                }
                return enemy;
            }

            return null;
        }

        public GameObject SpawnEnemy(string key, Vector3 position, Quaternion rotation)
        {
            if (string.IsNullOrEmpty(key)) return null;

            if (_keyToPoolMap.TryGetValue(key, out var pool) && pool != null)
            {
                GameObject enemy = null;
                try
                {
                    enemy = pool.Get();
                }
                catch (System.Exception)
                {
                    enemy = null;
                }

                if (enemy != null)
                {
                    enemy.transform.position = position;
                    enemy.transform.rotation = rotation;
                    if (!enemy.activeSelf) enemy.SetActive(true);
                }
                return enemy;
            }

            return null;
        }

    }
}
