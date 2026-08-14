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

        protected override bool CanAttack()
        {
            _currentTarget = FindNearestEnemy();
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

        private static readonly Collider2D[] _hitBuffer = new Collider2D[50];

        private Transform FindNearestEnemy()
        {
            float range = CharacterStats != null ? CharacterStats.AttackRange : 12f;
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
