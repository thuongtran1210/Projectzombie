using System;
using UnityEngine;
using ProjectZombie.Features.Defense3D.UI;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Current Dép MVP: immediate point AoE over a stable session target snapshot; no Physics2D or collider duplicates.</summary>
    public sealed class DefenseRelicSkillEffect3D : DefenseSkillEffect3D
    {
        [SerializeField] private DefenseHeroHealth3D _health;
        [SerializeField] private DefenseAimIndicator3D _impactView;
        [SerializeField, Min(1)] private int _targetCapacity = 256;
        [SerializeField] private DefenseHitStatusProfileSO _hitStatuses;
        private IDefenseTargetPosition[] _targets;
        private int _count;
        private bool _executing;
        private float _hideAt;
        public override DefenseSkillKind Kind => DefenseSkillKind.DepToOng;
        public override bool IsConfigured => _health != null && _impactView != null && _targetCapacity > 0 && (_hitStatuses == null || _hitStatuses.IsValid);

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
                _impactView.Show(new DefenseResolvedAim(prepared.Target, prepared.Target, default, 0f), Definition.Radius, false);
                _hideAt = GameplayTime + Definition.Duration;
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

        private void Update() { if (_hideAt > 0f && GameplayTime >= _hideAt) ClearImpact(); }
        private void ClearImpact() { _hideAt = 0f; _impactView?.Hide(); }
        protected override void OnGameplayGateChanged(bool active) { if (!active) ClearImpact(); }
        protected override void OnGameplayCleanup()
        {
            ClearImpact();
            if (!_executing && _targets != null) { Array.Clear(_targets, 0, _count); _count = 0; }
        }
        protected override void OnGameplayReset() => OnGameplayCleanup();
        private void OnDisable() => ClearImpact();
    }
}
