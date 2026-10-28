using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Vòng xung lực; giữ hành vi mẫu Dép Tổ Ong khi tách module khỏi pool.</summary>
    public sealed class DefenseRingEffectVisual3D : DefenseEffectVisual3D
    {
        [SerializeField] private MeshRenderer[] _renderers;
        private MaterialPropertyBlock _properties;
        private Color _color;
        private float _diameter;
        public override bool IsConfigured
        {
            get { if (_renderers != null) foreach (var renderer in _renderers) if (renderer != null) return true; return false; }
        }
        public override void Prepare(Color color, float diameter)
        { _properties = _properties ?? new MaterialPropertyBlock(); _color = color; _diameter = diameter; Render(0f); }
        public override void Render(float progress)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                var ring = _renderers[i]; if (ring == null) continue;
                float expansion = i == 0 ? Mathf.Lerp(.3f, 1f, progress) : Mathf.Lerp(.15f, .72f, progress);
                ring.transform.localScale = Vector3.one * (_diameter * expansion);
                var color = _color; color.a *= (1f - progress) * (i == 0 ? 1f : .55f);
                _properties.Clear(); _properties.SetColor("_BaseColor", color);
                ring.SetPropertyBlock(_properties); ring.enabled = true;
            }
        }
        public override void ResetVisual()
        { if (_renderers != null) foreach (var ring in _renderers) if (ring != null) { ring.SetPropertyBlock(null); ring.enabled = false; } }
    }
}
