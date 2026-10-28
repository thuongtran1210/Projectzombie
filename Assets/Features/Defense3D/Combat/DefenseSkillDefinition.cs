using System;

namespace ProjectZombie.Features.Defense3D
{
    public enum DefenseSkillKind
    {
        Dash = 0,
        DepToOng = 1,
        BasicAttack = 2,
        ScholarSignature = 3,

        // Chuẩn hóa các Slot kỹ năng mở rộng cho mọi Tướng và Pháp Bảo tương lai
        Relic = DepToOng,
        Signature = ScholarSignature
    }

    /// <summary>Immutable authoring snapshot shared by preview, effect and cooldown UI.</summary>
    public sealed class DefenseSkillDefinition
    {
        public DefenseSkillKind Kind { get; }
        public float Range { get; }
        public float Radius { get; }
        public float Damage { get; }
        public float Cooldown { get; }
        public float Duration { get; }
        public bool AutoTargetOnTap { get; }
        public DefenseTargetMask TargetMask { get; }

        public DefenseSkillDefinition(DefenseSkillKind kind, float range, float radius, float damage,
            float cooldown, float duration, bool autoTargetOnTap, DefenseTargetMask targetMask)
        {
            if (!Enum.IsDefined(typeof(DefenseSkillKind), kind) || !Positive(range) || !NonNegative(radius) ||
                !NonNegative(damage) || !Positive(cooldown) || !Positive(duration) ||
                (targetMask & ~DefenseTargetMask.All) != 0 || targetMask == 0)
                throw new ArgumentOutOfRangeException(nameof(range), "Skill definition has invalid finite values or enums.");
            Kind = kind;
            Range = range;
            Radius = radius;
            Damage = damage;
            Cooldown = cooldown;
            Duration = duration;
            AutoTargetOnTap = autoTargetOnTap;
            TargetMask = targetMask;
        }

        private static bool NonNegative(float value) => DefenseDamageRules.IsFinite(value) && value >= 0f;
        private static bool Positive(float value) => DefenseDamageRules.IsFinite(value) && value > 0f;
    }

    public readonly struct DefenseSkillCastRequest
    {
        public readonly DefenseResolvedAim Aim;
        public readonly object SessionIdentity;
        public readonly int RunGeneration;

        public DefenseSkillCastRequest(DefenseResolvedAim aim, object sessionIdentity, int runGeneration)
        {
            Aim = aim;
            SessionIdentity = sessionIdentity;
            RunGeneration = runGeneration;
        }
    }

    public interface IDefenseAbilitySource
    {
        DefenseAbilityRuntime Ability { get; }
    }

    public interface IDefenseSharedAbilityEffect
    {
        DefenseAbilityRuntime SharedAbility { get; }
        float CastCooldown { get; }
    }

    public interface IDefenseSkillEffect
    {
        bool TryPrepare(DefenseResolvedAim requested, out DefenseResolvedAim prepared);
        void Execute(DefenseResolvedAim prepared);
    }
}
