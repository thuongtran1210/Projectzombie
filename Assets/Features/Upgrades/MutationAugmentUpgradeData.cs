using System;
using UnityEngine;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Upgrades
{
    /// <summary>
    /// Thẻ bài Lõi Đột Biến (Mutation Augments) xuất hiện tại 3 mốc cao trào Lv.5, Lv.15, Lv.30.
    /// Mang lại hiệu ứng bẻ gãy quy tắc hoặc cường hóa giao tranh bùng nổ.
    /// </summary>
    [CreateAssetMenu(fileName = "NewMutationAugment", menuName = "ProjectZombie/Upgrades/Mutation Augment Data")]
    public class MutationAugmentUpgradeData : UpgradeData
    {
        [Header("Augment Identity")]
        public AugmentTier tier = AugmentTier.Silver;
        [Tooltip("Tag thuộc tính để kích hoạt Smart Synergy mở khóa thêm thẻ thường ở các level tiếp theo")]
        public string synergyTag;

        [Header("Stat Modifiers")]
        public PlayerStatModifier statModifier;

        [Header("Special Mechanic Prefab (Optional)")]
        [Tooltip("Prefab đính kèm runtime can thiệp cơ chế (nếu có)")]
        public GameObject mechanicRuntimePrefab;

        public MutationAugmentUpgradeData()
        {
            upgradeType = UpgradeType.BreakthroughUltimate;
        }

        public override bool IsAvailable(PlayerContext context)
        {
            if (context == null) return true;
            // Mỗi Lõi Đột Biến chỉ nhận tối đa 1 lần trong 1 ván đấu
            return maxLevel <= 1 || GetCurrentLevel(context) < maxLevel;
        }

        public override bool IsAvailable(GameObject player)
        {
            if (player == null) return true;
            return IsAvailable(PlayerContext.Create(player));
        }

        public override void ApplyUpgrade(PlayerContext context)
        {
            if (context?.PlayerStats != null)
            {
                context.PlayerStats.ApplyStatModifier(statModifier);
            }

            if (mechanicRuntimePrefab != null && context?.GameObject != null)
            {
                Instantiate(mechanicRuntimePrefab, context.GameObject.transform);
            }

            Debug.Log($"<color=#FFD700>[MutationAugment] Đã kích hoạt Lõi Đột Biến [{tier}]: {upgradeName} ({id})</color>");
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
            return tier switch
            {
                AugmentTier.Silver => "<color=#C0C0C0><b>[LÕI BẠC ĐỘT BIẾN]</b></color>",
                AugmentTier.Gold => "<color=#FFD700><b>[LÕI VÀNG ĐỘT BIẾN]</b></color>",
                AugmentTier.Prismatic => "<color=#00E5FF><b>[LÕI KIM CƯƠNG THẦN HÓA]</b></color>",
                _ => "<b>[LÕI ĐỘT BIẾN]</b>"
            };
        }
    }
}
