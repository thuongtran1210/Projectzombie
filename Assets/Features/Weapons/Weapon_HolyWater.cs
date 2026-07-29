using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Vũ khí Holy Water (W011): Thả bãi nước phép tại vị trí kẻ địch/dưới chân Player gây sát thương AoE liên tục theo thời gian (Ground Zone).
    /// </summary>
    public class Weapon_HolyWater : Weapon_RangedBase
    {
        private Transform _currentTarget;

        protected override bool CanAttack()
        {
            _currentTarget = FindNearestEnemy();
            return true; // Cho phép thả nước thánh dù không có mục tiêu (thả dưới chân)
        }

        protected override void PerformAttack()
        {
            if (projectileData == null) return;

            DamageData damageData = DamageUtility.CalculateDamage(GetFinalDamage(), GetFinalCritChance(), GetFinalCritDamage());
            int count = GetFinalProjectileCount();

            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPos;
                if (_currentTarget != null)
                {
                    Vector2 randomOffset = Random.insideUnitCircle * 2f;
                    spawnPos = _currentTarget.position + (Vector3)randomOffset;
                }
                else
                {
                    Vector2 randomOffset = Random.insideUnitCircle * 3f;
                    spawnPos = transform.position + (Vector3)randomOffset;
                }

                var proj = Projectiles.Core.ProjectileSystem.Instance.Spawn(projectileData, spawnPos, Vector2.zero, gameObject, damageData);
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
