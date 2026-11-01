using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Authored smoke cards, impact ring and embers driven exclusively by the session clock.</summary>
    public sealed class DefensePipeSmokeVisual3D : DefenseEffectVisual3D
    {
        [SerializeField] private MeshRenderer _ground;
        [SerializeField] private MeshRenderer _smoke;
        [SerializeField] private MeshRenderer _embers;
        private MaterialPropertyBlock _block;
        private float _diameter;
        private static readonly int PROGRESS = Shader.PropertyToID("_Progress");
        public override bool IsConfigured => _ground != null && _smoke != null && _embers != null;

        /// <summary>Radius comes from the same effective definition used by the aim indicator and damage query.</summary>
        public override void Prepare(Color color, float diameter)
        {
            if (_block == null) _block = new MaterialPropertyBlock();
            _diameter = Mathf.Max(.01f, diameter);
            _ground.transform.localScale = Vector3.one * _diameter;
            Render(0f);
        }

        /// <summary>Fast initial impact, a low readable smoke bed, then an upward dissolving tail.</summary>
        public override void Render(float progress)
        {
            if (!IsConfigured) return;
            if (_block == null) _block = new MaterialPropertyBlock();
            float p = Mathf.Clamp01(progress);
            float swell = Mathf.Lerp(.55f, 1f, Mathf.SmoothStep(0f, 1f, p / .09f));
            float scale = _diameter / 8f;
            _smoke.transform.localScale = Vector3.one * (scale * swell);
            _smoke.transform.localPosition = Vector3.up * (.12f + Mathf.Max(0f, p - .75f) * .8f);
            _smoke.transform.localRotation = Quaternion.Euler(0f, Mathf.Sin(p * 5f) * 9f, 0f);
            _embers.transform.localScale = Vector3.one * scale;
            _block.Clear();
            _block.SetFloat(PROGRESS, p);
            Set(_ground, p);
            Set(_smoke, p);
            Set(_embers, p);
        }

        private void Set(Renderer renderer, float progress)
        {
            renderer.SetPropertyBlock(_block);
            renderer.enabled = progress < 1f;
        }

        /// <summary>Clears every renderer and instance override before returning to the owning pool.</summary>
        public override void ResetVisual()
        {
            if (_ground != null) { _ground.enabled = false; _ground.SetPropertyBlock(null); }
            if (_smoke != null) { _smoke.enabled = false; _smoke.SetPropertyBlock(null); }
            if (_embers != null) { _embers.enabled = false; _embers.SetPropertyBlock(null); }
        }
    }
}
