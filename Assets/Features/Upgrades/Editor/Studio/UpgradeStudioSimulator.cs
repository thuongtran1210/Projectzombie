#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Upgrades;
using ProjectZombie.Features.Upgrades.Filters;
using ProjectZombie.Features.Upgrades.Weighting;

namespace ProjectZombie.Features.Upgrades.Editor.Studio
{
    public struct SimulationResult
    {
        public int TotalRolls;
        public int ChoiceCount;
        public MythicArchetype ActiveArchetype;
        public Dictionary<UpgradeData, int> PickCounts;
        public Dictionary<UpgradeType, int> CategoryCounts;
        public int SynergyGuaranteedCount;
    }

    /// <summary>
    /// Engine giả lập cơ chế bốc thẻ Roguelite Gacha (Monte Carlo Simulation) phục vụ cân bằng game.
    /// </summary>
    public static class UpgradeStudioSimulator
    {
        public static SimulationResult RunSimulation(
            IReadOnlyList<UpgradeData> allUpgrades,
            MythicArchetype archetype,
            int rollCount,
            int choicesPerRoll = 3)
        {
            var result = new SimulationResult
            {
                TotalRolls = rollCount,
                ChoiceCount = choicesPerRoll,
                ActiveArchetype = archetype,
                PickCounts = new Dictionary<UpgradeData, int>(),
                CategoryCounts = new Dictionary<UpgradeType, int>(),
                SynergyGuaranteedCount = 0
            };

            if (allUpgrades == null || allUpgrades.Count == 0 || rollCount <= 0)
            {
                return result;
            }

            // Khởi tạo bảng đếm
            foreach (var u in allUpgrades)
            {
                if (u != null) result.PickCounts[u] = 0;
            }

            // Lọc danh sách thẻ khả dụng dựa theo Archetype
            var validPool = new List<UpgradeData>();
            var weights = new List<float>();

            var synergyWeighter = new ArchetypeSynergyWeighter();

            for (int r = 0; r < rollCount; r++)
            {
                validPool.Clear();
                weights.Clear();
                float totalWeight = 0f;

                for (int i = 0; i < allUpgrades.Count; i++)
                {
                    var u = allUpgrades[i];
                    if (u == null) continue;

                    // Không bốc Đại Lõi trong các lượt quay nâng cấp thường
                    if (u.upgradeType == UpgradeType.MythicCore) continue;

                    // Nếu là Thần Binh Thuật, chỉ nhận thẻ của Archetype đang chọn
                    if (u is SynergyTraitUpgradeData trait)
                    {
                        if (archetype == MythicArchetype.None || trait.requiredArchetype != archetype)
                        {
                            continue;
                        }
                    }

                    float weight = Mathf.Max(0.1f, u.spawnWeight);
                    if (archetype != MythicArchetype.None && u is SynergyTraitUpgradeData)
                    {
                        weight *= 3.0f; // Hệ số Synergy
                    }

                    validPool.Add(u);
                    weights.Add(weight);
                    totalWeight += weight;
                }

                if (validPool.Count == 0 || totalWeight <= 0f) continue;

                var selectedThisRoll = new List<UpgradeData>(choicesPerRoll);

                // 1. Bảo hiểm Thần Binh Thuật (Guaranteed Synergy Slot)
                if (archetype != MythicArchetype.None && selectedThisRoll.Count < choicesPerRoll)
                {
                    var synergyIndices = new List<int>();
                    float synWeightSum = 0f;

                    for (int i = 0; i < validPool.Count; i++)
                    {
                        if (validPool[i] is SynergyTraitUpgradeData t && t.requiredArchetype == archetype)
                        {
                            synergyIndices.Add(i);
                            synWeightSum += weights[i];
                        }
                    }

                    if (synergyIndices.Count > 0 && synWeightSum > 0f)
                    {
                        float rnd = Random.Range(0f, synWeightSum);
                        float cur = 0f;
                        for (int s = 0; s < synergyIndices.Count; s++)
                        {
                            int targetIdx = synergyIndices[s];
                            cur += weights[targetIdx];
                            if (cur >= rnd || s == synergyIndices.Count - 1)
                            {
                                var chosen = validPool[targetIdx];
                                selectedThisRoll.Add(chosen);
                                totalWeight -= weights[targetIdx];
                                validPool.RemoveAt(targetIdx);
                                weights.RemoveAt(targetIdx);
                                result.SynergyGuaranteedCount++;
                                break;
                            }
                        }
                    }
                }

                // 2. Weighted Random cho các slot còn lại
                while (selectedThisRoll.Count < choicesPerRoll && validPool.Count > 0 && totalWeight > 0f)
                {
                    float rnd = Random.Range(0f, totalWeight);
                    float cur = 0f;

                    for (int i = 0; i < validPool.Count; i++)
                    {
                        cur += weights[i];
                        if (cur >= rnd || i == validPool.Count - 1)
                        {
                            var chosen = validPool[i];
                            selectedThisRoll.Add(chosen);
                            totalWeight -= weights[i];
                            validPool.RemoveAt(i);
                            weights.RemoveAt(i);
                            break;
                        }
                    }
                }

                // Ghi nhận kết quả
                foreach (var picked in selectedThisRoll)
                {
                    if (result.PickCounts.ContainsKey(picked))
                    {
                        result.PickCounts[picked]++;
                    }

                    if (!result.CategoryCounts.ContainsKey(picked.upgradeType))
                    {
                        result.CategoryCounts[picked.upgradeType] = 0;
                    }
                    result.CategoryCounts[picked.upgradeType]++;
                }
            }

            return result;
        }
    }
}
#endif
