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
            _health.ResetForRecruit();
            gameObject.SetActive(true);
            _agent.enabled = true;
            _agent.speed = _definition.Speed;
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

        private bool Move(Vector3 point, float stoppingDistance)
        {
            if (_agent == null || !CanReach(point)) { Stop(); return false; }
            _agent.stoppingDistance = stoppingDistance;
            _agent.isStopped = false;
            return _agent.SetPath(_path);
        }

        private void Stop()
        {
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh) { _agent.isStopped = true; _agent.ResetPath(); }
        }

        private void Update()
        {
            if (!CanAdvanceGameplay || !IsAvailable) { Stop(); return; }
            if (GameplayTime >= _nextThink)
            {
                _nextThink = GameplayTime + _definition.ThinkInterval;
                switch (_definition.Role)
                {
                    case DefenseMinionRole.Militia: Fight(); break;
                    case DefenseMinionRole.Gatherer: Gather(); break;
                    case DefenseMinionRole.Healer: Heal(); break;
                }
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

        private void Fight()
        {
            Vector3 p = transform.position;
            var target = (Gameplay.Targets as IDefenseSpatialTargetQuery)?.FindNearestSpatial(
                new DefensePoint3(p.x, p.y, p.z), _definition.SearchRange, DefenseTargetMask.Ground, _accepts)?.Identity as IDefenseEnemyTarget3D;
            if (target == null) { Move(_shop.Anchor(this), .5f); return; }
            Vector3 goal = target.TargetTransform.position;
            if (!Move(goal, _definition.AttackRange * .75f)) return;
            Vector3 delta = goal - p; delta.y = 0f;
            if (delta.sqrMagnitude > _definition.AttackRange * _definition.AttackRange || GameplayTime < _nextAction) return;
            if (!NavMesh.SamplePosition(goal, out var hit, 2.5f, _agent.areaMask) || NavMesh.Raycast(p, hit.position, out _, _agent.areaMask)) return;
            Stop();
            if (delta.sqrMagnitude > .0001f) transform.rotation = Quaternion.LookRotation(delta);
            _nextAction = GameplayTime + _definition.AttackInterval;
            bool heavy = (++_combo % 4) == 0;
            int generation = Gameplay.RunGeneration;
            if (!target.TakeDamage(new DefenseDamageContext(heavy ? _definition.HeavyDamage : _definition.Damage, this, goal,
                runGeneration: generation, sessionIdentity: Gameplay.SessionIdentity))) return;
            if (target is IDefenseStatusReceiver receiver && CanAdvanceGameplay && Gameplay.RunGeneration == generation && IsAvailable)
            {
                receiver.TryApplyStatus(new DefenseStatusRequest(DefenseStatusKind.Slow, _definition.SlowDuration,
                    _definition.SlowStrength, default, Gameplay.SessionIdentity, generation));
                if (heavy) receiver.TryApplyStatus(new DefenseStatusRequest(DefenseStatusKind.Stun, _definition.HeavyStunDuration,
                    1f, default, Gameplay.SessionIdentity, generation));
                Vector3 direction = delta.sqrMagnitude > .0001f ? delta.normalized : transform.forward;
                receiver.TryApplyStatus(new DefenseStatusRequest(DefenseStatusKind.Knockback, _definition.KnockDuration,
                    _definition.KnockStrength, new DefensePoint3(direction.x, 0f, direction.z), Gameplay.SessionIdentity, generation));
            }
        }

        private void Gather()
        {
            var drop = _cargo >= _definition.CarryCapacity ? null : _shop.FindWood(this);
            if (_cargo > 0 && (drop == null || _cargo >= _definition.CarryCapacity))
            {
                if (_shop.Hero.IsDown) { Move(_shop.Base.transform.position + Vector3.right * 3f, 1f); return; }
                Vector3 hero = _shop.Hero.transform.position;
                Move(hero, .8f);
                Vector3 delta = hero - transform.position; delta.y = 0f;
                if (delta.sqrMagnitude <= 1.8f * 1.8f)
                {
                    int cargo = _cargo; _cargo = 0;
                    _shop.Deliver(cargo);
                }
                return;
            }
            if (drop == null)
            {
                if (_shop.FindPatient(this, out var repairPoint, true) is IDefenseDamageable3D structure)
                {
                    Move(repairPoint, .7f);
                    Vector3 repairDelta = _shop.InteractionPoint(structure, transform.position, repairPoint) - transform.position; repairDelta.y = 0f;
                    if (repairDelta.sqrMagnitude <= _definition.AttackRange * _definition.AttackRange && GameplayTime >= _nextAction && Repair(structure))
                        _nextAction = GameplayTime + _definition.RepairInterval;
                }
                else Move(_shop.Anchor(this), .5f);
                return;
            }
            Move(drop.transform.position, .4f);
            Vector3 offset = drop.transform.position - transform.position; offset.y = 0f;
            if (offset.sqrMagnitude <= 1f && drop.TryGather(Gameplay, _definition.CarryCapacity - _cargo, out int amount)) _cargo += amount;
        }

        private void Heal()
        {
            var patient = _shop.FindPatient(this, out var point);
            if (patient == null) { Move(_shop.Anchor(this), .5f); return; }
            Move(point, _definition.AttackRange * .7f);
            Vector3 delta = _shop.InteractionPoint(patient, transform.position, point) - transform.position; delta.y = 0f;
            if (delta.sqrMagnitude > _definition.AttackRange * _definition.AttackRange || GameplayTime < _nextAction) return;
            bool structure = patient is DefenseBasePrototype3D || (patient is DefenseSupportTarget3D building && building.IsStructure);
            bool healed = structure ? Repair(patient) : patient is DefenseHeroHealth3D hero ? hero.Heal(_definition.HeroHealAmount) :
                patient is DefenseSupportTarget3D support && support.Heal(_definition.HealAmount);
            if (healed) _nextAction = GameplayTime + (structure ? _definition.RepairInterval : _definition.HealInterval);
        }

        private bool Repair(IDefenseDamageable3D structure) => structure is DefenseBasePrototype3D home ? home.Heal(_definition.BaseRepairAmount) :
            structure is DefenseSupportTarget3D support && support.IsStructure && support.Heal(_definition.RepairAmount);

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
