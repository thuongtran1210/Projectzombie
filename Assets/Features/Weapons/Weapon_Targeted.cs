using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Vũ khí tự động nhắm và bắn vào mục tiêu gần nhất.
    /// Giống hệt cơ chế Stream Blade cũ.
    /// </summary>
    public class Weapon_Targeted : Weapon_RangedBase
    {
        private Transform _currentTarget;

        protected override bool CanAttack()
        {
            _currentTarget = FindNearestEnemy();
            return _currentTarget != null;
        }

        protected override void PerformAttack()
        {
            if (projectileData == null) return;

            Vector2 direction = (_currentTarget.position - firePoint.position).normalized;
            
            // Lấy thông số tổng (Base + Local Upgrades)
            DamageData damageData = CreateDamageData();
            
            int count = GetFinalProjectileCount();
            
            // Logic bắn nhiều tia (Multi-projectile)
            if (count <= 1)
            {
                var proj = Projectiles.Core.ProjectileSystem.Instance.Spawn(projectileData, firePoint.position, direction, gameObject, damageData);
                ApplyScale(proj);
            }
            else
            {
                float spreadAngle = 15f; // Góc tỏa
                float startAngle = -spreadAngle * (count - 1) / 2f;

                for (int i = 0; i < count; i++)
                {
                    float angle = startAngle + i * spreadAngle;
                    Vector2 spreadDir = Quaternion.Euler(0, 0, angle) * direction;

                    var proj = Projectiles.Core.ProjectileSystem.Instance.Spawn(projectileData, firePoint.position, spreadDir, gameObject, damageData);
                    ApplyScale(proj);
                }
            }
        }
        
        private void ApplyScale(Projectiles.Components.ProjectileController proj)
        {
            if (proj != null && GetFinalScale() != 1f)
            {
                proj.transform.localScale = Vector3.one * GetFinalScale();
            }
        }

        // Buffer tĩnh tái sử dụng chung cho việc dò tìm quái vật
        private static readonly Collider2D[] _hitBuffer = new Collider2D[50];

        private Transform FindNearestEnemy()
        {
            float range = CharacterStats.AttackRange; 
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
