using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Scoped status receiver and movement-domain displacement adapter. No health, reward or actor ownership.</summary>
    [DefaultExecutionOrder(-100), DisallowMultipleComponent]
    public sealed class DefenseEnemyStatus3D : DefenseGameplayBehaviour, IDefenseStatusReceiver
    {
        [SerializeField] private float _maxDuration = 10f;
        [SerializeField] private float _maximumKnockDistance = 2f;
        [SerializeField] private int _slowCapacity = 8;
        [SerializeField] private float _displacementRadius = 0.45f;
        [SerializeField] private float _bodyHalfSegment = 0.5f;
        [SerializeField] private LayerMask _displacementMask = 4105;
        private DefenseEnemy3D _enemy;
        private DefenseGroundMovement3D _ground;
        private DefenseEnemyAttack3D _attack;
        private DefenseWaterMovement3D _water;
        private DefenseAirMovement3D _air;
        private DefenseStatusRuntime _runtime;
        private bool _subscribed;
        public bool BlocksActions => _runtime != null && _runtime.BlocksActions;
        public float MovementMultiplier => _runtime != null ? _runtime.MovementMultiplier : 1f;
        protected override void OnGameplayBound()
        {
            _enemy = GetComponent<DefenseEnemy3D>(); _ground = GetComponent<DefenseGroundMovement3D>();
            _attack = GetComponent<DefenseEnemyAttack3D>();
            _water = GetComponent<DefenseWaterMovement3D>(); _air = GetComponent<DefenseAirMovement3D>();
            if (_enemy == null || _ground == null) throw new InvalidOperationException("Status requires authored Enemy/Ground components.");
            if (!DefenseDamageRules.IsFinite(_displacementRadius) || _displacementRadius <= 0f ||
                !DefenseDamageRules.IsFinite(_bodyHalfSegment) || _bodyHalfSegment < 0f || _displacementMask.value == 0)
                throw new InvalidOperationException("Status displacement requires finite positive clearance and an obstacle mask.");
            _runtime = new DefenseStatusRuntime(Gameplay, _maxDuration, _maximumKnockDistance, _slowCapacity);
            _ground.BindStatus(this);
            if (!_subscribed) { _enemy.Initialized += Clear; _subscribed = true; }
        }
        public bool TryApplyStatus(DefenseStatusRequest request)
        {
            if (!CanAdvanceGameplay || _enemy == null || !_enemy.IsTargetable || _runtime == null || !request.IsValid ||
                request.Duration > _maxDuration || !ReferenceEquals(request.SessionIdentity, Gameplay.SessionIdentity) ||
                request.RunGeneration != Gameplay.RunGeneration) return false;
            if (request.Kind == DefenseStatusKind.Knockback)
            {
                var direction = request.Direction;
                if (_enemy.Stats.Locomotion != DefenseEnemyLocomotionType.AirFlight) direction = new DefensePoint3(direction.X, 0f, direction.Z);
                request = new DefenseStatusRequest(request.Kind, request.Duration, request.Strength, direction, request.SessionIdentity, request.RunGeneration);
                if (!DefenseStatusRuntime.TryKnockVector(request, _maximumKnockDistance, out var vector) ||
                    !Displace(new Vector3(vector.X, vector.Y, vector.Z), false)) return false;
            }
            bool applied = _runtime.TryApply(request);
            if (applied)
            {
                _ground.ApplyMotionState();
                if (_runtime.BlocksActions) _attack?.CancelPendingAttack();
            }
            return applied;
        }
        private bool Displace(Vector3 delta, bool apply)
        {
            if (_air != null) return _air.TryDisplace(delta, apply);
            Vector3 lower = transform.position - Vector3.up * _bodyHalfSegment;
            Vector3 upper = transform.position + Vector3.up * _bodyHalfSegment;
            float distance = delta.magnitude;
            if (Physics.CheckCapsule(lower + delta, upper + delta, _displacementRadius, _displacementMask, QueryTriggerInteraction.Ignore) ||
                (distance > Mathf.Epsilon && Physics.CapsuleCast(lower, upper, _displacementRadius, delta / distance, out _,
                    distance, _displacementMask, QueryTriggerInteraction.Ignore))) return false;
            if (_water != null && _water.CurrentPhase != DefenseWaterMovement3D.Phase.Ground)
                return _water.TryDisplace(delta, apply);
            return _ground.TryDisplace(delta, apply);
        }
        private void Update()
        {
            if (!CanAdvanceGameplay || _runtime == null || _enemy == null || !_enemy.IsTargetable) return;
            bool wasKnocking = _runtime.IsKnocking;
            if (_runtime.TryConsumeKnockStep(out var step) && !Displace(new Vector3(step.X, step.Y, step.Z), true)) _runtime.CancelKnockback();
            if (wasKnocking && !_runtime.IsKnocking)
            {
                _ground.ResumeAfterDisplacement();
                if (_ground.HasFailed) _enemy.RequestRelease(DefenseActorReleaseReason.ExternalDisabled);
            }
        }
        private void Clear() { _runtime?.Reset(); _ground?.ApplyMotionState(); }
        protected override void OnGameplayCleanup() => Clear();
        protected override void OnGameplayReset() => Clear();
        private void OnDisable() => Clear();
        protected override void OnDestroy()
        {
            if (_subscribed && _enemy != null) _enemy.Initialized -= Clear;
            base.OnDestroy();
        }
    }
}
