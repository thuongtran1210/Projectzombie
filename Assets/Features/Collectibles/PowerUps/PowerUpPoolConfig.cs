using UnityEngine;
using UnityEngine.Pool;

namespace ProjectZombie.Features.Collectibles.PowerUps
{
    /// <summary>
    /// Component đính kèm trên GameObject PowerUpItem để lưu tham chiếu tới ObjectPool tương ứng (0 GC).
    /// </summary>
    public class PowerUpPoolConfig : MonoBehaviour
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
                gameObject.SetActive(false);
            }
        }
    }
}
