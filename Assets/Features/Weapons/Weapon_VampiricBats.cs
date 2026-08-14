using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Vũ khí Vampiric Bats (W004): Bắn ra các con Dơi tự tìm kẻ địch gần nhất và hồi máu nhẹ cho Player khi trúng target.
    /// </summary>
    public class Weapon_VampiricBats : Weapon_RangedBase
    {
        [Header("Vampiric Bats Settings")]
        [SerializeField] private float lifestealAmountPerHit = 1.5f;

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
                : Random.insideUnitCircle.normalized;

            DamageData damageData = CreateDamageData();
            int count = GetFinalProjectileCount();

            for (int i = 0; i < count; i++)
            {
                float spreadAngle = Random.Range(-20f, 20f);
                Vector2 batDir = Quaternion.Euler(0, 0, spreadAngle) * direction;

                var proj = Projectiles.Core.ProjectileSystem.Instance.Spawn(projectileData, firePoint.position, batDir, gameObject, damageData);
                if (proj != null && GetFinalScale() != 1f)
                {
                    proj.transform.localScale = Vector3.one * GetFinalScale();
                }
            }
        }

        private void OnEnable()
        {
            if (Projectiles.Core.ProjectileSystem.Instance != null)
            {
                Projectiles.Core.ProjectileSystem.Instance.EventDispatcher.OnProjectileHit += HandleBatHitEnemy;
            }
        }

        private void OnDisable()
        {
            if (Projectiles.Core.ProjectileSystem.Instance != null)
            {
                Projectiles.Core.ProjectileSystem.Instance.EventDispatcher.OnProjectileHit -= HandleBatHitEnemy;
            }
        }

        private void HandleBatHitEnemy(Projectiles.Core.ProjectileEventContext context)
        {
            if (context.Projectile != null && context.Projectile.Owner == gameObject)
            {
                if (CharacterStats is HealthSystem healthSystem)
                {
                    healthSystem.Heal(lifestealAmountPerHit);
                }
                else if (transform.root.TryGetComponent<HealthSystem>(out var playerHealth))
                {
                    playerHealth.Heal(lifestealAmountPerHit);
                }
            }
        }

        private static readonly Collider2D[] _hitBuffer = new Collider2D[50];

        private Transform FindNearestEnemy()
        {
            float range = CharacterStats != null ? CharacterStats.AttackRange : 10f;
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
