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
            float range = CharacterStats != null ? CharacterStats.AttackRange : 8f;
            _currentTarget = TargetingUtility.FindNearestEnemy(transform.position, range);
            return _currentTarget != null;
        }

        protected override void PerformAttack()
        {
            if (projectileData == null || _currentTarget == null) return;

            Vector2 baseDirection = (_currentTarget.position - firePoint.position).normalized;
            DamageData damageData = CreateDamageData();

            // Phát hiệu ứng sóng âm xung kích Trống Đồng gắn bám theo nhân vật khi đánh (chống lệch pha khi di chuyển)
            if (shockwavePrefab != null && GlobalVFXPoolManager.Instance != null)
            {
                Transform attachTarget = firePoint != null ? firePoint : transform;
                GlobalVFXPoolManager.Instance.PlayEffectAttached(shockwavePrefab, attachTarget, 0.45f, Vector3.one * GetFinalScale());
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

