using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Vòng niệm theo bán kính thật; không quyết định thời điểm tung skill hoặc damage.</summary>
    public sealed class DefenseSealEffectVisual3D : DefenseEffectVisual3D
    {
        [SerializeField] private MeshRenderer _renderer;
        private MaterialPropertyBlock _properties;
        private Color _color;
        public override bool IsConfigured => _renderer != null;
        public override void Prepare(Color color, float diameter)
        {
            _properties = _properties ?? new MaterialPropertyBlock(); _color = color;
            _renderer.transform.localScale = Vector3.one * diameter;
            Render(0f);
        }
        public override void Render(float progress)
        {
            var color = _color; color.a *= 1f - progress;
            _properties.Clear(); _properties.SetColor("_BaseColor", color);
            _renderer.SetPropertyBlock(_properties); _renderer.enabled = true;
        }
        public override void ResetVisual()
        { if (_renderer != null) { _renderer.SetPropertyBlock(null); _renderer.enabled = false; } }
    }
}
