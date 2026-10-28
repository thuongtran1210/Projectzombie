using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    public readonly struct DefenseRelicImpact
    {
        public readonly Vector3 Position;
        public readonly float Radius;
        public readonly object SessionIdentity;
        public readonly int RunGeneration;
        public DefenseRelicImpact(Vector3 position, float radius, object sessionIdentity, int runGeneration)
        { Position = position; Radius = radius; SessionIdentity = sessionIdentity; RunGeneration = runGeneration; }
    }

    /// <summary>Adapter trình diễn pháp bảo, không tham gia damage hoặc cooldown.</summary>
    public sealed class DefenseRelicEffectPresenter3D : MonoBehaviour
    {
        [SerializeField] private DefenseRelicSkillEffect3D _source;
        [SerializeField] private DefenseEffectProfileSO _impact;
        [SerializeField] private DefenseIndicatorSurface3D _surface;
        private IDefenseGameplayContext _gameplay;
        private IDefenseEffectService _effects;
        public bool IsConfigured => _source != null && _impact != null && _impact.IsValid;
        public void Bind(IDefenseGameplayContext gameplay, IDefenseEffectService effects)
        {
            Unbind();
            if (!IsConfigured || gameplay == null || effects == null) throw new System.InvalidOperationException("Assign relic VFX source/profile/service.");
            _gameplay = gameplay; _effects = effects;
            _source.ImpactOccurred += OnImpact;
        }
        private void OnImpact(DefenseRelicImpact impact)
        {
            if (!isActiveAndEnabled || _gameplay == null || !_gameplay.AdvancesGameplay ||
                !ReferenceEquals(impact.SessionIdentity, _gameplay.SessionIdentity) || impact.RunGeneration != _gameplay.RunGeneration) return;
            var position = _surface != null ? _surface.Place(impact.Position, .06f, impact.Radius) : impact.Position + Vector3.up * .06f;
            _effects.Play(_impact, position, impact.Radius, _source);
        }
        public void Unbind()
        {
            if (_source != null) _source.ImpactOccurred -= OnImpact;
            _effects?.StopOwner(_source);
            _gameplay = null; _effects = null;
        }
        private void OnDisable() { _effects?.StopOwner(_source); }
        private void OnDestroy() => Unbind();
    }
}
