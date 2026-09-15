using UnityEngine;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Upgrades.Filters
{
    /// <summary>
    /// Bộ lọc loại trừ xung đột Lõi Thần Thoại:
    /// 1. Nếu đã chọn Đại Lõi -> Không cho ra thêm bất kỳ Đại Lõi Kim Cương nào khác.
    /// 2. Thẻ Nhánh Độc Quyền (SynergyTrait) chỉ xuất hiện khi Player đang mang đúng Đại Lõi tương ứng.
    /// </summary>
    public class ArchetypeExclusionFilter : IUpgradeFilter
    {
        public bool IsAllowed(UpgradeData upgrade, GameObject player)
        {
            if (upgrade == null) return false;
            if (player == null) return true;

            var mythicManager = player.GetComponent<PlayerMythicManager>();
            var currentArchetype = mythicManager != null ? mythicManager.CurrentArchetype : MythicArchetype.None;

            // 1. Kiểm tra Đại Lõi Kim Cương
            if (upgrade is MythicCoreUpgradeData)
            {
                // Chỉ cho phép xuất hiện khi chưa chọn Lõi nào
                return currentArchetype == MythicArchetype.None;
            }

            // 2. Kiểm tra Thẻ Nhánh Độc Quyền (SynergyTrait)
            if (upgrade is SynergyTraitUpgradeData trait)
            {
                if (trait.requiredArchetype != MythicArchetype.None)
                {
                    // Chỉ cho phép xuất hiện nếu trùng khớp với Lõi đang mang
                    return currentArchetype == trait.requiredArchetype;
                }
            }

            return true;
        }

        public bool IsAllowed(UpgradeData upgrade, PlayerContext context)
        {
            if (upgrade == null) return false;
            if (context == null) return true;

            var currentArchetype = context.MythicManager != null ? context.MythicManager.CurrentArchetype : MythicArchetype.None;

            if (upgrade is MythicCoreUpgradeData)
            {
                return currentArchetype == MythicArchetype.None;
            }

            if (upgrade is SynergyTraitUpgradeData trait)
            {
                if (trait.requiredArchetype != MythicArchetype.None)
                {
                    return currentArchetype == trait.requiredArchetype;
                }
            }

            return true;
        }
    }
}
