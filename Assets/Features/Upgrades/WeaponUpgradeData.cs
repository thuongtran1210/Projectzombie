using UnityEngine;
using ProjectZombie.Features.Weapons;
using System.Linq;

namespace ProjectZombie.Features.Upgrades
{
    [CreateAssetMenu(fileName = "NewWeaponUpgradeData", menuName = "ProjectZombie/Upgrades/Weapon Upgrade Data")]
    public class WeaponUpgradeData : UpgradeData
    {
        [Header("Weapon Settings")]
        [Tooltip("ID của Pháp Bảo để áp dụng cường hóa")]
        public string weaponId;
        
        [Tooltip("Cấp độ vũ khí hiện tại yêu cầu để thẻ này xuất hiện (1 = Nâng lên Lv2, 2 = Lv3, v.v.)")]
        public int requiredCurrentLevel = 1;

        [Header("Modifiers")]
        [Tooltip("Các chỉ số thay đổi được áp dụng cho vũ khí này khi nâng cấp.")]
        public WeaponStatModifier statModifier;
        
        [Tooltip("Prefab đạn mới sẽ thay thế đạn gốc khi nhận nâng cấp này. Bỏ trống nếu không thay đổi.")]
        public GameObject overrideProjectilePrefab;

        public override bool IsAvailable(GameObject player)
        {
            var weaponManager = player.GetComponent<WeaponManager>();
            if (weaponManager == null) return false;

            var baseWeapon = weaponManager.ActiveWeapons.FirstOrDefault(w => string.Equals(w.weaponId, weaponId, System.StringComparison.OrdinalIgnoreCase));
            if (baseWeapon == null) return false;

            // Nếu đã sở hữu trong Loadout, kiểm tra chưa đạt max level và khớp cấp độ
            return baseWeapon.WeaponLevel < baseWeapon.MaxLevel && baseWeapon.WeaponLevel == requiredCurrentLevel;
        }

        public override void ApplyUpgrade(GameObject player)
        {
            var weaponManager = player.GetComponent<WeaponManager>();
            if (weaponManager == null) return;

            // Thẻ Nâng cấp thông số Pháp Bảo
            WeaponBase targetWeapon = weaponManager.GetWeaponById(weaponId);
            if (targetWeapon != null)
            {
                targetWeapon.ApplyStatModifier(statModifier);
                targetWeapon.OnLevelUp(targetWeapon.WeaponLevel, this);
                weaponManager.NotifyWeaponsChanged();
                Debug.Log($"<color=#00FF88>[WeaponUpgradeData]</color> Đã cường hóa Pháp Bảo {weaponId} lên Cấp {targetWeapon.WeaponLevel}");
            }
            else
            {
                Debug.LogWarning($"[WeaponUpgradeData] Weapon with ID {weaponId} not found to apply upgrade.");
            }
        }

        public override string GetCategoryDisplayName()
        {
            return "<color=#007A4D><b>[CƯỜNG HÓA PHÁP BẢO]</b></color>";
        }

        public override string GetLevelDisplayName(GameObject player)
        {
            return $"Cấp {requiredCurrentLevel + 1}";
        }

        public override float GetDynamicWeightMultiplier(GameObject player)
        {
            return 2.5f;
        }
    }
}
