using System;

namespace ProjectZombie.Features.Defense3D
{
    public enum DefenseFriendlyKind { Hero, Support }
    public interface IDefenseAggroCandidate
    {
        object Identity { get; }
        bool IsAvailable { get; }
        DefensePoint3 Position { get; }
        DefenseFriendlyKind Kind { get; }
    }

    /// <summary>Shared immutable hysteresis settings, independent of movement domain.</summary>
    public sealed class DefenseAggroRules
    {
        public float AcquireRange { get; }
        public float RetainRange { get; }
        public float LockSeconds { get; }
        public float ThinkSeconds { get; }
        public float ProvokedSeconds { get; }
        public DefenseAggroRules(float acquire, float retain, float locked, float think, float provoked)
        {
            if (!DefenseDamageRules.IsFinite(acquire) || acquire <= 0f || !DefenseDamageRules.IsFinite(retain) || retain < acquire ||
                !DefenseDamageRules.IsFinite(locked) || locked < 0f || !DefenseDamageRules.IsFinite(think) || think <= 0f ||
                !DefenseDamageRules.IsFinite(provoked) || provoked < 0f) throw new ArgumentException("Invalid aggro rules.");
            AcquireRange = acquire; RetainRange = retain; LockSeconds = locked; ThinkSeconds = think; ProvokedSeconds = provoked;
        }
    }

    /// <summary>Nearest eligible Hero/Support with stable ties and one retained focus. Null means Base.</summary>
    public sealed class DefenseAggroPolicy
    {
        private readonly DefenseAggroRules _rules;
        private float _lockUntil;
        public IDefenseAggroCandidate Focus { get; private set; }
        public DefenseAggroPolicy(DefenseAggroRules rules) => _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        public void Reset() { Focus = null; _lockUntil = 0f; }

        public IDefenseAggroCandidate Select(float time, DefensePoint3 origin, IDefenseAggroCandidate[] candidates,
            bool spatial, Predicate<IDefenseAggroCandidate> reachable, object provoker = null)
        {
            if (!DefenseDamageRules.IsFinite(time) || time < 0f || !origin.IsFinite || candidates == null || reachable == null)
            { Reset(); return null; }
            bool registered = false;
            for (int i = 0; i < candidates.Length; i++) if (ReferenceEquals(candidates[i], Focus)) { registered = true; break; }
            if (registered && Valid(Focus) && reachable(Focus) &&
                (DefenseSpatialRules.InRange(origin, Focus.Position, _rules.RetainRange, spatial) || time < _lockUntil)) return Focus;
            IDefenseAggroCandidate next = null;
            double best = double.PositiveInfinity;
            for (int i = 0; i < candidates.Length; i++)
            {
                var candidate = candidates[i];
                if (!Valid(candidate)) continue;
                float range = ReferenceEquals(candidate.Identity, provoker) ? _rules.RetainRange : _rules.AcquireRange;
                if (!DefenseSpatialRules.InRange(origin, candidate.Position, range, spatial)) continue;
                var point = candidate.Position;
                double x = (double)point.X - origin.X, y = spatial ? (double)point.Y - origin.Y : 0d, z = (double)point.Z - origin.Z;
                double distance = x * x + y * y + z * z;
                if (distance >= best || !reachable(candidate)) continue;
                next = candidate; best = distance;
            }
            if (!ReferenceEquals(next, Focus)) _lockUntil = next != null ? time + _rules.LockSeconds : 0f;
            Focus = next;
            return Focus;
        }
        private static bool Valid(IDefenseAggroCandidate candidate) => candidate != null && candidate.Identity != null &&
            candidate.IsAvailable && candidate.Position.IsFinite &&
            (candidate.Kind == DefenseFriendlyKind.Hero || candidate.Kind == DefenseFriendlyKind.Support);
    }
}
