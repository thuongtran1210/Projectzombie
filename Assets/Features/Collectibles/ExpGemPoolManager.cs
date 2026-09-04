using UnityEngine;
using UnityEngine.Pool;
using ProjectZombie.Core.Pooling;

namespace ProjectZombie.Features.Collectibles
{
    /// <summary>
    /// Manager singleton quản lý Object Pooling cho Exp Gems (Kinh nghiệm rớt từ Enemy).
    /// Kế thừa từ CollectiblePoolBase, hỗ trợ cơ chế Nén & Gộp hạt (Gem Compression).
    /// </summary>
    public class ExpGemPoolManager : CollectiblePoolBase<ExpGemPoolManager, ExpGem>
    {
        // Backward-compatibility aliases
        public int ActiveGemCount => ActiveItemCount;
        public void RegisterActiveGem(ExpGem gem) => RegisterActiveItem(gem);
        public void UnregisterActiveGem(ExpGem gem) => UnregisterActiveItem(gem);

        protected override void AttachPoolConfig(GameObject obj, IObjectPool<GameObject> pool)
        {
            var config = obj.GetComponent<ExpGemPoolConfig>() ?? obj.AddComponent<ExpGemPoolConfig>();
            config.Pool = pool;
        }

        /// <summary>
        /// Spawn viên ExpGem từ Pool tại vị trí chỉ định.
        /// Tự động kích hoạt nén gộp hạt nếu số lượng vượt quá ngưỡng cho phép.
        /// </summary>
        public GameObject SpawnGem(GameObject prefab, Vector3 position, float expAmount)
        {
            if (prefab == null) return null;

            // Kiểm tra và nén hạt ở xa nếu vượt ngưỡng tối đa
            if (ActiveItems.Count >= maxGroundItems)
            {
                CompressDistantItems();
            }

            var pool = GetOrCreatePool(prefab);
            if (pool != null)
            {
                GameObject gemObj = pool.Get();
                if (gemObj != null)
                {
                    gemObj.transform.position = position;
                    gemObj.transform.rotation = Quaternion.identity;

                    var expGem = gemObj.GetComponent<ExpGem>();
                    if (expGem != null)
                    {
                        expGem.SetExpAmount(expAmount);
                    }

                    return gemObj;
                }
            }

            // Fallback nếu không tạo được pool
            Transform fallbackParent = PoolHierarchyManager.Instance != null 
                ? PoolHierarchyManager.Instance.GetCategoryRoot(PoolHierarchyManager.PoolCategory.Collectibles) 
                : transform;
            GameObject fallbackObj = Instantiate(prefab, position, Quaternion.identity, fallbackParent);
            var fallbackGem = fallbackObj.GetComponent<ExpGem>();
            if (fallbackGem != null) fallbackGem.SetExpAmount(expAmount);
            return fallbackObj;
        }

        /// <summary>
        /// Thuật toán nén gộp 2 hạt Exp ở xa người chơi nhất thành 1 hạt cấp cao hơn.
        /// </summary>
        protected override void CompressDistantItems()
        {
            if (ActiveItems.Count < 2) return;

            Vector3 playerPos = Vector3.zero;
            if (Player.PlayerController.Instance != null)
            {
                playerPos = Player.PlayerController.Instance.transform.position;
            }

            ExpGem furthest1 = null;
            ExpGem furthest2 = null;
            float maxDistSq1 = -1f;
            float maxDistSq2 = -1f;

            for (int i = 0; i < ActiveItems.Count; i++)
            {
                var gem = ActiveItems[i];
                if (gem == null || !gem.IsIdle) continue;

                float distSq = (gem.transform.position - playerPos).sqrMagnitude;

                if (distSq > maxDistSq1)
                {
                    maxDistSq2 = maxDistSq1;
                    furthest2 = furthest1;

                    maxDistSq1 = distSq;
                    furthest1 = gem;
                }
                else if (distSq > maxDistSq2)
                {
                    maxDistSq2 = distSq;
                    furthest2 = gem;
                }
            }

            // Gộp furthest2 vào furthest1
            if (furthest1 != null && furthest2 != null && furthest1 != furthest2)
            {
                furthest1.MergeExp(furthest2.ExpAmount);

                // Thu hồi furthest2 về Pool
                if (furthest2.TryGetComponent<ExpGemPoolConfig>(out var poolConfig))
                {
                    poolConfig.ReturnToPool();
                }
                else
                {
                    Destroy(furthest2.gameObject);
                }
            }
        }

        /// <summary>
        /// Hút toàn bộ hạt kinh nghiệm trên mặt đất về phía người chơi (dành cho vật phẩm Toàn Bản Đồ Magnet).
        /// </summary>
        public void CollectAllActiveGems(Transform player)
        {
            if (player == null) return;

            for (int i = ActiveItems.Count - 1; i >= 0; i--)
            {
                if (i >= ActiveItems.Count) continue;
                var gem = ActiveItems[i];
                if (gem != null && gem.IsActiveOnGround)
                {
                    gem.StartMagnetEffect(player);
                }
            }
        }
    }
}
