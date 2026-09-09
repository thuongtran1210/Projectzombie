using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Player;
using System.Linq;

namespace ProjectZombie.Features.Upgrades
{
    /// <summary>
    /// Thẻ Nâng Cấp Luyện Hóa & Gộp Thẻ Pháp Bảo (Relic Fusion Upgrade Data).
    /// Kiểm tra toàn bộ điều kiện thẻ thành phần (Vũ khí Base Max Level + Đủ danh sách Thẻ Bị Động),
    /// tự động thay thế/thức tỉnh Pháp bảo thành dạng Thần Binh Tối Thượng khi được người chơi chọn.
    /// </summary>
    [CreateAssetMenu(fileName = "NewFusionUpgradeData", menuName = "ProjectZombie/Upgrades/Relic Fusion Upgrade Data")]
    public class FusionUpgradeData : UpgradeData
    {
        [Header("Relic Fusion Configuration")]
        [Tooltip("Cấu hình công thức Luyện Hóa")]
        public RelicFusionRecipe recipe;

        private void Reset()
        {
            upgradeType = UpgradeType.RelicFusion;
            spawnWeight = 1.0f;
            maxLevel = 1;
        }

        public override bool IsAvailable(GameObject player)
        {
            if (player == null) return false;

            var weaponManager = player.GetComponent<WeaponManager>();
            var playerPassives = player.GetComponent<PlayerPassives>();

            if (weaponManager == null) return false;

            // 1. Kiểm tra điều kiện Vũ Khí Cơ Sở (Nếu công thức yêu cầu)
            if (!string.IsNullOrEmpty(recipe.requiredBaseWeaponId))
            {
                var baseWeapon = weaponManager.ActiveWeapons.FirstOrDefault(
                    w => string.Equals(w.weaponId, recipe.requiredBaseWeaponId, System.StringComparison.OrdinalIgnoreCase)
                );

                if (baseWeapon == null) return false;

                int targetLevel = recipe.requiredBaseWeaponLevel > 0 ? recipe.requiredBaseWeaponLevel : baseWeapon.MaxLevel;
                if (baseWeapon.WeaponLevel < targetLevel)
                {
                    return false;
                }
            }

            // 2. Kiểm tra điều kiện các Thẻ Bị Động / Phù Chú thành phần
            if (recipe.requiredPassiveIds != null && recipe.requiredPassiveIds.Count > 0)
            {
                if (playerPassives == null) return false;

                for (int i = 0; i < recipe.requiredPassiveIds.Count; i++)
                {
                    string reqPassive = recipe.requiredPassiveIds[i];
                    if (!string.IsNullOrEmpty(reqPassive) && !playerPassives.HasPassive(reqPassive))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override void ApplyUpgrade(GameObject player)
        {
            if (player == null) return;

            var weaponManager = player.GetComponent<WeaponManager>();
            if (weaponManager == null) return;

            Debug.Log($"<color=#FFD700><b>[Luyện Hóa Pháp Bảo]</b></color> Bắt đầu dung hợp thành công: {recipe.displayName} ({recipe.fusionId})");

            // 1. Nếu có vũ khí cũ cần thay thế
            if (!string.IsNullOrEmpty(recipe.requiredBaseWeaponId))
            {
                WeaponBase oldWeapon = weaponManager.GetWeaponById(recipe.requiredBaseWeaponId);
                if (oldWeapon != null)
                {
                    if (recipe.resultRelicPrefab != null)
                    {
                        // Gỡ bỏ vũ khí cũ và khởi tạo Thần Khí mới
                        weaponManager.RemoveWeapon(oldWeapon);
                        GameObject newRelicObj = Instantiate(recipe.resultRelicPrefab, weaponManager.transform);
                        WeaponBase newRelic = newRelicObj.GetComponent<WeaponBase>();
                        if (newRelic != null)
                        {
                            weaponManager.AddWeapon(newRelic);
                        }
                    }
                    else
                    {
                        // Fallback Siêu Cường: Thức tỉnh vũ khí hiện tại lên trạng thái Thần Uy Max Stat
                        var godTierModifier = new WeaponStatModifier
                        {
                            damageBonus = 50f,
                            projectileCountBonus = 3,
                            scaleBonus = 0.5f,
                            attackSpeedBonus = 0.35f,
                            critChanceBonus = 0.25f,
                            critDamageBonus = 0.5f
                        };
                        oldWeapon.ApplyStatModifier(godTierModifier);
                        oldWeapon.OnLevelUp(oldWeapon.WeaponLevel + 1, this);
                        weaponManager.NotifyWeaponsChanged();
                    }
                }
            }
            else if (recipe.resultRelicPrefab != null)
            {
                // Trường hợp Luyện Hóa tạo mới hoàn toàn một Pháp bảo thứ 2 hoặc Slot Pháp bảo đặc biệt
                GameObject newRelicObj = Instantiate(recipe.resultRelicPrefab, weaponManager.transform);
                WeaponBase newRelic = newRelicObj.GetComponent<WeaponBase>();
                if (newRelic != null)
                {
                    weaponManager.AddWeapon(newRelic);
                }
            }

            // 2. Ghi nhận vào PlayerPassives để tránh roll lặp lại
            var passives = player.GetComponent<PlayerPassives>();
            string recordKey = !string.IsNullOrEmpty(id) ? id : recipe.fusionId;
            if (passives != null && !string.IsNullOrEmpty(recordKey))
            {
                passives.AddPassive(recordKey, this);
            }
        }

        public override string GetCategoryDisplayName()
        {
            return "<color=#FFD700><b>[LUYỆN HÓA THẦN BINH]</b></color>";
        }

        public override string GetLevelDisplayName(GameObject player)
        {
            return "THẦN PHÁP";
        }

        public override float GetDynamicWeightMultiplier(GameObject player)
        {
            // Đạt độ ưu tiên tối cao (5.0x) khi người chơi đã thu thập đủ toàn bộ nguyên liệu
            return 5.0f;
        }
    }
}
