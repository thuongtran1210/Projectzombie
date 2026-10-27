using System;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Source PlayerCombatController timing and facing-weighted basic attack targeting.</summary>
    public static class DefenseBasicAttackRules
    {
        public const float Windup = .18f;
        public const float RepeatInterval = .25f;
        public const float Recovery = .85f;
        public const float ConeAngle = 210.14012f;
        public const float MoveMultiplier = 2.1f / 3.8f;
        public static float SearchRange(float radius) => Math.Max(radius * 1.8f, 4.5f);
        public static double TargetScore(DefensePoint3 origin, DefensePoint3 forward, DefensePoint3 target)
        {
            double dx = (double)target.X - origin.X, dz = (double)target.Z - origin.Z;
            double distance = Math.Sqrt(dx * dx + dz * dz);
            double length = Math.Sqrt((double)forward.X * forward.X + (double)forward.Z * forward.Z);
            double dot = distance < .001 || length < .000001 ? 1d : (dx * forward.X + dz * forward.Z) / (distance * length);
            return distance * (1.15d - dot * .25d);
        }

        public static bool TryTriangleHeight(DefensePoint3 p, DefensePoint3 a, DefensePoint3 b, DefensePoint3 c, out float height)
        {
            height = 0f;
            double denominator = ((double)b.Z - c.Z) * (a.X - c.X) + ((double)c.X - b.X) * (a.Z - c.Z);
            if (Math.Abs(denominator) < .0000001) return false;
            double u = (((double)b.Z - c.Z) * (p.X - c.X) + ((double)c.X - b.X) * (p.Z - c.Z)) / denominator;
            double v = (((double)c.Z - a.Z) * (p.X - c.X) + ((double)a.X - c.X) * (p.Z - c.Z)) / denominator;
            double w = 1d - u - v;
            if (u < -.00001 || v < -.00001 || w < -.00001) return false;
            height = (float)(u * a.Y + v * b.Y + w * c.Y);
            return DefenseDamageRules.IsFinite(height);
        }
    }
}
