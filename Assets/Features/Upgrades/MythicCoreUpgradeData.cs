using UnityEngine;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Upgrades
{
    /// <summary>
    /// Thẻ bài Đại Lõi Thần Thoại (Prismatic Core).
    /// Thuần túy chứa Metadata cấu hình và Reference tới Prefab Runtime (không trực tiếp can thiệp Scene).
    /// </summary>
    [CreateAssetMenu(fileName = "NewMythicCore", menuName = "ProjectZombie/Upgrades/Mythic Core Data")]
    public class MythicCoreUpgradeData : UpgradeData
    {
        [Header("Mythic Identity")]
        public MythicArchetype archetype = MythicArchetype.None;
        public string mythicTitle;

        [Header("Runtime Prefab")]
        [Tooltip("Prefab chứa component kế thừa MythicCoreRuntime")]
        public MythicCoreRuntime runtimePrefab;

        public MythicCoreUpgradeData()
        {
            upgradeType = UpgradeType.MythicCore;
        }

        public override bool IsAvailable(PlayerContext context)
        {
            if (context?.MythicManager == null) return false;
            // Chỉ xuất hiện khi người chơi chưa chọn Lõi nào
            return context.MythicManager.CurrentArchetype == MythicArchetype.None;
        }

        public override bool IsAvailable(GameObject player)
        {
            if (player == null) return false;
            return IsAvailable(PlayerContext.Create(player));
        }

        public override void ApplyUpgrade(PlayerContext context)
        {
            if (context?.MythicManager != null)
            {
                context.MythicManager.EquipMythicCore(this);
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
            return "<color=#FFD700><b>[ĐẠI LÕI THẦN THOẠI]</b></color>";
        }
    }
}
