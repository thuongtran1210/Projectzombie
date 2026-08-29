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

            // Kiểm tra trạng thái Máu Thấp (<35% HP): Kích hoạt Hồ Ly Cuồng Nộ x2 dơi hồ ly
            bool isBerserk = false;
            if (transform.root.TryGetComponent<HealthSystem>(out var hp) && hp.MaxHealth > 0f)
            {
                if (hp.CurrentHealth / hp.MaxHealth <= 0.35f)
                {
                    isBerserk = true;
                    count *= 2;
                }
            }

            for (int i = 0; i < count; i++)
            {
                float spreadAngle = Random.Range(-25f, 25f);
                Vector2 batDir = Quaternion.Euler(0, 0, spreadAngle) * direction;

                var proj = Projectiles.Core.ProjectileSystem.Instance.Spawn(projectileData, firePoint.position, batDir, gameObject, damageData);
                if (proj != null)
                {
                    proj.transform.localScale = Vector3.one * (GetFinalScale() * (isBerserk ? 1.35f : 1.0f));
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
                float heal = lifestealAmountPerHit;
                if (transform.root.TryGetComponent<HealthSystem>(out var playerHealth) && playerHealth.MaxHealth > 0f)
                {
                    if (playerHealth.CurrentHealth / playerHealth.MaxHealth <= 0.35f) heal *= 2.5f; // x2.5 Hút máu khi nguy kịch
                    playerHealth.Heal(heal);
                }
            }
        }
    }
}
