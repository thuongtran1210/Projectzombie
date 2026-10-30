using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Theo dõi một cast và các handle của nó; không hủy VFX kỹ năng khác trên cùng actor.</summary>
    public sealed class DefenseSignatureFeedbackAdapter : IDisposable
    {
        private enum Phase { None, Casting, Released, Completed }
        private readonly DefenseSignatureFeedbackEvents _source;
        private readonly IDefenseGameplayContext _gameplay;
        private readonly IDefenseEffectService _effects;
        private readonly DefenseSignatureEffectSetSO _style;
        private readonly Component _owner;
        private readonly Transform _anchor;
        private readonly Func<bool> _canPresent;
        private readonly Func<Vector3, float, Vector3> _placeArea;
        private readonly List<DefenseEffectHandle> _transients = new List<DefenseEffectHandle>();
        private DefenseEffectHandle _cast;
        private Phase _phase;
        private long _lastId, _castId, _revision;
        private object _identity;
        private int _generation, _hitCount;
        private bool _disposed;
        public DefenseSignatureFeedbackAdapter(DefenseSignatureFeedbackEvents source, IDefenseGameplayContext gameplay,
            IDefenseEffectService effects, DefenseSignatureEffectSetSO style, Component owner, Transform anchor,
            Func<bool> canPresent = null, Func<Vector3, float, Vector3> placeArea = null)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _gameplay = gameplay ?? throw new ArgumentNullException(nameof(gameplay));
            _effects = effects ?? throw new ArgumentNullException(nameof(effects));
            if (style == null || !style.IsValid || owner == null || anchor == null) throw new ArgumentException("Assign valid signature style/owner/anchor.");
            _style = style; _owner = owner; _anchor = anchor; _canPresent = canPresent; _placeArea = placeArea;
            _source.Started += OnStarted; _source.Released += OnReleased; _source.HitConfirmed += OnHit; _source.Ended += OnEnded;
            _gameplay.RunCleanup += Clear; _gameplay.GameplayGateChanged += OnGate;
        }
        private bool Current(object identity, int generation) => !_disposed && _gameplay.AdvancesGameplay &&
            ReferenceEquals(identity, _gameplay.SessionIdentity) && generation == _gameplay.RunGeneration && _owner != null &&
            _owner.gameObject.activeInHierarchy && _anchor != null && (_canPresent == null || _canPresent());
        private bool Matches(long id, object identity, int generation) => !_disposed && id == _castId && id > 0 &&
            ReferenceEquals(identity, _identity) && generation == _generation;
        private bool ValidArea(DefenseSignatureFeedback cue) => cue.Position.IsFinite && DefenseDamageRules.IsFinite(cue.Radius) && cue.Radius > 0f;
        private void OnStarted(DefenseSignatureFeedback cue)
        {
            if (!Current(cue.SessionIdentity, cue.RunGeneration) || !ValidArea(cue) || cue.CastId <= _lastId) return;
            _lastId = cue.CastId;
            long revision = ClearHandles();
            if (_revision != revision || !Current(cue.SessionIdentity, cue.RunGeneration)) return;
            _castId = cue.CastId; _identity = cue.SessionIdentity; _generation = cue.RunGeneration; _phase = Phase.Casting;
            var handle = _effects.Play(new DefenseEffectRequest(_style.Cast, _anchor.position + Vector3.up * _style.Lift, cue.Radius, _owner, _anchor,
                rotation: Quaternion.Euler(0f, _owner.transform.eulerAngles.y, 0f)));
            if (!Commit(cue.CastId, cue.SessionIdentity, cue.RunGeneration, revision, Phase.Casting)) Stop(handle);
            else _cast = handle;
        }
        private bool Commit(long id, object identity, int generation, long revision, Phase phase) => _revision == revision &&
            _phase == phase && Matches(id, identity, generation) && Current(identity, generation);
        private void OnReleased(DefenseSignatureFeedback cue)
        {
            if (!Matches(cue.CastId, cue.SessionIdentity, cue.RunGeneration) || _phase != Phase.Casting) return;
            if (!Current(cue.SessionIdentity, cue.RunGeneration) || !ValidArea(cue)) { Clear(); return; }
            long revision = _revision; _phase = Phase.Released;
            var cast = _cast; _cast = default; Stop(cast);
            if (!Commit(cue.CastId, cue.SessionIdentity, cue.RunGeneration, revision, Phase.Released)) return;
            var point = new Vector3(cue.Position.X, cue.Position.Y, cue.Position.Z);
            if (_placeArea != null) point = _placeArea(point, cue.Radius);
            var handle = _effects.Play(new DefenseEffectRequest(_style.Release, point + Vector3.up * _style.Lift, cue.Radius, _owner,
                rotation: Quaternion.Euler(0f, _owner.transform.eulerAngles.y, 0f)));
            KeepTransient(handle, cue.CastId, cue.SessionIdentity, cue.RunGeneration, revision);
        }
        private void OnHit(DefenseSignatureHitFeedback cue)
        {
            var hit = cue.Hit;
            if (_phase != Phase.Released || !Matches(cue.CastId, hit.SessionIdentity, hit.RunGeneration) || !Current(hit.SessionIdentity, hit.RunGeneration) ||
                !hit.Position.IsFinite || !hit.Direction.IsFinite || !DefenseDamageRules.IsFinite(hit.Damage) || hit.Damage <= 0f || _hitCount >= _style.MaxHitEffects) return;
            _hitCount++;
            long revision = _revision;
            var direction = new Vector3(hit.Direction.X, 0f, hit.Direction.Z);
            var rotation = direction.sqrMagnitude > .000001f ? Quaternion.LookRotation(direction) : Quaternion.identity;
            var handle = _effects.Play(new DefenseEffectRequest(_style.Hit, new Vector3(hit.Position.X, hit.Position.Y, hit.Position.Z),
                _style.HitRadius, _owner, rotation: rotation));
            KeepTransient(handle, cue.CastId, hit.SessionIdentity, hit.RunGeneration, revision);
        }
        private void KeepTransient(DefenseEffectHandle handle, long id, object identity, int generation, long revision)
        {
            if (!Commit(id, identity, generation, revision, Phase.Released)) Stop(handle);
            else if (handle.IsAssigned) _transients.Add(handle);
        }
        private void OnEnded(DefenseSignatureFeedback cue)
        {
            if (!Matches(cue.CastId, cue.SessionIdentity, cue.RunGeneration)) return;
            if (!cue.Completed || !Current(cue.SessionIdentity, cue.RunGeneration) || _phase == Phase.Casting) { Clear(); return; }
            if (_phase == Phase.Released) _phase = Phase.Completed;
            // Burst/spark đã phát tự kết thúc bằng profile; giữ handle để death/dash có thể dọn ngay.
        }
        private long ClearHandles()
        {
            long revision = ++_revision;
            _castId = 0; _identity = null; _phase = Phase.None; _hitCount = 0;
            var cast = _cast; _cast = default;
            var transients = _transients.ToArray(); _transients.Clear();
            List<Exception> failures = null;
            try { Stop(cast); } catch (Exception error) { failures = new List<Exception> { error }; }
            foreach (var handle in transients)
                try { Stop(handle); }
                catch (Exception error) { if (failures == null) failures = new List<Exception>(); failures.Add(error); }
            if (failures != null) throw new AggregateException(failures);
            return revision;
        }
        private void Stop(DefenseEffectHandle handle) { if (handle.IsAssigned) _effects.Stop(handle); }
        private void OnGate(bool active) { if (!active) Clear(); }
        public void Clear() { ClearHandles(); }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _source.Started -= OnStarted; _source.Released -= OnReleased; _source.HitConfirmed -= OnHit; _source.Ended -= OnEnded;
            _gameplay.RunCleanup -= Clear; _gameplay.GameplayGateChanged -= OnGate;
            Clear();
        }
    }
}
