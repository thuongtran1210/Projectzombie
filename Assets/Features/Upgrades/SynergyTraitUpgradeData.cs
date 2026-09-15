using UnityEngine;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Upgrades
{
    /// <summary>
    /// Thẻ bài Bổ Trợ Nhánh Độc Quyền (Synergy Trait - Vàng / Bạc) gắn liền với một Lõi Thần Thoại.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSynergyTrait", menuName = "ProjectZombie/Upgrades/Synergy Trait Data")]
    public class SynergyTraitUpgradeData : UpgradeData
    {
        [Header("Synergy Requirement")]
        [Tooltip("Trường phái Thần Thoại bắt buộc phải có để thẻ này xuất hiện trong pool")]
        public MythicArchetype requiredArchetype = MythicArchetype.None;

        [Header("Stat Modifiers")]
        public PlayerStatModifier playerStatModifier;

        public SynergyTraitUpgradeData()
        {
            upgradeType = UpgradeType.SynergyTrait;
        }

        public override bool IsAvailable(PlayerContext context)
        {
            if (context?.MythicManager == null) return false;

            // Nếu thẻ yêu cầu Archetype cụ thể -> Player phải đang mang đúng Archetype đó
            if (requiredArchetype != MythicArchetype.None)
            {
                if (context.MythicManager.CurrentArchetype != requiredArchetype)
                {
                    return false;
                }
            }

            // Kiểm tra max level từ PlayerPassives
            if (maxLevel > 0 && context.Passives != null)
            {
                int currentLevel = context.Passives.GetUpgradeCount(id);
                if (currentLevel >= maxLevel) return false;
            }

            return true;
        }

        public override bool IsAvailable(GameObject player)
        {
            if (player == null) return false;
            return IsAvailable(PlayerContext.Create(player));
        }

        public override void ApplyUpgrade(PlayerContext context)
        {
            if (context == null) return;

            // 1. Áp dụng các chỉ số bổ trợ
            if (context.Stats != null)
            {
                if (playerStatModifier.maxHealthBonus > 0) context.Stats.AddMaxHealth(playerStatModifier.maxHealthBonus);
                if (playerStatModifier.moveSpeedBonus > 0) context.Stats.AddMoveSpeed(playerStatModifier.moveSpeedBonus);
                if (playerStatModifier.critChanceBonus > 0) context.Stats.AddCritChance(playerStatModifier.critChanceBonus);
                if (playerStatModifier.baseDamageBonus > 0) context.Stats.AddBaseDamage(playerStatModifier.baseDamageBonus);
                if (playerStatModifier.pickupRangeBonus > 0) context.Stats.AddPickupRange(playerStatModifier.pickupRangeBonus);
                if (playerStatModifier.expMultiplierBonus > 0) context.Stats.AddExpMultiplier(playerStatModifier.expMultiplierBonus);
                if (playerStatModifier.attackSpeedBonus > 0) context.Stats.AddAttackSpeed(playerStatModifier.attackSpeedBonus);
                if (playerStatModifier.dashCooldownReduction > 0) context.Stats.ReduceDashCooldown(playerStatModifier.dashCooldownReduction);
                if (playerStatModifier.dashSpeedBonus > 0) context.Stats.AddDashSpeedMultiplier(playerStatModifier.dashSpeedBonus);
            }

            // 2. Ghi nhận số tầng vào PlayerPassives
            if (context.Passives != null)
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
            return $"<color=#00E5FF><b>[THẦN BÍNH THỦẬT: {requiredArchetype}]</b></color>";
        }
    }
}
