using System;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Locked cone snapshot. XYZ range plus XZ angle prevent hits above/below the attack volume.</summary>
    public readonly struct DefenseConeSnapshot
    {
        public readonly DefensePoint3 Origin, Forward;
        public readonly float Radius, Angle;
        public DefenseConeSnapshot(DefensePoint3 origin, DefensePoint3 forward, float radius, float angle)
        { Origin = origin; Forward = forward; Radius = radius; Angle = angle; }
        public bool Contains(DefensePoint3 point)
        {
            if (!Origin.IsFinite || !Forward.IsFinite || !point.IsFinite || !DefenseDamageRules.IsFinite(Radius) || Radius <= 0f ||
                !DefenseDamageRules.IsFinite(Angle) || Angle <= 0f || Angle > 360f || !DefenseSpatialRules.InRange(Origin, point, Radius, true)) return false;
            double fx = Forward.X, fz = Forward.Z, forwardLength = Math.Sqrt(fx * fx + fz * fz);
            if (forwardLength <= 0d) return false;
            double dx = (double)point.X - Origin.X, dz = (double)point.Z - Origin.Z;
            double length = Math.Sqrt(dx * dx + dz * dz);
            if (length <= 0d) return true;
            double dot = (dx * fx + dz * fz) / (length * forwardLength);
            return dot + 0.000001d >= Math.Cos(Angle * 0.5d * Math.PI / 180d);
        }
    }

    /// <summary>Single pending attack; cooldown remains owned by the enemy attack component.</summary>
    public sealed class DefenseConeTelegraphRuntime
    {
        private readonly IDefenseGameplayContext _gameplay;
        private readonly float _windup, _radius, _angle;
        private int _generation;
        private object _identity;
        private float _releaseAt;
        private DefenseConeSnapshot _snapshot;
        private bool _pending;
        public bool IsPending => _pending;
        public DefenseConeTelegraphRuntime(IDefenseGameplayContext gameplay, float windup, float radius, float angle)
        {
            if (gameplay == null || !DefenseDamageRules.IsFinite(windup) || windup <= 0f ||
                !DefenseDamageRules.IsFinite(radius) || radius <= 0f || !DefenseDamageRules.IsFinite(angle) || angle <= 0f || angle > 360f)
                throw new ArgumentException("Cone requires a finite positive windup/range and angle within 360 degrees.");
            _gameplay = gameplay; _windup = windup; _radius = radius; _angle = angle;
        }
        public bool TryBegin(DefensePoint3 origin, DefensePoint3 direction)
        {
            if (_pending || !_gameplay.AdvancesGameplay || !origin.IsFinite || !direction.IsFinite) return false;
            double x = direction.X, z = direction.Z, length = Math.Sqrt(x * x + z * z);
            if (length <= 0d || !DefenseDamageRules.IsFinite(_gameplay.ElapsedSeconds + _windup)) return false;
            _snapshot = new DefenseConeSnapshot(origin, new DefensePoint3((float)(x / length), 0f, (float)(z / length)), _radius, _angle);
            _identity = _gameplay.SessionIdentity; _generation = _gameplay.RunGeneration;
            _releaseAt = _gameplay.ElapsedSeconds + _windup; _pending = true;
            return true;
        }
        public DefenseConeSnapshot Preview => _snapshot;
        public bool TryResolve(out DefenseConeSnapshot snapshot)
        {
            snapshot = default;
            if (!_pending) return false;
            if (!_gameplay.AdvancesGameplay || _generation != _gameplay.RunGeneration || !ReferenceEquals(_identity, _gameplay.SessionIdentity))
            { Cancel(); return false; }
            if (_gameplay.ElapsedSeconds < _releaseAt) return false;
            snapshot = _snapshot;
            _pending = false;
            return true;
        }
        public void Cancel() => _pending = false;
    }
}
