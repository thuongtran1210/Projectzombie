using UnityEngine;
using UnityEngine.Pool;

namespace ProjectZombie.Features.Collectibles
{
    /// <summary>
    /// Component quản lý Object Pooling cho ExpGem (0 GC allocation).
    /// </summary>
    public class ExpGemPoolConfig : MonoBehaviour
    {
        public IObjectPool<GameObject> Pool { get; set; }

        public void ReturnToPool()
        {
            if (Pool != null && gameObject.activeSelf)
            {
                Pool.Release(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
