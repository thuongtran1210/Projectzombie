using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Upgrades.Filters;
using ProjectZombie.Features.Upgrades.Weighting;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Weapons;

namespace ProjectZombie.Features.Upgrades
{
    /// <summary>
    /// Thuật toán pure C# tính toán trọng số và lựa chọn ngẫu nhiên các thẻ nâng cấp (UpgradeData).
    /// Tách rời hoàn toàn khỏi MonoBehaviour, tích hợp UpgradeWeightPipeline và tái sử dụng Static Buffers (0 GC).
    /// </summary>
    public static class UpgradeSelector
    {
        // Reusable Buffers để triệt tiêu 100% GC Allocations trong Gacha selection loop
        private static readonly List<UpgradeData> _validBuffer = new List<UpgradeData>(64);
        private static readonly List<float> _weightsBuffer = new List<float>(64);

        /// <summary>
        /// Thuật toán Weighted Random chọn ngẫu nhiên danh sách thẻ nâng cấp qua PlayerContext & Pipeline.
        /// </summary>
        public static List<UpgradeData> SelectUpgrades(
            int count,
            PlayerContext context,
            IReadOnlyList<UpgradeData> availableUpgrades,
            IReadOnlyList<IUpgradeFilter> filters,
            UpgradeWeightPipeline weightPipeline,
            IReadOnlyList<UpgradeData> fallbackRewards)
        {
            _validBuffer.Clear();
            _weightsBuffer.Clear();
            float totalWeight = 0f;

            var playerGo = context?.GameObject;

            // 1. Lọc thẻ hợp lệ & tính trọng số qua Pipeline
            for (int i = 0; i < availableUpgrades.Count; i++)
            {
                var u = availableUpgrades[i];
                if (IsUpgradeAllowed(u, playerGo, filters))
                {
                    float effectiveWeight = weightPipeline != null && context != null
                        ? weightPipeline.CalculateFinalWeight(u, context)
                        : Mathf.Max(0.1f, u.spawnWeight);

                    _validBuffer.Add(u);
                    _weightsBuffer.Add(effectiveWeight);
                    totalWeight += effectiveWeight;
                }
            }

            Debug.Log($"<color=#FFFF00>[DIAG_UPGRADE_SELECTOR]</color> SelectUpgrades(count={count}) - availableInput: {availableUpgrades.Count}, validPassed: {_validBuffer.Count}, totalWeight: {totalWeight}");

            var selectedUpgrades = new List<UpgradeData>(count);

            // 2.1. Bảo hiểm nhánh Lõi (Guaranteed Synergy Slot): Luôn ưu tiên xuất hiện ít nhất 1 Thần Binh Thuật nếu đang mang Lõi
            var activeArchetype = context?.MythicManager != null ? context.MythicManager.CurrentArchetype : MythicArchetype.None;
            if (activeArchetype != MythicArchetype.None && selectedUpgrades.Count < count)
            {
                // Tìm tất cả các thẻ Synergy Trait hợp lệ trong buffer
                List<int> synergyIndices = new List<int>();
                float synergyWeightSum = 0f;

                for (int i = 0; i < _validBuffer.Count; i++)
                {
                    if (_validBuffer[i] is SynergyTraitUpgradeData trait && trait.requiredArchetype == activeArchetype)
                    {
                        synergyIndices.Add(i);
                        synergyWeightSum += _weightsBuffer[i];
                    }
                }

                if (synergyIndices.Count > 0 && synergyWeightSum > 0f)
                {
                    float rnd = Random.Range(0f, synergyWeightSum);
                    float cur = 0f;
                    for (int s = 0; s < synergyIndices.Count; s++)
                    {
                        int targetIdx = synergyIndices[s];
                        cur += _weightsBuffer[targetIdx];
                        if (cur >= rnd || s == synergyIndices.Count - 1)
                        {
                            var chosenSynergy = _validBuffer[targetIdx];
                            selectedUpgrades.Add(chosenSynergy);
                            totalWeight -= _weightsBuffer[targetIdx];

                            _validBuffer.RemoveAt(targetIdx);
                            _weightsBuffer.RemoveAt(targetIdx);
                            break;
                        }
                    }
                }
            }

            // 2.2. Thuật toán Weighted Random tiêu chuẩn cho các slot còn lại
            while (selectedUpgrades.Count < count && _validBuffer.Count > 0 && totalWeight > 0f)
            {
                float randomValue = Random.Range(0f, totalWeight);
                float currentSum = 0f;

                for (int i = 0; i < _validBuffer.Count; i++)
                {
                    currentSum += _weightsBuffer[i];
                    if (currentSum >= randomValue || i == _validBuffer.Count - 1)
                    {
                        var chosen = _validBuffer[i];
                        selectedUpgrades.Add(chosen);
                        totalWeight -= _weightsBuffer[i];

                        _validBuffer.RemoveAt(i);
                        _weightsBuffer.RemoveAt(i);
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

            Debug.Log($"<color=#FFFF00>[DIAG_UPGRADE_SELECTOR]</color> SelectUpgrades trả về: {selectedUpgrades.Count} thẻ");
            return selectedUpgrades;
        }

        /// <summary>
        /// Overload tương thích ngược cho GameObject player.
        /// </summary>
        public static List<UpgradeData> SelectUpgrades(
            int count,
            GameObject player,
            IReadOnlyList<UpgradeData> availableUpgrades,
            IReadOnlyList<IUpgradeFilter> filters,
            IReadOnlyList<UpgradeData> fallbackRewards)
        {
            var context = player != null ? new PlayerContext(player) : null;
            var pipeline = new UpgradeWeightPipeline();
            pipeline.RegisterWeighter(new ArchetypeSynergyWeighter());
            pipeline.RegisterWeighter(new ElementSynergyWeighter());
            pipeline.RegisterWeighter(new OwnedWeaponPriorityWeighter());

            return SelectUpgrades(count, context, availableUpgrades, filters, pipeline, fallbackRewards);
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
