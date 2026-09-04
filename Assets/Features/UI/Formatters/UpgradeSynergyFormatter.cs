using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.UI.Formatters
{
    /// <summary>
    /// Service chuyên trách tính toán và định dạng thông tin Duyên Phận (Synergy / Evolution Recipes) cho UI.
    /// Giúp tách biệt hoàn toàn nghiệp vụ công thức tiến hóa khỏi UpgradeUIPresenter.
    /// </summary>
    public static class UpgradeSynergyFormatter
    {
        public static void FormatSynergyInfo(
            ProjectZombie.Features.Upgrades.UpgradeData data,
            WeaponManager weaponManager,
            out Sprite icon,
            out string formattedText)
        {
            icon = null;
            formattedText = null;

            if (WeaponEvolutionManager.Instance == null || weaponManager == null || data == null) return;

            var playerPassives = weaponManager.GetComponent<PlayerPassives>();

            // 1. Trường hợp thẻ là Vũ Khí (WeaponUpgradeData / Base Weapon Unlock)
            if (data is ProjectZombie.Features.Upgrades.WeaponUpgradeData weaponData)
            {
                if (WeaponEvolutionManager.Instance.TryGetRecipeByWeaponId(weaponData.weaponId, out var recipe))
                {
                    bool hasPassive = playerPassives != null && playerPassives.HasPassive(recipe.requiredPassiveId);
                    if (hasPassive)
                    {
                        formattedText = $"<color=#007A4D><b>[Duyên Phận] Đã có {recipe.requiredPassiveId} (Sẵn Sàng)</b></color>";
                    }
                    else
                    {
                        formattedText = $"<color=#5C4033>Duyên Phận: Cần {recipe.requiredPassiveId} (Chưa có)</color>";
                    }
                }
            }
            // 2. Trường hợp thẻ là Thẻ Bị Động (Common / Passive Upgrade)
            else if (data is ProjectZombie.Features.Upgrades.CommonUpgradeData commonData)
            {
                var recipes = WeaponEvolutionManager.Instance.GetRecipesByPassiveId(commonData.id);
                if (recipes != null && recipes.Count > 0)
                {
                    List<string> weaponNames = new List<string>();
                    bool anyWeaponOwned = false;

                    foreach (var r in recipes)
                    {
                        bool hasWeapon = weaponManager.GetWeaponById(r.baseWeaponId) != null;
                        if (hasWeapon)
                        {
                            anyWeaponOwned = true;
                            weaponNames.Add($"<color=#007A4D><b>{r.baseWeaponId} (Đã có)</b></color>");
                        }
                        else
                        {
                            weaponNames.Add($"<color=#7A6855>{r.baseWeaponId}</color>");
                        }
                    }

                    if (anyWeaponOwned)
                    {
                        formattedText = $"<color=#FFD700>Hợp Thể:</color> {string.Join(", ", weaponNames)}";
                    }
                    else
                    {
                        formattedText = $"<color=#AAAAAA>Ghép Cùng:</color> {string.Join(", ", weaponNames)}";
                    }
                }
            }
        }
    }
}
