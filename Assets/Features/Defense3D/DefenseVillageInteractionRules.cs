namespace ProjectZombie.Features.Defense3D
{
    public enum DefenseVillageAction { None, Recruit, Build, Upgrade, Maximum }

    /// <summary>Source MobileContextAction priority and affordability without UI or scene dependencies.</summary>
    public readonly struct DefenseVillageInteraction
    {
        public readonly DefenseVillageAction Action;
        public readonly bool Enabled;
        public readonly int Cost, Shortage;
        public DefenseVillageInteraction(DefenseVillageAction action, bool enabled, int cost = 0, int shortage = 0)
        { Action = action; Enabled = enabled; Cost = cost; Shortage = shortage; }
    }

    public static class DefenseVillageInteractionRules
    {
        public static bool InZone(DefensePoint3 point, DefensePoint3 center, float radius, bool inclusive)
        {
            if (!point.IsFinite || !center.IsFinite || !DefenseDamageRules.IsFinite(radius) || radius <= 0f) return false;
            double x = (double)point.X - center.X, z = (double)point.Z - center.Z;
            double distance = x * x + z * z, limit = (double)radius * radius;
            return inclusive ? distance <= limit : distance < limit;
        }

        public static DefenseVillageInteraction Resolve(bool available, bool nearHut, bool nearSocket, int towerLevel,
            int wood, int buildCost, int upgradeCost)
        {
            if (!available || (!nearHut && !nearSocket)) return new DefenseVillageInteraction(DefenseVillageAction.None, false);
            if (nearHut) return new DefenseVillageInteraction(DefenseVillageAction.Recruit, true);
            if (towerLevel >= 3) return new DefenseVillageInteraction(DefenseVillageAction.Maximum, false);
            int cost = towerLevel > 0 ? upgradeCost : buildCost;
            int shortage = cost > wood ? cost - wood : 0;
            return new DefenseVillageInteraction(towerLevel > 0 ? DefenseVillageAction.Upgrade : DefenseVillageAction.Build,
                cost > 0 && shortage == 0, cost, shortage);
        }
    }
}
