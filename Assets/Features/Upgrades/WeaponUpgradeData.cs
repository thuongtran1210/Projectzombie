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

            var baseWeapon = weaponManager.ActiveWeapons.FirstOrDefault(w => string.Equals(w.weaponId, weaponId, System.StringComparison.OrdinalIgnoreCase));
            bool hasWeapon = baseWeapon != null;

            if (!hasWeapon)
            {
                // Action RPG: Không cho phép nhặt vũ khí mới ngẫu nhiên giữa trận
                return false;
            }
            else
            {
                // Nếu đã sở hữu trong Loadout, kiểm tra chưa đạt max level và khớp cấp độ
                return baseWeapon.WeaponLevel < baseWeapon.MaxLevel && baseWeapon.WeaponLevel == requiredCurrentLevel;
            }
        }

        public override void ApplyUpgrade(GameObject player)
        {
            var weaponManager = player.GetComponent<WeaponManager>();
            if (weaponManager == null) return;

            if (requiredCurrentLevel == 0) // Thẻ Mở khóa vũ khí mới
            {
                GameObject prefabToSpawn = weaponPrefab;

                // Cơ chế tự động Fallback: Tìm kiếm trong Resources nếu chưa gán trực tiếp trên Inspector
                if (prefabToSpawn == null)
                {
                    var allWeaponData = Resources.LoadAll<WeaponData>("ScriptableObjects/Weapons");
                    var matchedData = allWeaponData.FirstOrDefault(wd => wd.weaponId == weaponId);
                    if (matchedData != null && matchedData.weaponPrefab != null)
                    {
                        prefabToSpawn = matchedData.weaponPrefab.gameObject;
                    }
                }

                if (prefabToSpawn != null)
                {
                    GameObject weaponObj = Instantiate(prefabToSpawn, weaponManager.transform);
                    WeaponBase weapon = weaponObj.GetComponent<WeaponBase>();
                    if (weapon != null)
                    {
                        if (string.IsNullOrEmpty(weapon.weaponId)) weapon.weaponId = weaponId;
                        weaponManager.AddWeapon(weapon);
                        Debug.Log($"<color=#00FF00>[WeaponUpgradeData]</color> Đã mở khóa & trang bị thành công Pháp Bảo: {weaponId}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[WeaponUpgradeData] Không thể spawn vũ khí {weaponId}: weaponPrefab là null và không tìm thấy trong Resources/ScriptableObjects/Weapons");
                }
            }
            else
            {
                // Thẻ Nâng cấp thông số
                WeaponBase targetWeapon = weaponManager.GetWeaponById(weaponId);
                if (targetWeapon != null)
                {
                    targetWeapon.ApplyStatModifier(statModifier);
                    targetWeapon.OnLevelUp(targetWeapon.WeaponLevel, this);
                    weaponManager.NotifyWeaponsChanged();
                }
                else
                {
                    Debug.LogWarning($"[WeaponUpgradeData] Weapon with ID {weaponId} not found to apply upgrade.");
                }
            }
        }
    }
}
