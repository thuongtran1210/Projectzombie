using UnityEngine;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Upgrades
{
    [CreateAssetMenu(fileName = "NewCommonUpgradeData", menuName = "ProjectZombie/Upgrades/Common Upgrade Data")]
    public class CommonUpgradeData : UpgradeData
    {
        [Header("Player Passives")]
        public PlayerStatModifier playerStatModifier;

        public override bool IsAvailable(GameObject player)
        {
            if (maxLevel > 0 && player != null)
            {
                var playerPassives = player.GetComponent<PlayerPassives>();
                if (playerPassives != null)
                {
                    int currentCount = playerPassives.GetUpgradeCount(this.upgradeName);
                    if (currentCount >= maxLevel)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override void ApplyUpgrade(GameObject player)
        {
            var playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.AddMaxHealth(playerStatModifier.maxHealthBonus);
                playerStats.AddMoveSpeed(playerStatModifier.moveSpeedBonus);
                playerStats.AddCritChance(playerStatModifier.critChanceBonus);
                playerStats.AddBaseDamage(playerStatModifier.baseDamageBonus);
                playerStats.AddPickupRange(playerStatModifier.pickupRangeBonus);
                playerStats.AddExpMultiplier(playerStatModifier.expMultiplierBonus);
            }

            var playerPassives = player.GetComponent<PlayerPassives>();
            if (playerPassives != null)
            {
                playerPassives.AddPassive(this.upgradeName, this); // Using upgradeName instead of file name
                playerPassives.IncrementUpgradeCount(this.upgradeName);
            }
        }
    }
}
