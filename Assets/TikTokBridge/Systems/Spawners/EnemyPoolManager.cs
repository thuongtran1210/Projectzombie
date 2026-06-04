using UnityEngine;
using System.Collections.Generic;

namespace TikTokBridge.Systems.Spawners
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
                // Optional: DontDestroyOnLoad(gameObject); if you want it across scenes
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
                    // Add config if it doesn't exist
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
                // Ngăn chặn khởi tạo mới (Instantiate) khi Pool trống trong lúc chơi để tránh giật lag (spike).
                if (pool.CountInactive == 0)
                {
                    Debug.LogWarning($"[EnemyPoolManager] Pool cho {prefab.name} đã trống! Bỏ qua Spawn để tránh giật lag.");
                    return null;
                }

                GameObject enemy = pool.Get();
                enemy.transform.position = position;
                enemy.transform.rotation = rotation;
                return enemy;
            }

            return null;
        }
    }
}
