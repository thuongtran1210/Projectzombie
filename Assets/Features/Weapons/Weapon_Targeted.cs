using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Vũ khí tự động nhắm và bắn vào mục tiêu gần nhất.
    /// Giống hệt cơ chế Stream Blade cũ.
    /// </summary>
    public class Weapon_Targeted : Weapon_RangedBase
    {
        private Transform _currentTarget;

        protected override bool CanAttack()
        {
            float range = CharacterStats != null ? CharacterStats.AttackRange : 8f;
            _currentTarget = TargetingUtility.FindNearestEnemy(transform.position, range);
            return _currentTarget != null;
        }

        protected override void PerformAttack()
        {
            if (projectileData == null || _currentTarget == null) return;

            Vector2 direction = (_currentTarget.position - firePoint.position).normalized;
            
            // Lấy thông số tổng (Base + Local Upgrades)
            DamageData damageData = CreateDamageData();
            
            int count = GetFinalProjectileCount();
            
            // Logic bắn nhiều tia (Multi-projectile)
            if (count <= 1)
            {
                var proj = Projectiles.Core.ProjectileSystem.Instance.Spawn(projectileData, firePoint.position, direction, gameObject, damageData);
                ApplyScale(proj);
            }
            else
            {
                float spreadAngle = 15f; // Góc tỏa
                float startAngle = -spreadAngle * (count - 1) / 2f;

                for (int i = 0; i < count; i++)
                {
                    float angle = startAngle + i * spreadAngle;
                    Vector2 spreadDir = Quaternion.Euler(0, 0, angle) * direction;

                    var proj = Projectiles.Core.ProjectileSystem.Instance.Spawn(projectileData, firePoint.position, spreadDir, gameObject, damageData);
                    ApplyScale(proj);
                }
            }
        }
        
        private void ApplyScale(Projectiles.Components.ProjectileController proj)
        {
            if (proj != null && GetFinalScale() != 1f)
            {
                proj.transform.localScale = Vector3.one * GetFinalScale();
            }
        }
    }
}
