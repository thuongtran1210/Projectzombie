using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Gán dependency của dash trong scene, theo dõi Downed để dọn VFX ngay cả khi motor chưa bị disable.</summary>
    public sealed class DefenseHeroDashPresenter3D : MonoBehaviour
    {
        [SerializeField] private DefenseHeroMotor3D _motor;
        [SerializeField] private DefenseHeroHealth3D _health;
        [SerializeField] private Transform _groundAnchor;
        [SerializeField] private DefenseDashEffectSetSO _style;
        [SerializeField] private DefenseIndicatorSurface3D _surface;
        private DefenseDashFeedbackAdapter _adapter;
        public bool IsConfigured => _motor != null && _health != null && _groundAnchor != null &&
            _groundAnchor.IsChildOf(_motor.transform) && _style != null && _style.IsValid;
        public void Bind(IDefenseGameplayContext gameplay, IDefenseEffectService effects)
        {
            Unbind();
            if (!IsConfigured) throw new System.InvalidOperationException("Assign dash motor/health/ground anchor/style.");
            _adapter = new DefenseDashFeedbackAdapter(_motor.DashFeedback, gameplay, effects, _style, _motor, _groundAnchor,
                () => isActiveAndEnabled && _motor != null && _motor.isActiveAndEnabled && _health != null && !_health.IsDown,
                point => _surface != null ? _surface.Place(point, .04f) : point + (_groundAnchor.position - _motor.transform.position));
            _health.Defeated += OnDowned;
        }
        private void OnDowned() { _motor?.CancelDash(); _adapter?.Clear(); }
        public void Unbind()
        {
            if (_health != null) _health.Defeated -= OnDowned;
            _adapter?.Dispose(); _adapter = null;
        }
        private void OnDisable() => _adapter?.Clear();
        private void OnDestroy() => Unbind();
    }
}
