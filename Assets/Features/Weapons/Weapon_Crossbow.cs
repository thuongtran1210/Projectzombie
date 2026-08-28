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

        /// <summary>
        /// Kỹ năng chủ động: Vạn Tiễn Phá Trận — Bắn chùm 5 linh tiễn thần lực cực mạnh xuyên quái theo hình nan quạt.
        /// </summary>
        protected override void PerformActiveRelicSkill()
        {
            float range = CharacterStats != null ? CharacterStats.AttackRange * 1.5f : 15f;
            Transform target = TargetingUtility.FindNearestEnemy(transform.position, range);

            Vector2 direction;
            if (target != null)
            {
                direction = (target.position - firePoint.position).normalized;
            }
            else
            {
                direction = transform.root.localScale.x >= 0 ? Vector2.right : Vector2.left;
            }

            DamageData damageData = CreateDamageData();
            damageData = new DamageData(damageData.Amount * 1.5f, damageData.IsCritical, ElementType.Kim, damageData.IsCounter, this);
            int burstCount = Mathf.Max(5, GetFinalProjectileCount() + 3);

            for (int i = 0; i < burstCount; i++)
            {
                float offsetAngle = (i - (burstCount - 1) / 2f) * 7.5f;
                Vector2 boltDir = Quaternion.Euler(0, 0, offsetAngle) * direction;

                if (projectileData != null)
                {
                    var proj = Projectiles.Core.ProjectileSystem.Instance.Spawn(projectileData, firePoint.position, boltDir, gameObject, damageData);
                    if (proj != null)
                    {
                        proj.transform.localScale = Vector3.one * Mathf.Max(1.2f, GetFinalScale() * 1.2f);
                    }
                }
            }

            global::Core.Audio.AudioManager.Instance?.PlaySlash(true, transform.position);
        }
    }
}
