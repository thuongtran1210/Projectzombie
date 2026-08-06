using UnityEngine;
using System.Collections.Generic;

namespace ProjectZombie.Features.Spawners
{
    public class EnemyPoolManager : MonoBehaviour
    {
        public static EnemyPoolManager Instance { get; private set; }

        private Dictionary<GameObject, UnityEngine.Pool.ObjectPool<GameObject>> _prefabToPoolMap = new Dictionary<GameObject, UnityEngine.Pool.ObjectPool<GameObject>>();

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

        public UnityEngine.Pool.ObjectPool<GameObject> GetOrCreatePool(GameObject prefab)
        {
            if (prefab == null) return null;

            if (_prefabToPoolMap.TryGetValue(prefab, out var existingPool))
            {
                return existingPool;
            }

            UnityEngine.Pool.ObjectPool<GameObject> pool = null;
            pool = new UnityEngine.Pool.ObjectPool<GameObject>(
                createFunc: () => {
                    GameObject obj = Instantiate(prefab);
                    var config = obj.GetComponent<EnemyPoolConfig>();
                    if (config == null) config = obj.AddComponent<EnemyPoolConfig>();
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

        public void PrewarmPool(GameObject prefab, int count)
        {
            if (prefab == null) return;
            var pool = GetOrCreatePool(prefab);
            
            var tempObjects = new List<GameObject>(count);
            for (int i = 0; i < count; i++)
            {
                tempObjects.Add(pool.Get());
            }
            foreach (var obj in tempObjects)
            {
                pool.Release(obj);
            }
        }

        public GameObject SpawnEnemy(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;

            var pool = GetOrCreatePool(prefab);
            if (pool != null)
            {
                // Sử dụng cơ chế tự phình rộng của UnityEngine.Pool.ObjectPool (Tối đa maxSize = 500)
                GameObject enemy = pool.Get();
                if (enemy != null)
                {
                    enemy.transform.position = position;
                    enemy.transform.rotation = rotation;
                }
                return enemy;
            }

            return null;
        }

    }
}
