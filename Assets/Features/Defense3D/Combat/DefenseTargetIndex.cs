using System;
using System.Collections.Generic;

namespace ProjectZombie.Features.Defense3D
{
    [Flags]
    public enum DefenseTargetMask { Ground = 1, Air = 2, All = Ground | Air }

    public interface IDefenseTargetPosition
    {
        object Identity { get; }
        bool IsAvailable { get; }
        bool IsAirborne { get; }
        DefensePoint3 Position { get; }
    }

    public interface IDefenseTargetQuery
    {
        IDefenseTargetPosition FindNearest(DefensePoint3 origin, float range, DefenseTargetMask mask = DefenseTargetMask.All);
    }

    public interface IDefenseSpatialTargetQuery
    {
        IDefenseTargetPosition FindNearestSpatial(DefensePoint3 origin, float range, DefenseTargetMask mask,
            Predicate<IDefenseTargetPosition> accepts);
    }

    public interface IDefenseAreaTargetQuery
    {
        bool TryCopyWithin(DefensePoint3 center, float radius, DefenseTargetMask mask,
            IDefenseTargetPosition[] buffer, out int count);
    }

    /// <summary>Session-owned index. Equal-distance ties retain registration order; no scene-global state.</summary>
    public sealed class DefenseTargetIndex : IDefenseTargetQuery, IDefenseAreaTargetQuery, IDefenseSpatialTargetQuery, IDisposable
    {
        private readonly List<IDefenseTargetPosition> _targets = new List<IDefenseTargetPosition>();
        private readonly Dictionary<object, IDefenseTargetPosition> _byIdentity = new Dictionary<object, IDefenseTargetPosition>();
        private bool _disposed;
        public int Count => _targets.Count;

        /// <summary>Copies a stable cast snapshot before damage callbacks can unregister targets. Saturation rejects the whole query.</summary>
        public bool TryCopyWithin(DefensePoint3 center, float radius, DefenseTargetMask mask,
            IDefenseTargetPosition[] buffer, out int count)
        {
            count = 0;
            if (_disposed || !center.IsFinite || !DefenseDamageRules.IsFinite(radius) || radius < 0f || buffer == null ||
                mask == 0 || (mask & ~DefenseTargetMask.All) != 0) return false;
            double squared = (double)radius * radius;
            foreach (var target in _targets)
            {
                if (!target.IsAvailable || (mask & (target.IsAirborne ? DefenseTargetMask.Air : DefenseTargetMask.Ground)) == 0) continue;
                var position = target.Position;
                if (!position.IsFinite) continue;
                double x = (double)position.X - center.X, z = (double)position.Z - center.Z;
                if (x * x + z * z > squared) continue;
                if (count == buffer.Length)
                {
                    Array.Clear(buffer, 0, count);
                    count = 0;
                    return false;
                }
                buffer[count++] = target;
            }
            return true;
        }

        public bool Register(IDefenseTargetPosition target)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DefenseTargetIndex));
            if (target == null || target.Identity == null) throw new ArgumentNullException(nameof(target));
            if (_byIdentity.ContainsKey(target.Identity)) return false;
            _byIdentity.Add(target.Identity, target);
            _targets.Add(target);
            return true;
        }

        public void Unregister(object identity)
        {
            if (identity == null || !_byIdentity.TryGetValue(identity, out var target)) return;
            _byIdentity.Remove(identity);
            _targets.Remove(target);
        }

        public IDefenseTargetPosition FindNearest(DefensePoint3 origin, float range, DefenseTargetMask mask = DefenseTargetMask.All)
        {
            if (_disposed || !origin.IsFinite || !DefenseDamageRules.IsFinite(range) || range <= 0f) return null;
            IDefenseTargetPosition nearest = null;
            double best = (double)range * range;
            foreach (var target in _targets)
            {
                if (!target.IsAvailable || (mask & (target.IsAirborne ? DefenseTargetMask.Air : DefenseTargetMask.Ground)) == 0) continue;
                var point = target.Position;
                if (!point.IsFinite) continue;
                double dx = (double)point.X - origin.X, dz = (double)point.Z - origin.Z;
                double distance = dx * dx + dz * dz;
                if (distance > best || (nearest != null && distance == best)) continue;
                nearest = target;
                best = distance;
            }
            return nearest;
        }

        /// <summary>Ground keeps planar distance; Air uses XYZ. Filtering happens before nearest selection.</summary>
        public IDefenseTargetPosition FindNearestSpatial(DefensePoint3 origin, float range, DefenseTargetMask mask,
            Predicate<IDefenseTargetPosition> accepts)
        {
            if (_disposed || !origin.IsFinite || !DefenseDamageRules.IsFinite(range) || range <= 0f ||
                mask == 0 || (mask & ~DefenseTargetMask.All) != 0) return null;
            IDefenseTargetPosition nearest = null;
            double best = (double)range * range;
            foreach (var target in _targets)
            {
                if (!target.IsAvailable || (mask & (target.IsAirborne ? DefenseTargetMask.Air : DefenseTargetMask.Ground)) == 0) continue;
                var point = target.Position;
                if (!DefenseSpatialRules.InRange(origin, point, range, target.IsAirborne)) continue;
                double x = (double)point.X - origin.X, y = target.IsAirborne ? (double)point.Y - origin.Y : 0d;
                double z = (double)point.Z - origin.Z;
                double distance = x * x + y * y + z * z;
                if (distance > best || (nearest != null && distance == best) || (accepts != null && !accepts(target))) continue;
                nearest = target;
                best = distance;
            }
            return nearest;
        }

        public void Clear() { _targets.Clear(); _byIdentity.Clear(); }
        public void Dispose() { Clear(); _disposed = true; }
    }
}
