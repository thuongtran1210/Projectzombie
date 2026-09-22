using UnityEngine;
using UnityEngine.Pool;

namespace ProjectZombie.Features.Collectibles.PowerUps
{
    /// <summary>
    /// Singleton Pool Manager quản lý việc cấp phát và thu hồi các loại vật phẩm bổ trợ (PowerUpItem).
    /// Kế thừa CollectiblePoolBase để tận dụng cơ chế Pooling chuẩn hóa và quản lý Active Items.
    /// </summary>
    public class PowerUpPoolManager : CollectiblePoolBase<PowerUpPoolManager, PowerUpItem>
    {
        public PowerUpItem SpawnItem(GameObject prefab, Vector3 position, float lifeTime = 30f, IPowerUpEffect overrideEffect = null)
        {
            if (prefab == null) return null;

            // Nếu số lượng vượt ngưỡng tối đa thì thu hồi vật phẩm ở xa nhất
            if (ActiveItems.Count >= maxGroundItems)
            {
                CompressDistantItems();
            }

            var pool = GetOrCreatePool(prefab);
            if (pool == null) return null;

            var go = pool.Get();
            if (go == null) return null;

            go.transform.position = position;
            go.transform.rotation = Quaternion.identity;

            if (go.TryGetComponent<PowerUpItem>(out var item))
            {
                if (overrideEffect != null)
                {
                    item.SetEffectStrategy(overrideEffect);
                }
                item.Initialize(lifeTime);
                RegisterActiveItem(item);
                return item;
            }

            return null;
        }

        public void ReleaseItem(GameObject instance, PowerUpItem item)
        {
            if (instance == null) return;

            UnregisterActiveItem(item);

            if (instance.TryGetComponent<PowerUpPoolConfig>(out var config) && config.Pool != null)
            {
                config.ReturnToPool();
            }
            else
            {
                instance.SetActive(false);
            }
        }

        protected override void AttachPoolConfig(GameObject obj, IObjectPool<GameObject> pool)
        {
            if (!obj.TryGetComponent<PowerUpPoolConfig>(out var config))
            {
                config = obj.AddComponent<PowerUpPoolConfig>();
            }
            config.Pool = pool;
        }

        protected override void CompressDistantItems()
        {
            if (ActiveItems.Count == 0) return;

            // Tìm vật phẩm ở xa người chơi nhất để thu hồi về pool (giải phóng slot)
            float maxDistSq = -1f;
            int farthestIndex = -1;

            for (int i = 0; i < ActiveItems.Count; i++)
            {
                var it = ActiveItems[i];
                if (it != null && it.IsActiveOnGround)
                {
                    float distSq = GetMinDistanceSqToPlayers(it.transform.position);
                    if (distSq > maxDistSq)
                    {
                        maxDistSq = distSq;
                        farthestIndex = i;
                    }
                }
            }

            if (farthestIndex >= 0 && farthestIndex < ActiveItems.Count)
            {
                var farthestItem = ActiveItems[farthestIndex];
                ReleaseItem(farthestItem.gameObject, farthestItem);
            }
        }
    }
}
