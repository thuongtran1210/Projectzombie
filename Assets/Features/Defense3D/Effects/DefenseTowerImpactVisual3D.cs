using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Baked shards/flames; presentation only, using the effect service's session progress.</summary>
    public sealed class DefenseTowerImpactVisual3D : DefenseEffectVisual3D
    {
        [SerializeField] private Renderer[] _pieces;
        [SerializeField] private bool _lingering;
        private Vector3[] _positions, _scales;
        private MaterialPropertyBlock _block;
        private Color _color;
        private float _diameter;
        public override bool IsConfigured => _pieces != null && _pieces.Length > 0;
        private void Cache()
        {
            if (_block != null) return;
            _block = new MaterialPropertyBlock();
            _positions = new Vector3[_pieces.Length]; _scales = new Vector3[_pieces.Length];
            for (int i = 0; i < _pieces.Length; i++)
            { _positions[i] = _pieces[i].transform.localPosition; _scales[i] = _pieces[i].transform.localScale; }
        }
        public override void Prepare(Color color, float diameter)
        { Cache(); _color = color; _diameter = diameter; Render(0f); }
        public override void Render(float progress)
        {
            Cache();
            float p = Mathf.Clamp01(progress);
            float growth = _lingering ? 1f : Mathf.Lerp(.25f, 1f, 1f - Mathf.Pow(1f - p, 3f));
            for (int i = 0; i < _pieces.Length; i++)
            {
                var piece = _pieces[i];
                piece.transform.localPosition = _positions[i] * (_diameter * growth);
                piece.transform.localScale = _scales[i] * (_diameter * (_lingering ? 1f : Mathf.Lerp(.8f, .15f, p)));
                var color = Color.Lerp(_color, new Color(1f, 1f, .85f, _color.a), i % 3 == 0 ? .65f : 0f);
                color.a *= _lingering ? .7f : 1f - p;
                _block.Clear(); _block.SetColor("_BaseColor", color);
                piece.SetPropertyBlock(_block); piece.enabled = color.a > .005f;
            }
        }
        public override void ResetVisual()
        {
            if (_pieces == null) return;
            foreach (var piece in _pieces) if (piece != null) { piece.SetPropertyBlock(null); piece.enabled = false; }
        }
    }
}
