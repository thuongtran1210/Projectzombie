using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Cooldown and damage delivery; receives a target from the route/controller.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(DefenseEnemy3D))]
    public sealed class DefenseEnemyAttack3D : DefenseGameplayBehaviour, IDefenseAttackStrategy3D, IDefenseAttackFeedbackSource
    {
        private DefenseEnemy3D enemy;
        private IDefenseDamageable3D target;
        private float nextAttackTime;
        private Collider _targetCollider;
        [SerializeField] private LayerMask _lineOfSightMask;
        [SerializeField] private bool _useSpatialRange;
        [SerializeField] private DefenseConeAttack3D _coneAttack;
        private DefenseAttackFeedbackEvents _feedback;
        public DefenseAttackFeedbackEvents Feedback => _coneAttack != null ? _coneAttack.Feedback :
            (_feedback ?? (_feedback = new DefenseAttackFeedbackEvents(error => Debug.LogException(error, this))));
        public void CancelPendingAttack() => _coneAttack?.Cancel();
        public bool IsWindingUp => _coneAttack != null && _coneAttack.IsWindingUp;
        protected override void OnGameplayBound()
        {
            enemy = GetComponent<DefenseEnemy3D>();
            if (_coneAttack != null) _coneAttack.Bind(Gameplay, enemy);
        }
        public void BindAreaTargets(DefenseFriendlyTargets3D friends, DefenseBasePrototype3D target) => _coneAttack?.BindTargets(friends, target);
        protected override void OnGameplayGateChanged(bool active) { if (!active) _coneAttack?.Cancel(); }
        protected override void OnGameplayCleanup() => _coneAttack?.Cancel();
        protected override void OnGameplayReset() { _coneAttack?.Cancel(); nextAttackTime = 0f; }
        private DefenseGroundNavigationRules _groundRules;
        public void BindGroundRules(DefenseGroundNavigationRules rules) => _groundRules=rules;
        public bool IsAttacking => target != null;
        private void Awake() => enemy = GetComponent<DefenseEnemy3D>();
        private void OnDisable() => StopAttack();
        public void Begin(IDefenseDamageable3D victim,Collider targetCollider=null)
        {
            if (ReferenceEquals(target,victim)) return;
            target = victim;
            _targetCollider=targetCollider;
            if (_targetCollider==null && victim is Component component) component.TryGetComponent(out _targetCollider);
        }
        public void StopAttack() { _coneAttack?.Cancel(); target = null; _targetCollider=null; }
        public bool IsTargetInRange
        {
            get
            {
                if (enemy==null || target==null || target.IsDead || !(target is Component component)) return false;
                if (_coneAttack != null) return _coneAttack.CanStart(target, _targetCollider);
                if (_groundRules!=null && !_groundRules.SameBank(
                    new DefensePoint3(transform.position.x,transform.position.y,transform.position.z),
                    new DefensePoint3(component.transform.position.x,component.transform.position.y,component.transform.position.z))) return false;
                Vector3 destination=_targetCollider!=null ? _targetCollider.ClosestPoint(transform.position) : component.transform.position;
                Vector3 delta=destination-transform.position;
                if (!_useSpatialRange) delta.y=0f;
                if (delta.sqrMagnitude>enemy.Stats.AttackRange*enemy.Stats.AttackRange) return false;
                return _lineOfSightMask.value==0 || !Physics.Linecast(transform.position,destination,out var hit,_lineOfSightMask,
                    QueryTriggerInteraction.Ignore) || hit.collider==_targetCollider || hit.transform.IsChildOf(component.transform);
            }
        }
        private void Update()
        {
            if (!CanAdvanceGameplay || !enemy.IsTargetable || target == null || (enemy.Status != null && enemy.Status.BlocksActions))
            {
                _coneAttack?.Cancel();
                return;
            }
            if ((target is Object unityObject && unityObject == null) || target.IsDead)
            {
                StopAttack();
                return;
            }
            if (IsWindingUp)
            {
                _coneAttack.Tick();
                return;
            }
            if (GameplayTime < nextAttackTime) return;
            if (!IsTargetInRange) return;
            if (_coneAttack != null)
            {
                if (_coneAttack.TryBegin(target, _targetCollider)) nextAttackTime = GameplayTime + Mathf.Max(0.1f, enemy.Stats.AttackInterval);
                return;
            }
            int generation = Gameplay.RunGeneration;
            object identity = Gameplay.SessionIdentity;
            var lifetime = enemy.Lifetime;
            var origin = DefenseAttackFeedbackPoints.Point(transform.position);
            var victim = target;
            var targetTransform = ((Component)victim).transform;
            var direction = targetTransform.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < .000001f) direction = transform.forward;
            direction.Normalize();
            var forward = DefenseAttackFeedbackPoints.Point(direction);
            var hitPosition = DefenseAttackFeedbackPoints.Capture(targetTransform, targetTransform.position + Vector3.up * .7f);
            Feedback.PublishSwing(new DefenseSwingFeedback(new DefenseConeSnapshot(origin, forward, enemy.Stats.AttackRange, 90f), identity, generation));
            if (!CanAdvanceGameplay || Gameplay.RunGeneration != generation || !ReferenceEquals(enemy.Lifetime, lifetime) || !enemy.IsTargetable) return;
            float hitDamage = enemy.Stats.Damage;
            bool accepted = victim.TakeDamage(new DefenseDamageContext(hitDamage, enemy, transform.position,
                runGeneration: generation, sessionIdentity: identity));
            if (accepted) Feedback.PublishHit(new DefenseHitFeedback(hitPosition, forward, hitDamage, identity, generation));
            if (CanAdvanceGameplay && Gameplay.RunGeneration == generation && ReferenceEquals(enemy.Lifetime, lifetime))
                nextAttackTime = GameplayTime + Mathf.Max(0.1f, enemy.Stats.AttackInterval);
        }
    }
}
