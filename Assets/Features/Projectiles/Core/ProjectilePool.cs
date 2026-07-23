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

        public GameObject Get()
        {
            if (_pool.Count > 0)
            {
                var obj = _pool.Dequeue();
                obj.SetActive(true);
                _activeCount++;
                return obj;
            }

            // Nếu pool trống nhưng chưa đạt giới hạn MaxPoolSize, cấp phát linh hoạt (Dynamic Expansion)
            if (_activeCount < _maxPoolSize)
            {
                var newObj = Instantiate(_prefab, transform);
                newObj.SetActive(true);
                _activeCount++;
                return newObj;
            }

            // Đạt giới hạn MaxPoolSize
            Debug.LogWarning($"[ProjectilePool] Pool cho {_prefab.name} đã đạt giới hạn tối đa ({_maxPoolSize})!");
            return null;
        }

        public void Return(GameObject obj)
        {
            if (obj == null) return;
            
            obj.SetActive(false);
            _pool.Enqueue(obj);
            _activeCount--;
            if (_activeCount < 0) _activeCount = 0;
        }
    }
}
