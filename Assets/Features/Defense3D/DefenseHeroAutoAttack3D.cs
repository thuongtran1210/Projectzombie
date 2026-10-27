using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Single basic-attack ability owner. Manual and optional auto input feed the same delayed cone hit.</summary>
    public sealed class DefenseHeroAutoAttack3D : DefenseGameplayBehaviour
    {
        [SerializeField, Min(0.5f)] private float attackRange = 5f;
        [SerializeField, Min(1f)] private float damage = 35f;
        [SerializeField, Min(0.1f)] private float attackCooldown = 0.65f;
        [SerializeField] private bool drawAttackRange = true;
        [SerializeField] private DefenseAttackTargetingProfileSO _targeting;
        [SerializeField] private bool _autoAttackEnabled = true;
        [SerializeField] private float _coneAngle = 210f;
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
        public event Action<DefenseConeSnapshot> AttackResolved;
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
            _motor != null && !_motor.IsDashing && (_signature == null || !_signature.IsCasting);
        public bool QueueAttack(DefenseResolvedAim aim)
        {
            if (!CanPrepare || _windup == null) return false;
            Vector3 direction = new Vector3(aim.Direction.X, 0f, aim.Direction.Z);
            if (direction.sqrMagnitude < 0.000001f) direction = transform.forward;
            return _windup.TryBegin(Point(transform.position), Point(direction));
        }
        public void CancelPendingAttack() => _windup?.Cancel();
        private void Update()
        {
            if (!CanAdvanceGameplay || _health == null || _health.IsDown || (_motor != null && _motor.IsDashing)) { CancelPendingAttack(); return; }
            if (HasPendingAttack)
            {
                if (_windup.TryResolve(out var locked)) Resolve(new DefenseConeSnapshot(Point(transform.position), locked.Forward, locked.Radius, locked.Angle));
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
                    enemy.TakeDamage(new DefenseDamageContext(EffectiveDamage, this, enemy.TargetTransform.position,
                        runGeneration: generation, sessionIdentity: Gameplay.SessionIdentity));
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
