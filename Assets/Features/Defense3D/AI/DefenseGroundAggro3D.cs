using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Ground route/chase/attack coordinator, including explicit amphibian/landing handoff. Boss remains separate.</summary>
    public sealed class DefenseGroundAggro3D : DefenseGameplayBehaviour, IDefenseTargetingStrategy3D
    {
        [SerializeField] private DefenseEnemy3D _enemy;
        [SerializeField] private DefenseGroundMovement3D _movement;
        [SerializeField] private DefenseEnemyAttack3D _attack;
        [SerializeField] private DefenseGroundNavigationProfileSO _profile;
        [SerializeField] private DefenseAggroProfileSO _aggroProfile;
        private DefenseGroundNavigationRules _rules;
        private DefenseAggroRules _aggroRules;
        private DefenseAggroPolicy _policy;
        private DefenseFriendlyTargets3D _friends;
        private IDefenseAggroCandidate[] _candidates;
        private Predicate<IDefenseAggroCandidate> _reachable;
        private DefenseFriendlyTarget3D _focus;
        private object _provoker;
        private DefenseBasePrototype3D _base;
        private Collider _baseCollider;
        private int _lane;
        private float _nextThink, _provokedUntil;
        private bool _bound, _configured;
        private readonly DefensePoint3[] _points = new DefensePoint3[4];
        private readonly Vector3[] _route = new Vector3[4];
        public bool OwnsCombat => _configured;
        public void BindFriendlyTargets(DefenseFriendlyTargets3D friends) => _friends = friends;

        protected override void OnGameplayBound()
        {
            if (_enemy == null || _movement == null || _attack == null || _profile == null || _aggroProfile == null)
                throw new InvalidOperationException("Ground aggro requires authored Enemy/Movement/Attack/Navigation/Aggro refs.");
            if (_bound) return;
            _reachable = CanReach;
            _enemy.Damaged += OnDamaged;
            _enemy.Defeated += Stop;
            _movement.Arrived += OnArrived;
            _bound = true;
        }

        public void Configure(int lane, DefenseBasePrototype3D target, DefenseHeroHealth3D hero, bool grounded = false)
        {
            if (_enemy.Stats.IsBoss || _enemy.Stats.Locomotion == DefenseEnemyLocomotionType.AirFlight ||
                (_enemy.Stats.Locomotion != DefenseEnemyLocomotionType.GroundNavMesh && !grounded))
                throw new InvalidOperationException("Ground aggro requires a Ground actor or explicit completed ground handoff.");
            if (_friends != null && _friends.Candidates == null)
                throw new InvalidOperationException("Bind the friendly roster before configuring Ground aggro.");
            _rules = _profile.Capture();
            _aggroRules = _aggroProfile.Capture();
            _attack.BindGroundRules(_rules);
            _policy = new DefenseAggroPolicy(_aggroRules);
            _candidates = _friends != null ? _friends.Candidates : hero != null ?
                new IDefenseAggroCandidate[] { new DefenseFriendlyTarget3D(hero, hero, DefenseFriendlyKind.Hero) } : Array.Empty<IDefenseAggroCandidate>();
            _base = target; _lane = lane; _focus = null;
            if (_base == null) throw new ArgumentNullException(nameof(target));
            _base.TryGetComponent(out _baseCollider);
            _configured = true; _nextThink = 0f; _provokedUntil = 0f; _provoker = null;
            if (!MarchToBase(false)) throw new InvalidOperationException("Ground handoff requires a complete bank-to-Base route.");
        }
        private bool MarchToBase(bool allowExistingBridge)
        {
            _attack.StopAttack();
            if (!_rules.TryWriteRoute(Point(transform.position), Point(_base.transform.position), _lane, _points, out int count, allowExistingBridge)) return false;
            for (int i = 0; i < count; i++) _route[i] = new Vector3(_points[i].X, _points[i].Y, _points[i].Z);
            _movement.FollowBuffer(_route, count, _enemy.Stats.Speed, _enemy.Stats.AttackRange + Extent(_baseCollider));
            return !_movement.HasFailed;
        }
        private bool CanReach(IDefenseAggroCandidate candidate)
        {
            return candidate is DefenseFriendlyTarget3D target && target.Transform != null &&
                _rules.SameBank(Point(transform.position), candidate.Position) && _movement.CanReach(target.Transform.position);
        }
        private void Update()
        {
            if (!_configured || !CanAdvanceGameplay || !_enemy.IsTargetable || _base == null || (_enemy.Status != null && _enemy.Status.BlocksActions)) return;
            if (GameplayTime < _nextThink && (_focus == null || _focus.IsAvailable)) return;
            _nextThink = GameplayTime + _aggroRules.ThinkSeconds;
            var previous = _focus;
            _focus = _policy.Select(GameplayTime, Point(transform.position), _candidates, false, _reachable,
                GameplayTime < _provokedUntil ? _provoker : null) as DefenseFriendlyTarget3D;
            if (_focus == null)
            {
                if (previous != null && !MarchToBase(true)) RetireBlocked();
                return;
            }
            _attack.Begin(_focus.Damageable, _focus.Collider);
            if (_attack.IsTargetInRange) _movement.Stop();
            else
            {
                _movement.Pursue(_focus.Transform.position, _enemy.Stats.Speed, _enemy.Stats.AttackRange + Extent(_focus.Collider));
                if (_movement.HasFailed)
                {
                    _policy.Reset(); _focus = null;
                    if (!MarchToBase(true)) RetireBlocked();
                }
            }
        }
        private void RetireBlocked()
        {
            Stop();
            Debug.LogWarning("[TREBRAVE Defense3D] Ground retarget lost a complete lane route; retiring actor.", this);
            _enemy.RequestRelease(DefenseActorReleaseReason.ExternalDisabled);
        }
        private void OnArrived()
        {
            if (!_configured || !CanAdvanceGameplay || !_enemy.IsTargetable) return;
            if (_focus != null && _focus.IsAvailable) _attack.Begin(_focus.Damageable, _focus.Collider);
            else if (_base != null && !_base.IsDestroyed) _attack.Begin(_base, _baseCollider);
        }
        private void OnDamaged(DefenseDamageContext context)
        {
            if (!_configured || _candidates == null) return;
            for (int i = 0; i < _candidates.Length; i++)
                if (_candidates[i] is DefenseFriendlyTarget3D target && target.MatchesSource(context.Source))
                {
                    _provoker = target.Identity; _provokedUntil = GameplayTime + _aggroRules.ProvokedSeconds; return;
                }
        }
        public void Stop()
        {
            _configured = false; _focus = null; _policy?.Reset();
            _movement?.Stop(); _attack?.StopAttack(); _attack?.BindGroundRules(null);
        }
        private static float Extent(Collider collider) => collider != null ? Mathf.Max(collider.bounds.extents.x, collider.bounds.extents.z) : 0f;
        private static DefensePoint3 Point(Vector3 p) => new DefensePoint3(p.x, p.y, p.z);
        protected override void OnGameplayCleanup() => Stop();
        private void OnDisable() => Stop();
        protected override void OnDestroy()
        {
            if (_bound)
            {
                if (_enemy != null) { _enemy.Damaged -= OnDamaged; _enemy.Defeated -= Stop; }
                if (_movement != null) _movement.Arrived -= OnArrived;
            }
            base.OnDestroy();
        }
    }
}
