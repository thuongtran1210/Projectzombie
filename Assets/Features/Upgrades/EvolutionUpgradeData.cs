using UnityEngine;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Player;
using System.Linq;

namespace ProjectZombie.Features.Upgrades
{
    [CreateAssetMenu(fileName = "NewEvolutionUpgradeData", menuName = "ProjectZombie/Upgrades/Evolution Upgrade Data")]
    public class EvolutionUpgradeData : UpgradeData
    {
        [Header("Evolution Settings")]
        [Tooltip("ID vũ khí gốc cần để tiến hóa")]
        public string weaponId;

        [Tooltip("Cấp độ vũ khí hiện tại yêu cầu để thẻ này xuất hiện (thường là cấp Max)")]
        public int requiredCurrentLevel = 6;

        [Tooltip("ID của Vật phẩm Bị động (Passive) yêu cầu để tiến hóa. Bỏ trống nếu không yêu cầu.")]
        public string requiredPassiveId;

        [Tooltip("Prefab vũ khí Tiến hóa sẽ thay thế vũ khí cũ")]
        public GameObject weaponPrefab;

        public override bool IsAvailable(GameObject player)
        {
            var weaponManager = player.GetComponent<WeaponManager>();
            var playerPassives = player.GetComponent<PlayerPassives>();

            if (weaponManager == null) return false;

            var baseWeapon = weaponManager.ActiveWeapons.FirstOrDefault(w => w.weaponId == weaponId);
            bool hasWeapon = baseWeapon != null;

            if (hasWeapon && baseWeapon.WeaponLevel >= requiredCurrentLevel)
            {
                if (!string.IsNullOrEmpty(requiredPassiveId))
                {
                    // Check if player has the required passive
                    if (playerPassives != null && !playerPassives.HasPassive(requiredPassiveId))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public override void ApplyUpgrade(GameObject player)
        {
            var weaponManager = player.GetComponent<WeaponManager>();
            if (weaponManager == null) return;

            WeaponBase oldWeapon = weaponManager.GetWeaponById(weaponId);
            if (oldWeapon != null)
            {
                // Gọi hàm xóa vũ khí an toàn
                weaponManager.RemoveWeapon(oldWeapon);

                // Tạo vũ khí tiến hóa mới
                if (weaponPrefab != null)
                {
                    GameObject weaponObj = Instantiate(weaponPrefab, weaponManager.transform);
                    WeaponBase newWeapon = weaponObj.GetComponent<WeaponBase>();
                    if (newWeapon != null)
                    {
                        weaponManager.AddWeapon(newWeapon);
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[EvolutionUpgradeData] Old weapon with ID {weaponId} not found for evolution.");
            }
        }
    }
}
