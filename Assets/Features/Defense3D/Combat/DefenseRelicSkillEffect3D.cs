using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Pháp bảo gây sát thương tức thì; phát sự kiện kết quả để adapter trình diễn độc lập.</summary>
    public sealed class DefenseRelicSkillEffect3D : DefenseSkillEffect3D
    {
        [SerializeField] private DefenseHeroHealth3D _health;
        [SerializeField, Min(1)] private int _targetCapacity = 256;
        [SerializeField] private DefenseHitStatusProfileSO _hitStatuses;
        private IDefenseTargetPosition[] _targets;
        private int _count;
        private bool _executing;
        public event Action<DefenseRelicImpact> ImpactOccurred;
        public override DefenseSkillKind Kind => DefenseSkillKind.DepToOng;
        public override bool IsConfigured => _health != null && _targetCapacity > 0 && (_hitStatuses == null || _hitStatuses.IsValid);

        private void Awake() => _targets = new IDefenseTargetPosition[Mathf.Max(1, _targetCapacity)];

        public override bool TryPrepare(DefenseResolvedAim requested, out DefenseResolvedAim prepared)
        {
            prepared = requested;
            if (!CanAdvanceGameplay || Definition == null || !IsConfigured || _health.IsDown || _executing ||
                !(Gameplay.Targets is IDefenseAreaTargetQuery area)) return false;
            Array.Clear(_targets, 0, _count);
            return area.TryCopyWithin(requested.Target, Definition.Radius, Definition.TargetMask, _targets, out _count);
        }

        public override void Execute(DefenseResolvedAim prepared)
        {
            _executing = true;
            int generation = Gameplay.RunGeneration;
            try
            {
                // VFX không được quyết định hoặc chặn damage; lỗi adapter được ghi riêng.
                var impact = new DefenseRelicImpact(new Vector3(prepared.Target.X, prepared.Target.Y, prepared.Target.Z),
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
                        bool hit = enemy.TakeDamage(new DefenseDamageContext(Definition.Damage, this, prepared.Target,
                            runGeneration: generation, sessionIdentity: Gameplay.SessionIdentity));
                        if (hit && enemy.IsTargetable && _hitStatuses != null && enemy is IDefenseStatusReceiver receiver)
                        {
                            Vector3 offset = enemy.TargetTransform.position - new Vector3(prepared.Target.X, prepared.Target.Y, prepared.Target.Z);
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

        protected override void OnGameplayCleanup()
        {
            if (!_executing && _targets != null) { Array.Clear(_targets, 0, _count); _count = 0; }
        }
        protected override void OnGameplayReset() => OnGameplayCleanup();
    }
}
