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
            float range = CharacterStats != null ? CharacterStats.AttackRange : 10f;
            _currentTarget = TargetingUtility.FindNearestEnemy(transform.position, range);
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
            if (Projectiles.Core.ProjectileSystem.Instance != null && Projectiles.Core.ProjectileSystem.Instance.EventDispatcher != null)
            {
                Projectiles.Core.ProjectileSystem.Instance.EventDispatcher.OnProjectileHit += HandleBatHitEnemy;
            }
        }

        private void OnDisable()
        {
            if (Projectiles.Core.ProjectileSystem.Instance != null && Projectiles.Core.ProjectileSystem.Instance.EventDispatcher != null)
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
    }
}
