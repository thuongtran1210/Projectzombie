using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Vũ khí đánh xa bắn ra các loại đạn (Projectile) ngẫu nhiên từ một danh sách.
    /// Có thể kết hợp với việc bắn theo hướng ngẫu nhiên hoặc tự tìm mục tiêu.
    /// </summary>
    public class Weapon_RandomProjectile : Weapon_RangedBase
    {
        [Header("Random Projectiles Settings")]
        [Tooltip("Danh sách các loại đạn có thể bắn ra. Vũ khí sẽ chọn ngẫu nhiên 1 loại mỗi khi bắn.")]
        public Projectiles.Data.ProjectileData[] randomProjectiles;

        protected override bool CanAttack()
        {
            // Tạm thời cho phép luôn bắn (không cần tìm mục tiêu). 
            // Nếu bạn muốn nó vừa random đạn vừa TÌM mục tiêu, bạn có thể copy logic FindNearestEnemy từ Weapon_Targeted sang đây.
            return true;
        }

        protected override void PerformAttack()
        {
            if (randomProjectiles == null || randomProjectiles.Length == 0) return;

            DamageData damageData = DamageUtility.CalculateDamage(GetFinalDamage(), GetFinalCritChance(), GetFinalCritDamage());
            int count = GetFinalProjectileCount();

            for (int i = 0; i < count; i++)
            {
                // 1. Chọn ngẫu nhiên 1 loại đạn trong danh sách
                var randomProjData = randomProjectiles[Random.Range(0, randomProjectiles.Length)];

                // 2. Sinh ngẫu nhiên một góc bắn (nếu bạn muốn hướng cũng ngẫu nhiên)
                float randomAngle = Random.Range(0f, 360f);
                Vector2 randomDir = Quaternion.Euler(0, 0, randomAngle) * Vector2.up;

                // 3. Spawn đạn
                var proj = Projectiles.Core.ProjectileSystem.Instance.Spawn(randomProjData, firePoint.position, randomDir, gameObject, damageData);
                
                if (proj != null && GetFinalScale() != 1f)
                {
                    proj.transform.localScale = Vector3.one * GetFinalScale();
                }
            }
        }
    }
}
