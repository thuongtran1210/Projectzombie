using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Shared.VFX;

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

        [Header("VFX Settings")]
        [SerializeField] private GameObject shockwavePrefab;

        private Transform _currentTarget;

        protected override bool CanAttack()
        {
            float range = CharacterStats != null ? CharacterStats.AttackRange : 8f;
            _currentTarget = TargetingUtility.FindNearestEnemy(transform.position, range);
            return _currentTarget != null;
        }

        protected override void PerformAttack()
        {
            if (projectileData == null || _currentTarget == null) return;

            Vector2 baseDirection = (_currentTarget.position - firePoint.position).normalized;
            DamageData damageData = CreateDamageData();

            // Phát hiệu ứng sóng âm xung kích Trống Đồng tại tâm / firePoint khi đánh
            if (shockwavePrefab != null && GlobalVFXPoolManager.Instance != null)
            {
                GlobalVFXPoolManager.Instance.PlayEffect(shockwavePrefab, firePoint.position, Quaternion.identity, 0.45f, Vector3.one * GetFinalScale());
            }

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
    }
}

