using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Upgrades.Filters
{
    /// <summary>
    /// Bộ lọc loại trừ các thẻ đang bị người chơi cấm (Ban) trong trận đấu.
    /// </summary>
    public class BannedUpgradeFilter : IUpgradeFilter
    {
        private readonly HashSet<UpgradeData> _bannedUpgrades;

        public BannedUpgradeFilter(HashSet<UpgradeData> bannedUpgrades)
        {
            _bannedUpgrades = bannedUpgrades;
        }

        public bool IsAllowed(UpgradeData upgrade, GameObject player)
        {
            if (upgrade == null) return false;
            return _bannedUpgrades == null || !_bannedUpgrades.Contains(upgrade);
        }
    }
}
