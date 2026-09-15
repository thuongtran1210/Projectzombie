using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Upgrades.Weighting
{
    /// <summary>
    /// Tự động tăng +200% (3.0x) trọng số xuất hiện cho các thẻ nhánh tương thích với Đại Lõi Thần Thoại đang mang.
    /// </summary>
    public class ArchetypeSynergyWeighter : IUpgradeWeighter
    {
        private const float SYNERGY_MULTIPLIER = 3.0f; // +200% (3.0x)

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
