using System;
using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades
{
    /// <summary>
    /// Thẻ Nâng Cấp Level Thường (Micro-Cards): Bồi đắp chỉ số nền tảng (SMCK, Tốc đánh, Máu, Giáp...).
    /// Thiết kế tối giản 1 dòng tóm tắt (< 1s đọc), xuất hiện tại Lv.2-4, Lv.6-14, Lv.16-29 thay thế Shop đồ.
    /// </summary>
    [CreateAssetMenu(fileName = "NewStatMicroUpgrade", menuName = "ProjectZombie/Upgrades/Stat Micro Upgrade Data")]
    public class StatMicroUpgradeData : UpgradeData
    {
        [Header("Micro-Card Presentation")]
        [Tooltip("Dòng mô tả chỉ số siêu ngắn 1 dòng (Ví dụ: +15% Tốc Đánh, +120 Máu Tối Đa)")]
        public string oneLineSummary;

        [Header("Clean Pool Filter")]
        [Tooltip("Ưu tiên xuất hiện cho Archetype cụ thể (None = dùng chung cho mọi Archetype)")]
        public MythicArchetype preferredArchetype = MythicArchetype.None;
        [Tooltip("Tag thuộc tính để kích hoạt Smart Synergy khi chọn Lõi Đột Biến tương ứng")]
        public string synergyTag;

        [Header("Stat Modifier")]
        public PlayerStatModifier statModifier;

        public StatMicroUpgradeData()
        {
            upgradeType = UpgradeType.CommonUpgrade;
            maxLevel = 5;
            spawnWeight = 60f;
        }

        public override bool IsAvailable(PlayerContext context)
        {
            if (context == null) return true;
            int currentCount = context.Passives != null ? context.Passives.GetUpgradeCount(upgradeName) : 0;
            return maxLevel <= 0 || currentCount < maxLevel;
        }

        public override bool IsAvailable(GameObject player)
        {
            if (player == null) return true;
            return IsAvailable(PlayerContext.Create(player));
        }

        public override void ApplyUpgrade(PlayerContext context)
        {
            if (context?.Stats != null)
            {
                context.Stats.ApplyStatModifier(statModifier);
            }

            if (context?.Passives != null)
            {
                string key = !string.IsNullOrEmpty(id) ? id : upgradeName;
                context.Passives.AddPassive(key, this);
                context.Passives.IncrementUpgradeCount(upgradeName);
            }
        }

        public override void ApplyUpgrade(GameObject player)
        {
            if (player != null)
            {
                ApplyUpgrade(PlayerContext.Create(player));
            }
        }

        public override string GetCategoryDisplayName()
        {
            return "<color=#00FF88><b>[CHỈ SỐ NỀN TẢNG]</b></color>";
        }
    }
}
