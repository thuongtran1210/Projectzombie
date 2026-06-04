using UnityEngine;
using UnityEngine.Pool;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Lớp cơ sở cho các vũ khí đánh xa (Ranged).
    /// Quản lý Object Pool cho Đạn (Projectile).
    /// </summary>
    public abstract class Weapon_RangedBase : WeaponBase
    {
        [Header("Projectile Settings")]
        [SerializeField] protected Projectiles.Data.ProjectileData projectileData;
        
        public override void Initialize(ProjectZombie.Features.Shared.ICharacterStats stats)
        {
            base.Initialize(stats);
            
            // Tạo bản sao (clone) của ScriptableObject để có thể ghi đè Prefab mà không ảnh hưởng file gốc
            if (projectileData != null)
            {
                projectileData = Instantiate(projectileData);
            }
        }

        public override void ApplyStatModifier(Upgrades.WeaponStatModifier modifier)
        {
            base.ApplyStatModifier(modifier);

            // Vì projectileData đã được clone độc lập cho vũ khí này, 
            // ta có thể thay đổi trực tiếp Speed mà không sợ dính líu đến các vũ khí khác.
            if (projectileData != null)
            {
                projectileData.Speed += modifier.projectileSpeedBonus;
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
    }
}
