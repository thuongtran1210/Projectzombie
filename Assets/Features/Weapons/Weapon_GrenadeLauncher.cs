using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Vũ khí Grenade Launcher (W006): Bắn lựu đạn gây sát thương nổ diện rộng (AoE Explosive) tại điểm bắn/mục tiêu.
    /// </summary>
    public class Weapon_GrenadeLauncher : Weapon_RangedBase
    {
        private Transform _currentTarget;

        protected override bool CanAttack()
        {
            float range = CharacterStats != null ? CharacterStats.AttackRange : 9f;
            _currentTarget = TargetingUtility.FindNearestEnemy(transform.position, range);
            return _currentTarget != null;
        }

        protected override void PerformAttack()
        {
            if (projectileData == null || _currentTarget == null) return;

            Vector2 direction = (_currentTarget.position - firePoint.position).normalized;
            DamageData damageData = CreateDamageData();
            int count = GetFinalProjectileCount();

            for (int i = 0; i < count; i++)
            {
                float offsetAngle = (i - (count - 1) / 2f) * 12f;
                Vector2 grenadeDir = Quaternion.Euler(0, 0, offsetAngle) * direction;

                var proj = Projectiles.Core.ProjectileSystem.Instance.Spawn(projectileData, firePoint.position, grenadeDir, gameObject, damageData);
                if (proj != null && GetFinalScale() != 1f)
                {
                    proj.transform.localScale = Vector3.one * GetFinalScale();
                }
            }
        }
    }
}
