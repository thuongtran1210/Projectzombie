using System;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Session-bound recruitment and squad commands using the same wood wallet as tower construction.</summary>
    public sealed class DefenseVillageShop3D : DefenseGameplayBehaviour, IMinionRecruitContext, IMinionSquadCommands, IMinionRecruitmentCommands
    {
        [SerializeField] private DefenseMinionDefinitionSO[] _catalog = Array.Empty<DefenseMinionDefinitionSO>();
        [SerializeField] private DefenseMinionCatalogSO _minionCatalog;
        [SerializeField] private DefenseMinion3D[] _slots = Array.Empty<DefenseMinion3D>();
        [SerializeField] private DefenseHeroHealth3D _hero;
        [SerializeField] private DefenseBasePrototype3D _base;
        [SerializeField] private DefenseWoodWallet3D _wallet;
        [SerializeField] private DefenseSceneInfo3D _map;
        [SerializeField] private DefenseFriendlyTargets3D _friends;
        [SerializeField] private DefenseRuntimeRoots3D _roots;
        [SerializeField] private DefenseTowerBuildPrototype3D _towerBuilder;
        [SerializeField] private Transform _recruitmentAnchor;
        [SerializeField, Min(.1f)] private float _recruitmentRadius = 1.6f;
        private IDefenseWoodDrops _woodDrops;
        public void BindWoodDrops(IDefenseWoodDrops woodDrops) => _woodDrops = woodDrops;
        private Collider _baseCollider;
        private readonly MinionRosterRuntime _roster = new MinionRosterRuntime();
        private DefenseMinionSlotPool _pool;
        private MinionRecruitmentService _recruitment;
        public IMinionRosterReader Roster => _roster;
        public object SessionIdentity => Gameplay?.SessionIdentity;
        public int RunGeneration => Gameplay != null ? Gameplay.RunGeneration : -1;
        public event Action Changed;
        public bool CanTrade => isActiveAndEnabled && CanAdvanceGameplay && _hero != null && _hero.isActiveAndEnabled && !_hero.IsDown && !_base.IsDestroyed;
        public DefenseHeroHealth3D Hero => _hero;
        public DefenseBasePrototype3D Base => _base;
        public DefenseWoodWallet3D Wallet => _wallet;
        public DefenseSceneInfo3D Map => _map;
        public DefenseTowerBuildPrototype3D TowerBuilder => _towerBuilder;
        public Vector3 RecruitmentPosition => _recruitmentAnchor != null ? _recruitmentAnchor.position : new Vector3(-5.4f, 0f, -2.2f);
        public bool IsNearRecruitment
        {
            get
            {
                if (_hero == null) return false;
                Vector3 hero = _hero.transform.position, hut = RecruitmentPosition;
                return DefenseVillageInteractionRules.InZone(new DefensePoint3(hero.x, hero.y, hero.z), new DefensePoint3(hut.x, hut.y, hut.z), _recruitmentRadius, true);
            }
        }
        public bool CanRecruit => CanTrade && IsNearRecruitment;
        /// <summary>Composition selects exact equipped IDs; no role-based replacement or implicit support units.</summary>
        public void ConfigureRoster(System.Collections.Generic.IEnumerable<string> minionIds)
        {
            if (CanAdvanceGameplay) throw new InvalidOperationException("Configure the roster before the session starts.");
            _roster.Select(minionIds, _minionCatalog != null ? _minionCatalog.Definitions : _catalog ?? Array.Empty<DefenseMinionDefinitionSO>());
            NotifyChanged();
        }

        public DefenseMinionOrder Order(string id) => _roster.TryGetSquad(id, out var squad) ? squad.Order : DefenseMinionOrder.Defend;
        public int Count(string id) => _pool != null ? _pool.Count(id) : 0;
        public int Capacity(string id) => _roster.TryGetSquad(id, out var squad) && _pool != null
            ? Math.Min(squad.Definition.Capacity, _pool.Capacity(id)) : 0;

        /// <summary>Applies upgrades to this type only, even if another type shares its behavior.</summary>
        public void SetSquadStatMultipliers(string id, float hpMultiplier, float speedMultiplier)
        {
            if (!_roster.TryGetSquad(id, out var squad)) return;
            squad.HpMultiplier = Mathf.Max(.1f, hpMultiplier);
            squad.SpeedMultiplier = Mathf.Max(.1f, speedMultiplier);
            foreach (var unit in _slots)
                if (unit != null && unit.Definition != null && string.Equals(unit.Definition.Id, id, StringComparison.OrdinalIgnoreCase))
                    unit.ApplyMetaMultipliers(squad.HpMultiplier, squad.SpeedMultiplier);
        }

        protected override void OnGameplayBound()
        {
            _base.TryGetComponent(out _baseCollider);
            _pool?.Dispose();
            _pool = new DefenseMinionSlotPool(_slots ?? Array.Empty<DefenseMinion3D>(), _base.transform);
            _recruitment = new MinionRecruitmentService(_roster, this, _wallet, _pool);
            foreach (var unit in _slots) unit.Bind(this, Gameplay);
            _wallet.Changed -= NotifyChanged;
            _wallet.Changed += NotifyChanged;
        }

        /// <summary>Warms selected actors before the run starts, yielding between instances to avoid a large synchronous spike.</summary>
        public async System.Threading.Tasks.Task PrepareRosterAsync(Camera camera)
        {
            var slots = new System.Collections.Generic.List<DefenseMinion3D>(_slots ?? Array.Empty<DefenseMinion3D>());
            var supports = new System.Collections.Generic.List<DefenseSupportTarget3D>();
            foreach (var squad in _roster.Squads)
            {
                if (squad.Definition.Prefab == null) throw new InvalidOperationException("Missing minion prefab: " + squad.Id);
                int existing = 0;
                foreach (var unit in slots) if (unit != null && unit.Definition.Id == squad.Id) existing++;
                while (existing++ < squad.Definition.Capacity)
                {
                    var actor = DefenseEnemyPrefab3D.Instantiate(squad.Definition.Prefab.gameObject, _roots.Allies);
                    actor.SetActive(false);
                    var unit = actor.GetComponent<DefenseMinion3D>();
                    if (unit.Definition != squad.Definition) throw new InvalidOperationException("Prefab definition mismatch: " + squad.Id);
                    unit.Bind(this, Gameplay);
                    unit.ApplyMetaMultipliers(squad.HpMultiplier, squad.SpeedMultiplier);
                    foreach (var view in actor.GetComponentsInChildren<UI.DefenseMinionHealthView3D>(true)) view.BindCamera(camera);
                    slots.Add(unit); supports.Add(unit.Health);
                    await System.Threading.Tasks.Task.Yield();
                    if (this == null || Gameplay == null || (Gameplay is DefenseSession session && session.IsDisposed)) return;
                }
            }
            _slots = slots.ToArray();
            _friends.AppendRuntimeSupports(supports);
            _pool?.Dispose();
            _pool = new DefenseMinionSlotPool(_slots, _base.transform);
            _recruitment = new MinionRecruitmentService(_roster, this, _wallet, _pool);
            NotifyChanged();
        }

        public MinionRecruitResult TryRecruit(string id)
        {
            var result = _recruitment != null ? _recruitment.TryRecruit(id) : MinionRecruitResult.Unavailable;
            if (result == MinionRecruitResult.Success) NotifyChanged();
            return result;
        }

        public bool CycleOrder(string id)
        {
            if (!CanTrade || !_roster.CycleOrder(id)) return false;
            NotifyChanged(); return true;
        }

        public Vector3 Anchor(DefenseMinion3D unit)
        {
            if (Order(unit.Definition.Id) == DefenseMinionOrder.Follow && !_hero.IsDown)
                return _hero.transform.position - _hero.transform.forward * (unit.Definition.Role == DefenseMinionRole.Healer ? 1.8f : 2.4f) + unit.FormationOffset;
            if (Order(unit.Definition.Id) == DefenseMinionOrder.Advance)
            {
                if (unit.Definition.Role == DefenseMinionRole.Healer)
                {
                    DefenseMinion3D frontline = null;
                    foreach (var candidate in _slots)
                        if (candidate.IsAvailable && candidate.Definition.Role == DefenseMinionRole.Militia &&
                            (frontline == null || candidate.transform.position.x > frontline.transform.position.x)) frontline = candidate;
                    if (frontline != null) return frontline.transform.position - Vector3.right * 1.8f + unit.FormationOffset;
                }
                return _map.EastEnemySpawn.position + unit.FormationOffset;
            }
            return _base.transform.position + Vector3.right * 4f + unit.FormationOffset;
        }

        public bool WithinOrders(DefenseMinion3D unit, Vector3 point)
        {
            var order = Order(unit.Definition.Id);
            if (order == DefenseMinionOrder.Advance) return true;
            if (order == DefenseMinionOrder.Defend && point.x > 4.2f) return false;
            Vector3 delta = point - Anchor(unit); delta.y = 0f;
            return delta.sqrMagnitude <= unit.Definition.SearchRange * unit.Definition.SearchRange;
        }

        public DefenseWoodDrop3D FindWood(DefenseMinion3D unit)
        {
            var pool = _woodDrops;
            if (pool == null) return null;
            DefenseWoodDrop3D best = null;
            float distance = float.PositiveInfinity;
            for (int i = 0; i < pool.ActiveDrops.Count; i++)
            {
                var drop = pool.ActiveDrops[i];
                if (drop == null || !drop.CanBeGathered(Gameplay) || !WithinOrders(unit, drop.transform.position)) continue;
                float candidate = (drop.transform.position - unit.transform.position).sqrMagnitude;
                if (candidate >= distance || !unit.CanReach(drop.transform.position)) continue;
                distance = candidate; best = drop;
            }
            return best;
        }

        public IDefenseDamageable3D FindPatient(DefenseMinion3D unit, out Vector3 point, bool structuresOnly = false)
        {
            IDefenseDamageable3D best = null;
            point = Anchor(unit);
            float score = float.PositiveInfinity;
            ConsiderPatient(unit, _base, _base.transform.position, ref best, ref point, ref score);
            var candidates = _friends.Candidates;
            if (candidates == null) return best;
            foreach (var item in candidates)
            {
                if (!(item is DefenseFriendlyTarget3D target) || !target.IsAvailable) continue;
                if (structuresOnly && !(target.Damageable is DefenseSupportTarget3D support && support.IsStructure)) continue;
                ConsiderPatient(unit, target.Damageable, target.Transform.position, ref best, ref point, ref score);
            }
            return best;
        }

        private void ConsiderPatient(DefenseMinion3D unit, IDefenseDamageable3D patient, Vector3 position,
            ref IDefenseDamageable3D best, ref Vector3 point, ref float score)
        {
            if ((patient is Behaviour actor && !actor.isActiveAndEnabled) || patient.IsDead || patient.CurrentHP >= patient.MaxHP || !WithinOrders(unit, position) || !unit.CanReach(position)) return;
            float candidate = patient.CurrentHP / patient.MaxHP;
            if (ReferenceEquals(patient, _hero) && Order(unit.Definition.Id) == DefenseMinionOrder.Follow) candidate -= 1f;
            if (candidate >= score) return;
            score = candidate; best = patient; point = position;
        }

        public Vector3 InteractionPoint(IDefenseDamageable3D patient, Vector3 from, Vector3 fallback)
        {
            if (ReferenceEquals(patient, _base) && _baseCollider != null) return _baseCollider.ClosestPoint(from);
            if (_friends.Candidates != null)
                foreach (var item in _friends.Candidates)
                    if (item is DefenseFriendlyTarget3D target && ReferenceEquals(target.Damageable, patient) && target.Collider != null)
                        return target.Collider.ClosestPoint(from);
            return fallback;
        }

        public void Deliver(int amount)
        {
            if (CanAdvanceGameplay && !_hero.IsDown) _wallet.CollectWood(amount);
        }

        public void DropCargo(Vector3 point, int amount)
        {
            if (CanAdvanceGameplay && amount > 0) DefenseWoodDrop3D.Spawn(point, amount, _hero.transform, _wallet, Gameplay, _roots.Pickups);
        }

        public void NotifyChanged() => Changed?.Invoke();
        protected override void OnGameplayCleanup() { foreach (var unit in _slots) unit.Retire(); NotifyChanged(); }
        protected override void OnGameplayReset() { _roster.ResetOrders(); foreach (var unit in _slots) unit.Retire(); NotifyChanged(); }
        protected override void OnGameplayGateChanged(bool active) => NotifyChanged();
        protected override void OnDestroy() { if (_wallet != null) _wallet.Changed -= NotifyChanged; _pool?.Dispose(); base.OnDestroy(); }
    }
}
