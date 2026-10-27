using System;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Session-bound recruitment and squad commands using the same wood wallet as tower construction.</summary>
    public sealed class DefenseVillageShop3D : DefenseGameplayBehaviour
    {
        [SerializeField] private DefenseMinionDefinitionSO[] _catalog;
        [SerializeField] private DefenseMinion3D[] _slots;
        [SerializeField] private DefenseHeroHealth3D _hero;
        [SerializeField] private DefenseBasePrototype3D _base;
        [SerializeField] private DefenseWoodWallet3D _wallet;
        [SerializeField] private DefenseSceneInfo3D _map;
        [SerializeField] private DefenseFriendlyTargets3D _friends;
        [SerializeField] private DefenseRuntimeRoots3D _roots;
        [SerializeField] private DefenseTowerBuildPrototype3D _towerBuilder;
        [SerializeField] private Transform _recruitmentAnchor;
        [SerializeField, Min(.1f)] private float _recruitmentRadius = 1.6f;
        private bool _busy;
        private Collider _baseCollider;
        private readonly DefenseMinionOrder[] _orders = new DefenseMinionOrder[3];
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
        public DefenseMinionDefinitionSO Definition(int role) => role >= 0 && role < _catalog.Length ? _catalog[role] : null;
        public DefenseMinionOrder Order(int role) => _orders[role];

        protected override void OnGameplayBound()
        {
            _base.TryGetComponent(out _baseCollider);
            foreach (var unit in _slots) unit.Bind(this, Gameplay);
            _wallet.Changed -= NotifyChanged;
            _wallet.Changed += NotifyChanged;
        }

        public int Count(int role)
        {
            int count = 0;
            foreach (var unit in _slots) if ((int)unit.Definition.Role == role && unit.IsAvailable) count++;
            return count;
        }

        public int Capacity(int role)
        {
            int count = 0;
            foreach (var unit in _slots) if ((int)unit.Definition.Role == role) count++;
            return count;
        }

        public bool TryRecruit(int role)
        {
            var definition = Definition(role);
            if (_busy || !CanRecruit || definition == null || definition.WoodCost <= 0 || _wallet.Wood < definition.WoodCost) return false;
            DefenseMinion3D slot = null;
            int index = 0;
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].Definition == definition && !_slots[i].gameObject.activeSelf) { slot = _slots[i]; index = i; break; }
            }
            if (slot == null) return false;
            Vector3 spawn = _base.transform.position + new Vector3(2.6f + index % 3 * 0.35f, 0f, (index % 5 - 2) * 0.45f);
            if (!NavMesh.SamplePosition(spawn, out var hit, 3f, NavMesh.AllAreas)) return false;
            _busy = true;
            try
            {
                // Deployment may fail if NavMesh data was removed; charge only after a successful warp.
                if (!slot.Deploy(hit.position, index)) return false;
                if (!CanRecruit || !_wallet.TrySpend(definition.WoodCost)) { slot.Retire(); return false; }
                NotifyChanged();
                return true;
            }
            finally { _busy = false; }
        }

        public bool CycleOrder(int role)
        {
            if (!CanTrade || Definition(role) == null) return false;
            _orders[role] = (DefenseMinionOrder)(((int)_orders[role] + 1) % 3);
            NotifyChanged();
            return true;
        }

        public Vector3 Anchor(DefenseMinion3D unit)
        {
            if (Order((int)unit.Definition.Role) == DefenseMinionOrder.Follow && !_hero.IsDown)
                return _hero.transform.position - _hero.transform.forward * (unit.Definition.Role == DefenseMinionRole.Healer ? 1.8f : 2.4f) + unit.FormationOffset;
            if (Order((int)unit.Definition.Role) == DefenseMinionOrder.Advance)
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
            var order = Order((int)unit.Definition.Role);
            if (order == DefenseMinionOrder.Advance) return true;
            if (order == DefenseMinionOrder.Defend && point.x > 4.2f) return false;
            Vector3 delta = point - Anchor(unit); delta.y = 0f;
            return delta.sqrMagnitude <= unit.Definition.SearchRange * unit.Definition.SearchRange;
        }

        public DefenseWoodDrop3D FindWood(DefenseMinion3D unit)
        {
            var pool = DefenseWoodDropPool.ActiveSessionPool;
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
            if (ReferenceEquals(patient, _hero) && Order((int)unit.Definition.Role) == DefenseMinionOrder.Follow) candidate -= 1f;
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
        protected override void OnGameplayReset() { Array.Clear(_orders, 0, _orders.Length); foreach (var unit in _slots) unit.Retire(); NotifyChanged(); }
        protected override void OnGameplayGateChanged(bool active) => NotifyChanged();
        protected override void OnDestroy() { if (_wallet != null) _wallet.Changed -= NotifyChanged; base.OnDestroy(); }
    }
}
