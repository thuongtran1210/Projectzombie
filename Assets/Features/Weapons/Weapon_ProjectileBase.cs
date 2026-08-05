using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Lớp cơ sở cho tất cả các vũ khí có sinh ra đạn (Projectile-based Weapons).
    /// Quản lý dữ liệu ProjectileData, tạo bản sao ScriptableObject và cập nhật đạn khi thăng cấp.
    /// </summary>
    public abstract class Weapon_ProjectileBase : WeaponBase
    {
        [Header("Projectile Settings")]
        [SerializeField] protected Projectiles.Data.ProjectileData projectileData;

        private bool _isDataCloned;

        public override void Initialize(ICharacterStats stats)
        {
            base.Initialize(stats);

            // Tạo bản sao (clone) của ScriptableObject để ghi đè mà không ảnh hưởng file gốc
            if (projectileData != null && !_isDataCloned)
            {
                projectileData = Instantiate(projectileData);
                _isDataCloned = true;
            }
        }

        public override void OnLevelUp(int newLevel, Upgrades.UpgradeData appliedUpgrade)
        {
            base.OnLevelUp(newLevel, appliedUpgrade);

            // Nếu thẻ nâng cấp có cung cấp Prefab đạn mới (ví dụ thẻ Level 4 hoặc 6)
            if (appliedUpgrade is Upgrades.WeaponUpgradeData weaponUpgrade)
            {
                if (weaponUpgrade.overrideProjectilePrefab != null && projectileData != null)
                {
                    projectileData.LogicPrefab = weaponUpgrade.overrideProjectilePrefab;
                    Debug.Log($"[{gameObject.name}] Đã thay đổi đạn thành: {weaponUpgrade.overrideProjectilePrefab.name}");
                }
            }
        }

        protected virtual void OnDestroy()
        {
            if (_isDataCloned && projectileData != null)
            {
                Destroy(projectileData);
                projectileData = null;
            }
        }
    }
}
