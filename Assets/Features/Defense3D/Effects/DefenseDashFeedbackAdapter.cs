using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Sở hữu đúng ba handle của dash; không dừng nhầm hiệu ứng skill khác trên cùng actor.</summary>
    public sealed class DefenseDashFeedbackAdapter : IDisposable
    {
        private readonly DefenseDashFeedbackEvents _source;
        private readonly IDefenseGameplayContext _gameplay;
        private readonly IDefenseEffectService _effects;
        private readonly DefenseDashEffectSetSO _style;
        private readonly Component _owner;
        private readonly Transform _anchor;
        private readonly Func<bool> _canPresent;
        private readonly Func<Vector3, Vector3> _placeBurst;
        private DefenseEffectHandle _start, _trail, _end;
        private long _lastDashId, _activeDashId, _revision;
        private object _identity;
        private int _generation;
        private bool _disposed;
        public DefenseDashFeedbackAdapter(DefenseDashFeedbackEvents source, IDefenseGameplayContext gameplay,
            IDefenseEffectService effects, DefenseDashEffectSetSO style, Component owner, Transform anchor,
            Func<bool> canPresent = null, Func<Vector3, Vector3> placeBurst = null)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _gameplay = gameplay ?? throw new ArgumentNullException(nameof(gameplay));
            _effects = effects ?? throw new ArgumentNullException(nameof(effects));
            if (style == null || !style.IsValid || owner == null || anchor == null) throw new ArgumentException("Assign valid dash style/owner/anchor.");
            _style = style; _owner = owner; _anchor = anchor; _canPresent = canPresent; _placeBurst = placeBurst;
            _source.Started += OnStarted; _source.Ended += OnEnded;
            _gameplay.RunCleanup += Clear; _gameplay.GameplayGateChanged += OnGate;
        }
        private bool Current(DefenseDashFeedback feedback) => !_disposed && _gameplay.AdvancesGameplay &&
            ReferenceEquals(feedback.SessionIdentity, _gameplay.SessionIdentity) && feedback.RunGeneration == _gameplay.RunGeneration &&
            _owner != null && _owner.gameObject.activeInHierarchy && _anchor != null && (_canPresent == null || _canPresent());
        private void OnStarted(DefenseDashFeedback feedback)
        {
            if (!Current(feedback) || !feedback.Position.IsFinite || !feedback.Direction.IsFinite || feedback.DashId <= _lastDashId) return;
            var direction = new Vector3(feedback.Direction.X, 0f, feedback.Direction.Z);
            if (direction.sqrMagnitude < .000001f) return;
            _lastDashId = feedback.DashId;
            long revision = ClearHandles();
            if (_revision != revision || !Current(feedback)) return;
            _activeDashId = feedback.DashId; _identity = feedback.SessionIdentity; _generation = feedback.RunGeneration;
            var rotation = Quaternion.LookRotation(direction);
            var burstPosition = new Vector3(feedback.Position.X, feedback.Position.Y, feedback.Position.Z);
            if (_placeBurst != null) burstPosition = _placeBurst(burstPosition);
            burstPosition += Vector3.up * _style.Lift;
            var start = _effects.Play(new DefenseEffectRequest(_style.Start, burstPosition, _style.BurstRadius, _owner, rotation: rotation));
            // Play có thể gây callback cancel/reset; handle vừa trả về phải được thu hồi nếu lượt đã đổi.
            if (!Matches(feedback, revision)) { Stop(start); return; }
            _start = start;
            var trail = _effects.Play(new DefenseEffectRequest(_style.Trail, _anchor.position + Vector3.up * _style.Lift,
                _style.TrailRadius, _owner, _anchor, rotation));
            if (!Matches(feedback, revision)) { Stop(trail); return; }
            _trail = trail;
        }
        private bool Matches(DefenseDashFeedback feedback, long revision) => !_disposed && _revision == revision &&
            _activeDashId == feedback.DashId && ReferenceEquals(_identity, feedback.SessionIdentity) && _generation == feedback.RunGeneration;
        private void OnEnded(DefenseDashFeedback feedback)
        {
            if (_disposed || _activeDashId == 0 || _activeDashId != feedback.DashId ||
                !ReferenceEquals(_identity, feedback.SessionIdentity) || _generation != feedback.RunGeneration) return;
            long revision = ClearHandles();
            if (_revision != revision || !feedback.Completed || !Current(feedback) || !feedback.Position.IsFinite || !feedback.Direction.IsFinite) return;
            var position = new Vector3(feedback.Position.X, feedback.Position.Y, feedback.Position.Z);
            if (_placeBurst != null) position = _placeBurst(position);
            position += Vector3.up * _style.Lift;
            var direction = new Vector3(feedback.Direction.X, 0f, feedback.Direction.Z);
            var rotation = direction.sqrMagnitude > .000001f ? Quaternion.LookRotation(direction) : Quaternion.identity;
            var end = _effects.Play(new DefenseEffectRequest(_style.End, position, _style.BurstRadius, _owner, rotation: rotation));
            if (_disposed || _revision != revision) Stop(end);
            else _end = end;
        }
        private long ClearHandles()
        {
            long revision = ++_revision;
            _activeDashId = 0; _identity = null;
            var start = _start; var trail = _trail; var end = _end;
            _start = default; _trail = default; _end = default;
            try { Stop(start); }
            finally { try { Stop(trail); } finally { Stop(end); } }
            return revision;
        }
        private void Stop(DefenseEffectHandle handle) { if (handle.IsAssigned) _effects.Stop(handle); }
        private void OnGate(bool active) { if (!active) Clear(); }
        public void Clear() { ClearHandles(); }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _source.Started -= OnStarted; _source.Ended -= OnEnded;
            _gameplay.RunCleanup -= Clear; _gameplay.GameplayGateChanged -= OnGate;
            Clear();
        }
    }
}
