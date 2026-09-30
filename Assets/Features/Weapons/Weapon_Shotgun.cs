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
        [SerializeField] private float spreadAngle = 35f;
        [SerializeField] private int pelletsCount = 3;

        [Header("VFX Settings")]
        [SerializeField] private GameObject shockwavePrefab;

        private Transform _currentTarget;

        protected override bool CanAttack()
        {
            if (this == null || gameObject == null) return false;
            _currentTarget = FindCurrentTarget();
            return _currentTarget != null;
        }

        protected override bool CanActivateActiveRelicSkill()
        {
            _currentTarget = FindCurrentTarget();
            return projectileData != null &&
                   _currentTarget != null &&
                   Projectiles.Core.ProjectileSystem.Instance != null;
        }

        protected override void PerformAttack()
        {
            _currentTarget = FindCurrentTarget();
            if (projectileData == null || _currentTarget == null) return;

            var projectileSystem = Projectiles.Core.ProjectileSystem.Instance;
            if (projectileSystem == null) return;

            Transform launchPoint = firePoint != null ? firePoint : transform;

            Vector2 baseDirection = (_currentTarget.position - launchPoint.position).normalized;
            DamageData damageData = CreateDamageData();

            // Phát hiệu ứng sóng âm xung kích Trống Đồng gắn bám theo nhân vật khi đánh (chống lệch pha khi di chuyển)
            if (shockwavePrefab != null && GlobalVFXPoolManager.Instance != null)
            {
                GlobalVFXPoolManager.Instance.PlayEffectAttached(shockwavePrefab, launchPoint, 0.45f, Vector3.one * GetFinalScale());
            }

            int totalPellets = pelletsCount + (GetFinalProjectileCount() - 1);
            float startAngle = -spreadAngle / 2f;
            float angleStep = totalPellets > 1 ? spreadAngle / (totalPellets - 1) : 0f;

            for (int i = 0; i < totalPellets; i++)
            {
                float currentAngle = startAngle + i * angleStep;
                Vector2 pelletDir = Quaternion.Euler(0, 0, currentAngle) * baseDirection;

                var proj = projectileSystem.Spawn(projectileData, launchPoint.position, pelletDir, gameObject, damageData);
                if (proj != null && GetFinalScale() != 1f)
                {
                    proj.transform.localScale = Vector3.one * GetFinalScale();
                }
            }
        }

        private Transform FindCurrentTarget()
        {
            float range = CharacterStats != null ? CharacterStats.AttackRange : 8f;
            return TargetingUtility.FindNearestEnemy(transform.position, range);
        }
    }
}

