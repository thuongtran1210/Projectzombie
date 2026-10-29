using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Tia trúng đòn nở nhanh rồi tắt; không giữ reference tới mục tiêu đã chết/despawn.</summary>
    public sealed class DefenseHitSparkVisual3D : DefenseEffectVisual3D
    {
        [SerializeField] private MeshRenderer _renderer;
        private MaterialPropertyBlock _properties;
        private Color _color;
        private float _diameter;
        public override bool IsConfigured => _renderer != null;
        public override void Prepare(Color color, float diameter)
        { _properties = _properties ?? new MaterialPropertyBlock(); _color = color; _diameter = diameter; Render(0f); }
        public override void Render(float progress)
        {
            _renderer.transform.localScale = Vector3.one * (_diameter * Mathf.Lerp(.65f, 1.15f, progress));
            var color = _color; color.a *= 1f - progress;
            _properties.Clear(); _properties.SetColor("_BaseColor", color); _properties.SetFloat("_Progress", progress);
            _renderer.SetPropertyBlock(_properties); _renderer.enabled = true;
        }
        public override void ResetVisual()
        { if (_renderer != null) { _renderer.SetPropertyBlock(null); _renderer.enabled = false; } }
    }
}
