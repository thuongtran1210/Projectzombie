using System;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Fixed-capacity timed effects. Slow uses the strongest live entry; Stun refreshes without shortening.</summary>
    public sealed class DefenseStatusRuntime
    {
        private struct Slow { public float Strength, Until; }
        private readonly Slow[] _slows;
        private readonly IDefenseGameplayContext _gameplay;
        private readonly float _maxDuration, _knockDistance;
        private int _generation;
        private float _stunnedUntil, _knockUntil, _knockSample, _knockDuration;
        private bool _knocking;
        private DefensePoint3 _knockVector;
        public DefenseStatusRuntime(IDefenseGameplayContext gameplay, float maxDuration, float knockDistance, int capacity = 8)
        {
            if (gameplay == null || !DefenseDamageRules.IsFinite(maxDuration) || maxDuration <= 0f ||
                !DefenseDamageRules.IsFinite(knockDistance) || knockDistance <= 0f || capacity < 1) throw new ArgumentException("Invalid status runtime settings.");
            _gameplay = gameplay; _maxDuration = maxDuration; _knockDistance = knockDistance; _slows = new Slow[capacity]; Reset();
        }
        private bool Current => _generation == _gameplay.RunGeneration;
        public bool IsStunned => Current && _gameplay.ElapsedSeconds < _stunnedUntil;
        public bool IsKnocking => Current && _knocking;
        public bool BlocksActions => IsStunned || IsKnocking;
        public float MovementMultiplier
        {
            get
            {
                if (!Current) return 1f;
                float strongest = 0f, now = _gameplay.ElapsedSeconds;
                for (int i = 0; i < _slows.Length; i++) if (_slows[i].Until > now) strongest = Math.Max(strongest, _slows[i].Strength);
                return 1f - strongest;
            }
        }
        public bool TryApply(DefenseStatusRequest request)
        {
            if (!Current || !_gameplay.AdvancesGameplay || !request.IsValid || request.Duration > _maxDuration ||
                !ReferenceEquals(request.SessionIdentity, _gameplay.SessionIdentity) || request.RunGeneration != _gameplay.RunGeneration) return false;
            float now = _gameplay.ElapsedSeconds;
            if (!DefenseDamageRules.IsFinite(now) || !DefenseDamageRules.IsFinite(now + request.Duration)) return false;
            if (request.Kind == DefenseStatusKind.Stun)
            {
                _stunnedUntil = Math.Max(_stunnedUntil, now + request.Duration);
                return true;
            }
            if (request.Kind == DefenseStatusKind.Slow)
            {
                if (request.Strength <= 0f) return false;
                for (int i = 0; i < _slows.Length; i++)
                    if (_slows[i].Until <= now)
                    {
                        _slows[i] = new Slow { Strength = request.Strength, Until = now + request.Duration };
                        return true;
                    }
                return false;
            }
            if (_knocking || request.Strength <= 0f || !TryKnockVector(request, _knockDistance, out var vector)) return false;
            _knockVector = vector; _knockSample = now; _knockUntil = now + request.Duration; _knockDuration = request.Duration; _knocking = true;
            return true;
        }
        public static bool TryKnockVector(DefenseStatusRequest request, float maximumDistance, out DefensePoint3 vector)
        {
            vector = default;
            if (!request.IsValid || request.Kind != DefenseStatusKind.Knockback || request.Strength <= 0f ||
                !DefenseDamageRules.IsFinite(maximumDistance) || maximumDistance <= 0f) return false;
            double x = request.Direction.X, y = request.Direction.Y, z = request.Direction.Z;
            double length = Math.Sqrt(x * x + y * y + z * z);
            if (length <= 0d) return false;
            double distance = maximumDistance * (double)request.Strength;
            vector = new DefensePoint3((float)(x / length * distance), (float)(y / length * distance), (float)(z / length * distance));
            return vector.IsFinite;
        }
        public bool TryConsumeKnockStep(out DefensePoint3 delta)
        {
            delta = default;
            if (!Current || !_gameplay.AdvancesGameplay || !_knocking) return false;
            float now = Math.Min(_gameplay.ElapsedSeconds, _knockUntil);
            float fraction = Math.Max(0f, now - _knockSample) / _knockDuration;
            _knockSample = now;
            delta = new DefensePoint3(_knockVector.X * fraction, _knockVector.Y * fraction, _knockVector.Z * fraction);
            if (now >= _knockUntil) _knocking = false;
            return fraction > 0f;
        }
        public void CancelKnockback() => _knocking = false;
        public void Reset()
        {
            Array.Clear(_slows, 0, _slows.Length); _generation = _gameplay.RunGeneration;
            _stunnedUntil = 0f; _knocking = false; _knockUntil = _knockSample = 0f; _knockVector = default;
        }
    }
}
