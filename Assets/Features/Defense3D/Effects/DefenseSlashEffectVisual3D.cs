using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Vệt chém có hướng theo đòn đánh; chỉ thay đổi visual, không quyết định mục tiêu hoặc damage.</summary>
    public sealed class DefenseSlashEffectVisual3D : DefenseEffectVisual3D
    {
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField, Range(0f, 180f)] private float _sweepDegrees = 100f;
        [SerializeField, Min(.01f)] private float _radiusScale = .72f;
        private MaterialPropertyBlock _properties;
        private Quaternion _baseRotation;
        private Color _color;
        private bool _cached;
        public override bool IsConfigured => _renderer != null;
        public override void Prepare(Color color, float diameter)
        {
            if (!_cached) { _baseRotation = _renderer.transform.localRotation; _cached = true; }
            _properties = _properties ?? new MaterialPropertyBlock(); _color = color;
            _renderer.transform.localScale = Vector3.one * (diameter * _radiusScale);
            Render(0f);
        }
        public override void Render(float progress)
        {
            _renderer.transform.localRotation = Quaternion.AngleAxis(Mathf.Lerp(-.5f, .5f, progress) * _sweepDegrees, Vector3.up) * _baseRotation;
            var color = _color; color.a *= 1f - progress;
            _properties.Clear(); _properties.SetColor("_BaseColor", color); _properties.SetFloat("_Progress", progress);
            _renderer.SetPropertyBlock(_properties); _renderer.enabled = true;
        }
        public override void ResetVisual()
        {
            if (_renderer == null) return;
            _renderer.SetPropertyBlock(null); _renderer.enabled = false;
            if (_cached) _renderer.transform.localRotation = _baseRotation;
        }
    }
}
