using System;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Finite, overflow-safe geometry shared by flight bounds and ranged target selection.</summary>
    public static class DefenseSpatialRules
    {
        public static bool InRange(DefensePoint3 origin, DefensePoint3 target, float range, bool spatial)
        {
            if (!origin.IsFinite || !target.IsFinite || !DefenseDamageRules.IsFinite(range) || range < 0f) return false;
            double x = (double)target.X - origin.X, y = spatial ? (double)target.Y - origin.Y : 0d;
            double z = (double)target.Z - origin.Z;
            return x * x + y * y + z * z <= (double)range * range;
        }

        public static bool WithinHeight(float height, float minimum, float maximum)
        {
            return DefenseDamageRules.IsFinite(height) && DefenseDamageRules.IsFinite(minimum) &&
                DefenseDamageRules.IsFinite(maximum) && minimum <= maximum && height >= minimum && height <= maximum;
        }
    }
}
