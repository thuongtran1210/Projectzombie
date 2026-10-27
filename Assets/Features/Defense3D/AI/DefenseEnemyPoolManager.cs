using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Quản lý Object Pool cho toàn bộ các Prefab quái vật trong chế độ Defense 3D.
    /// Triệt tiêu 100% GC Allocations sinh ra do Instantiate và Destroy liên tục mỗi wave.
    /// </summary>
    public sealed class DefenseEnemyPoolManager : IDisposable
    {
        private static DefenseEnemyPoolManager _activeSessionPool;
        public static DefenseEnemyPoolManager ActiveSessionPool => _activeSessionPool;

        private readonly Transform _poolsRoot;
        private readonly Dictionary<GameObject, ObjectPool<GameObject>> _poolsByPrefab = new Dictionary<GameObject, ObjectPool<GameObject>>();
        private readonly Dictionary<GameObject, GameObject> _instanceToPrefabMap = new Dictionary<GameObject, GameObject>();

        public DefenseEnemyPoolManager(Transform poolsRoot = null)
        {
            _poolsRoot = poolsRoot;
        }

        public static void RegisterSessionPool(DefenseEnemyPoolManager poolManager)
        {
            _activeSessionPool?.Dispose();
            _activeSessionPool = poolManager;
        }

        public static void UnregisterSessionPool(DefenseEnemyPoolManager poolManager)
        {
            if (ReferenceEquals(_activeSessionPool, poolManager))
            {
                _activeSessionPool = null;
            }
        }

        public ObjectPool<GameObject> GetOrCreatePool(GameObject prefab, int defaultCapacity = 10, int maxSize = 50)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));
            if (!_poolsByPrefab.TryGetValue(prefab, out var pool))
            {
                pool = new ObjectPool<GameObject>(
                    createFunc: () => CreateEnemyInstance(prefab),
                    actionOnGet: OnGetEnemy,
                    actionOnRelease: OnReleaseEnemy,
                    actionOnDestroy: OnDestroyEnemy,
                    collectionCheck: false,
                    defaultCapacity: defaultCapacity,
                    maxSize: maxSize
                );
                _poolsByPrefab.Add(prefab, pool);
            }
            return pool;
        }

        private GameObject CreateEnemyInstance(GameObject prefab)
        {
#if UNITY_EDITOR
            var instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, _poolsRoot);
#else
            var instance = UnityEngine.Object.Instantiate(prefab, _poolsRoot);
#endif
            instance.SetActive(false);
            _instanceToPrefabMap[instance] = prefab;
            return instance;
        }

        private void OnGetEnemy(GameObject instance)
        {
            if (instance != null)
            {
                instance.SetActive(false); // Sẽ được active sau khi gán tọa độ và bind
            }
        }

        private void OnReleaseEnemy(GameObject instance)
        {
            if (instance != null)
            {
                instance.SetActive(false);
                // Release can run inside OnDisable while an ancestor is changing activation.
                // Unity forbids reparenting there; leave the inactive instance in place until Get.
            }
        }

        private void OnDestroyEnemy(GameObject instance)
        {
            if (instance != null)
            {
                _instanceToPrefabMap.Remove(instance);
                UnityEngine.Object.Destroy(instance);
            }
        }

        public GameObject Get(GameObject prefab, Transform parent = null)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));
            var pool = GetOrCreatePool(prefab);
            var instance = pool.Get();
            Transform destination = parent != null ? parent : _poolsRoot;
            if (destination != null && instance.transform.parent != destination)
            {
                instance.transform.SetParent(destination, false);
            }
            return instance;
        }

        public bool TryRelease(GameObject instance)
        {
            if (instance == null) return false;
            if (_instanceToPrefabMap.TryGetValue(instance, out var prefab) &&
                _poolsByPrefab.TryGetValue(prefab, out var pool))
            {
                pool.Release(instance);
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            foreach (var pool in _poolsByPrefab.Values)
            {
                pool.Clear();
            }
            _poolsByPrefab.Clear();
            _instanceToPrefabMap.Clear();

            if (ReferenceEquals(_activeSessionPool, this))
            {
                _activeSessionPool = null;
            }
        }
    }
}
