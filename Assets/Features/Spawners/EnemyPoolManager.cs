using UnityEngine;
using System.Collections.Generic;
using ProjectZombie.Features.Enemies;

namespace ProjectZombie.Features.Spawners
{
    public class EnemyPoolManager : MonoBehaviour
    {
        public static EnemyPoolManager Instance { get; private set; }

        private Dictionary<GameObject, UnityEngine.Pool.ObjectPool<GameObject>> _prefabToPoolMap = new Dictionary<GameObject, UnityEngine.Pool.ObjectPool<GameObject>>();
        private Dictionary<string, UnityEngine.Pool.ObjectPool<GameObject>> _keyToPoolMap = new Dictionary<string, UnityEngine.Pool.ObjectPool<GameObject>>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                _prefabToPoolMap.Clear();
                _keyToPoolMap.Clear();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
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
            pool = new UnityEngine.Pool.ObjectPool<GameObject>(
                createFunc: () => {
                    if (prefab == null) return null;
                    GameObject obj = Instantiate(prefab);
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
            
            var tempObjects = new List<GameObject>(count);
            for (int i = 0; i < count; i++)
            {
                GameObject obj = null;
                try { obj = pool.Get(); } catch { }
                if (obj != null) tempObjects.Add(obj);
            }
            foreach (var obj in tempObjects)
            {
                if (obj != null) pool.Release(obj);
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
