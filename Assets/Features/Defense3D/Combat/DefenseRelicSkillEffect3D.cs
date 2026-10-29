using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Quăng pháp bảo, gây damage khi chạm; bấm lần hai để lao tới mốc còn hiệu lực.</summary>
    public sealed class DefenseRelicSkillEffect3D : DefenseSkillEffect3D
    {
        [SerializeField] private DefenseHeroHealth3D _health;
        [SerializeField, Min(1)] private int _targetCapacity = 256;
        [SerializeField] private DefenseHitStatusProfileSO _hitStatuses;
        private IDefenseTargetPosition[] _targets;
        private int _count;
        private bool _executing;
        [SerializeField, Min(.1f)] private float _recastWindow = 4f;
        [SerializeField, Min(.05f)] private float _recastDashDuration = .3f;
        private readonly DefenseRelicCastState _cast = new DefenseRelicCastState();
        private DefenseHeroMotor3D _motor;
        private DefenseDashTraversal3D _traversal;
        private DefenseHeroAutoAttack3D _attack;
        private DefenseScholarSignatureEffect3D _signature;
        private Vector3 _throwStart, _anchor;
        private float _throwAt;
        public DefenseRelicPhase Phase => _cast.Phase;
        public Vector3 AnchorPosition => _anchor;
        public Vector3 FlightPosition
        {
            get
            {
                float t = Definition == null ? 0f : Mathf.Clamp01((GameplayTime - _throwAt) / Definition.Duration);
                return Vector3.Lerp(_throwStart, _anchor, t) + Vector3.up * (Mathf.Sin(t * Mathf.PI) * 1.5f);
            }
        }
        public bool CanRecast => isActiveAndEnabled && CanAdvanceGameplay && _cast.CanRecast(GameplayTime) &&
            _health != null && !_health.IsDown && _motor != null && _motor.isActiveAndEnabled && !_motor.IsDashing &&
            _traversal != null && _traversal.IsConfigured && (_signature == null || !_signature.IsCasting);
        public event Action<DefenseRelicImpact> ImpactOccurred;
        public override DefenseSkillKind Kind => DefenseSkillKind.DepToOng;
        public override bool IsConfigured => _health != null && _targetCapacity > 0 && (_hitStatuses == null || _hitStatuses.IsValid) &&
            DefenseDamageRules.IsFinite(_recastWindow) && _recastWindow > 0f &&
            DefenseDamageRules.IsFinite(_recastDashDuration) && _recastDashDuration > 0f;

        private void Awake()
        {
            _targets = new IDefenseTargetPosition[Mathf.Max(1, _targetCapacity)];
            TryGetComponent(out _motor);
            TryGetComponent(out _traversal);
            TryGetComponent(out _attack);
            TryGetComponent(out _signature);
        }

        public override bool TryPrepare(DefenseResolvedAim requested, out DefenseResolvedAim prepared)
        {
            prepared = requested;
            return CanAdvanceGameplay && Definition != null && IsConfigured && !_health.IsDown && !_executing &&
                _cast.Phase == DefenseRelicPhase.Ready && (_motor == null || !_motor.IsDashing) &&
                (_signature == null || !_signature.IsCasting) && Gameplay.Targets is IDefenseAreaTargetQuery;
        }

        public override void Execute(DefenseResolvedAim prepared)
        {
            _throwStart = transform.position + Vector3.up;
            _anchor = new Vector3(prepared.Target.X, prepared.Target.Y, prepared.Target.Z);
            _throwAt = GameplayTime;
            _cast.Begin(GameplayTime, Definition.Duration, _recastWindow);
        }

        /// <summary>Consumes the recast only after a safe destination has been resolved. Cooldown remains from the throw.</summary>
        public bool TryRecast()
        {
            if (!CanRecast || !_motor.CanAim(this) ||
                !_traversal.TryResolve(_motor.transform.position, _anchor, out var destination)) return false;
            _attack?.CancelPendingAttack();
            _motor.BeginDash(destination, _recastDashDuration, _traversal);
            if (!_motor.IsDashing) return false;
            return _cast.Consume(GameplayTime);
        }

        private void Update()
        {
            if (_health == null || _health.IsDown) { _cast.Reset(); return; }
            if (!CanAdvanceGameplay) return;
            if (_cast.Advance(GameplayTime)) ResolveImpact();
        }

        private void ResolveImpact()
        {
            if (!(Gameplay.Targets is IDefenseAreaTargetQuery area)) return;
            var targetPoint = new DefensePoint3(_anchor.x, _anchor.y, _anchor.z);
            Array.Clear(_targets, 0, _count);
            if (!area.TryCopyWithin(targetPoint, Definition.Radius, Definition.TargetMask, _targets, out _count)) return;
            _executing = true;
            int generation = Gameplay.RunGeneration;
            try
            {
                // VFX không được quyết định hoặc chặn damage; lỗi adapter được ghi riêng.
                var impact = new DefenseRelicImpact(_anchor,
                    Definition.Radius, Gameplay.SessionIdentity, generation);
                if (ImpactOccurred != null)
                    foreach (Action<DefenseRelicImpact> observer in ImpactOccurred.GetInvocationList())
                        try { observer(impact); } catch (Exception error) { Debug.LogException(error, this); }
                for (int i = 0; i < _count; i++)
                {
                    if (!CanAdvanceGameplay || Gameplay.RunGeneration != generation || _health.IsDown) break;
                    var target = _targets[i];
                    if (target != null && target.IsAvailable && target.Identity is IDefenseEnemyTarget3D enemy)
                    {
                        bool hit = enemy.TakeDamage(new DefenseDamageContext(Definition.Damage, this, targetPoint,
                            runGeneration: generation, sessionIdentity: Gameplay.SessionIdentity));
                        if (hit && enemy.IsTargetable && _hitStatuses != null && enemy is IDefenseStatusReceiver receiver)
                        {
                            Vector3 offset = enemy.TargetTransform.position - _anchor;
                            _hitStatuses.Apply(receiver, Gameplay, new DefensePoint3(offset.x, offset.y, offset.z), generation);
                        }
                    }
                }
            }
            finally
            {
                Array.Clear(_targets, 0, _count);
                _count = 0;
                _executing = false;
            }
        }

        private void OnDisable() => OnGameplayCleanup();
        protected override void OnGameplayCleanup()
        {
            _cast.Reset();
            if (!_executing && _targets != null) { Array.Clear(_targets, 0, _count); _count = 0; }
        }
        protected override void OnGameplayReset() => OnGameplayCleanup();
    }
}
