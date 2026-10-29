using System;

namespace ProjectZombie.Features.Defense3D
{
    public enum DefenseRelicPhase { Ready, Flying, Anchored }

    /// <summary>Gameplay-time throw and one-use recast window, independent of visual callbacks.</summary>
    public sealed class DefenseRelicCastState
    {
        public DefenseRelicPhase Phase { get; private set; }
        private float _impactAt, _expiresAt;
        public bool Begin(float now, float flight, float window)
        {
            if (Phase != DefenseRelicPhase.Ready || !Finite(now) || !Finite(flight) || !Finite(window) || flight <= 0f || window <= 0f || !Finite(now + flight + window)) return false;
            _impactAt = now + flight;
            _expiresAt = _impactAt + window;
            Phase = DefenseRelicPhase.Flying;
            return true;
        }
        /// <summary>Returns true exactly once when flight completes; late ticks cannot extend the recast window.</summary>
        public bool Advance(float now)
        {
            if (!Finite(now)) return false;
            bool impact = Phase == DefenseRelicPhase.Flying && now >= _impactAt;
            if (impact) Phase = DefenseRelicPhase.Anchored;
            if (Phase == DefenseRelicPhase.Anchored && now >= _expiresAt) Reset();
            return impact;
        }
        public bool CanRecast(float now) => Phase == DefenseRelicPhase.Anchored && Finite(now) && now < _expiresAt;
        public bool Consume(float now)
        {
            if (!CanRecast(now)) return false;
            Reset();
            return true;
        }
        public void Reset() => Phase = DefenseRelicPhase.Ready;
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
