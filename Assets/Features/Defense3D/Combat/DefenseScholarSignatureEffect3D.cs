using System;
using UnityEngine;
using ProjectZombie.Features.Defense3D.UI;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Scholar self-AoE signature and direct relic cooldown refund. No elemental system dependency.</summary>
    public sealed class DefenseScholarSignatureEffect3D : DefenseSkillEffect3D
    {
        [SerializeField] private DefenseHeroHealth3D _health;
        [SerializeField] private DefenseHeroAutoAttack3D _attack;
        [SerializeField] private DefenseHeroMotor3D _motor;
        [SerializeField, Tooltip("Component implementing IDefenseAbilitySource for the relic slot.")] private MonoBehaviour _relic;
        private IDefenseAbilitySource _relicSource;
        [SerializeField] private DefenseAimIndicator3D _impactView;
        [SerializeField, Range(0f, 1f)] private float _relicCooldownReduction = 0.2f;
        private readonly IDefenseTargetPosition[] _targets = new IDefenseTargetPosition[256];
        private DefenseConeTelegraphRuntime _windup;
        private float _hideAt;
        public bool IsCasting => _windup != null && _windup.IsPending;
        public override DefenseSkillKind Kind => DefenseSkillKind.ScholarSignature;
        public override bool IsConfigured => _health != null && _attack != null && _motor != null && _relic is IDefenseAbilitySource &&
            _impactView != null && _impactView.IsConfigured && DefenseDamageRules.IsFinite(_relicCooldownReduction) &&
            _relicCooldownReduction >= 0f && _relicCooldownReduction <= 1f;
        protected override void OnGameplayBound() => _relicSource = _relic as IDefenseAbilitySource;
        protected override void OnConfigured(DefenseSkillDefinition definition) => _windup = new DefenseConeTelegraphRuntime(Gameplay, definition.Duration, definition.Radius, 360f);
        public override bool TryPrepare(DefenseResolvedAim requested, out DefenseResolvedAim prepared)
        {
            Vector3 p = transform.position;
            var center = new DefensePoint3(p.x, requested.Target.Y, p.z);
            prepared = new DefenseResolvedAim(new DefensePoint3(p.x, p.y, p.z), center, default, 0f);
            return isActiveAndEnabled && CanAdvanceGameplay && IsConfigured && Definition != null && !_health.IsDown && !_motor.IsDashing && !IsCasting && Gameplay.Targets is IDefenseAreaTargetQuery;
        }
        public override void Execute(DefenseResolvedAim prepared)
        {
            _attack.CancelPendingAttack();
            _windup.TryBegin(prepared.Origin, new DefensePoint3(0f, 0f, 1f));
            _impactView.Show(prepared, Definition.Radius, false);
        }
        private void Update()
        {
            if (!CanAdvanceGameplay || _health == null || _health.IsDown || (_motor == null || _motor.IsDashing)) { CancelCasting(); return; }
            if (IsCasting)
            {
                ShowAtFeet();
                if (_windup.TryResolve(out _)) Resolve();
            }
            if (_hideAt > 0f && GameplayTime >= _hideAt) { _hideAt = 0f; _impactView.Hide(); }
        }
        private void Resolve()
        {
            Vector3 p = transform.position;
            var center = new DefensePoint3(p.x, p.y, p.z);
            if (!(Gameplay.Targets is IDefenseAreaTargetQuery area) || !area.TryCopyWithin(center, Definition.Radius, Definition.TargetMask, _targets, out int count)) { CancelCasting(); return; }
            int generation = Gameplay.RunGeneration;
            try
            {
                for (int i = 0; i < count; i++)
                {
                    if (!isActiveAndEnabled || !CanAdvanceGameplay || Gameplay.RunGeneration != generation || !_health.isActiveAndEnabled || _health.IsDown || _motor.IsDashing) break;
                    var target = _targets[i];
                    if (target != null && target.IsAvailable && target.Identity is IDefenseEnemyTarget3D enemy)
                        enemy.TakeDamage(new DefenseDamageContext(_attack.BaseDamage > 0f ? _attack.EffectiveDamage * (Definition.Damage / _attack.BaseDamage) : Definition.Damage, _health,
                            enemy.TargetTransform.position, runGeneration: generation, sessionIdentity: Gameplay.SessionIdentity));
                }
                if (isActiveAndEnabled && CanAdvanceGameplay && Gameplay.RunGeneration == generation && _health.isActiveAndEnabled && !_health.IsDown && !_motor.IsDashing)
                {
                    if (_relicSource.Ability != null && _relicSource.Ability.BelongsTo(Gameplay))
                        _relicSource.Ability.ReduceRemainingCooldownFraction(_relicCooldownReduction);
                    ShowAtFeet();
                    _hideAt = GameplayTime + 0.25f;
                }
            }
            finally { Array.Clear(_targets, 0, count); }
        }
        private void ShowAtFeet()
        {
            Vector3 p = transform.position;
            float y = p.y;
            if (Physics.Raycast(p + Vector3.up * 2f, Vector3.down, out var hit, 8f, 4096, QueryTriggerInteraction.Ignore)) y = hit.point.y;
            var origin = new DefensePoint3(p.x, p.y, p.z);
            _impactView.Show(new DefenseResolvedAim(origin, new DefensePoint3(p.x, y, p.z), default, 0f), Definition.Radius, false);
        }
        public void CancelCasting() { _windup?.Cancel(); _hideAt = 0f; _impactView?.Hide(); }
        protected override void OnGameplayGateChanged(bool active) { if (!active) CancelCasting(); }
        protected override void OnGameplayCleanup() => CancelCasting();
        protected override void OnGameplayReset() => CancelCasting();
        private void OnDisable() => CancelCasting();
    }
}
