using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Session-driven beam pulse. Damage is resolved by the tower, never by the visual.</summary>
    public sealed class DefenseTowerLaserVisual3D : DefenseEffectVisual3D
    {
        [SerializeField] private LineRenderer _core;
        [SerializeField] private LineRenderer _halo;
        [SerializeField] private MeshRenderer _startGlow;
        [SerializeField] private MeshRenderer _endGlow;
        private MaterialPropertyBlock _properties;
        private float _length;
        public override bool IsConfigured => _core != null && _halo != null && _core.sharedMaterial != null && _halo.sharedMaterial != null &&
            _startGlow != null && _endGlow != null && _startGlow.sharedMaterial != null && _endGlow.sharedMaterial != null;
        private Color _color;
        public override void Prepare(Color color, float diameter)
        {
            _color = color;
            _length = diameter;
            _properties = _properties ?? new MaterialPropertyBlock();
            if (_endGlow != null) _endGlow.transform.localPosition = Vector3.forward * diameter;
            ConfigureLine(_core, diameter); ConfigureLine(_halo, diameter);
            Render(0f);
        }
        private static void ConfigureLine(LineRenderer line, float length)
        { line.positionCount = 2; line.SetPosition(0, Vector3.zero); line.SetPosition(1, Vector3.forward * length); }
        public override void Render(float progress)
        {
            float p = Mathf.Clamp01(progress), alpha = 1f - p * p;
            var core = Color.Lerp(_color, Color.white, .8f); core.a = alpha;
            var halo = _color; halo.a *= alpha * .45f;
            _core.startColor = _core.endColor = core; _halo.startColor = _halo.endColor = halo;
            _core.widthMultiplier = Mathf.Lerp(.075f, .02f, p);
            _halo.widthMultiplier = Mathf.Lerp(.3f, .08f, p);
            _properties = _properties ?? new MaterialPropertyBlock();
            _properties.Clear(); _properties.SetFloat("_Progress", p); _properties.SetFloat("_Length", _length); _properties.SetColor("_Tint", _color);
            _core.SetPropertyBlock(_properties); _halo.SetPropertyBlock(_properties);
            if (_startGlow != null) { _startGlow.SetPropertyBlock(_properties); _startGlow.enabled = alpha > .001f; }
            if (_endGlow != null) { _endGlow.SetPropertyBlock(_properties); _endGlow.enabled = alpha > .001f; }
            _core.enabled = _halo.enabled = alpha > .001f;
        }
        public override void ResetVisual()
        {
            if (_core != null) { _core.enabled = false; _core.widthMultiplier = 0f; _core.SetPropertyBlock(null); }
            if (_halo != null) { _halo.enabled = false; _halo.widthMultiplier = 0f; _halo.SetPropertyBlock(null); }
            if (_startGlow != null) { _startGlow.enabled = false; _startGlow.SetPropertyBlock(null); }
            if (_endGlow != null) { _endGlow.enabled = false; _endGlow.SetPropertyBlock(null); }
        }
    }
}
