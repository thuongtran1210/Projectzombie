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

        public override Combat.Aiming.SkillAimConfig AimConfig => Combat.Aiming.SkillAimConfig.DefaultInstant;

        protected override bool CanActivateActiveRelicSkill() => CanAttack();

        protected override void PerformActiveRelicSkill()
        {
            PerformAttack();
        }

        protected override bool CanAttack()
        {
            if (projectileData == null || Projectiles.Core.ProjectileSystem.Instance == null)
                return false;
            float range = CharacterStats != null ? CharacterStats.AttackRange : 10f;
            _currentTarget = TargetingUtility.FindNearestEnemy(transform.position, range);
            return _currentTarget != null;
        }

        protected override void PerformAttack()
        {
            TryFireAt(_currentTarget);
        }

        /// <summary>Fires directly at a supplied target. Used by the isolated test harness.</summary>
        public bool TryFireAt(Transform target)
        {
            var projectileSystem = Projectiles.Core.ProjectileSystem.Instance;
            if (target == null || projectileData == null || projectileSystem == null || firePoint == null)
                return false;

            Vector2 direction = (target.position - firePoint.position).normalized;
            DamageData damageData = CreateDamageData();
            int count = GetFinalProjectileCount();
            bool spawnedAny = false;

            for (int i = 0; i < count; i++)
            {
                float spreadAngle = Random.Range(-15f, 15f);
                Vector2 orbDir = Quaternion.Euler(0, 0, spreadAngle) * direction;

                var proj = projectileSystem.Spawn(projectileData, firePoint.position, orbDir, gameObject, damageData);
                if (proj != null)
                {
                    spawnedAny = true;
                    proj.State.RemainingBounce = 5; // Nảy chuỗi 5 lần qua 6 quái
                    proj.State.BonusPierce = 3;
                    if (GetFinalScale() != 1f)
                    {
                        proj.transform.localScale = Vector3.one * GetFinalScale();
                    }
                }
            }
            if (spawnedAny) Audio?.PlayMagicOrbit(firePoint.position);
            return spawnedAny;
        }
    }
}
