using UnityEngine;

namespace ProjectZombie.Features.Upgrades.Filters
{
    /// <summary>
    /// Bộ lọc kiểm tra tính khả dụng logic nội tại của thẻ (Level tối đa, đã sở hữu vũ khí gốc, v.v.).
    /// </summary>
    public class AvailabilityUpgradeFilter : IUpgradeFilter
    {
        public bool IsAllowed(UpgradeData upgrade, GameObject player)
        {
            if (upgrade == null) return false;
            return upgrade.IsAvailable(player);
        }
    }
}
