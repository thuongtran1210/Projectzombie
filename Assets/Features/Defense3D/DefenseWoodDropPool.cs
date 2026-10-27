using System;
using UnityEngine;
using UnityEngine.Pool;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Object Pool cho vật phẩm rơi Wood Drops trong chế độ Defense 3D.
    /// Giúp loại bỏ hoàn toàn việc Instantiate và Destroy liên tục trong Gameplay Loop.
    /// </summary>
    public sealed class DefenseWoodDropPool : IDisposable
    {
        private static DefenseWoodDropPool _activeSessionPool;
        public static DefenseWoodDropPool ActiveSessionPool => _activeSessionPool;

        private readonly ObjectPool<DefenseWoodDrop3D> _pool;
        private readonly GameObject _prefab;
        private readonly Transform _poolRoot;

        public DefenseWoodDropPool(GameObject prefab, Transform poolRoot = null, int defaultCapacity = 20, int maxSize = 80)
        {
            _prefab = prefab != null ? prefab : throw new ArgumentNullException(nameof(prefab));
            _poolRoot = poolRoot;
            _pool = new ObjectPool<DefenseWoodDrop3D>(
                createFunc: CreateInstance,
                actionOnGet: OnGetFromPool,
                actionOnRelease: OnReturnToPool,
                actionOnDestroy: OnDestroyPoolItem,
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
        }

        public static void RegisterSessionPool(DefenseWoodDropPool pool)
        {
            _activeSessionPool?.Dispose();
            _activeSessionPool = pool;
        }

        public static void UnregisterSessionPool(DefenseWoodDropPool pool)
        {
            if (ReferenceEquals(_activeSessionPool, pool))
            {
                _activeSessionPool = null;
            }
        }

        private DefenseWoodDrop3D CreateInstance()
        {
#if UNITY_EDITOR
            var instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(_prefab, _poolRoot);
#else
            var instance = UnityEngine.Object.Instantiate(_prefab, _poolRoot);
#endif
            instance.SetActive(false);
            var drop = instance.GetComponent<DefenseWoodDrop3D>();
            drop.SetPoolRelease(item => _pool.Release(item));
            return drop;
        }

        private void OnGetFromPool(DefenseWoodDrop3D drop)
        {
            if (drop != null)
            {
                drop.gameObject.SetActive(true);
            }
        }

        private void OnReturnToPool(DefenseWoodDrop3D drop)
        {
            if (drop != null)
            {
                drop.gameObject.SetActive(false);
                if (_poolRoot != null)
                {
                    drop.transform.SetParent(_poolRoot, false);
                }
            }
        }

        private void OnDestroyPoolItem(DefenseWoodDrop3D drop)
        {
            if (drop != null && drop.gameObject != null)
            {
                UnityEngine.Object.Destroy(drop.gameObject);
            }
        }

        public DefenseWoodDrop3D Get() => _pool.Get();

        public void Release(DefenseWoodDrop3D drop)
        {
            if (drop != null)
            {
                _pool.Release(drop);
            }
        }

        public void Dispose()
        {
            _pool.Clear();
            if (ReferenceEquals(_activeSessionPool, this))
            {
                _activeSessionPool = null;
            }
        }
    }
}
