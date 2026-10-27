using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Single basic-attack ability owner. Manual and optional auto input feed the same delayed cone hit.</summary>
    public sealed class DefenseHeroAutoAttack3D : DefenseGameplayBehaviour
    {
        [SerializeField, Min(0.5f)] private float attackRange = 5f;
        [SerializeField, Min(1f)] private float damage = 35f;
        [SerializeField, Min(0.1f)] private float attackCooldown = DefenseBasicAttackRules.RepeatInterval;
        [SerializeField] private bool drawAttackRange = true;
        [SerializeField] private DefenseAttackTargetingProfileSO _targeting;
        [SerializeField] private bool _autoAttackEnabled;
        [SerializeField] private float _coneAngle = DefenseBasicAttackRules.ConeAngle;
        [SerializeField, Range(0f, 1f)] private float _knockbackStrength = .275f;
        [SerializeField, Min(.01f)] private float _knockbackDuration = .2f;
        private float _recoveryUntil;
        public bool IsRecovering => CanAdvanceGameplay && GameplayTime < _recoveryUntil;
        private DefenseAttackTargetSelector3D _selector;
        private DefenseAbilityRuntime _ability;
        private DefenseConeTelegraphRuntime _windup;
        private DefenseHeroHealth3D _health;
        private DefenseHeroMotor3D _motor;
        private DefenseScholarSignatureEffect3D _signature;
        private DefenseSkillDefinition _definition;
        private readonly IDefenseTargetPosition[] _targets = new IDefenseTargetPosition[256];
        public DefenseAbilityRuntime Ability => _ability;
        public bool HasPendingAttack => _windup != null && _windup.IsPending;
        public float ConeAngle => _coneAngle;
        public float BaseDamage => _definition != null ? _definition.Damage : damage;
        public float EffectiveDamage => Gameplay != null ? Gameplay.Stats.Evaluate(DefenseUpgradeType.HeroAttackDamage, _definition != null ? _definition.Damage : damage) : damage;
        public float EffectiveAttackInterval => Gameplay != null ? Gameplay.Stats.EvaluateHeroAttackInterval(_definition != null ? _definition.Cooldown : attackCooldown) : attackCooldown;
        private float Range => _definition != null ? _definition.Radius : attackRange;
        public float AttackRange => Range;
        public event Action<DefenseConeSnapshot> AttackResolved;
        public event Action AttackStarted;
        public bool TryResolveBasicAim(out DefenseResolvedAim aim)
        {
            Vector3 position = transform.position;
            var origin = Point(position);
            var forward = Point(transform.forward);
            DefensePoint3 desired = new DefensePoint3(origin.X + forward.X * Range, origin.Y, origin.Z + forward.Z * Range);
            if (Gameplay != null && Gameplay.Targets is IDefenseAreaTargetQuery query && query.TryCopyWithin(origin,
                DefenseBasicAttackRules.SearchRange(Range), _definition != null ? _definition.TargetMask : DefenseTargetMask.All, _targets, out int count))
            {
                double best = double.PositiveInfinity;
                try
                {
                    for (int i = 0; i < count; i++)
                    {
                        var target = _targets[i];
                        if (target == null || !target.IsAvailable || !_selector.CanHit(target, position)) continue;
                        double score = DefenseBasicAttackRules.TargetScore(origin, forward, target.Position);
                        if (score >= best) continue;
                        best = score; desired = target.Position;
                    }
                }
                finally { Array.Clear(_targets, 0, count); }
            }
            return DefenseAimResolver.TryResolve(origin, desired, Range, out aim);
        }
        protected override void OnGameplayBound()
        {
            _health = GetComponent<DefenseHeroHealth3D>(); _motor = GetComponent<DefenseHeroMotor3D>();
            _signature = GetComponent<DefenseScholarSignatureEffect3D>();
            _ability?.Dispose(); _ability = new DefenseAbilityRuntime(Gameplay);
            _selector = new DefenseAttackTargetSelector3D(transform, _targeting);
            _windup = new DefenseConeTelegraphRuntime(Gameplay, 0.18f, Range, _coneAngle);
        }
        public void ConfigureAttack(DefenseSkillDefinition definition)
        {
            if (definition == null || definition.Kind != DefenseSkillKind.BasicAttack) throw new ArgumentException("Expected basic-attack definition.");
            _definition = definition;
            _windup = new DefenseConeTelegraphRuntime(Gameplay, definition.Duration, definition.Radius, _coneAngle);
        }
        public bool CanPrepare => isActiveAndEnabled && CanAdvanceGameplay && _health != null && _health.isActiveAndEnabled && !_health.IsDown && !HasPendingAttack &&
            _motor != null && !_motor.IsDashing && _motor.CanAim(this) && (_signature == null || !_signature.IsCasting);
        public bool QueueAttack(DefenseResolvedAim aim)
        {
            if (!CanPrepare || _windup == null) return false;
            Vector3 direction = new Vector3(aim.Direction.X, 0f, aim.Direction.Z);
            if (direction.sqrMagnitude < 0.000001f) direction = transform.forward;
            if (!_windup.TryBegin(Point(transform.position), Point(direction))) return false;
            _recoveryUntil = GameplayTime + DefenseBasicAttackRules.Recovery;
            _motor.FaceBasicAttack(this, Point(direction));
            AttackStarted?.Invoke();
            return true;
        }
        public void CancelPendingAttack() { _windup?.Cancel(); _motor?.ClearAimFacing(this); _recoveryUntil = 0f; }
        private void Update()
        {
            if (!CanAdvanceGameplay || _health == null || _health.IsDown || (_motor != null && _motor.IsDashing)) { CancelPendingAttack(); return; }
            if (HasPendingAttack)
            {
                if (_windup.TryResolve(out var locked))
                {
                    try { Resolve(new DefenseConeSnapshot(Point(transform.position), locked.Forward, locked.Radius, locked.Angle)); }
                    finally { _motor.ClearAimFacing(this); }
                }
                return;
            }
            if (!_autoAttackEnabled || !CanPrepare || _ability == null || _ability.Phase != DefenseAbilityPhase.Ready || _motor.HasAimFacing) return;
            var target = _selector.Find(Gameplay.Targets, Range);
            if (target == null || !_ability.TryActivate(EffectiveAttackInterval, true)) return;
            if (DefenseAimResolver.TryResolve(Point(transform.position), Point(target.TargetTransform.position), Range, out var aim)) QueueAttack(aim);
        }
        private void Resolve(DefenseConeSnapshot cone)
        {
            if (!(Gameplay.Targets is IDefenseAreaTargetQuery area) || !area.TryCopyWithin(cone.Origin, cone.Radius,
                _definition != null ? _definition.TargetMask : DefenseTargetMask.All, _targets, out int count)) return;
            int generation = Gameplay.RunGeneration;
            try
            {
                for (int i = 0; i < count; i++)
                {
                    if (!isActiveAndEnabled || !CanAdvanceGameplay || Gameplay.RunGeneration != generation || !_health.isActiveAndEnabled || _health.IsDown || _motor.IsDashing) break;
                    var candidate = _targets[i];
                    if (candidate == null || !candidate.IsAvailable || !cone.Contains(candidate.Position) || !_selector.CanHit(candidate, transform.position) ||
                        !(candidate.Identity is IDefenseEnemyTarget3D enemy)) continue;
                    bool hit = enemy.TakeDamage(new DefenseDamageContext(EffectiveDamage, this, enemy.TargetTransform.position,
                        runGeneration: generation, sessionIdentity: Gameplay.SessionIdentity));
                    if (hit && CanAdvanceGameplay && Gameplay.RunGeneration == generation && candidate.IsAvailable && enemy is IDefenseStatusReceiver status)
                    {
                        Vector3 outward = enemy.TargetTransform.position - new Vector3(cone.Origin.X, cone.Origin.Y, cone.Origin.Z);
                        outward.y = 0f; outward.Normalize();
                        status.TryApplyStatus(new DefenseStatusRequest(DefenseStatusKind.Knockback, _knockbackDuration,
                            _knockbackStrength, Point(outward), Gameplay.SessionIdentity, generation));
                    }
                }
                if (isActiveAndEnabled && CanAdvanceGameplay && Gameplay.RunGeneration == generation && _health.isActiveAndEnabled && !_health.IsDown && !_motor.IsDashing) AttackResolved?.Invoke(cone);
            }
            finally { Array.Clear(_targets, 0, count); }
        }
        protected override void OnGameplayGateChanged(bool active) { if (!active) CancelPendingAttack(); }
        protected override void OnGameplayCleanup() => CancelPendingAttack();
        protected override void OnGameplayReset() => CancelPendingAttack();
        private void OnDisable() => CancelPendingAttack();
        private void OnDrawGizmosSelected() { if (drawAttackRange) { Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, Range); } }
        private static DefensePoint3 Point(Vector3 p) => new DefensePoint3(p.x, p.y, p.z);
        protected override void OnDestroy() { _ability?.Dispose(); base.OnDestroy(); }
    }
}
