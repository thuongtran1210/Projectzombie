using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Session/actor adapter for the single Boss FSM. No duplicate phase flags, arena timer or HP.</summary>
    public sealed class DefenseYakshaEncounter3D : IDisposable
    {
        private readonly IDefenseGameplayContext _gameplay;
        private readonly DefenseBasePrototype3D _base;
        private readonly DefenseHeroHealth3D _hero;
        private readonly float _arenaDuration, _bossHealth;
        private DefenseYakshaArenaCombat3D _combat;
        private DefenseYakshaBehaviorFSM _state;
        private DefenseEnemy3D _enemy;
        private DefenseActorLifetime _lifetime;
        private object _identity;
        private int _generation;
        private float _lastTickTime;
        private bool _disposed;
        private Action<DefenseYakshaPhase> _phaseObserver;
        private Action _stopObserver;
        public DefenseEnemyBridgeWalker3D Boss { get; private set; }
        public DefenseYakshaPhase Phase => CurrentScope && _state != null ? _state.Phase : DefenseYakshaPhase.Dormant;
        private bool CurrentScope => ReferenceEquals(_identity, _gameplay.SessionIdentity) && _generation == _gameplay.RunGeneration;
        public bool HasSpawned => CurrentScope && _state != null && _state.HasSpawned;
        public bool AssaultStarted => CurrentScope && _state != null && _state.HasEnteredAssault;
        public bool IsDefeated => CurrentScope && _state != null && _state.Phase == DefenseYakshaPhase.Defeated;
        public event Action Defeated;
        public DefenseYakshaEncounter3D(IDefenseGameplayContext gameplay, DefenseBasePrototype3D defenseBase,
            DefenseHeroHealth3D hero, float arenaDuration, float bossHealth)
        {
            _gameplay = gameplay ?? throw new ArgumentNullException(nameof(gameplay));
            _base = defenseBase != null ? defenseBase : throw new ArgumentNullException(nameof(defenseBase));
            _hero = hero != null ? hero : throw new ArgumentNullException(nameof(hero));
            if (!DefenseDamageRules.IsFinite(arenaDuration) || arenaDuration <= 0f || !DefenseDamageRules.IsFinite(bossHealth) || bossHealth <= 0f)
                throw new ArgumentException("Arena duration and boss health must be finite and positive.");
            _arenaDuration = arenaDuration; _bossHealth = bossHealth;
        }
        public void Begin(DefenseEnemyBridgeWalker3D boss, DefenseYakshaArenaCombat3D combat)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DefenseYakshaEncounter3D));
            if (_state != null) throw new InvalidOperationException("Yaksha has already been acquired in this run.");
            if (!_gameplay.AdvancesGameplay || boss == null || combat == null || !combat.HasWarningRenderer || boss.gameObject != combat.gameObject)
                throw new ArgumentException("Boss requires a live session and authored Walker/Arena Combat on the same actor.");
            var enemy = boss.GetComponent<DefenseEnemy3D>();
            if (enemy == null || enemy.Lifetime == null || !enemy.Lifetime.IsCurrent(_gameplay))
                throw new InvalidOperationException("Encounter must receive the current session-owned Boss lifetime.");
            var cycle = combat.CaptureCycle();
            try
            {
                Boss = boss; _enemy = enemy; _combat = combat; _lifetime = enemy.Lifetime;
                _identity = _gameplay.SessionIdentity; _generation = _gameplay.RunGeneration; _lastTickTime = _gameplay.ElapsedSeconds;
                boss.PrepareArenaBoss(_bossHealth);
                _state = new DefenseYakshaBehaviorFSM(enemy.HealthAuthority, _arenaDuration, cycle);
                combat.BindGameplay(_gameplay);
                combat.BindState(_state, _hero);
                var capturedState = _state;
                _phaseObserver = phase => { if (ReferenceEquals(_state, capturedState)) OnPhaseChanged(phase); };
                _stopObserver = () => { if (ReferenceEquals(_state, capturedState)) OnCombatStopped(); };
                _state.PhaseChanged += _phaseObserver; _state.CombatStopped += _stopObserver;
                _state.SpawnInArena();
            }
            catch { Reset(); throw; }
        }
        public void Tick()
        {
            if (_disposed || _state == null || !CurrentScope || !_gameplay.AdvancesGameplay || IsDefeated) return;
            if (!ReferenceEquals(_enemy, null) && ReferenceEquals(_enemy.Lifetime, _lifetime) && _state.Health.IsDead)
            {
                _state.Tick(0f, false);
                return;
            }
            if (_enemy == null || !ReferenceEquals(_enemy.Lifetime, _lifetime) || !_enemy.HasCurrentLifetime || !_enemy.gameObject.activeInHierarchy)
            {
                // External removal is not a kill. Detach health listeners before an actor could be reused.
                if (!_state.IsStopped) Debug.LogWarning("[TREBRAVE Defense3D] Yaksha lifetime ended without defeat; encounter stopped without granting a kill.");
                _state.Dispose();
                return;
            }
            float now = _gameplay.ElapsedSeconds;
            float delta = Math.Max(0f, now - _lastTickTime); _lastTickTime = now;
            _state.Tick(delta, false);
        }
        private void OnPhaseChanged(DefenseYakshaPhase phase)
        {
            if (_disposed || !CurrentScope || _state == null) return;
            if (phase == DefenseYakshaPhase.AssaultBase)
            {
                if (_enemy == null || !_enemy.IsTargetable || !ReferenceEquals(_enemy.Lifetime, _lifetime)) { StopCombat(); return; }
                Boss.BeginBossAssault(_base);
                if (Boss.GetComponent<DefenseGroundMovement3D>().HasFailed)
                {
                    StopCombat();
                    Debug.LogWarning("[TREBRAVE Defense3D] Yaksha assault route failed; combat stopped without resetting HP or declaring a kill.");
                }
            }
            else if (phase == DefenseYakshaPhase.Defeated) Defeated?.Invoke();
        }
        private void OnCombatStopped()
        {
            if (_combat != null) _combat.HideWarning();
            if (Boss != null) Boss.StopBossCombat();
        }
        public void StopCombat()
        {
            try { _state?.StopCombat(); }
            finally { OnCombatStopped(); }
        }
        public void Reset()
        {
            var state = _state; _state = null;
            try
            {
                if (state != null)
                {
                    state.PhaseChanged -= _phaseObserver; state.CombatStopped -= _stopObserver;
                    state.Dispose();
                }
            }
            finally
            {
                OnCombatStopped();
                _phaseObserver = null; _stopObserver = null;
                Boss = null; _combat = null; _enemy = null; _lifetime = null; _identity = null; _generation = 0; _lastTickTime = 0f;
            }
        }
        public void Dispose()
        {
            if (_disposed) return;
            try { Reset(); }
            finally { _disposed = true; Defeated = null; }
        }
    }
}
