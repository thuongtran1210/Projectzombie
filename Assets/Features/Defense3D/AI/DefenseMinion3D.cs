using System;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>One pre-authored reusable friendly actor: fight, carry physical wood, or heal living allies.</summary>
    public sealed class DefenseMinion3D : DefenseGameplayBehaviour
    {
        [SerializeField] private DefenseMinionDefinitionSO _definition;
        [SerializeField] private DefenseSupportTarget3D _health;
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private Transform _visualBody;
        [SerializeField] private Transform[] _legs;
        [SerializeField] private UI.DefenseMinionHealthView3D _healthView;
        private DefenseVillageShop3D _shop;
        private NavMeshPath _path;
        private Predicate<IDefenseTargetPosition> _accepts;
        private Vector3 _bodyOrigin;
        private Quaternion[] _legRotations;
        private float _nextThink, _nextAction, _walkTime;
        private float _metaHpMultiplier = 1f;
        private float _metaSpeedMultiplier = 1f;

        public void ApplyMetaMultipliers(float hpMultiplier, float speedMultiplier)
        {
            _metaHpMultiplier = Mathf.Max(0.1f, hpMultiplier);
            _metaSpeedMultiplier = Mathf.Max(0.1f, speedMultiplier);
        }
        private int _cargo, _combo;
        private bool _bound;
        public DefenseMinionDefinitionSO Definition => _definition;
        public DefenseSupportTarget3D Health => _health;
        public bool IsAvailable => isActiveAndEnabled && !_health.IsDead;
        public Vector3 FormationOffset { get; private set; }

        private void Awake() => EnsureAgent();

        private void EnsureAgent()
        {
            if (_path == null) _path = new NavMeshPath();
            if (_agent == null) _agent = GetComponent<NavMeshAgent>();
        }

        public void Bind(DefenseVillageShop3D shop, IDefenseGameplayContext gameplay)
        {
            // Authored pool slots are inactive, so binding can precede their first Awake.
            EnsureAgent();
            _shop = shop;
            if (!_bound)
            {
                _bound = true;
                _accepts = AcceptTarget;
                _bodyOrigin = _visualBody != null ? _visualBody.localPosition : Vector3.zero;
                _legRotations = new Quaternion[_legs.Length];
                for (int i = 0; i < _legs.Length; i++) _legRotations[i] = _legs[i].localRotation;
                _health.Defeated += OnDefeated;
                _health.HealthChanged += RefreshHealth;
            }
            _health.BindGameplay(gameplay);
            BindGameplay(gameplay);
        }

        public bool Deploy(Vector3 point, int index)
        {
            if (gameObject.activeSelf || !CanAdvanceGameplay || (transform.parent != null && !transform.parent.gameObject.activeInHierarchy)) return false;
            EnsureAgent();
            if (_agent == null) return false;
            _cargo = _combo = 0;
            _nextThink = _nextAction = GameplayTime;
            _walkTime = 0f;
            FormationOffset = new Vector3(-(index / 3 % 3) * .45f, 0f, (index % 3 - 1) * .8f);
            transform.position = point;
            if (_definition != null && _definition.MaxHp > 0f) _health.ConfigureCapacity(_definition.MaxHp * _metaHpMultiplier);
            _health.ResetForRecruit();
            gameObject.SetActive(true);
            _agent.enabled = true;
            _agent.speed = _definition.Speed * _metaSpeedMultiplier;
            if (!_agent.Warp(point)) { Retire(); return false; }
            _agent.isStopped = false;
            return true;
        }

        public void Retire()
        {
            _cargo = 0;
            Stop();
            if (_agent != null) _agent.enabled = false;
            if (_visualBody != null) _visualBody.localPosition = _bodyOrigin;
            if (_legRotations != null && _legs != null)
                for (int i = 0; i < _legs.Length; i++)
                    if (_legs[i] != null) _legs[i].localRotation = _legRotations[i];
            if (this != null && gameObject != null) gameObject.SetActive(false);
        }

        private void OnDefeated()
        {
            int cargo = _cargo;
            _cargo = 0;
            _shop.DropCargo(transform.position, cargo);
            Retire();
            _shop.NotifyChanged();
        }

        public bool CanReach(Vector3 point)
        {
            if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh || !NavMesh.SamplePosition(point, out var hit, 2.5f, _agent.areaMask)) return false;
            return _agent.CalculatePath(hit.position, _path) && _path.status == NavMeshPathStatus.PathComplete;
        }

        private IMinionRoleBehavior _behavior;
        public IMinionRoleBehavior Behavior => _behavior;
        public float CurrentGameplayTime => GameplayTime;
        public bool IsGameplayAdvancing => CanAdvanceGameplay;
        public int Cargo { get => _cargo; set => _cargo = value; }
        public float NextActionTime { get => _nextAction; set => _nextAction = value; }
        public int AdvanceCombo() => ++_combo;
        public Predicate<IDefenseTargetPosition> TargetAcceptancePredicate => _accepts;

        public bool SampleAreaPosition(Vector3 point, out NavMeshHit hit, float maxDistance)
        {
            int mask = _agent != null ? _agent.areaMask : NavMesh.AllAreas;
            return NavMesh.SamplePosition(point, out hit, maxDistance, mask);
        }

        public bool RaycastNavMesh(Vector3 source, Vector3 target)
        {
            int mask = _agent != null ? _agent.areaMask : NavMesh.AllAreas;
            return NavMesh.Raycast(source, target, out _, mask);
        }

        public bool RepairStructure(IDefenseDamageable3D structure)
        {
            if (structure is DefenseBasePrototype3D home) return home.Heal(_definition.BaseRepairAmount);
            if (structure is DefenseSupportTarget3D support && support.IsStructure) return support.Heal(_definition.RepairAmount);
            return false;
        }

        private void Update()
        {
            if (!CanAdvanceGameplay || !IsAvailable) { Stop(); return; }
            if (GameplayTime >= _nextThink)
            {
                _nextThink = GameplayTime + _definition.ThinkInterval;
                if (_behavior == null && _definition != null)
                    _behavior = MinionBehaviorFactory.GetBehavior(_definition.Role);
                _behavior?.Tick(this, _shop, Gameplay);
            }
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh && _agent.velocity.sqrMagnitude > .04f)
            {
                _walkTime += Time.deltaTime * 8f;
                if (_visualBody != null) _visualBody.localPosition = _bodyOrigin + Vector3.up * (Mathf.Abs(Mathf.Sin(_walkTime)) * .035f);
                if (_legs != null && _legRotations != null)
                {
                    for (int i = 0; i < _legs.Length; i++)
                        if (_legs[i] != null) _legs[i].localRotation = _legRotations[i] * Quaternion.Euler(Mathf.Sin(_walkTime) * (i % 2 == 0 ? 25f : -25f), 0f, 0f);
                }
            }
        }

        private bool AcceptTarget(IDefenseTargetPosition target)
        {
            if (!target.IsAvailable || target.IsAirborne || !(target.Identity is IDefenseEnemyTarget3D)) return false;
            if (target.Identity is DefenseEnemy3D actor && actor.Stats.EnemyId == DefenseEnemyRegistry.ID_YAKSHA_BOSS) return false;
            Vector3 point = new Vector3(target.Position.X, target.Position.Y, target.Position.Z);
            return _shop.WithinOrders(this, point) && CanReach(point);
        }

        public bool Move(Vector3 point, float stoppingDistance)
        {
            if (_agent == null || !CanReach(point)) { Stop(); return false; }
            _agent.stoppingDistance = stoppingDistance;
            _agent.isStopped = false;
            return _agent.SetPath(_path);
        }

        public void Stop()
        {
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh) { _agent.isStopped = true; _agent.ResetPath(); }
        }

        protected override void OnGameplayGateChanged(bool active) { if (!active) Stop(); }
        protected override void OnGameplayCleanup() => Retire();
        protected override void OnGameplayReset() => Retire();
        private void OnDisable() => Stop();
        private void RefreshHealth(float current, float maximum) { if (_healthView != null) _healthView.SetRatio(maximum > 0f ? current / maximum : 0f); }
        protected override void OnDestroy()
        {
            Stop();
            if (_health != null) { _health.Defeated -= OnDefeated; _health.HealthChanged -= RefreshHealth; }
            base.OnDestroy();
        }
    }
}
