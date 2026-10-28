using System;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Moves an amphibian through water and an explicit shore exit before handing off to Ground.</summary>
    [DisallowMultipleComponent]
    public sealed class DefenseWaterMovement3D : DefenseGameplayBehaviour, IDefenseLocomotionStrategy3D
    {
        public enum Phase { Idle, Swimming, Docking, Ground }
        public bool IsMoving => CurrentPhase == Phase.Swimming || CurrentPhase == Phase.Docking;
        public bool HasFailed => false;
        public void Stop() { CurrentPhase = Phase.Idle; }
        [SerializeField, Min(0.01f)] private float _arrivalDistance = 0.08f;
        [SerializeField, Min(0.1f)] private float _shoreRetrySeconds = 1f;
        [SerializeField, Min(1f)] private float _shoreTimeoutSeconds = 25f;
        private DefenseWaterNavigation3D _navigation;
        private DefenseEnemy3D _enemy;
        private DefenseGroundMovement3D _ground;
        private NavMeshAgent _agent;
        private DefenseGroundAggro3D _groundedAggro;
        private DefenseBasePrototype3D _target;
        private Vector3 _water;
        private Vector3 _shore;
        private Vector3 _landing;
        private float _retryAt;
        private float _timeoutAt;
        public Phase CurrentPhase { get; private set; }

        public void Bind(IDefenseGameplayContext gameplay, DefenseWaterNavigation3D navigation)
        {
            _navigation = navigation != null ? navigation : throw new ArgumentNullException(nameof(navigation));
            _enemy = GetComponent<DefenseEnemy3D>();
            _ground = GetComponent<DefenseGroundMovement3D>();
            _agent = GetComponent<NavMeshAgent>();
            _groundedAggro = GetComponent<DefenseGroundAggro3D>();
            if (_groundedAggro == null) throw new InvalidOperationException("Amphibian prefab requires authored grounded aggro.");
            if (_agent != null) _agent.enabled = false;
            BindGameplay(gameplay);
        }

        public void Configure(int lane, DefenseBasePrototype3D target) => Begin(target);

        public void Begin(DefenseBasePrototype3D target)
        {
            if (_navigation == null || target == null ||
                !_navigation.TryGetExit(transform.position, out _water, out _shore))
                throw new InvalidOperationException("Water actor requires an authored canal exit.");
            _target = target;
            _groundedAggro?.Stop();
            _ground?.Stop();
            if (_agent != null) _agent.enabled = false;
            CurrentPhase = Phase.Swimming;
            _timeoutAt = float.PositiveInfinity;
            _retryAt = GameplayTime;
        }

        private void Update()
        {
            if (!CanAdvanceGameplay || _enemy == null || !_enemy.IsTargetable ||
                CurrentPhase == Phase.Idle || CurrentPhase == Phase.Ground) return;
            if (_target == null || _target.IsDestroyed) return;
            float multiplier = _enemy.Status != null ? _enemy.Status.MovementMultiplier : 1f;
            if ((_enemy.Status != null && _enemy.Status.BlocksActions) || multiplier <= 0f)
            {
                _timeoutAt += Time.deltaTime;
                return;
            }
            _timeoutAt += Time.deltaTime * (1f - multiplier);
            if (GameplayTime >= _timeoutAt)
            {
                CurrentPhase = Phase.Idle;
                _enemy.RequestRelease(DefenseActorReleaseReason.ExternalDisabled);
                return;
            }
            Vector3 destination = CurrentPhase == Phase.Swimming ? _water : _landing;
            if ((transform.position - destination).sqrMagnitude > _arrivalDistance * _arrivalDistance)
            {
                transform.position = Vector3.MoveTowards(transform.position, destination, _enemy.Stats.Speed * multiplier * Time.deltaTime);
                return;
            }
            if (CurrentPhase == Phase.Swimming)
            {
                if (float.IsPositiveInfinity(_timeoutAt)) _timeoutAt = GameplayTime + _shoreTimeoutSeconds;
                if (GameplayTime < _retryAt) return;
                _retryAt = GameplayTime + _shoreRetrySeconds;
                if (!_navigation.TryResolveShore(_shore, _target.transform.position, out _landing)) return;
                // Water travel stays off NavMesh. Ground receives only a validated shore point.
                _landing.y += _agent.baseOffset;
                CurrentPhase = Phase.Docking;
                return;
            }
            if (!_navigation.TryResolveShore(_shore, _target.transform.position, out var landing))
            {
                CurrentPhase = Phase.Swimming;
                return;
            }
            transform.position = landing;
            _agent.enabled = true;
            if (!_agent.Warp(landing))
            {
                _agent.enabled = false;
                CurrentPhase = Phase.Idle;
                _enemy.RequestRelease(DefenseActorReleaseReason.ExternalDisabled);
                return;
            }
            CurrentPhase = Phase.Ground;
            try { _groundedAggro.Configure(1, _target, null, true); }
            catch (Exception exception)
            {
                _groundedAggro.Stop(); CurrentPhase = Phase.Idle;
                Debug.LogWarning("[TREBRAVE Defense3D] Grounded amphibian handoff failed: " + exception.Message, this);
                _enemy.RequestRelease(DefenseActorReleaseReason.ExternalDisabled);
            }
        }

        public bool TryDisplace(Vector3 delta, bool apply)
        {
            if (CurrentPhase != Phase.Swimming || _navigation == null || Mathf.Abs(delta.y) > Mathf.Epsilon ||
                !_navigation.Contains(transform.position) || !_navigation.Contains(transform.position + delta)) return false;
            if (apply) transform.position += delta;
            return true;
        }

        protected override void OnGameplayCleanup() => CurrentPhase = Phase.Idle;
        private void OnDisable() => CurrentPhase = Phase.Idle;
    }
}
