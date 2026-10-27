using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Delayed cone hit strategy. No separate attack cooldown or ownership/reward path.</summary>
    public sealed class DefenseConeAttack3D : DefenseGameplayBehaviour
    {
        [SerializeField] private DefenseConeAttackProfileSO _profile;
        [SerializeField] private DefenseConeTelegraphView3D _view;
        private DefenseEnemy3D _enemy;
        private float _radius;
        private int _lineOfSightMask;
        private DefenseConeTelegraphRuntime _runtime;
        private IDefenseAggroCandidate[] _friends;
        private DefenseBasePrototype3D _base;
        private Collider _baseCollider;
        private IDefenseDamageable3D _lockedVictim;
        public bool IsWindingUp => _runtime != null && _runtime.IsPending;
        public void Bind(IDefenseGameplayContext gameplay, DefenseEnemy3D enemy)
        {
            if (_profile == null || _view == null || enemy == null) throw new InvalidOperationException("Author cone Profile/View/Enemy dependencies.");
            _profile.Validate(); _view.ValidateBindings();
            _enemy = enemy;
            _radius = _profile.Radius; _lineOfSightMask = _profile.LineOfSightMask;
            _runtime = new DefenseConeTelegraphRuntime(gameplay, _profile.Windup, _profile.Radius, _profile.Angle);
            BindGameplay(gameplay);
        }
        public void BindTargets(DefenseFriendlyTargets3D friends, DefenseBasePrototype3D target)
        {
            if (friends == null || friends.Candidates == null || target == null) throw new InvalidOperationException("Cone needs injected friendly roster and Base.");
            _friends = friends.Candidates; _base = target; _base.TryGetComponent(out _baseCollider);
        }
        public bool CanStart(IDefenseDamageable3D victim, Collider collider)
        {
            if (!Available(victim) || _profile == null || !(_enemy != null && _enemy.IsTargetable)) return false;
            Vector3 point = HitPoint(victim, collider, transform.position);
            return (point - transform.position).sqrMagnitude <= _radius * _radius && ClearLine(transform.position, point, victim, collider);
        }
        public bool TryBegin(IDefenseDamageable3D victim, Collider collider)
        {
            if (_runtime == null || _friends == null || !CanStart(victim, collider)) return false;
            Vector3 direction = ((Component)victim).transform.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= Mathf.Epsilon) direction = transform.forward;
            if (!_runtime.TryBegin(Point(transform.position), Point(direction))) return false;
            _lockedVictim = victim;
            try { _view.Show(_runtime.Preview); }
            catch { Cancel(); throw; }
            return true;
        }
        public void Tick()
        {
            if (!CanAdvanceGameplay || _enemy == null || !_enemy.IsTargetable || !Available(_lockedVictim) ||
                (_enemy.Status != null && _enemy.Status.BlocksActions)) { Cancel(); return; }
            if (!_runtime.TryResolve(out var cone)) return;
            int generation = Gameplay.RunGeneration;
            var lifetime = _enemy.Lifetime;
            _view.Hide(); _lockedVictim = null;
            for (int i = 0; i < _friends.Length; i++)
            {
                if (!Current(generation, lifetime)) break;
                if (_friends[i] is DefenseFriendlyTarget3D target && target.IsAvailable)
                    Strike(cone, target.Damageable, target.Collider, generation);
            }
            if (Current(generation, lifetime) && _base != null && !_base.IsDestroyed) Strike(cone, _base, _baseCollider, generation);
        }
        private bool Current(int generation, DefenseActorLifetime lifetime) => CanAdvanceGameplay && Gameplay.RunGeneration == generation && _enemy != null && ReferenceEquals(_enemy.Lifetime, lifetime) && _enemy.IsTargetable && (_enemy.Status == null || !_enemy.Status.BlocksActions);
        private void Strike(DefenseConeSnapshot cone, IDefenseDamageable3D victim, Collider collider, int generation)
        {
            if (!Available(victim)) return;
            Vector3 origin = new Vector3(cone.Origin.X, cone.Origin.Y, cone.Origin.Z);
            Vector3 point = HitPoint(victim, collider, origin);
            if (!cone.Contains(Point(point)) || !ClearLine(origin, point, victim, collider)) return;
            victim.TakeDamage(new DefenseDamageContext(_enemy.Stats.Damage, _enemy, point,
                runGeneration: generation, sessionIdentity: Gameplay.SessionIdentity));
        }
        private bool ClearLine(Vector3 origin, Vector3 point, IDefenseDamageable3D victim, Collider collider)
        {
            return !Physics.Linecast(origin, point, out var hit, _lineOfSightMask, QueryTriggerInteraction.Ignore) ||
                hit.collider == collider || hit.transform.IsChildOf(((Component)victim).transform);
        }
        private static Vector3 HitPoint(IDefenseDamageable3D victim, Collider collider, Vector3 origin) =>
            collider != null && collider.enabled && collider.gameObject.activeInHierarchy ? collider.ClosestPoint(origin) : ((Component)victim).transform.position;
        private static bool Available(IDefenseDamageable3D victim) => victim is Component component && component != null &&
            component.gameObject.activeInHierarchy && (!(component is Behaviour behaviour) || behaviour.isActiveAndEnabled) && !victim.IsDead;
        private static DefensePoint3 Point(Vector3 p) => new DefensePoint3(p.x, p.y, p.z);
        public void Cancel() { _runtime?.Cancel(); _lockedVictim = null; _view?.Hide(); }
        protected override void OnGameplayGateChanged(bool active) { if (!active) Cancel(); }
        protected override void OnGameplayCleanup() => Cancel();
        protected override void OnGameplayReset() => Cancel();
        private void OnDisable() => Cancel();
    }
}
