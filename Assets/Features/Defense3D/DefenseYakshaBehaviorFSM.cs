using System;

namespace ProjectZombie.Features.Defense3D
{
    public enum DefenseYakshaPhase { Dormant, ArenaCombat, AssaultBase, Defeated }
    public enum DefenseYakshaAttackTelegraph { None, TelegraphSlam, TelegraphCleave, TelegraphCrescent }

    /// <summary>Captured prototype arena cycle; production preserves authored warning/damage instead of legacy FSM defaults.</summary>
    public sealed class DefenseYakshaArenaCycle
    {
        public float Interval { get; }
        public float Windup { get; }
        public float Damage { get; }
        public DefenseYakshaArenaCycle(float interval, float windup, float damage)
        {
            if (!DefenseDamageRules.IsFinite(interval) || interval <= 0f || !DefenseDamageRules.IsFinite(windup) || windup <= 0f ||
                !DefenseDamageRules.IsFinite(damage) || damage < 0f) throw new ArgumentException("Invalid Yaksha arena cycle.");
            Interval = interval; Windup = windup; Damage = damage;
        }
    }

    /// <summary>Single phase/intent authority. Production shares the actor's health; no phase transition resets HP.</summary>
    public sealed class DefenseYakshaBehaviorFSM : IDisposable
    {
        private const float TIMER_EPSILON = 0.00001f;
        public const float DefaultArenaDuration = 45f;
        public const float SlamTelegraphTime = 1.2f;
        public const float CleaveTelegraphTime = 0.8f;
        public const float SlamDamage = 60f;
        public const float CleaveDamage = 40f;
        private readonly DefenseYakshaArenaCycle _cycle;
        private float _nextAttackRemaining;
        private readonly float _arenaDuration;
        private double _arenaElapsed;
        private bool _disposed;
        public DefenseYakshaPhase Phase { get; private set; }
        public DefenseYakshaAttackTelegraph ActiveTelegraph { get; private set; }
        public float TelegraphTimerRemaining { get; private set; }
        public float ArenaTimerRemaining { get; private set; }
        public DefenseHealthCore Health { get; }
        public bool HasSpawned { get; private set; }
        public bool HasEnteredAssault { get; private set; }
        public bool IsStopped { get; private set; }
        public event Action<DefenseYakshaPhase> PhaseChanged;
        public event Action<DefenseYakshaAttackTelegraph> AttackTelegraphed;
        public event Action<DefenseYakshaAttackTelegraph, float> AttackExecuted;
        public event Action CombatStopped;

        /// <summary>Standalone compatibility constructor; production must inject the existing actor health.</summary>
        public DefenseYakshaBehaviorFSM(float maxHp = 3200f, float arenaDuration = DefaultArenaDuration)
            : this(new DefenseHealthCore(maxHp), arenaDuration) { }

