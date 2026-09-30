using System;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Elements
{
    public enum SteamSlipDiagnosticKind
    {
        ElementPrimed,
        ReactionTriggered,
        ReactionCooldownRejected,
        PatchCreated,
        PatchCapacityRejected,
        OverlapBufferSaturated,
        TargetAffected,
        CollisionStaggered
    }

    public readonly struct SteamSlipDiagnosticEvent
    {
        public readonly SteamSlipDiagnosticKind Kind;
        public readonly int EnemyId;
        public readonly ElementType Element;
        public readonly float Value;
        public readonly int Count;

        public SteamSlipDiagnosticEvent(SteamSlipDiagnosticKind kind, int enemyId, ElementType element, float value, int count)
        {
            Kind = kind;
            EnemyId = enemyId;
            Element = element;
            Value = value;
            Count = count;
        }
    }

    /// <summary>Optional observer seam. Reports compile out of non-development builds.</summary>
    public static class SteamSlipReactionDiagnostics
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public static event Action<SteamSlipDiagnosticEvent> Reported;

        public static void Report(SteamSlipDiagnosticEvent diagnosticEvent) => Reported?.Invoke(diagnosticEvent);
#else
        public static void Report(SteamSlipDiagnosticEvent diagnosticEvent) { }
#endif
    }
}
