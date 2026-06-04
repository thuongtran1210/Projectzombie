using UnityEngine;
using ProjectZombie.Features.Weapons;
using System.Linq;

namespace ProjectZombie.Features.Upgrades
{
    [CreateAssetMenu(fileName = "NewWeaponUpgradeData", menuName = "ProjectZombie/Upgrades/Weapon Upgrade Data")]
    public class WeaponUpgradeData : UpgradeData
    {
        [Header("Weapon Settings")]
        [Tooltip("ID của vũ khí để áp dụng hoặc Mở khóa")]
        public string weaponId;
        
        [Tooltip("Cấp độ vũ khí hiện tại yêu cầu để thẻ này xuất hiện. 0 = Mở khóa vũ khí mới")]
        public int requiredCurrentLevel = 1;

        [Tooltip("Prefab vũ khí sẽ được instantiate (Dùng khi requiredCurrentLevel = 0)")]
        public GameObject weaponPrefab;

        [Header("Modifiers")]
        [Tooltip("Các chỉ số thay đổi được áp dụng cho vũ khí này khi nâng cấp.")]
        public WeaponStatModifier statModifier;
        
        [Tooltip("Prefab đạn mới sẽ thay thế đạn gốc khi nhận nâng cấp này. Bỏ trống nếu không thay đổi.")]
        public GameObject overrideProjectilePrefab;

        public override bool IsAvailable(GameObject player)
        {
            var weaponManager = player.GetComponent<WeaponManager>();
            if (weaponManager == null) return false;

            var baseWeapon = weaponManager.ActiveWeapons.FirstOrDefault(w => w.weaponId == weaponId);
            bool hasWeapon = baseWeapon != null;

            if (!hasWeapon)
            {
                // Nếu chưa sở hữu, chỉ cho phép thẻ Mở Khóa (yêu cầu cấp 0)
                return requiredCurrentLevel == 0;
            }
            else
            {
                // Nếu đã sở hữu, kiểm tra chưa đạt max level và khớp cấp độ
                return baseWeapon.WeaponLevel < baseWeapon.MaxLevel && baseWeapon.WeaponLevel == requiredCurrentLevel;
            }
        }

        public override void ApplyUpgrade(GameObject player)
        {
            var weaponManager = player.GetComponent<WeaponManager>();
            if (weaponManager == null) return;

            if (requiredCurrentLevel == 0) // Thẻ Mở khóa vũ khí mới
            {
                if (weaponPrefab != null)
                {
                    GameObject weaponObj = Instantiate(weaponPrefab, weaponManager.transform);
                    WeaponBase weapon = weaponObj.GetComponent<WeaponBase>();
                    if (weapon != null)
                    {
                        weaponManager.AddWeapon(weapon);
                    }
                }
            }
            else
            {
                // Thẻ Nâng cấp thông số
                WeaponBase targetWeapon = weaponManager.GetWeaponById(weaponId);
                if (targetWeapon != null)
                {
                    targetWeapon.ApplyStatModifier(statModifier);
                    // Có thể thêm logic thay overrideProjectilePrefab vào targetWeapon tại đây nếu WeaponBase hỗ trợ
                    targetWeapon.OnLevelUp(targetWeapon.WeaponLevel, this);
                    weaponManager.NotifyWeaponsChanged(); // Thay cho OnWeaponsChanged?.Invoke()
                }
                else
                {
                    Debug.LogWarning($"[WeaponUpgradeData] Weapon with ID {weaponId} not found to apply upgrade.");
                }
            }
        }
    }
}
