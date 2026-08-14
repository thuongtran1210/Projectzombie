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
            _currentTarget = FindNearestEnemy();
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

        private static readonly Collider2D[] _hitBuffer = new Collider2D[50];

        private Transform FindNearestEnemy()
        {
            float range = CharacterStats != null ? CharacterStats.AttackRange : 6f;
            int numHits = Physics2D.OverlapCircleNonAlloc(transform.position, range, _hitBuffer);

            Transform nearestEnemy = null;
            float minSqrDistance = float.MaxValue;

            for (int i = 0; i < numHits; i++)
            {
                var hitCollider = _hitBuffer[i];
                if (hitCollider.CompareTag("Enemy"))
                {
                    var healthSystem = hitCollider.GetComponent<HealthSystem>();
                    if (healthSystem != null && healthSystem.CurrentHealth <= 0) continue;

                    float sqrDistance = (transform.position - hitCollider.transform.position).sqrMagnitude;
                    if (sqrDistance < minSqrDistance)
                    {
                        minSqrDistance = sqrDistance;
                        nearestEnemy = hitCollider.transform;
                    }
                }
            }
            return nearestEnemy;
        }
    }
}
