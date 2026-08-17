using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Vũ khí Flamethrower (W008): Phun lửa liên tục gây sát thương tích tắc (DoT) trong hình quạt phía trước.
    /// </summary>
    public class Weapon_Flamethrower : Weapon_RangedBase
    {
        [Header("Flamethrower Settings")]
        [SerializeField] private float coneAngle = 45f;

        private Transform _currentTarget;

        protected override bool CanAttack()
        {
            float range = CharacterStats != null ? CharacterStats.AttackRange : 6f;
            _currentTarget = TargetingUtility.FindNearestEnemy(transform.position, range);
            return _currentTarget != null;
        }

        protected override void PerformAttack()
        {
            if (projectileData == null) return;

            Vector2 direction = _currentTarget != null 
                ? (Vector2)(_currentTarget.position - firePoint.position).normalized 
                : (Vector2)transform.right;

            DamageData damageData = CreateDamageData();
            int count = Mathf.Max(1, GetFinalProjectileCount());

            for (int i = 0; i < count; i++)
            {
                float randomAngle = Random.Range(-coneAngle / 2f, coneAngle / 2f);
                Vector2 flameDir = Quaternion.Euler(0, 0, randomAngle) * direction;

                var proj = Projectiles.Core.ProjectileSystem.Instance.Spawn(projectileData, firePoint.position, flameDir, gameObject, damageData);
                if (proj != null && GetFinalScale() != 1f)
                {
                    proj.transform.localScale = Vector3.one * GetFinalScale();
                }
            }
        }
    }
}
