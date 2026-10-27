using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Scene executor for FSM warning/hit intents. Owns no phase, scheduling timer or Boss health.</summary>
    public sealed class DefenseYakshaArenaCombat3D : DefenseGameplayBehaviour
    {
        [SerializeField, Min(0.5f)] private float blastRadius = 5f;
        [SerializeField, Min(0.2f)] private float blastInterval = 3f;
        [SerializeField, Min(0.1f)] private float warningDuration = 1.1f;
        [SerializeField, Min(0f)] private float blastDamage = 30f;
        [SerializeField] private LineRenderer ring;
        private DefenseHeroHealth3D _hero;
        private DefenseEnemy3D _enemy;
        private DefenseYakshaBehaviorFSM _state;
        private DefenseActorLifetime _lifetime;
        private int _generation;
        private float _radius;
        private Vector3 _blastOrigin;
        private Action<DefenseYakshaAttackTelegraph> _warningObserver;
        private Action<DefenseYakshaAttackTelegraph, float> _hitObserver;
        private Action<DefenseYakshaPhase> _phaseObserver;
        private Action _stopObserver;
        public bool HasWarningRenderer => ring != null && ring.positionCount >= 3;
        private bool WarningActive => _state != null && !_state.IsStopped && _state.Phase == DefenseYakshaPhase.ArenaCombat &&
            _state.ActiveTelegraph == DefenseYakshaAttackTelegraph.TelegraphSlam;

        public DefenseYakshaArenaCycle CaptureCycle()
        {
            if (!HasWarningRenderer || !DefenseDamageRules.IsFinite(blastRadius) || blastRadius <= 0f)
                throw new InvalidOperationException("Yaksha requires an authored warning renderer and finite positive blast radius.");
            return new DefenseYakshaArenaCycle(blastInterval, warningDuration, blastDamage);
        }
        public void BindState(DefenseYakshaBehaviorFSM state, DefenseHeroHealth3D hero)
        {
            if (Gameplay == null || state == null || hero == null) throw new InvalidOperationException("Bind gameplay and the encounter FSM before arena execution.");
            DetachState();
            _enemy = GetComponent<DefenseEnemy3D>();
            if (_enemy == null || _enemy.Lifetime == null || !_enemy.Lifetime.IsCurrent(Gameplay) || !ReferenceEquals(state.Health, _enemy.HealthAuthority))
                throw new InvalidOperationException("Boss FSM must share the existing actor health authority.");
            _state = state; _hero = hero; _radius = blastRadius;
            _lifetime = _enemy.Lifetime; _generation = Gameplay.RunGeneration;
            var capturedState = state;
            _warningObserver = type => { if (ReferenceEquals(_state, capturedState)) OnTelegraphed(type); };
            _hitObserver = (type, damage) => { if (ReferenceEquals(_state, capturedState)) OnExecuted(type, damage); };
            _phaseObserver = phase => { if (ReferenceEquals(_state, capturedState)) OnPhaseChanged(phase); };
            _stopObserver = () => { if (ReferenceEquals(_state, capturedState)) HideWarning(); };
            _state.AttackTelegraphed += _warningObserver;
            _state.AttackExecuted += _hitObserver;
            _state.PhaseChanged += _phaseObserver;
            _state.CombatStopped += _stopObserver;
            HideWarning();
        }
        /// <summary>Compatibility entry point; an encounter-owned FSM is mandatory, no autonomous arena is created.</summary>
        public void Begin(DefenseHeroHealth3D heroHealth)
        {
            if (_state == null) throw new InvalidOperationException("Arena Begin requires an encounter-bound FSM; autonomous arena updates were removed.");
            _hero = heroHealth;
        }
        public void EndArena() { _state?.StopCombat(); HideWarning(); }
        public void HideWarning() { if (ring != null) ring.enabled = false; }
        private bool CanExecute => CanAdvanceGameplay && _state != null && !_state.IsStopped &&
            _state.Phase == DefenseYakshaPhase.ArenaCombat && Gameplay.RunGeneration == _generation && _enemy != null &&
            ReferenceEquals(_enemy.Lifetime, _lifetime) && _lifetime.IsCurrent(Gameplay) && _enemy.IsTargetable;
        private void OnTelegraphed(DefenseYakshaAttackTelegraph type)
        {
            if (!CanExecute || type != DefenseYakshaAttackTelegraph.TelegraphSlam) return;
            _blastOrigin = transform.position;
            UpdateRingPosition();
            ring.enabled = true;
        }
        private void OnExecuted(DefenseYakshaAttackTelegraph type, float damage)
        {
            HideWarning();
            if (!CanExecute || type != DefenseYakshaAttackTelegraph.TelegraphSlam || _hero == null || !_hero.isActiveAndEnabled || _hero.IsDown) return;
            Vector3 delta = _hero.transform.position - _blastOrigin; delta.y = 0f;
            if (delta.sqrMagnitude > _radius * _radius) return;
            _hero.TakeDamage(new DefenseDamageContext(damage, this, _hero.transform.position,
                runGeneration: _generation, sessionIdentity: Gameplay.SessionIdentity));
        }
        private void OnPhaseChanged(DefenseYakshaPhase phase) { if (phase != DefenseYakshaPhase.ArenaCombat) HideWarning(); }
        protected override void OnGameplayGateChanged(bool active)
        {
            if (!active) HideWarning();
            else if (WarningActive && CanExecute) { UpdateRingPosition(); ring.enabled = true; }
        }
        private void UpdateRingPosition()
        {
            if (ring == null) return;
            for (int i = 0; i < ring.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2f / ring.positionCount;
                ring.SetPosition(i, _blastOrigin + new Vector3(Mathf.Cos(angle) * _radius, 0.35f, Mathf.Sin(angle) * _radius));
            }
        }
        private void DetachState()
        {
            var state = _state; _state = null;
            if (state != null)
            {
                state.AttackTelegraphed -= _warningObserver; state.AttackExecuted -= _hitObserver;
                state.PhaseChanged -= _phaseObserver; state.CombatStopped -= _stopObserver;
            }
            _warningObserver = null; _hitObserver = null; _phaseObserver = null; _stopObserver = null;
            HideWarning();
        }
        protected override void OnGameplayCleanup() => DetachState();
        private void OnDisable() => HideWarning();
        protected override void OnDestroy() { DetachState(); base.OnDestroy(); }
    }
}
