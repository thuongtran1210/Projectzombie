using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>AoE quanh Thư Sinh và giảm hồi pháp bảo; chỉ phát kết quả để adapter trình diễn.</summary>
    public sealed class DefenseScholarSignatureEffect3D : DefenseSkillEffect3D
    {
        [SerializeField] private DefenseHeroHealth3D _health;
        [SerializeField] private DefenseHeroAutoAttack3D _attack;
        [SerializeField] private DefenseHeroMotor3D _motor;
        [SerializeField, Tooltip("Component cung cấp IDefenseAbilitySource cho ô pháp bảo.")] private MonoBehaviour _relic;
        [SerializeField, Range(0f, 1f)] private float _relicCooldownReduction = .2f;
        private IDefenseAbilitySource _relicSource;
        private readonly IDefenseTargetPosition[] _targets = new IDefenseTargetPosition[256];
        private DefenseConeTelegraphRuntime _windup;
        private DefenseSignatureFeedbackEvents _feedback;
        private long _nextCastId, _activeCastId;
        private object _castIdentity;
        private int _castGeneration;
        private DefensePoint3 _castPosition;
        private bool _resolving;
        public DefenseSignatureFeedbackEvents Feedback => _feedback ?? (_feedback = new DefenseSignatureFeedbackEvents(error => Debug.LogException(error, this)));
        public bool IsCasting => _windup != null && _windup.IsPending;
        public override DefenseSkillKind Kind => DefenseSkillKind.ScholarSignature;
        public override bool IsConfigured => _health != null && _attack != null && _motor != null && _relic is IDefenseAbilitySource &&
            DefenseDamageRules.IsFinite(_relicCooldownReduction) && _relicCooldownReduction >= 0f && _relicCooldownReduction <= 1f;
        protected override void OnGameplayBound()
        { CancelCasting(); _relicSource = _relic as IDefenseAbilitySource; }
        protected override void OnConfigured(DefenseSkillDefinition definition)
        {
            CancelCasting();
            _windup = new DefenseConeTelegraphRuntime(Gameplay, definition.Duration, definition.Radius, 360f);
        }
        public override bool TryPrepare(DefenseResolvedAim requested, out DefenseResolvedAim prepared)
        {
            Vector3 p = transform.position;
            var center = new DefensePoint3(p.x, requested.Target.Y, p.z);
            prepared = new DefenseResolvedAim(new DefensePoint3(p.x, p.y, p.z), center, default, 0f);
            return isActiveAndEnabled && CanAdvanceGameplay && IsConfigured && Definition != null && !_health.IsDown && !_motor.IsDashing &&
                !IsCasting && !_resolving && _activeCastId == 0 && Gameplay.Targets is IDefenseAreaTargetQuery;
        }
        public override void Execute(DefenseResolvedAim prepared)
        {
            if (!CanAdvanceGameplay || !isActiveAndEnabled || !IsConfigured || _windup == null || _resolving || _activeCastId != 0 ||
                _health.IsDown || _motor.IsDashing || !_windup.TryBegin(prepared.Origin, new DefensePoint3(0, 0, 1))) return;
            _activeCastId = checked(++_nextCastId);
            _castIdentity = Gameplay.SessionIdentity; _castGeneration = Gameplay.RunGeneration; _castPosition = prepared.Origin;
            _attack.CancelPendingAttack();
            Feedback.PublishStarted(Snapshot());
        }
        private DefenseSignatureFeedback Snapshot(bool completed = false) => new DefenseSignatureFeedback(
            _activeCastId, _castPosition, Definition.Radius, _castIdentity, _castGeneration, completed);
        private bool Current(long castId, object identity, int generation) => _activeCastId == castId &&
            isActiveAndEnabled && CanAdvanceGameplay && ReferenceEquals(Gameplay.SessionIdentity, identity) && Gameplay.RunGeneration == generation &&
            _health != null && _health.isActiveAndEnabled && !_health.IsDown && _motor != null && !_motor.IsDashing;
        private void Update()
        {
            if (!CanAdvanceGameplay || _health == null || _health.IsDown || _motor == null || _motor.IsDashing) { CancelCasting(); return; }
            if (IsCasting && _windup.TryResolve(out _)) Resolve();
        }
        private void Resolve()
        {
            if (_activeCastId == 0 || _resolving) return;
            long castId = _activeCastId; object identity = _castIdentity; int generation = _castGeneration;
            if (!Current(castId, identity, generation)) { CancelCasting(); return; }
            Vector3 p = transform.position;
            var center = new DefensePoint3(p.x, p.y, p.z);
            if (!(Gameplay.Targets is IDefenseAreaTargetQuery area) || !area.TryCopyWithin(center, Definition.Radius, Definition.TargetMask, _targets, out int count))
            { CancelCasting(); return; }
            _resolving = true; _castPosition = center;
            try
            {
                Feedback.PublishReleased(Snapshot());
                for (int i = 0; i < count; i++)
                {
                    if (!Current(castId, identity, generation)) break;
                    var target = _targets[i];
                    if (target == null || !target.IsAvailable || !(target.Identity is IDefenseEnemyTarget3D enemy)) continue;
                    Vector3 targetPosition = enemy.TargetTransform.position;
                    var hitPosition = DefenseAttackFeedbackPoints.Capture(enemy.TargetTransform, targetPosition + Vector3.up * .7f);
                    Vector3 direction = targetPosition - p; direction.y = 0f;
                    if (direction.sqrMagnitude < .000001f) direction = Vector3.forward;
                    direction.Normalize();
                    float damage = _attack.BaseDamage > 0f ? _attack.EffectiveDamage * (Definition.Damage / _attack.BaseDamage) : Definition.Damage;
                    bool accepted = enemy.TakeDamage(new DefenseDamageContext(damage, _health, targetPosition, runGeneration: generation, sessionIdentity: identity));
                    if (accepted) Feedback.PublishHit(new DefenseSignatureHitFeedback(castId,
                        new DefenseHitFeedback(hitPosition, DefenseAttackFeedbackPoints.Point(direction), damage, identity, generation)));
                }
                if (Current(castId, identity, generation))
                {
                    // Giữ nguyên giảm hồi một lần sau khi xử lý AoE, kể cả khi không có mục tiêu.
                    if (_relicSource.Ability != null && _relicSource.Ability.BelongsTo(Gameplay))
                        _relicSource.Ability.ReduceRemainingCooldownFraction(_relicCooldownReduction);
                    CloseCast(true);
                }
                else if (_activeCastId == castId) CancelCasting();
            }
            catch { if (_activeCastId == castId) CancelCasting(); throw; }
            finally { Array.Clear(_targets, 0, count); _resolving = false; }
        }
        private void CloseCast(bool completed)
        {
            if (_activeCastId == 0) return;
            var feedback = Snapshot(completed);
            _activeCastId = 0; _castIdentity = null; _castGeneration = -1;
            Feedback.PublishEnded(feedback);
        }
        public void CancelCasting() { _windup?.Cancel(); CloseCast(false); }
        protected override void OnGameplayGateChanged(bool active) { if (!active) CancelCasting(); }
        protected override void OnGameplayCleanup() => CancelCasting();
        protected override void OnGameplayReset() => CancelCasting();
        private void OnDisable() => CancelCasting();
    }
}
