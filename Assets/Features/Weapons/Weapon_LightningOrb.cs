using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Vũ khí Lightning Orb (W009): Bắn cầu sét ngẫu nhiên nảy giữa các mục tiêu hoặc nổ dòng điện AoE.
    /// </summary>
    public class Weapon_LightningOrb : Weapon_RangedBase
    {
        private Transform _currentTarget;

        protected override bool CanAttack()
        {
            float range = CharacterStats != null ? CharacterStats.AttackRange : 10f;
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
                float spreadAngle = Random.Range(-15f, 15f);
                Vector2 orbDir = Quaternion.Euler(0, 0, spreadAngle) * direction;

                var proj = Projectiles.Core.ProjectileSystem.Instance.Spawn(projectileData, firePoint.position, orbDir, gameObject, damageData);
                if (proj != null && GetFinalScale() != 1f)
                {
                    proj.transform.localScale = Vector3.one * GetFinalScale();
                }
            }
        }
    }
}
