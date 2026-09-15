using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Upgrades.Filters;
using ProjectZombie.Features.Upgrades.Weighting;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Upgrades
{
    /// <summary>
    /// Thuật toán pure C# tính toán trọng số và lựa chọn ngẫu nhiên các thẻ nâng cấp theo tiến trình Logarithmic 4 giai đoạn:
    /// - Level 1: Đại Lõi Khởi Nguyên (Archetype Core Selection).
    /// - Mốc Đột Biến (Lv.5, Lv.15, Lv.30): Lõi Biến Dị Quy Tắc (Bạc / Vàng / Kim Cương).
    /// - Level Thường (Lv.2-4, Lv.6-14, Lv.16-29): Bồi đắp chỉ số nền tảng (Micro-Stats 1 dòng, Clean Pool).
    /// </summary>
    public static class UpgradeSelector
    {
        // Reusable Buffers để triệt tiêu 100% GC Allocations trong Gacha selection loop
        private static readonly List<UpgradeData> _validBuffer = new List<UpgradeData>(64);
        private static readonly List<float> _weightsBuffer = new List<float>(64);

        #region 1. Tuyển Chọn Đại Lõi Khởi Nguyên (Level 1)
        /// <summary>
        /// Tuyển chọn 3 (hoặc 5) Đại Lõi Khởi Nguyên tại Level 1.
        /// </summary>
        public static List<UpgradeData> SelectArchetypeCores(
            int count,
            IReadOnlyList<UpgradeData> allUpgrades)
        {
            var cores = allUpgrades.OfType<MythicCoreUpgradeData>().Cast<UpgradeData>().ToList();
            if (cores.Count == 0) return new List<UpgradeData>();

            // Shuffle ngẫu nhiên
            var result = cores.OrderBy(_ => Random.value).Take(Mathf.Min(count, cores.Count)).ToList();
            return result;
        }
        #endregion

        #region 2. Tuyển Chọn Lõi Đột Biến (Mốc Lv.5, Lv.15, Lv.30)
        /// <summary>
        /// Tuyển chọn 3 Lõi Đột Biến theo Phẩm cấp (Bạc / Vàng / Kim Cương) tại các mốc Lv.5, 15, 30.
        /// </summary>
        public static List<UpgradeData> SelectMutationAugments(
            int count,
            int playerLevel,
            PlayerContext context,
            IReadOnlyList<UpgradeData> allUpgrades)
        {
            _validBuffer.Clear();

            // Xác định dải phẩm cấp theo mốc level
            List<AugmentTier> allowedTiers = new List<AugmentTier>();
            if (playerLevel <= 5)
            {
                allowedTiers.Add(AugmentTier.Silver);
                allowedTiers.Add(AugmentTier.Gold);
            }
            else if (playerLevel <= 15)
            {
                allowedTiers.Add(AugmentTier.Gold);
                allowedTiers.Add(AugmentTier.Prismatic);
            }
            else // Lv.30+
            {
                allowedTiers.Add(AugmentTier.Prismatic);
            }

            // Lọc các Lõi Đột Biến hợp lệ
            for (int i = 0; i < allUpgrades.Count; i++)
            {
                var u = allUpgrades[i];
                if (u is MutationAugmentUpgradeData mut && allowedTiers.Contains(mut.tier))
                {
                    if (mut.IsAvailable(context))
                    {
                        _validBuffer.Add(mut);
                    }
                }
            }

            // Nếu thiếu thẻ MutationAugment, fallback sang các thẻ Breakthrough hoặc SynergyTrait
            if (_validBuffer.Count < count)
            {
                foreach (var u in allUpgrades)
                {
                    if ((u is SynergyTraitUpgradeData || u.upgradeType == UpgradeType.BreakthroughUltimate) && !_validBuffer.Contains(u))
                    {
                        if (u.IsAvailable(context)) _validBuffer.Add(u);
                    }
                }
            }

            return _validBuffer.OrderBy(_ => Random.value).Take(Mathf.Min(count, _validBuffer.Count)).ToList();
        }
        #endregion

        #region 3. Tuyển Chọn Thẻ Chỉ Số Nền Tảng (Level Thường - Clean Pool)
        /// <summary>
        /// Tuyển chọn 3 thẻ chỉ số thô (Micro-Stats 1 dòng) tại các level thường theo cơ chế Kho Thẻ Sạch (Clean Pool).
        /// </summary>
        public static List<UpgradeData> SelectMicroStats(
            int count,
            PlayerContext context,
            IReadOnlyList<UpgradeData> allUpgrades,
            IReadOnlyList<UpgradeData> fallbackRewards)
        {
            _validBuffer.Clear();
            _weightsBuffer.Clear();
            float totalWeight = 0f;

            var activeArchetype = context?.MythicManager != null ? context.MythicManager.CurrentArchetype : MythicArchetype.None;

            for (int i = 0; i < allUpgrades.Count; i++)
            {
                var u = allUpgrades[i];
                if (u == null) continue;

                // 1. Tuyệt đối không ra Đại Lõi hay Lõi Đột Biến ở Level thường
                if (u is MythicCoreUpgradeData || u is MutationAugmentUpgradeData) continue;

                // 2. Cơ chế Clean Pool: Lọc bỏ thẻ của Archetype khác
                if (u is StatMicroUpgradeData micro)
                {
                    if (micro.preferredArchetype != MythicArchetype.None && activeArchetype != MythicArchetype.None && micro.preferredArchetype != activeArchetype)
                    {
                        continue;
                    }
                }
                else if (u is SynergyTraitUpgradeData trait)
                {
                    if (trait.requiredArchetype != MythicArchetype.None && activeArchetype != MythicArchetype.None && trait.requiredArchetype != activeArchetype)
                    {
                        continue;
                    }
                }

                if (u.IsAvailable(context))
                {
                    float weight = Mathf.Max(1f, u.spawnWeight);
                    _validBuffer.Add(u);
                    _weightsBuffer.Add(weight);
                    totalWeight += weight;
                }
            }

            var selected = new List<UpgradeData>(count);

            // Weighted Random
            while (selected.Count < count && _validBuffer.Count > 0 && totalWeight > 0f)
            {
                float rnd = Random.Range(0f, totalWeight);
                float cur = 0f;
                for (int i = 0; i < _validBuffer.Count; i++)
                {
                    cur += _weightsBuffer[i];
                    if (cur >= rnd || i == _validBuffer.Count - 1)
                    {
                        var chosen = _validBuffer[i];
                        selected.Add(chosen);
                        totalWeight -= _weightsBuffer[i];
                        _validBuffer.RemoveAt(i);
                        _weightsBuffer.RemoveAt(i);
                        break;
                    }
                }
            }

            // Fallback nếu cạn pool
            int fbIdx = 0;
            while (selected.Count < count && fallbackRewards != null && fallbackRewards.Count > 0)
            {
                var fb = fallbackRewards[fbIdx % fallbackRewards.Count];
                if (!selected.Contains(fb)) selected.Add(fb);
                fbIdx++;
            }

            return selected;
        }
        #endregion

        #region 4. Phương Thức Điều Phối Chung (General Progression Selector)
        /// <summary>
        /// Thuật toán chính tự động nhận diện cấp độ và điều phối sang đúng phương thức tuyển chọn.
        /// </summary>
        public static List<UpgradeData> SelectUpgradesByProgression(
            int count,
            int playerLevel,
            PlayerContext context,
            IReadOnlyList<UpgradeData> allUpgrades,
            IReadOnlyList<UpgradeData> fallbackRewards)
        {
            // Mốc 1: Khởi đầu Level 1 ➔ Chọn Đại Lõi Khởi Nguyên
            if (playerLevel <= 1 || (context?.MythicManager != null && context.MythicManager.CurrentArchetype == MythicArchetype.None))
            {
                return SelectArchetypeCores(count, allUpgrades);
            }

            // Mốc Đột Biến: Lv.5, Lv.15, Lv.30 ➔ Chọn Lõi Đột Biến Bạc/Vàng/Kim Cương
            if (playerLevel == 5 || playerLevel == 15 || playerLevel == 30)
            {
                return SelectMutationAugments(count, playerLevel, context, allUpgrades);
            }

            // Level Thường: Lv.2-4, 6-14, 16-29 ➔ Bốc Thẻ Chỉ Số Nền Tảng (Clean Pool)
            return SelectMicroStats(count, context, allUpgrades, fallbackRewards);
        }

        /// <summary>
        /// Overload tương thích ngược.
        /// </summary>
        public static List<UpgradeData> SelectUpgrades(
            int count,
            PlayerContext context,
            IReadOnlyList<UpgradeData> availableUpgrades,
            IReadOnlyList<IUpgradeFilter> filters,
            UpgradeWeightPipeline weightPipeline,
            IReadOnlyList<UpgradeData> fallbackRewards)
        {
            int currentLvl = context?.Experience != null ? context.Experience.CurrentLevel : 2;
            return SelectUpgradesByProgression(count, currentLvl, context, availableUpgrades, fallbackRewards);
        }
        #endregion
    }
}
