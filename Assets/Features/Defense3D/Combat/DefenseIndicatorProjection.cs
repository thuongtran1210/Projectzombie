using System;

namespace ProjectZombie.Features.Defense3D
{
    public enum DefenseSpawnWarningSymbol { Demon, Fish, Bat, Elite, Boss, Parachute, Cannon, Armored, Commander }

    public readonly struct DefenseIndicatorProjection
    {
        public readonly float X, Y, Angle;
        public readonly bool IsOffscreen;

        public DefenseIndicatorProjection(float x, float y, float angle, bool offscreen)
        {
            X = x;
            Y = y;
            Angle = angle;
            IsOffscreen = offscreen;
        }

        /// <summary>Unity viewport coordinates; output pixels have a bottom-left origin.</summary>
        public static bool TryProject(DefensePoint3 viewport, float left, float bottom, float width, float height,
            float margin, out DefenseIndicatorProjection result)
        {
            result = default;
            if (!viewport.IsFinite || !Finite(left) || !Finite(bottom) || !Finite(width) || !Finite(height) ||
                !Finite(margin) || margin < 0f || width <= margin * 2f || height <= margin * 2f) return false;
            double x = viewport.X * 2d - 1d;
            double y = viewport.Y * 2d - 1d;
            bool behind = viewport.Z <= 0f;
            if (behind) { x = -x; y = -y; }
            double halfW = width * .5d - margin, halfH = height * .5d - margin;
            double px = x * width * .5d, py = y * height * .5d;
            bool on = !behind && Math.Abs(x) <= .90d && Math.Abs(y) <= .88d && Math.Abs(px) <= halfW && Math.Abs(py) <= halfH;
            if (!on)
            {
                if (Math.Abs(px) + Math.Abs(py) < .000001d) py = -1d;
                double scale = 1d / Math.Max(Math.Abs(px) / halfW, Math.Abs(py) / halfH);
                px *= scale;
                py *= scale;
            }
            result = new DefenseIndicatorProjection((float)(left + width * .5d + px), (float)(bottom + height * .5d + py),
                on ? 0f : (float)(Math.Atan2(py, px) * 180d / Math.PI), !on);
            return Finite(result.X) && Finite(result.Y);
        }

        public static float WarningFraction(float now, float trigger, float lead)
        {
            if (!Finite(now) || !Finite(trigger) || !Finite(lead) || now < 0f || trigger < 0f || lead <= 0f ||
                now >= trigger || now < Math.Max(0f, trigger - lead)) return 0f;
            return Math.Min(1f, (trigger - now) / lead);
        }

        /// <summary>Stable greedy separation along an edge; the arrow keeps its true target bearing.</summary>
        public static bool TryPlace(DefenseIndicatorProjection projection, float left, float bottom, float width,
            float height, float margin, float spacing, DefensePoint3[] occupied, int count, out DefensePoint3 placed)
        {
            placed = default;
            if (occupied == null || count < 0 || count > occupied.Length || !Finite(spacing) || spacing <= 0f ||
                !Finite(projection.X) || !Finite(projection.Y) || !Finite(left) || !Finite(bottom) || !Finite(width) ||
                !Finite(height) || !Finite(margin) || margin < 0f || width <= margin * 2f || height <= margin * 2f) return false;
            bool vertical = Math.Abs(projection.X - left - width * .5f) / (width * .5f - margin) > .99f;
            for (int step = 0; step <= count * 2; step++)
            {
                float offset = step == 0 ? 0f : ((step + 1) / 2) * spacing * (step % 2 == 1 ? 1f : -1f);
                float x = projection.X + (vertical ? 0f : offset);
                float y = projection.Y + (vertical ? offset : 0f);
                if (x < left + margin || x > left + width - margin || y < bottom + margin || y > bottom + height - margin) continue;
                bool free = true;
                for (int i = 0; i < count; i++)
                {
                    if (!occupied[i].IsFinite) return false;
                    double dx = x - occupied[i].X, dy = y - occupied[i].Y;
                    if (dx * dx + dy * dy < spacing * spacing) { free = false; break; }
                }
                if (free) { placed = new DefensePoint3(x, y, 0f); return true; }
            }
            return false;
        }

        private static bool Finite(float value) => DefenseDamageRules.IsFinite(value);
    }
}
