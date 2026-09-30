using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Elements
{
    public enum ElementReactionType
    {
        None,
        BocHoi,
        HuoHoan,
        ToaiGiap,
        SinhTruong,
        SaLay
    }

    /// <summary>Maps ordered element pairs and natural affinities to reaction identities.</summary>
    internal static class SteamSlipReactionResolver
    {
        public static ElementReactionType ResolveOrderedPair(ElementType previous, ElementType incoming)
        {
            if (previous == ElementType.Thuy && incoming == ElementType.Hoa)
                return ElementReactionType.BocHoi;
            if (previous == ElementType.Tho && incoming == ElementType.Kim)
                return ElementReactionType.ToaiGiap;
            if (previous == ElementType.Thuy && incoming == ElementType.Moc)
                return ElementReactionType.SinhTruong;
            if (previous == ElementType.Tho && incoming == ElementType.Thuy)
                return ElementReactionType.SaLay;

            return ElementReactionType.None;
        }

        public static ElementReactionType ResolveNaturalAffinity(ElementType target, ElementType incoming)
        {
            if (target != ElementType.Hoa)
                return ElementReactionType.None;

            if (incoming == ElementType.Moc)
                return ElementReactionType.HuoHoan;
            if (incoming == ElementType.Thuy)
                return ElementReactionType.BocHoi;

            return ElementReactionType.None;
        }
    }
}
