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
            if (player == null) return false;

            var playerPassives = player.GetComponent<PlayerPassives>();
            if (playerPassives == null) return false;

            string key = !string.IsNullOrEmpty(id) ? id : upgradeName;
            bool alreadyOwned = playerPassives.HasPassive(key) || playerPassives.HasPassive(upgradeName);

            if (!alreadyOwned)
            {
                // Nếu chưa sở hữu, kiểm tra xem đã đầy 6 slot Bị động chưa
                if (playerPassives.IsFull())
                {
                    return false;
                }
            }
            else
            {
                // Nếu đã sở hữu, kiểm tra cấp độ tối đa
                if (maxLevel > 0)
                {
                    int currentCount = playerPassives.GetUpgradeCount(upgradeName);
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
                if (playerStatModifier.maxHealthBonus > 0f) playerStats.AddMaxHealth(playerStatModifier.maxHealthBonus);
                if (playerStatModifier.moveSpeedBonus > 0f) playerStats.AddMoveSpeed(playerStatModifier.moveSpeedBonus);
                if (playerStatModifier.critChanceBonus > 0f) playerStats.AddCritChance(playerStatModifier.critChanceBonus);
                if (playerStatModifier.baseDamageBonus > 0f) playerStats.AddBaseDamage(playerStatModifier.baseDamageBonus);
                if (playerStatModifier.pickupRangeBonus > 0f) playerStats.AddPickupRange(playerStatModifier.pickupRangeBonus);
                if (playerStatModifier.expMultiplierBonus > 0f) playerStats.AddExpMultiplier(playerStatModifier.expMultiplierBonus);
                if (playerStatModifier.attackSpeedBonus > 0f) playerStats.AddAttackSpeed(playerStatModifier.attackSpeedBonus);
                if (playerStatModifier.dashCooldownReduction > 0f) playerStats.ReduceDashCooldown(playerStatModifier.dashCooldownReduction);
                if (playerStatModifier.dashSpeedBonus > 0f) playerStats.AddDashSpeedMultiplier(playerStatModifier.dashSpeedBonus);
                if (playerStatModifier.areaScaleBonus > 0f) playerStats.AddAreaScale(playerStatModifier.areaScaleBonus);
                if (playerStatModifier.fireDamageBonus > 0f) playerStats.AddFireDamageBonus(playerStatModifier.fireDamageBonus);
            }

            var playerPassives = player.GetComponent<PlayerPassives>();
            if (playerPassives != null)
            {
                string key = !string.IsNullOrEmpty(id) ? id : upgradeName;
                playerPassives.AddPassive(key, this);
                playerPassives.IncrementUpgradeCount(upgradeName);
            }
        }

        public override string GetCategoryDisplayName()
        {
            return "<color=#1B4D7E><b>[BỔ TRỢ KHÍ VẬN]</b></color>";
        }

        public override string GetLevelDisplayName(GameObject player)
        {
            if (player == null) return string.Empty;
            var passives = player.GetComponent<PlayerPassives>();
            int count = passives != null ? passives.GetUpgradeCount(upgradeName) : 0;
            int nextLevel = count + 1;
            if (maxLevel > 0)
            {
                return $"Cấp {nextLevel}/{maxLevel}";
            }
            return $"Cấp {nextLevel}";
        }

        public override float GetDynamicWeightMultiplier(GameObject player)
        {
            if (player == null) return 1.0f;
            var passives = player.GetComponent<PlayerPassives>();
            string key = !string.IsNullOrEmpty(id) ? id : upgradeName;
            if (passives != null && (passives.HasPassive(key) || passives.HasPassive(upgradeName)))
            {
                return 2.0f; // Ưu tiên nâng max nhánh đang có
            }
            return 1.0f;
        }
    }
}
