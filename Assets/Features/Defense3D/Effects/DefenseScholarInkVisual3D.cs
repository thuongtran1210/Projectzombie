using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Pool-safe airborne brush ribbons and secondary ground cue driven by the session clock.</summary>
    public sealed class DefenseScholarInkVisual3D : DefenseEffectVisual3D
    {
        private static readonly int COLOR_ID = Shader.PropertyToID("_BaseColor");
        private static readonly int PROGRESS_ID = Shader.PropertyToID("_Progress");
        private static readonly int PHASE_ID = Shader.PropertyToID("_Phase");
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField, Range(0, 2)] private int _phase;
        [SerializeField] private MeshRenderer[] _airRenderers;
        [SerializeField] private MeshRenderer[] _fragments;
        private Vector3[] _fragmentOrigins;
        private float _airScale;
        private static readonly int KIND_ID = Shader.PropertyToID("_Kind");
        private MaterialPropertyBlock _properties;
        private Color _color;

        public override bool IsConfigured => _renderer != null;

        /// <summary>Scales the ground footprint and baked airborne geometry without generating runtime meshes.</summary>
        public override void Prepare(Color color, float diameter)
        {
            if (!IsConfigured) return;
            if (_properties == null) _properties = new MaterialPropertyBlock();
            _color = color;
            _renderer.transform.localScale = Vector3.one * diameter;
            _airScale = diameter / 12f;
            if (_fragments != null && _fragmentOrigins == null)
            {
                _fragmentOrigins = new Vector3[_fragments.Length];
                for (int i = 0; i < _fragments.Length; i++)
                    if (_fragments[i] != null) _fragmentOrigins[i] = _fragments[i].transform.localPosition;
            }
            Render(0f);
        }

        /// <summary>Uses normalized lifetime instead of global time so pause and reuse are deterministic.</summary>
        public override void Render(float progress)
        {
            if (!IsConfigured || _properties == null) return;
            _properties.Clear();
            _properties.SetColor(COLOR_ID, _color);
            _properties.SetFloat(PROGRESS_ID, Mathf.Clamp01(progress));
            _properties.SetFloat(PHASE_ID, _phase);
            _renderer.SetPropertyBlock(_properties);
            _renderer.enabled = true;
            if (_airRenderers != null)
            {
                for (int i = 0; i < _airRenderers.Length; i++)
                {
                    var ribbon = _airRenderers[i];
                    if (ribbon == null) continue;
                    float expansion = _phase == 0 ? Mathf.Lerp(1.12f, .92f, progress) : Mathf.Lerp(.88f, 1.08f, 1f - Mathf.Pow(1f - progress, 3f));
                    ribbon.transform.localScale = Vector3.one * (_airScale * expansion);
                    ribbon.transform.localRotation = Quaternion.Euler(0f, _phase == 0 ? -55f * progress : 18f * progress, 0f);
                    ApplyAir(ribbon, progress, 0f);
                }
            }
            if (_fragments != null && _fragmentOrigins != null)
            {
                for (int i = 0; i < _fragments.Length; i++)
                {
                    var fragment = _fragments[i];
                    if (fragment == null) continue;
                    var origin = _fragmentOrigins[i];
                    float flight = 1f - Mathf.Pow(1f - progress, 2f);
                    fragment.transform.localPosition = (origin + new Vector3(origin.x * flight, flight * .9f - progress * progress * 1.5f, origin.z * flight)) * _airScale;
                    fragment.transform.localScale = Vector3.one * (_airScale * (1f - progress * .65f));
                    fragment.transform.localRotation = Quaternion.Euler(18f, i * 137.5f, i * 47f + progress * 160f);
                    ApplyAir(fragment, progress, 1f);
                }
            }
        }

        private void ApplyAir(MeshRenderer renderer, float progress, float kind)
        {
            _properties.Clear();
            _properties.SetColor(COLOR_ID, _color);
            _properties.SetFloat(PROGRESS_ID, Mathf.Clamp01(progress));
            _properties.SetFloat(PHASE_ID, _phase);
            _properties.SetFloat(KIND_ID, kind);
            renderer.SetPropertyBlock(_properties);
            renderer.enabled = progress < 1f;
        }

        /// <summary>Clears all per-cast shader state before returning to the existing effect pool.</summary>
        public override void ResetVisual()
        {
            if (_renderer == null) return;
            _renderer.SetPropertyBlock(null);
            _renderer.enabled = false;
            ClearAir(_airRenderers);
            ClearAir(_fragments);
        }
        private static void ClearAir(MeshRenderer[] renderers)
        {
            if (renderers == null) return;
            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;
                renderer.SetPropertyBlock(null);
                renderer.enabled = false;
            }
        }
    }
}
