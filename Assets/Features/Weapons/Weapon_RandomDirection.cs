using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Vũ khí đánh xa bắn đạn theo các hướng ngẫu nhiên, không tự tìm mục tiêu.
    /// </summary>
    public class Weapon_RandomDirection : Weapon_RangedBase
    {
        protected override bool CanAttack()
        {
            // Không cần mục tiêu, luôn có thể bắn miễn là hết thời gian cooldown
            return true;
        }

        protected override void PerformAttack()
        {
            if (projectileData == null) return;

            // Lấy thông số tổng (Base + Local Upgrades)
            DamageData damageData = DamageUtility.CalculateDamage(GetFinalDamage(), GetFinalCritChance(), GetFinalCritDamage());
            
            int count = GetFinalProjectileCount();

            for (int i = 0; i < count; i++)
            {
                // Sinh ngẫu nhiên một góc từ 0 đến 360 độ
                float randomAngle = Random.Range(0f, 360f);
                Vector2 randomDir = Quaternion.Euler(0, 0, randomAngle) * Vector2.up;

                // Spawn đạn
                var proj = Projectiles.Core.ProjectileSystem.Instance.Spawn(projectileData, firePoint.position, randomDir, gameObject, damageData);
                
                // Cập nhật kích thước (Scale) nếu có nâng cấp
                if (proj != null && GetFinalScale() != 1f)
                {
                    proj.transform.localScale = Vector3.one * GetFinalScale();
                }
            }
        }
    }
}
