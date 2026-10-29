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
        [SerializeField] private Sprite _projectileSprite;
        private SpriteRenderer _projectile;
        private IDefenseGameplayContext _gameplay;
        private IDefenseEffectService _effects;
        public bool IsConfigured => _source != null && _impact != null && _impact.IsValid;
        public void Bind(IDefenseGameplayContext gameplay, IDefenseEffectService effects)
        {
            Unbind();
            if (!IsConfigured || gameplay == null || effects == null) throw new System.InvalidOperationException("Assign relic VFX source/profile/service.");
            _gameplay = gameplay; _effects = effects;
            _source.ImpactOccurred += OnImpact;
            if (_projectile == null && _projectileSprite != null)
            {
                var visual = new GameObject("RelicThrowVisual");
                visual.transform.SetParent(transform, false);
                _projectile = visual.AddComponent<SpriteRenderer>();
                _projectile.sprite = _projectileSprite;
                float size = Mathf.Max(_projectileSprite.bounds.size.x, _projectileSprite.bounds.size.y);
                visual.transform.localScale = Vector3.one * (1.3f / Mathf.Max(.01f, size));
                _projectile.sortingOrder = 10;
                _projectile.enabled = false;
            }
        }
        private void OnImpact(DefenseRelicImpact impact)
        {
            if (!isActiveAndEnabled || _gameplay == null || !_gameplay.AdvancesGameplay ||
                !ReferenceEquals(impact.SessionIdentity, _gameplay.SessionIdentity) || impact.RunGeneration != _gameplay.RunGeneration) return;
            var position = _surface != null ? _surface.Place(impact.Position, .06f, impact.Radius) : impact.Position + Vector3.up * .06f;
            _effects.Play(_impact, position, impact.Radius, _source);
        }
        private void LateUpdate()
        {
            if (_projectile == null) return;
            bool visible = isActiveAndEnabled && _gameplay != null && _source != null &&
                _source.Phase != DefenseRelicPhase.Ready;
            _projectile.enabled = visible;
            if (!visible) return;
            bool flying = _source.Phase == DefenseRelicPhase.Flying;
            _projectile.transform.position = flying ? _source.FlightPosition : _source.AnchorPosition + Vector3.up * .12f;
            _projectile.transform.rotation = Quaternion.Euler(90f, flying ? _gameplay.ElapsedSeconds * 720f : 0f, 0f);
        }
        public void Unbind()
        {
            if (_source != null) _source.ImpactOccurred -= OnImpact;
            _effects?.StopOwner(_source);
            _gameplay = null; _effects = null;
            if (_projectile != null) _projectile.enabled = false;
        }
        private void OnDisable() { _effects?.StopOwner(_source); if (_projectile != null) _projectile.enabled = false; }
        private void OnDestroy() => Unbind();
    }
}
