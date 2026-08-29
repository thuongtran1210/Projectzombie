using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Vũ khí Crossbow (W007): Bắn nỏ xuyên thấu nhiều kẻ địch trên một đường thẳng.
    /// </summary>
    public class Weapon_Crossbow : Weapon_RangedBase
    {
        private Transform _currentTarget;

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);
            if (activeCooldown <= 0f || activeCooldown == 8.0f) activeCooldown = 6.0f;
            if (string.IsNullOrEmpty(skillActionName)) skillActionName = "Vạn Tiễn Phá Trận";
        }

        protected override bool CanAttack()
        {
            float range = CharacterStats != null ? CharacterStats.AttackRange : 12f;
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
                float offsetAngle = (i - (count - 1) / 2f) * 8f;
                Vector2 boltDir = Quaternion.Euler(0, 0, offsetAngle) * direction;

                var proj = Projectiles.Core.ProjectileSystem.Instance.Spawn(projectileData, firePoint.position, boltDir, gameObject, damageData);
                if (proj != null && GetFinalScale() != 1f)
                {
                    proj.transform.localScale = Vector3.one * GetFinalScale();
                }
            }
        }

        public override Combat.Aiming.SkillAimConfig AimConfig => new Combat.Aiming.SkillAimConfig(Combat.Aiming.SkillAimType.LineArrow, 14.0f, 1.4f, 0f, true);

        /// <summary>
        /// Kỹ năng chủ động: Vạn Tiễn Phá Trận — Khai hỏa 3 đợt bão Linh Tiễn Thần Uy xuyên thấu 100% mục tiêu trên đường bay và đẩy lùi bầy quái 8m.
        /// </summary>
        protected override void PerformActiveRelicSkill(Vector2 customAimDirection = default)
        {
            Vector2 direction = customAimDirection;
            if (direction == Vector2.zero)
            {
                float range = CharacterStats != null ? CharacterStats.AttackRange * 1.5f : 15f;
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

            StartCoroutine(RoutinePiercingVolley(direction));
        }

        private System.Collections.IEnumerator RoutinePiercingVolley(Vector2 direction)
        {
            DamageData damageData = CreateDamageData();
            damageData = new DamageData(damageData.Amount * 2.2f, true, ElementType.Kim, true, this);
            int waves = 3;
            int arrowsPerWave = Mathf.Max(5, GetFinalProjectileCount() + 3);

            for (int w = 0; w < waves; w++)
            {
                global::Core.Audio.AudioManager.Instance?.PlaySlash(true, transform.position);

                for (int i = 0; i < arrowsPerWave; i++)
                {
                    float offsetAngle = (i - (arrowsPerWave - 1) / 2f) * 6.5f;
                    Vector2 boltDir = Quaternion.Euler(0, 0, offsetAngle) * direction;

                    if (projectileData != null)
                    {
                        var proj = Projectiles.Core.ProjectileSystem.Instance?.Spawn(projectileData, firePoint.position, boltDir, gameObject, damageData);
                        if (proj != null)
                        {
                            proj.State.BonusPierce = 99; // Xuyên thấu vô tận
                            proj.State.SpeedMultiplier = 1.6f;
                            proj.transform.localScale = Vector3.one * Mathf.Max(1.3f, GetFinalScale() * 1.3f);
                        }
                    }
                }

                // Sóng đẩy lùi phía trước mũi tên
                Collider2D[] hits = Physics2D.OverlapBoxAll(firePoint.position + (Vector3)(direction * 4f), new Vector2(8f, 3f), 0f, TargetingUtility.EnemyLayerMask);
                foreach (var hit in hits)
                {
                    if (hit != null && hit.TryGetComponent<Rigidbody2D>(out var rb))
                    {
                        rb.AddForce(direction * 12f, ForceMode2D.Impulse);
                    }
                }

                yield return new WaitForSeconds(0.12f);
            }
        }
    }
}
