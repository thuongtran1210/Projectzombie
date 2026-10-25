using System;

namespace ProjectZombie.Features.Defense3D
{
    public enum DefenseYakshaPhase
    {
        Dormant,
        ArenaCombat,    // Dual with Hero in East Arena with telegraphed cleave/slam
        AssaultBase,    // March down Bridge 2 to destroy Nha Chinh
        Defeated
    }

    public enum DefenseYakshaAttackTelegraph
    {
        None,
        TelegraphSlam,
        TelegraphCleave,
        TelegraphCrescent
    }

    /// <summary>
    /// U3D-021: Yaksha Boss FSM and combat state.
    /// Invariants:
    /// - Starts Dormant until 510s (or test trigger).
    /// - ArenaCombat: Telegraphed skills with counterplay/warning time.
    /// - Transitions to AssaultBase after ArenaDuration (e.g. 45s).
    /// - Boss HP NEVER resets upon phase transition.
    /// - Terminal Loss (BaseHP=0) immediately halts Boss assault.
    /// </summary>
    public sealed class DefenseYakshaBehaviorFSM
    {
        public const float DefaultArenaDuration = 45f;
        public const float SlamTelegraphTime = 1.2f;
        public const float CleaveTelegraphTime = 0.8f;
        public const float SlamDamage = 60f;
        public const float CleaveDamage = 40f;

        public DefenseYakshaPhase Phase { get; private set; }
        public DefenseYakshaAttackTelegraph ActiveTelegraph { get; private set; }
        public float TelegraphTimerRemaining { get; private set; }
        public float ArenaTimerRemaining { get; private set; }
        public DefenseHealthCore Health { get; }

        public event Action<DefenseYakshaPhase> PhaseChanged;
        public event Action<DefenseYakshaAttackTelegraph> AttackTelegraphed;
        public event Action<DefenseYakshaAttackTelegraph, float> AttackExecuted;

        public DefenseYakshaBehaviorFSM(float maxHp = 3200f, float arenaDuration = DefaultArenaDuration)
        {
            Health = new DefenseHealthCore(maxHp);
            Health.Defeated += OnBossDefeated;

            Phase = DefenseYakshaPhase.Dormant;
            ActiveTelegraph = DefenseYakshaAttackTelegraph.None;
            ArenaTimerRemaining = arenaDuration;
        }

        public void SpawnInArena()
        {
            if (Phase != DefenseYakshaPhase.Dormant) return;
            Phase = DefenseYakshaPhase.ArenaCombat;
            PhaseChanged?.Invoke(Phase);
        }

        public void TransitionToAssaultBase()
        {
            if (Phase != DefenseYakshaPhase.ArenaCombat || Health.IsDead) return;
            Phase = DefenseYakshaPhase.AssaultBase;
            ActiveTelegraph = DefenseYakshaAttackTelegraph.None;
            PhaseChanged?.Invoke(Phase);
        }

        public void Tick(float gameplayDt, bool isPaused)
        {
            if (isPaused || Phase == DefenseYakshaPhase.Dormant || Phase == DefenseYakshaPhase.Defeated)
                return;

            if (Phase == DefenseYakshaPhase.ArenaCombat)
            {
                ArenaTimerRemaining = Math.Max(0f, ArenaTimerRemaining - gameplayDt);
                if (ArenaTimerRemaining <= 0f)
                {
                    TransitionToAssaultBase();
                    return;
                }

                // Telegraph tick
                if (ActiveTelegraph != DefenseYakshaAttackTelegraph.None)
                {
                    TelegraphTimerRemaining = Math.Max(0f, TelegraphTimerRemaining - gameplayDt);
                    if (TelegraphTimerRemaining <= 0f)
                    {
                        var executedType = ActiveTelegraph;
                        float damage = executedType == DefenseYakshaAttackTelegraph.TelegraphSlam ? SlamDamage : CleaveDamage;
                        ActiveTelegraph = DefenseYakshaAttackTelegraph.None;
                        AttackExecuted?.Invoke(executedType, damage);
                    }
                }
            }
        }

        public bool TryStartTelegraph(DefenseYakshaAttackTelegraph type)
        {
            if (Phase != DefenseYakshaPhase.ArenaCombat || ActiveTelegraph != DefenseYakshaAttackTelegraph.None)
                return false;

            ActiveTelegraph = type;
            TelegraphTimerRemaining = type == DefenseYakshaAttackTelegraph.TelegraphSlam ? SlamTelegraphTime : CleaveTelegraphTime;
            AttackTelegraphed?.Invoke(type);
            return true;
        }

        private void OnBossDefeated()
        {
            Phase = DefenseYakshaPhase.Defeated;
            ActiveTelegraph = DefenseYakshaAttackTelegraph.None;
            PhaseChanged?.Invoke(Phase);
        }
    }
}
