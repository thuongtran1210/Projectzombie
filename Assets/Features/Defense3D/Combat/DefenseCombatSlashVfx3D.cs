using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Renders a transient 3D melee slash arc (Combat VFX); decoupled from UI ground aim indicators.
    /// Uses waist-height orientation and surface projection for clean terrain alignment.
    /// </summary>
    public sealed class DefenseCombatSlashVfx3D : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _slashRenderer;
        [SerializeField] private DefenseIndicatorSurface3D _surface;
        [SerializeField, Min(0f)] private float _surfaceOffset = 0.05f;
        [SerializeField, Min(0.1f)] private float _waistHeight = 0.68f;
        [SerializeField] private Color _slashColor = new Color(0.392f, 1f, 0.855f, 0.85f);

        private static readonly int BASE_COLOR = Shader.PropertyToID("_BaseColor");
        private static readonly int SHAPE = Shader.PropertyToID("_Shape");
        private static readonly int PROGRESS = Shader.PropertyToID("_Progress");
        private static readonly int SECTOR_COS = Shader.PropertyToID("_SectorCos");

        private MaterialPropertyBlock _tint;

        public bool IsConfigured => _slashRenderer != null;

        private void Awake()
        {
            _tint = new MaterialPropertyBlock();
            Hide();
        }

        public void ShowSlash(DefenseConeSnapshot cone, float progress)
        {
            if (_slashRenderer == null) return;
            Vector3 direction = new Vector3(cone.Forward.X, 0f, cone.Forward.Z);
            if (direction.sqrMagnitude < 0.000001f) direction = Vector3.forward;

            Vector3 origin = new Vector3(cone.Origin.X, cone.Origin.Y, cone.Origin.Z);
            Vector3 center = SurfaceAt(origin) + Vector3.up * _waistHeight + direction * 0.35f;

            _slashRenderer.transform.SetPositionAndRotation(
                center,
                Quaternion.LookRotation(direction) *
                Quaternion.AngleAxis(progress * 4.2f * Mathf.Rad2Deg, Vector3.up) *
                Quaternion.Euler(90f, 0f, 0f)
            );

            _slashRenderer.transform.localScale = Vector3.one * (cone.Radius * (1.4f / 2.05f) * 2f);

            if (_tint == null) _tint = new MaterialPropertyBlock();
            _tint.Clear();
            _tint.SetFloat(SHAPE, 6f);
            _tint.SetFloat(PROGRESS, progress);
            _tint.SetFloat(SECTOR_COS, Mathf.Cos(117f * Mathf.Deg2Rad));

            Color color = _slashColor;
            color.a *= (1f - progress);
            _tint.SetColor(BASE_COLOR, color);

            _slashRenderer.SetPropertyBlock(_tint);
            _slashRenderer.enabled = true;
        }

        public void Hide()
        {
            if (_slashRenderer != null) _slashRenderer.enabled = false;
        }

        private Vector3 SurfaceAt(Vector3 origin)
        {
            if (_surface != null) return _surface.Place(origin, _surfaceOffset, 0f);
            if (Physics.Raycast(origin + Vector3.up * 20f, Vector3.down, out var hit, 40f, 4096, QueryTriggerInteraction.Ignore))
                origin.y = hit.point.y;
            return origin + Vector3.up * _surfaceOffset;
        }
    }
}
