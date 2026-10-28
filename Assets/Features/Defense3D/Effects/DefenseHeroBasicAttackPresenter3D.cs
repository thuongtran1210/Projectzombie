using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Wiring scene cho đánh thường của player; không có vòng Update riêng.</summary>
    public sealed class DefenseHeroBasicAttackPresenter3D : MonoBehaviour
    {
        [SerializeField] private DefenseHeroAutoAttack3D _source;
        [SerializeField] private DefenseHeroHealth3D _health;
        [SerializeField] private DefenseBasicAttackEffectSetSO _style;
        [SerializeField] private DefenseIndicatorSurface3D _surface;
        private DefenseBasicAttackFeedbackAdapter _adapter;
        private IDefenseEffectService _effects;
        public bool IsConfigured => _source != null && _health != null && _style != null && _style.IsValid;
        public void Bind(IDefenseGameplayContext gameplay, IDefenseEffectService effects)
        {
            Unbind();
            if (!IsConfigured) throw new System.InvalidOperationException("Assign hero basic-attack presentation bindings.");
            _effects = effects;
            _adapter = new DefenseBasicAttackFeedbackAdapter(_source.Feedback, gameplay, effects, _style, _source,
                () => isActiveAndEnabled && _source != null && _source.isActiveAndEnabled && _health != null && !_health.IsDown,
                point => _surface != null ? _surface.Place(point, .05f) : point);
        }
        public void Unbind()
        {
            _adapter?.Dispose(); _adapter = null;
            if (_source != null) _effects?.StopOwner(_source);
            _effects = null;
        }
        private void OnDisable() { if (_source != null) _effects?.StopOwner(_source); }
        private void OnDestroy() => Unbind();
    }
}
