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

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            if (activeCooldown <= 0f || activeCooldown == 8.0f) activeCooldown = 8.0f;
            if (string.IsNullOrEmpty(skillActionName)) skillActionName = "Bão Lửa Thần Sa";
        }

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

        public override Combat.Aiming.SkillAimConfig AimConfig => new Combat.Aiming.SkillAimConfig(Combat.Aiming.SkillAimType.CircleReticle, 7.5f, 2.4f, 0f, true);

        /// <summary>
        /// Kỹ năng chủ động: Bão Lửa Thần Sa — Quăng 3 quả lựu đạn thần sa nổ liên hoàn tạo bão lửa diện rộng.
        /// </summary>
        protected override void PerformActiveRelicSkill(Vector2 customAimDirection = default)
        {
            Vector2 direction = customAimDirection;
            if (direction == Vector2.zero)
            {
                float range = CharacterStats != null ? CharacterStats.AttackRange * 1.5f : 12f;
                Transform target = TargetingUtility.FindNearestEnemy(transform.position, range);
                if (target != null)
                {
                    direction = (target.position - firePoint.position).normalized;
                }
                else
                {
                    direction = transform.root.localScale.x >= 0 ? Vector2.right : Vector2.left;
                }
            }

            DamageData damageData = CreateDamageData();
            damageData = new DamageData(damageData.Amount * 1.6f, true, ElementType.Hoa, damageData.IsCounter, this);
            int burstCount = Mathf.Max(3, GetFinalProjectileCount() + 2);

            for (int i = 0; i < burstCount; i++)
            {
                float offsetAngle = (i - (burstCount - 1) / 2f) * 14f;
                Vector2 grenadeDir = Quaternion.Euler(0, 0, offsetAngle) * direction;

                if (projectileData != null)
                {
                    var proj = Projectiles.Core.ProjectileSystem.Instance.Spawn(projectileData, firePoint.position, grenadeDir, gameObject, damageData);
                    if (proj != null)
                    {
                        proj.transform.localScale = Vector3.one * Mathf.Max(1.3f, GetFinalScale() * 1.3f);
                    }
                }
            }

            global::Core.Audio.AudioManager.Instance?.PlayProjectileExplode(firePoint.position);
        }
    }
}
