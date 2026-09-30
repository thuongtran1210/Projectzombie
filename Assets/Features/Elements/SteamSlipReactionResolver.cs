using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Elements
{
    /// <summary>Defines the ordered element pair that triggers the Bốc Hơi pilot.</summary>
    internal static class SteamSlipReactionResolver
    {
        public static bool ShouldTrigger(ElementType previous, ElementType incoming) =>
            previous == ElementType.Thuy && incoming == ElementType.Hoa;

        public static bool ShouldTriggerNaturalMatch(ElementType target, ElementType incoming) =>
            target == ElementType.Hoa && incoming == ElementType.Thuy;
    }
}
