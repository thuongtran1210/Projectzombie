using System;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Base-directed XYZ flight with swept obstacle avoidance; never enables a NavMeshAgent.</summary>
    [DisallowMultipleComponent]
    public sealed class DefenseAirMovement3D : DefenseGameplayBehaviour, IDefenseLocomotionStrategy3D
    {
        public bool IsMoving => _flying;
        public bool HasFailed => false;
        public void Stop() { _flying = false; _focus = null; }
        private DefenseAirNavigation3D _navigation;
        private DefenseEnemy3D _enemy;
        private DefenseEnemyAttack3D _attack;
        [SerializeField] private DefenseAggroProfileSO _aggroProfile;
        private DefenseFriendlyTargets3D _friendlyRoster;
        private DefenseBasePrototype3D _target;
        private Collider _baseCollider;
        private IDefenseAggroCandidate[] _candidates;
        private DefenseAggroRules _aggroRules;
        private DefenseAggroPolicy _policy;
        private Predicate<IDefenseAggroCandidate> _reachable;
        private DefenseFriendlyTarget3D _focus;
        private object _provoker;
        private float _nextThink, _provokedUntil;
        private bool _subscribed;
        private bool _flying;
        private float _blockedSince;
        private float _bestDistance;

        public void Bind(IDefenseGameplayContext gameplay, DefenseAirNavigation3D navigation, DefenseFriendlyTargets3D friends = null)
        {
            _navigation = navigation != null ? navigation : throw new ArgumentNullException(nameof(navigation));
            _enemy = GetComponent<DefenseEnemy3D>();
            _attack = GetComponent<DefenseEnemyAttack3D>();
            if (TryGetComponent<NavMeshAgent>(out var agent)) agent.enabled = false;
            if (_enemy == null || _attack == null ||
                !TryGetComponent<SphereCollider>(out var hitbox) || !hitbox.isTrigger)
                throw new InvalidOperationException("Air prefab requires Enemy, Attack and authored spherical trigger hitbox.");
            Vector3 scale = transform.lossyScale;
            float radius = hitbox.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            if (hitbox.center != Vector3.zero || radius > _navigation.Profile.ClearanceRadius)
                throw new InvalidOperationException("Air clearance must enclose the authored centered hitbox.");
            if (_aggroProfile == null || friends == null || friends.Candidates == null)
                throw new InvalidOperationException("Air aggro requires an authored profile and bound friendly roster.");
            _aggroRules = _aggroProfile.Capture();
            _policy = new DefenseAggroPolicy(_aggroRules);
            _friendlyRoster = friends;
            _candidates = friends.Candidates;
            _reachable = CanReach;
            if (!_subscribed) { _enemy.Damaged += OnDamaged; _subscribed = true; }
            BindGameplay(gameplay);
        }

        public void Configure(int lane, DefenseBasePrototype3D target) => Begin(target);

        public void Begin(DefenseBasePrototype3D target)
        {
            if (_navigation == null || target == null || !_navigation.CanOccupy(transform.position))
                throw new InvalidOperationException("Air actor requires a valid authored flight spawn.");
            _target = target;
            _target.TryGetComponent(out _baseCollider);
            _attack.BindAreaTargets(_friendlyRoster, target);
            _focus = null; _policy.Reset(); _provoker = null; _provokedUntil = 0f; _nextThink = 0f;
            _attack.Begin(target, _baseCollider);
            _flying = true;
            _blockedSince = GameplayTime;
            _bestDistance = float.PositiveInfinity;
        }

        private void Update()
        {
            if (!_flying || !CanAdvanceGameplay || !_enemy.IsTargetable || _target == null || _target.IsDestroyed) return;
            if (_enemy.Status != null && _enemy.Status.BlocksActions)
            {
                _blockedSince = GameplayTime;
                return;
            }
            if (_attack.IsWindingUp)
            {
                _blockedSince = GameplayTime;
                return;
            }
            if (GameplayTime >= _nextThink || (_focus != null && !_focus.IsAvailable))
            {
                _nextThink = GameplayTime + _aggroRules.ThinkSeconds;
                var selected = _policy.Select(GameplayTime, Point(transform.position), _candidates, true, _reachable,
                    GameplayTime < _provokedUntil ? _provoker : null) as DefenseFriendlyTarget3D;
                if (!ReferenceEquals(selected, _focus)) SetFocus(selected);
            }
            if (_attack.IsTargetInRange)
            {
                _blockedSince = GameplayTime;
                return;
            }
            if (_enemy.Stats.Speed * (_enemy.Status != null ? _enemy.Status.MovementMultiplier : 1f) <= 0f)
            {
                _blockedSince = GameplayTime;
                return;
            }
            Vector3 current = transform.position;
            Vector3 destination = _focus != null ? _focus.Transform.position : _target.transform.position;
            destination.y = _navigation.Profile.CruiseHeight;
            float remaining = (destination - current).sqrMagnitude;
            if (remaining + 0.01f < _bestDistance)
            {
                _bestDistance = remaining;
                _blockedSince = GameplayTime;
            }
            if (GameplayTime - _blockedSince >= _navigation.Profile.BlockedTimeout)
            {
                if (_focus != null)
                {
                    _policy.Reset(); SetFocus(null);
                    _nextThink = GameplayTime + _aggroRules.LockSeconds;
                    return;
                }
                _flying = false;
                _attack.StopAttack();
                Debug.LogWarning("[TREBRAVE Defense3D] Air route blocked; releasing without reward: " + name);
                _enemy.RequestRelease(DefenseActorReleaseReason.ExternalDisabled);
                return;
            }
            float step = _enemy.Stats.Speed * (_enemy.Status != null ? _enemy.Status.MovementMultiplier : 1f) * Time.deltaTime;
            if (step <= 0f) return;
            Vector3 direction = (destination - current).normalized;
            Vector3 side = Vector3.Cross(Vector3.up, direction).normalized;
            // Bounded probes, no route allocations or NavMesh fallback. Occupancy is rechecked for every step.
            for (int i = 0; i < 5; i++)
            {
                Vector3 candidateDirection = i == 0 ? direction : i == 1 ? Vector3.up :
                    i == 2 ? side : i == 3 ? -side : Vector3.down;
                Vector3 next = current + candidateDirection * Mathf.Min(step, Mathf.Sqrt(remaining));
                if (!_navigation.CanMove(current, next)) continue;
                transform.position = next;
                return;
            }
        }

        public bool TryDisplace(Vector3 delta, bool apply)
        {
            if (!_flying || _navigation == null || !_navigation.CanMove(transform.position, transform.position + delta)) return false;
            if (apply) { transform.position += delta; _blockedSince = GameplayTime; _bestDistance = float.PositiveInfinity; }
            return true;
        }
        private bool CanReach(IDefenseAggroCandidate candidate)
        {
            if (!(candidate is DefenseFriendlyTarget3D target) || target.Transform == null) return false;
            Vector3 destination = target.Transform.position;
            destination.y = _navigation.Profile.CruiseHeight;
            return _navigation.CanMove(transform.position, destination);
        }
        private void SetFocus(DefenseFriendlyTarget3D target)
        {
            _focus = target;
            _bestDistance = float.PositiveInfinity; _blockedSince = GameplayTime;
            if (_focus != null) _attack.Begin(_focus.Damageable, _focus.Collider);
            else _attack.Begin(_target, _baseCollider);
        }
        private void OnDamaged(DefenseDamageContext context)
        {
            if (!_flying || _candidates == null) return;
            for (int i = 0; i < _candidates.Length; i++)
                if (_candidates[i] is DefenseFriendlyTarget3D target && target.MatchesSource(context.Source))
                { _provoker = target.Identity; _provokedUntil = GameplayTime + _aggroRules.ProvokedSeconds; return; }
        }
        private static DefensePoint3 Point(Vector3 p) => new DefensePoint3(p.x, p.y, p.z);
        protected override void OnGameplayCleanup() { _flying = false; _focus = null; _policy?.Reset(); }
        protected override void OnDestroy()
        {
            if (_subscribed && _enemy != null) _enemy.Damaged -= OnDamaged;
            base.OnDestroy();
        }
        private void OnDisable() => _flying = false;
    }
}
