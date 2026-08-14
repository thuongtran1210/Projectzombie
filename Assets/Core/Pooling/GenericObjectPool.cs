using System;
using UnityEngine;
using UnityEngine.Pool;

namespace ProjectZombie.Core.Pooling
{
    /// <summary>
    /// Base Generic Object Pool bọc quanh UnityEngine.Pool.ObjectPool.
    /// Tự động gọi IPoolable callbacks để đảm bảo an toàn bộ nhớ và 0 GC Allocation.
    /// </summary>
    public class GenericObjectPool<T> where T : Component
    {
        private readonly ObjectPool<T> _pool;
        private readonly T _prefab;
        private readonly Transform _parent;

        public GenericObjectPool(T prefab, Transform parent = null, int defaultCapacity = 20, int maxSize = 200)
        {
            _prefab = prefab;
            _parent = parent;

            _pool = new ObjectPool<T>(
                createFunc: CreateInstance,
                actionOnGet: OnTakeFromPool,
                actionOnRelease: OnReturnedToPool,
                actionOnDestroy: OnDestroyPoolObject,
                collectionCheck: true,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
        }

        private T CreateInstance()
        {
            T instance = UnityEngine.Object.Instantiate(_prefab, _parent);
            instance.gameObject.SetActive(false);
            return instance;
        }

        private void OnTakeFromPool(T instance)
        {
            instance.gameObject.SetActive(true);
            if (instance is IPoolable poolable)
            {
                poolable.OnSpawnFromPool();
            }
        }

        private void OnReturnedToPool(T instance)
        {
            if (instance is IPoolable poolable)
            {
                poolable.OnReturnToPool();
            }
            instance.gameObject.SetActive(false);
        }

        private void OnDestroyPoolObject(T instance)
        {
            if (instance != null)
            {
                UnityEngine.Object.Destroy(instance.gameObject);
            }
        }

        /// <summary>
        /// Lấy một phần tử ra từ Pool.
        /// </summary>
        public T Get() => _pool.Get();

        /// <summary>
        /// Trả phần tử về Pool.
        /// </summary>
        public void Release(T instance)
        {
            if (instance != null && instance.gameObject.activeSelf)
            {
                _pool.Release(instance);
            }
        }

        /// <summary>
        /// Xóa toàn bộ pool.
        /// </summary>
        public void Clear() => _pool.Clear();

        public int CountActive => _pool.CountActive;
        public int CountInactive => _pool.CountInactive;
        public int CountAll => _pool.CountAll;
    }
}
