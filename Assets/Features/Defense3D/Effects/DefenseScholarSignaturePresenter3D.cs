using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Wiring VFX Thư Sinh; hủy riêng các handle của signature khi dash, Downed hoặc đổi session.</summary>
    public sealed class DefenseScholarSignaturePresenter3D : MonoBehaviour
    {
        [SerializeField] private DefenseScholarSignatureEffect3D _source;
        [SerializeField] private DefenseHeroHealth3D _health;
        [SerializeField] private DefenseHeroMotor3D _motor;
        [SerializeField] private Transform _groundAnchor;
        [SerializeField] private DefenseSignatureEffectSetSO _style;
        [SerializeField] private DefenseIndicatorSurface3D _surface;
        [SerializeField] private ProjectZombie.Features.Player.Camera3DFollower _camera;
        private IDefenseGameplayContext _gameplay;
        private DefenseSignatureFeedbackAdapter _adapter;
        public bool IsConfigured => _source != null && _health != null && _motor != null && _groundAnchor != null &&
            _groundAnchor.IsChildOf(_source.transform) && _style != null && _style.IsValid;
        public void Bind(IDefenseGameplayContext gameplay, IDefenseEffectService effects)
        {
            Unbind();
            if (!IsConfigured) throw new System.InvalidOperationException("Assign Scholar signature presentation bindings.");
            _gameplay = gameplay;
            if (_camera == null && Camera.main != null)
                Camera.main.TryGetComponent(out _camera);
            _source.Feedback.Released += OnReleased;
            _gameplay.GameplayGateChanged += OnGate;
            _gameplay.RunCleanup += ClearImpulse;
            _adapter = new DefenseSignatureFeedbackAdapter(_source.Feedback, gameplay, effects, _style, _source, _groundAnchor,
                () => isActiveAndEnabled && _source != null && _source.isActiveAndEnabled && _health != null && !_health.IsDown && _motor != null && !_motor.IsDashing,
                (point, radius) => _surface != null ? _surface.Place(point, .04f, radius) : point + (_groundAnchor.position - _source.transform.position));
            _health.Defeated += OnInterrupted; _motor.DashFeedback.Started += OnDash;
        }
        private void OnReleased(DefenseSignatureFeedback cue)
        {
            if (_gameplay == null || !_gameplay.AdvancesGameplay || !isActiveAndEnabled ||
                !ReferenceEquals(cue.SessionIdentity, _gameplay.SessionIdentity) || cue.RunGeneration != _gameplay.RunGeneration) return;
            if (_camera != null) _camera.PlayImpulse(_style.CameraImpulseAmplitude, _style.CameraImpulseDuration);
        }
        private void ClearImpulse() { if (_camera != null) _camera.ClearImpulse(); }
        private void OnGate(bool active) { if (!active) ClearImpulse(); }
        private void OnInterrupted() { _source?.CancelCasting(); _adapter?.Clear(); ClearImpulse(); }
        private void OnDash(DefenseDashFeedback dash) => OnInterrupted();
        public void Unbind()
        {
            if (_source != null) _source.Feedback.Released -= OnReleased;
            if (_gameplay != null)
            {
                _gameplay.GameplayGateChanged -= OnGate;
                _gameplay.RunCleanup -= ClearImpulse;
            }
            _gameplay = null;
            ClearImpulse();
            if (_health != null) _health.Defeated -= OnInterrupted;
            if (_motor != null) _motor.DashFeedback.Started -= OnDash;
            _adapter?.Dispose(); _adapter = null;
        }
        private void OnDisable() { _adapter?.Clear(); ClearImpulse(); }
        private void OnDestroy() => Unbind();
    }
}
