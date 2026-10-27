using System;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>One immutable geometry result for both indicator rendering and effect execution.</summary>
    public readonly struct DefenseResolvedAim
    {
        public readonly DefensePoint3 Origin, Target, Direction;
        public readonly float Distance;

        internal DefenseResolvedAim(DefensePoint3 origin, DefensePoint3 target, DefensePoint3 direction, float distance)
        {
            Origin = origin;
            Target = target;
            Direction = direction;
            Distance = distance;
        }
    }

    /// <summary>Resolves/clamps horizontal aim without physics, camera or implicit XY conversion.</summary>
    public static class DefenseAimResolver
    {
        public static bool TryResolve(DefensePoint3 origin, DefensePoint3 desiredTarget, float maxRange,
            out DefenseResolvedAim result)
        {
            result = default;
            if (!origin.IsFinite || !desiredTarget.IsFinite || !DefenseDamageRules.IsFinite(maxRange) || maxRange < 0f)
                return false;
            double x = (double)desiredTarget.X - origin.X, z = (double)desiredTarget.Z - origin.Z;
            double length = Math.Sqrt(x * x + z * z);
            float distance = (float)Math.Min(length, maxRange);
            var direction = length > 0d ? new DefensePoint3((float)(x / length), 0f, (float)(z / length)) : default;
            var target = new DefensePoint3(origin.X + direction.X * distance, origin.Y, origin.Z + direction.Z * distance);
            if (!target.IsFinite) return false;
            result = new DefenseResolvedAim(origin, target, direction, distance);
            return true;
        }

        public static bool TryResolveDrag(DefensePoint3 origin, DefensePoint3 screenRight, DefensePoint3 screenUp,
            float directionX, float directionY, float pull, float maxRange, DefensePoint3 fallbackDirection,
            out DefenseResolvedAim result, float minimumDistance = 0f)
        {
            result = default;
            if (!origin.IsFinite || !DefenseDamageRules.IsFinite(directionX) || !DefenseDamageRules.IsFinite(directionY) ||
                !DefenseDamageRules.IsFinite(pull) || !DefenseDamageRules.IsFinite(maxRange) || maxRange < 0f ||
                !DefenseDamageRules.IsFinite(minimumDistance) || minimumDistance < 0f ||
                !TryNormalize(screenRight, out var right) || !TryNormalize(screenUp, out var up)) return false;
            // Reject collapsed/ambiguous camera axes instead of silently aiming in the wrong plane.
            double determinant = (double)right.X * up.Z - (double)right.Z * up.X;
            if (Math.Abs(determinant) < 0.001d) return false;
            var direction = new DefensePoint3(right.X * directionX + up.X * directionY, 0f,
                right.Z * directionX + up.Z * directionY);
            if (!TryNormalize(direction, out direction) && !TryNormalize(fallbackDirection, out direction)) return false;
            float distance = Math.Min(maxRange, Math.Max(minimumDistance, maxRange * Math.Max(0f, Math.Min(1f, pull))));
            var desired = new DefensePoint3(origin.X + direction.X * distance, origin.Y, origin.Z + direction.Z * distance);
            return TryResolve(origin, desired, maxRange, out result);
        }

        private static bool TryNormalize(DefensePoint3 value, out DefensePoint3 result)
        {
            result = default;
            if (!value.IsFinite) return false;
            double length = Math.Sqrt((double)value.X * value.X + (double)value.Z * value.Z);
            if (length < 0.000001d) return false;
            result = new DefensePoint3((float)(value.X / length), 0f, (float)(value.Z / length));
            return true;
        }
    }
}
