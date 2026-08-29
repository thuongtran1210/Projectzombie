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

        public override Combat.Aiming.SkillAimConfig AimConfig => new Combat.Aiming.SkillAimConfig(Combat.Aiming.SkillAimType.CircleReticle, 8.5f, 3.2f, 0f, true);

        /// <summary>
        /// Kỹ năng chủ động: Bão Lửa Thần Sa — Quăng chùm 3 hạt Thần Sa nổ tung liên hoàn tạo bão lửa thiêu rụi vùng rộng trong 4s.
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

            StartCoroutine(RoutineClusterBombBarrage(direction));
        }

        private System.Collections.IEnumerator RoutineClusterBombBarrage(Vector2 direction)
        {
            DamageData damageData = CreateDamageData();
            damageData = new DamageData(damageData.Amount * 2.5f, true, ElementType.Hoa, true, this);
            int clusterCount = 3;

            for (int c = 0; c < clusterCount; c++)
            {
                float spreadAngle = (c - 1) * 18f;
                Vector2 bombDir = Quaternion.Euler(0, 0, spreadAngle) * direction;
                Vector3 targetLandPos = firePoint.position + (Vector3)(bombDir * (6.5f + c * 1.2f));

                if (projectileData != null)
                {
                    var proj = Projectiles.Core.ProjectileSystem.Instance?.Spawn(projectileData, firePoint.position, bombDir, gameObject, damageData);
                    if (proj != null)
                    {
                        proj.transform.localScale = Vector3.one * Mathf.Max(1.4f, GetFinalScale() * 1.4f);
                    }
                }

                global::Core.Audio.AudioManager.Instance?.PlayProjectileShoot(firePoint.position);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
