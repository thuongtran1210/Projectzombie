using System;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>World-space coordinates independent of a rendering/physics engine.</summary>
    public readonly struct DefensePoint3
    {
        public readonly float X, Y, Z;
        public DefensePoint3(float x, float y, float z) { X = x; Y = y; Z = z; }
        public bool IsFinite => DefenseDamageRules.IsFinite(X) && DefenseDamageRules.IsFinite(Y) && DefenseDamageRules.IsFinite(Z);
    }

    /// <summary>Damage payload; scoped production requests reject old runs and foreign sessions.</summary>
    public partial struct DefenseDamageContext
    {
        public float Amount;
        public object Source;
        public DefensePoint3 WorldHitPoint;
        public bool IsCritical;
        public int RunGeneration;
        public object SessionIdentity;

        public DefenseDamageContext(float amount, object source = null, DefensePoint3 hitPoint = default,
            bool isCritical = false, int runGeneration = -1, object sessionIdentity = null)
        {
            Amount = amount;
            Source = source;
            WorldHitPoint = hitPoint;
            IsCritical = isCritical;
            RunGeneration = runGeneration;
            SessionIdentity = sessionIdentity;
        }
    }

    public static class DefenseDamageRules
    {
        public static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        public static bool IsValid(DefenseDamageContext request) => request.Amount > 0f && IsFinite(request.Amount) && request.WorldHitPoint.IsFinite;
        public static bool CanApply(DefenseDamageContext request, IDefenseGameplayContext gameplay, bool isDead)
        {
            return gameplay != null && gameplay.AdvancesGameplay && !isDead && IsValid(request) &&
                (request.RunGeneration == -1 || request.RunGeneration == gameplay.RunGeneration) &&
                (request.SessionIdentity == null || ReferenceEquals(request.SessionIdentity, gameplay.SessionIdentity));
        }
    }

    /// <summary>Capability boundary for future skill effects; no Legacy XY knockback contract is implied.</summary>
    public interface IDefenseStatusReceiver
    {
        bool TryApplyStatus(DefenseStatusRequest request);
    }

    public enum DefenseStatusKind { Slow, Stun, Knockback }

    public readonly struct DefenseStatusRequest
    {
        public readonly DefenseStatusKind Kind;
        public readonly float Duration, Strength;
        public readonly DefensePoint3 Direction;
        public readonly object SessionIdentity;
        public readonly int RunGeneration;
        public DefenseStatusRequest(DefenseStatusKind kind, float duration, float strength, DefensePoint3 direction,
            object sessionIdentity = null, int runGeneration = -1)
        {
            Kind = kind; Duration = duration; Strength = strength; Direction = direction;
            SessionIdentity = sessionIdentity; RunGeneration = runGeneration;
        }
        public bool IsValid => (Kind == DefenseStatusKind.Slow || Kind == DefenseStatusKind.Stun || Kind == DefenseStatusKind.Knockback) && Duration > 0f &&
            DefenseDamageRules.IsFinite(Duration) && DefenseDamageRules.IsFinite(Strength) &&
            Strength >= 0f && Strength <= 1f && Direction.IsFinite;
    }
}
