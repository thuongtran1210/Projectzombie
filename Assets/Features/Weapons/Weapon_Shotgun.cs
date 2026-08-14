using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Vũ khí Shotgun (W005): Bắn chùm đạn tỏa góc (Spread Pellets) hướng về mục tiêu gần nhất hoặc hướng di chuyển.
    /// </summary>
    public class Weapon_Shotgun : Weapon_RangedBase
    {
        [Header("Shotgun Custom Settings")]
        [SerializeField] private float spreadAngle = 30f;
        [SerializeField] private int pelletsCount = 5;

        private Transform _currentTarget;

        protected override bool CanAttack()
        {
            _currentTarget = FindNearestEnemy();
            return _currentTarget != null;
        }

        protected override void PerformAttack()
        {
            if (projectileData == null || _currentTarget == null) return;

            Vector2 baseDirection = (_currentTarget.position - firePoint.position).normalized;
            DamageData damageData = CreateDamageData();

            int totalPellets = pelletsCount + (GetFinalProjectileCount() - 1);
            float startAngle = -spreadAngle / 2f;
            float angleStep = totalPellets > 1 ? spreadAngle / (totalPellets - 1) : 0f;

            for (int i = 0; i < totalPellets; i++)
            {
                float currentAngle = startAngle + i * angleStep;
                Vector2 pelletDir = Quaternion.Euler(0, 0, currentAngle) * baseDirection;

                var proj = Projectiles.Core.ProjectileSystem.Instance.Spawn(projectileData, firePoint.position, pelletDir, gameObject, damageData);
                if (proj != null && GetFinalScale() != 1f)
                {
                    proj.transform.localScale = Vector3.one * GetFinalScale();
                }
            }
        }

        private static readonly Collider2D[] _hitBuffer = new Collider2D[50];

        private Transform FindNearestEnemy()
        {
            float range = CharacterStats != null ? CharacterStats.AttackRange : 8f;
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
