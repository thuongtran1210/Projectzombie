using UnityEngine;
using UnityEngine.Pool;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Spawners
{
    /// <summary>
    /// Component gắn vào các Enemy Prefab để tự động trả về ObjectPool khi quái vật chết (Health <= 0).
    /// Giúp loại bỏ hoàn toàn GC Allocation và lỗi giật lag (spike) khi Instantiate/Destroy liên tục.
    /// </summary>
    [RequireComponent(typeof(HealthSystem))]
    public class EnemyPoolConfig : MonoBehaviour
    {
        public IObjectPool<GameObject> Pool { get; set; }

        private HealthSystem _health;

        private void Awake()
        {
            _health = GetComponent<HealthSystem>();
            if (_health != null)
            {
                _health.OnDied += ReturnToPool;
            }
        }

        public void ReturnToPool()
        {
            if (Pool != null && gameObject.activeSelf)
            {
                Pool.Release(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.OnDied -= ReturnToPool;
            }
        }
    }
}
