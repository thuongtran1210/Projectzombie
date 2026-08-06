using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace ProjectZombie.Features.Collectibles
{
    /// <summary>
    /// Manager singleton quản lý Object Pooling cho Exp Gems (Kinh nghiệm rớt từ Enemy).
    /// Triệt tiêu hoàn toàn GC Spikes và Instantiate/Destroy khi diệt nhiều quái.
    /// </summary>
    public class ExpGemPoolManager : MonoBehaviour
    {
        public static ExpGemPoolManager Instance { get; private set; }

        private Dictionary<GameObject, IObjectPool<GameObject>> _prefabToPoolMap = new Dictionary<GameObject, IObjectPool<GameObject>>();

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
                actionOnGet: (obj) => obj.SetActive(true),
                actionOnRelease: (obj) => obj.SetActive(false),
                actionOnDestroy: (obj) => Destroy(obj),
                collectionCheck: false,
                defaultCapacity: 100,
                maxSize: 1000
            );

            _prefabToPoolMap[prefab] = pool;
            return pool;
        }

        /// <summary>
        /// Spawn viên ExpGem từ Pool tại vị trí chỉ định.
        /// </summary>
        public GameObject SpawnGem(GameObject prefab, Vector3 position, float expAmount)
        {
            if (prefab == null) return null;

            var pool = GetOrCreatePool(prefab);
            if (pool != null)
            {
                GameObject gemObj = pool.Get();
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
    }
}
