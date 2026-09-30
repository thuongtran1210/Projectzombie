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

        public override Combat.Aiming.SkillAimConfig AimConfig =>
            new Combat.Aiming.SkillAimConfig(Combat.Aiming.SkillAimType.LineArrow, 12f, 1.2f, 0f, true);

        protected override void PerformActiveRelicSkill(Vector2 customAimDirection = default)
        {
            if (projectileData == null || Projectiles.Core.ProjectileSystem.Instance == null) return;

            Vector2 direction = customAimDirection;
            if (direction.sqrMagnitude <= 0.001f)
            {
                float range = CharacterStats != null ? CharacterStats.AttackRange * 1.5f : 12f;
                _currentTarget = TargetingUtility.FindNearestEnemy(transform.position, range);
                direction = _currentTarget != null
                    ? (Vector2)(_currentTarget.position - firePoint.position).normalized
                    : (Vector2)transform.right;
            }

            DamageData damageData = CreateDamageData();
            int count = Mathf.Max(1, GetFinalProjectileCount());
            for (int i = 0; i < count; i++)
            {
                float offsetAngle = (i - (count - 1) / 2f) * 5f;
                Vector2 shotDirection = Quaternion.Euler(0f, 0f, offsetAngle) * direction.normalized;
                var projectile = Projectiles.Core.ProjectileSystem.Instance.Spawn(
                    projectileData, firePoint.position, shotDirection, gameObject, damageData);
                ApplyScale(projectile);
            }
        }

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
