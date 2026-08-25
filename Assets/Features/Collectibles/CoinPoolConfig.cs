using UnityEngine;
using UnityEngine.Pool;

namespace ProjectZombie.Features.Collectibles
{
    /// <summary>
    /// Component gắn kèm Coin Prefab để hỗ trợ hoàn trả đối tượng về ObjectPool<GameObject>.
    /// </summary>
    public class CoinPoolConfig : MonoBehaviour
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
