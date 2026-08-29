using UnityEngine;
using System.Collections.Generic;

namespace ProjectZombie.Features.Projectiles.Core
{
    public class ProjectilePool : MonoBehaviour
    {
        private GameObject _prefab;
        private Queue<GameObject> _pool = new Queue<GameObject>();
        private int _maxPoolSize;
        private int _activeCount;

        public void Initialize(GameObject prefab, int prewarmCount, int maxPoolSize)
        {
            _prefab = prefab;
            _maxPoolSize = maxPoolSize;
            _activeCount = 0;

            Prewarm(prewarmCount);
        }

        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (_pool.Count + _activeCount >= _maxPoolSize) break;
                
                var obj = Instantiate(_prefab, transform);
                obj.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        private readonly HashSet<GameObject> _activeObjects = new HashSet<GameObject>();

        public void ReturnAllActive()
        {
            var activeList = new List<GameObject>(_activeObjects);
            foreach (var obj in activeList)
            {
                if (obj != null)
                {
                    Return(obj);
                }
            }
            _activeObjects.Clear();
            _activeCount = 0;
        }

        public GameObject Get()
        {
            GameObject obj = null;
            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
                obj.SetActive(true);
                _activeCount++;
            }
            else if (_activeCount < _maxPoolSize)
            {
                obj = Instantiate(_prefab, transform);
                obj.SetActive(true);
                _activeCount++;
            }
            else
            {
                Debug.LogWarning($"[ProjectilePool] Pool cho {_prefab.name} đã đạt giới hạn tối đa ({_maxPoolSize})!");
                return null;
            }

            if (obj != null)
            {
                _activeObjects.Add(obj);
            }
            return obj;
        }

        public void Return(GameObject obj)
        {
            if (obj == null) return;
            
            _activeObjects.Remove(obj);
            obj.SetActive(false);
            _pool.Enqueue(obj);
            _activeCount--;
            if (_activeCount < 0) _activeCount = 0;
        }
    }
}
