using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Upgrades.Weighting
{
    /// <summary>
    /// Tự động tăng +50% trọng số xuất hiện cho các thẻ nhánh tương thích với Đại Lõi Thần Thoại đang mang.
    /// </summary>
    public class ArchetypeSynergyWeighter : IUpgradeWeighter
    {
        private const float SYNERGY_MULTIPLIER = 1.50f; // +50%

        public float CalculateMultiplier(UpgradeData upgrade, PlayerContext context)
        {
            if (upgrade == null || context?.MythicManager == null) return 1.0f;

            var currentArchetype = context.MythicManager.CurrentArchetype;
            if (currentArchetype == MythicArchetype.None) return 1.0f;

            // Nếu là Thẻ Nhánh Độc Quyền (SynergyTraitUpgradeData) trùng với Archetype đang chọn
            if (upgrade is SynergyTraitUpgradeData trait && trait.requiredArchetype == currentArchetype)
            {
                return SYNERGY_MULTIPLIER;
            }

            return 1.0f;
        }
    }
}
