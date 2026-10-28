using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Nhận sự kiện xác nhận từ gameplay; gỡ subscription theo đúng vòng đời source.</summary>
    public sealed class DefenseBasicAttackFeedbackAdapter : IDisposable
    {
        private readonly DefenseAttackFeedbackEvents _source;
        private readonly IDefenseGameplayContext _gameplay;
        private readonly IDefenseEffectService _effects;
        private readonly DefenseBasicAttackEffectSetSO _style;
        private readonly Component _owner;
        private readonly Func<bool> _isCurrent;
        private readonly Func<Vector3, Vector3> _placeSwing;
        private bool _disposed;
        public DefenseBasicAttackFeedbackAdapter(DefenseAttackFeedbackEvents source, IDefenseGameplayContext gameplay,
            IDefenseEffectService effects, DefenseBasicAttackEffectSetSO style, Component owner = null,
            Func<bool> isCurrent = null, Func<Vector3, Vector3> placeSwing = null)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _gameplay = gameplay ?? throw new ArgumentNullException(nameof(gameplay));
            _effects = effects ?? throw new ArgumentNullException(nameof(effects));
            if (style == null || !style.IsValid) throw new ArgumentException("Assign valid basic-attack effect profiles.", nameof(style));
            _style = style; _owner = owner; _isCurrent = isCurrent; _placeSwing = placeSwing;
            _source.SwingResolved += OnSwing;
            _source.HitConfirmed += OnHit;
        }
        private bool Current(object identity, int generation) => !_disposed && _gameplay.AdvancesGameplay &&
            ReferenceEquals(identity, _gameplay.SessionIdentity) && generation == _gameplay.RunGeneration && (_isCurrent == null || _isCurrent());
        private void OnSwing(DefenseSwingFeedback swing)
        {
            var cone = swing.Cone;
            if (!Current(swing.SessionIdentity, swing.RunGeneration) || !cone.Origin.IsFinite || !cone.Forward.IsFinite ||
                !DefenseDamageRules.IsFinite(cone.Radius) || cone.Radius <= 0f || !DefenseDamageRules.IsFinite(cone.Angle) || cone.Angle <= 0f || cone.Angle > 360f) return;
            var direction = new Vector3(cone.Forward.X, 0f, cone.Forward.Z);
            if (direction.sqrMagnitude < .000001f) return;
            direction.Normalize();
            var position = new Vector3(cone.Origin.X, cone.Origin.Y, cone.Origin.Z);
            if (_placeSwing != null) position = _placeSwing(position);
            position += Vector3.up * _style.SwingHeight + direction * _style.ForwardOffset;
            _effects.Play(new DefenseEffectRequest(_style.Swing, position, cone.Radius, _owner, rotation: Quaternion.LookRotation(direction)));
        }
        private void OnHit(DefenseHitFeedback hit)
        {
            if (!Current(hit.SessionIdentity, hit.RunGeneration) || !hit.Position.IsFinite || !hit.Direction.IsFinite ||
                !DefenseDamageRules.IsFinite(hit.Damage) || hit.Damage <= 0f) return;
            var direction = new Vector3(hit.Direction.X, 0f, hit.Direction.Z);
            if (direction.sqrMagnitude < .000001f) direction = Vector3.forward;
            var position = new Vector3(hit.Position.X, hit.Position.Y, hit.Position.Z);
            // Không bám target: target có thể đã chết hoặc được tái sử dụng ngay trong TakeDamage.
            _effects.Play(new DefenseEffectRequest(_style.Hit, position, _style.HitRadius, _owner, rotation: Quaternion.LookRotation(direction)));
        }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _source.SwingResolved -= OnSwing;
            _source.HitConfirmed -= OnHit;
        }
    }
}
