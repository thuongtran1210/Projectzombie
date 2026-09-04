using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using ProjectZombie.Core.Pooling;

namespace ProjectZombie.Features.Projectiles.Core
{
    /// <summary>
    /// Pool quản lý đạn (Projectiles) dựa trên UnityEngine.Pool.IObjectPool.
    /// Hỗ trợ Prewarm, ReturnAllActive và cơ chế FIFO Recycling khi đạt trần MaxPoolSize.
    /// </summary>
    public class ProjectilePool : MonoBehaviour
    {
        private GameObject _prefab;
        private IObjectPool<GameObject> _pool;
        private int _maxPoolSize = 200;

        // Quản lý active projectiles theo thứ tự FIFO (First In First Out)
        private readonly LinkedList<GameObject> _activeList = new();
        private readonly HashSet<GameObject> _activeSet = new();

        public int ActiveCount => _activeList.Count;

        public void Initialize(GameObject prefab, int prewarmCount, int maxPoolSize)
        {
            _prefab = prefab;
            _maxPoolSize = maxPoolSize > 0 ? maxPoolSize : 200;

            _pool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(_prefab, transform),
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
                defaultCapacity: Mathf.Min(prewarmCount, _maxPoolSize),
                maxSize: _maxPoolSize
            );

            Prewarm(prewarmCount);
        }

        public void Prewarm(int count)
        {
            if (_pool == null || _prefab == null) return;

            int targetCount = Mathf.Min(count, _maxPoolSize);
            var temp = new List<GameObject>(targetCount);

            for (int i = 0; i < targetCount; i++)
            {
                var obj = _pool.Get();
                if (obj != null) temp.Add(obj);
            }

            for (int i = 0; i < temp.Count; i++)
            {
                if (temp[i] != null) _pool.Release(temp[i]);
            }
        }

        public void ReturnAllActive()
        {
            var nodes = new List<GameObject>(_activeList);
            foreach (var obj in nodes)
            {
                if (obj != null)
                {
                    Return(obj);
                }
            }
            _activeList.Clear();
            _activeSet.Clear();
        }

        public GameObject Get()
        {
            if (_pool == null) return null;

            // FIFO Recycling: Nếu số lượng active đã chạm trần maxPoolSize, thu hồi viên đạn lâu đời nhất (First)
            if (_activeList.Count >= _maxPoolSize && _activeList.First != null)
            {
                GameObject oldestObj = _activeList.First.Value;
                if (oldestObj != null)
                {
                    Return(oldestObj);
                }
            }

            GameObject obj = _pool.Get();
            if (obj != null)
            {
                _activeList.AddLast(obj);
                _activeSet.Add(obj);
            }
            return obj;
        }

        public void Return(GameObject obj)
        {
            if (obj == null || _pool == null) return;

            if (_activeSet.Remove(obj))
            {
                _activeList.Remove(obj);
            }

            _pool.Release(obj);
        }
    }
}
