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

        [Tooltip("Cấp độ vũ khí hiện tại yêu cầu để thẻ này xuất hiện (mặc định Max Level = 5 trong GDD v4.0)")]
        public int requiredCurrentLevel = 5;

        [Tooltip("ID của Vật phẩm Bị động (Passive) yêu cầu để tiến hóa. Bỏ trống nếu không yêu cầu.")]
        public string requiredPassiveId;

        [Tooltip("Prefab vũ khí Tiến hóa sẽ thay thế vũ khí cũ")]
        public GameObject weaponPrefab;

        public override bool IsAvailable(GameObject player)
        {
            var weaponManager = player.GetComponent<WeaponManager>();
            var playerPassives = player.GetComponent<PlayerPassives>();

            if (weaponManager == null) return false;

            var baseWeapon = weaponManager.ActiveWeapons.FirstOrDefault(w => string.Equals(w.weaponId, weaponId, System.StringComparison.OrdinalIgnoreCase));
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
                // Tạo vũ khí tiến hóa mới nếu có Prefab riêng
                if (weaponPrefab != null)
                {
                    weaponManager.RemoveWeapon(oldWeapon);
                    GameObject weaponObj = Instantiate(weaponPrefab, weaponManager.transform);
                    WeaponBase newWeapon = weaponObj.GetComponent<WeaponBase>();
                    if (newWeapon != null)
                    {
                        weaponManager.AddWeapon(newWeapon);
                    }
                }
                else
                {
                    // Fallback an toàn: Nếu chưa gắn Prefab Tiến Hóa riêng, nâng cấp vũ khí hiện tại lên trạng thái Siêu Cường
                    Debug.Log($"<color=#FFD700>[EvolutionUpgradeData]</color> Kích hoạt Tiến Hóa Siêu Cường cho vũ khí: {weaponId} ({oldWeapon.displayName})");
                    var superMod = new WeaponStatModifier
                    {
                        damageBonus = 25f,
                        projectileCountBonus = 2,
                        scaleBonus = 0.3f,
                        attackSpeedBonus = 0.25f
                    };
                    oldWeapon.ApplyStatModifier(superMod);
                    oldWeapon.OnLevelUp(6, this);
                    weaponManager.NotifyWeaponsChanged();
                }
            }
            else
            {
                Debug.LogWarning($"[EvolutionUpgradeData] Old weapon with ID {weaponId} not found for evolution.");
            }
        }
    }
}
