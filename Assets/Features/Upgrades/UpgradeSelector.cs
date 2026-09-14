using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Upgrades.Filters;
using ProjectZombie.Features.Weapons;

namespace ProjectZombie.Features.Upgrades
{
    /// <summary>
    /// Thuật toán pure C# tính toán trọng số và lựa chọn ngẫu nhiên các thẻ nâng cấp (UpgradeData).
    /// Tách rời hoàn toàn khỏi MonoBehaviour để hỗ trợ Unit Testing và tuân thủ SRP.
    /// </summary>
    public static class UpgradeSelector
    {
        /// <summary>
        /// Tính toán trọng số xuất hiện động (Dynamic Synergy Weight):
        /// - Đồ đang có trong ba lô / Thẻ Tiến Hóa: Dynamic multiplier
        /// - Cùng hệ hoặc Tương Sinh Ngũ Hành: Thêm +35% / +25%
        /// </summary>
        public static float CalculateEffectiveWeight(
            UpgradeData upgrade, 
            GameObject player, 
            HashSet<ElementType> activeElements)
        {
            if (upgrade == null) return 0f;

            float weight = Mathf.Max(1f, upgrade.spawnWeight);

            // 1. Phân loại trọng số động theo từng loại thẻ (Polymorphic Dynamic Multiplier)
            weight *= upgrade.GetDynamicWeightMultiplier(player);

            // 2. Cộng hưởng Ngũ Hành (Element Synergy Bonus qua ElementSynergyRules)
            if (upgrade.element != ElementType.None && activeElements != null && activeElements.Count > 0)
            {
                if (activeElements.Contains(upgrade.element))
                {
                    // Đồng Hệ (Cùng nguyên tố) -> +35%
                    weight *= 1.35f;
                }
                else
                {
                    // Tương Sinh (Thủy sinh Mộc, Mộc sinh Hỏa,...)
                    foreach (var activeElem in activeElements)
                    {
                        if (ElementSynergyRules.IsElementGenerative(activeElem, upgrade.element))
                        {
                            weight *= 1.25f;
                            break;
                        }
                    }
                }
            }

            return weight;
        }

        /// <summary>
        /// Thuật toán Weighted Random chọn ngẫu nhiên danh sách thẻ nâng cấp từ pool khả dụng.
        /// </summary>
        public static List<UpgradeData> SelectUpgrades(
            int count,
            GameObject player,
            IReadOnlyList<UpgradeData> availableUpgrades,
            IReadOnlyList<IUpgradeFilter> filters,
            IReadOnlyList<UpgradeData> fallbackRewards)
        {
            var weaponManager = player != null ? player.GetComponent<WeaponManager>() : null;

            // Thu thập các nguyên tố Ngũ Hành mà người chơi đang sở hữu
            var activeElements = new HashSet<ElementType>();
            if (weaponManager != null)
            {
                for (int i = 0; i < weaponManager.ActiveWeapons.Count; i++)
                {
                    var w = weaponManager.ActiveWeapons[i];
                    if (w != null && w.element != ElementType.None)
                    {
                        activeElements.Add(w.element);
                    }
                }
            }

            var validUpgrades = new List<UpgradeData>();
            var weights = new List<float>();
            float totalWeight = 0f;

            // 1. Lọc thẻ hợp lệ & tính trọng số động
            for (int i = 0; i < availableUpgrades.Count; i++)
            {
                var u = availableUpgrades[i];
                if (IsUpgradeAllowed(u, player, filters))
                {
                    float effectiveWeight = CalculateEffectiveWeight(u, player, activeElements);
                    validUpgrades.Add(u);
                    weights.Add(effectiveWeight);
                    totalWeight += effectiveWeight;
                }
            }

            var selectedUpgrades = new List<UpgradeData>();

            // 2. Thuật toán Weighted Random tiêu chuẩn
            while (selectedUpgrades.Count < count && validUpgrades.Count > 0 && totalWeight > 0f)
            {
                float randomValue = Random.Range(0f, totalWeight);
                float currentSum = 0f;

                for (int i = 0; i < validUpgrades.Count; i++)
                {
                    currentSum += weights[i];
                    if (currentSum >= randomValue || i == validUpgrades.Count - 1)
                    {
                        var chosen = validUpgrades[i];
                        selectedUpgrades.Add(chosen);
                        totalWeight -= weights[i];

                        validUpgrades.RemoveAt(i);
                        weights.RemoveAt(i);
                        break;
                    }
                }
            }

            // 3. Fallback Buffer (Bảo hiểm chống cạn pool khi Max Level toàn bộ)
            int fallbackIndex = 0;
            while (selectedUpgrades.Count < count && fallbackRewards != null && fallbackRewards.Count > 0)
            {
                var fallback = fallbackRewards[fallbackIndex % fallbackRewards.Count];
                if (!selectedUpgrades.Contains(fallback))
                {
                    selectedUpgrades.Add(fallback);
                }
                fallbackIndex++;
            }

            return selectedUpgrades;
        }

        private static bool IsUpgradeAllowed(UpgradeData upgrade, GameObject player, IReadOnlyList<IUpgradeFilter> filters)
        {
            if (upgrade == null) return false;
            if (filters == null) return true;

            for (int i = 0; i < filters.Count; i++)
            {
                if (!filters[i].IsAllowed(upgrade, player))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
