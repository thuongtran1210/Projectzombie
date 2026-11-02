using UnityEngine;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>Passive single-quad health bar; no gameplay queries or material instances.</summary>
    public sealed class DefenseEnemyHealthView3D : MonoBehaviour
    {
        private static readonly int RATIO_ID = Shader.PropertyToID("_Ratio");
        private static readonly int TINT_ID = Shader.PropertyToID("_FillColor");
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private Vector2 _normalSize = new Vector2(1.1f, .12f);
        [SerializeField] private Vector2 _bossSize = new Vector2(1.9f, .17f);
        [SerializeField] private Color _normalColor = new Color(.85f,.18f,.14f,1f);
        [SerializeField] private Color _bossColor = new Color(1f,.56f,.16f,1f);
        private MaterialPropertyBlock _properties;
        private Vector2 _size;
        private Vector3 _cachedParentScale = Vector3.one;
        private bool _hasCachedScale;
        public bool IsConfigured => _renderer != null && _renderer.sharedMaterial != null;

        public void InvalidateScaleCache() => _hasCachedScale = false;

        public void SetHealth(float ratio, bool boss)
        {
            if (!IsConfigured) return;
            if (_properties == null) _properties = new MaterialPropertyBlock();
            _size = boss ? _bossSize : _normalSize;
            _properties.Clear();
            _properties.SetFloat(RATIO_ID, Mathf.Clamp01(ratio));
            _properties.SetColor(TINT_ID, boss ? _bossColor : _normalColor);
            _renderer.SetPropertyBlock(_properties);
        }

        public void SetPose(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            if (!_hasCachedScale)
            {
                _cachedParentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
                _hasCachedScale = true;
            }
            transform.localScale = new Vector3(_size.x / Mathf.Max(.001f, Mathf.Abs(_cachedParentScale.x)),
                _size.y / Mathf.Max(.001f, Mathf.Abs(_cachedParentScale.y)), 1f / Mathf.Max(.001f, Mathf.Abs(_cachedParentScale.z)));
        }

        public void SetVisible(bool visible)
        {
            if (_renderer != null) _renderer.enabled = visible;
        }

        public void ResetView()
        {
            if (_renderer == null) return;
            _renderer.enabled = false;
            _renderer.SetPropertyBlock(null);
            _hasCachedScale = false;
        }
    }
}
