using UnityEngine;
using UnityEngine.Pool;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Component gắn vào Prefab đạn để Pool tự động quản lý.
    /// Nó sẽ giữ reference tới IObjectPool để Release đạn thay vì Destroy.
    /// </summary>
    public class ProjectilePoolConfig : MonoBehaviour
    {
        public IObjectPool<GameObject> Pool { get; set; }

        public void ReturnToPool()
        {
            if (Pool != null)
            {
                Pool.Release(gameObject);
            }
            else
            {
                // Fallback nếu chạy test không có Pool
                Destroy(gameObject);
            }
        }
    }
}