        public DefenseYakshaBehaviorFSM(DefenseHealthCore health, float arenaDuration, DefenseYakshaArenaCycle cycle = null)
        {
            if (health == null || !DefenseDamageRules.IsFinite(arenaDuration) || arenaDuration <= 0f)
                throw new ArgumentException("Yaksha requires shared health and a finite positive arena duration.");
            Health = health; _cycle = cycle; _arenaDuration = arenaDuration;
            Health.Defeated += OnBossDefeated;
            Phase = DefenseYakshaPhase.Dormant;
            ArenaTimerRemaining = arenaDuration;
            _nextAttackRemaining = cycle != null ? cycle.Interval : 0f;
            if (Health.IsDead) OnBossDefeated();
        }
        public void SpawnInArena()
        {
            if (_disposed || IsStopped || Phase != DefenseYakshaPhase.Dormant || Health.IsDead) return;
            HasSpawned = true; Phase = DefenseYakshaPhase.ArenaCombat;
            PhaseChanged?.Invoke(Phase);
        }
        public void TransitionToAssaultBase()
        {
            if (_disposed || IsStopped || Phase != DefenseYakshaPhase.ArenaCombat || Health.IsDead) return;
            ClearTelegraph(); HasEnteredAssault = true; Phase = DefenseYakshaPhase.AssaultBase;
            PhaseChanged?.Invoke(Phase);
        }
        public void Tick(float gameplayDt, bool isPaused)
        {
            if (!DefenseDamageRules.IsFinite(gameplayDt) || gameplayDt < 0f) throw new ArgumentOutOfRangeException(nameof(gameplayDt));
            if (!_disposed && Health.IsDead) { OnBossDefeated(); return; }
            if (_disposed || IsStopped || isPaused || Phase != DefenseYakshaPhase.ArenaCombat || gameplayDt == 0f) return;
            _arenaElapsed += gameplayDt;
            ArenaTimerRemaining = (float)Math.Max(0d, _arenaDuration - _arenaElapsed);
            // Arena cutoff wins over a blast due in the same tick.
            if (ArenaTimerRemaining <= TIMER_EPSILON) { ArenaTimerRemaining = 0f; TransitionToAssaultBase(); return; }
            if (ActiveTelegraph != DefenseYakshaAttackTelegraph.None)
            {
                TelegraphTimerRemaining = Math.Max(0f, TelegraphTimerRemaining - gameplayDt);
                if (TelegraphTimerRemaining > TIMER_EPSILON) return;
                var executed = ActiveTelegraph;
                float damage = _cycle != null ? _cycle.Damage : executed == DefenseYakshaAttackTelegraph.TelegraphSlam ? SlamDamage : CleaveDamage;
                ClearTelegraph();
                if (_cycle != null) _nextAttackRemaining = Math.Max(0.1f, _cycle.Interval - _cycle.Windup);
                AttackExecuted?.Invoke(executed, damage);
                return;
            }
            if (_cycle == null) return;
            _nextAttackRemaining = Math.Max(0f, _nextAttackRemaining - gameplayDt);
            if (_nextAttackRemaining <= TIMER_EPSILON) TryStartTelegraph(DefenseYakshaAttackTelegraph.TelegraphSlam);
        }
        public bool TryStartTelegraph(DefenseYakshaAttackTelegraph type)
        {
            if (_disposed || IsStopped || Phase != DefenseYakshaPhase.ArenaCombat || Health.IsDead || ActiveTelegraph != DefenseYakshaAttackTelegraph.None ||
                (type != DefenseYakshaAttackTelegraph.TelegraphSlam && type != DefenseYakshaAttackTelegraph.TelegraphCleave) ||
                (_cycle != null && (type != DefenseYakshaAttackTelegraph.TelegraphSlam || _nextAttackRemaining > TIMER_EPSILON))) return false;
            ActiveTelegraph = type;
            TelegraphTimerRemaining = _cycle != null ? _cycle.Windup : type == DefenseYakshaAttackTelegraph.TelegraphSlam ? SlamTelegraphTime : CleaveTelegraphTime;
            AttackTelegraphed?.Invoke(type);
            return !_disposed && !IsStopped && Phase == DefenseYakshaPhase.ArenaCombat && ActiveTelegraph == type;
        }
        private void ClearTelegraph() { ActiveTelegraph = DefenseYakshaAttackTelegraph.None; TelegraphTimerRemaining = 0f; }
        public void StopCombat()
        {
            if (_disposed || IsStopped) return;
            IsStopped = true; ClearTelegraph(); CombatStopped?.Invoke();
        }
        private void OnBossDefeated()
        {
            if (_disposed || !Health.IsDead || Phase == DefenseYakshaPhase.Defeated) return;
            bool alreadyStopped = IsStopped;
            IsStopped = true; ClearTelegraph(); Phase = DefenseYakshaPhase.Defeated;
            PhaseChanged?.Invoke(Phase);
            if (!alreadyStopped && !_disposed) CombatStopped?.Invoke();
        }
        public void Dispose()
        {
            if (_disposed) return;
            try { StopCombat(); }
            finally
            {
                _disposed = true; Health.Defeated -= OnBossDefeated;
                PhaseChanged = null; AttackTelegraphed = null; AttackExecuted = null; CombatStopped = null;
            }
        }
    }
}
