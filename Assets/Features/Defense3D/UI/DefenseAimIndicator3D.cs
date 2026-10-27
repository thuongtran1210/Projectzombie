using UnityEngine;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>Passive view over authored line/ring geometry. No runtime objects, materials or vertex arrays.</summary>
    public sealed class DefenseAimIndicator3D : MonoBehaviour
    {
        [SerializeField] private LineRenderer _targetRing;
        [SerializeField] private LineRenderer _directionLine;
        [SerializeField] private MeshRenderer _rangeRing;
        [SerializeField] private MeshRenderer _rangeFill;
        [SerializeField] private MeshRenderer _arrow;
        [SerializeField] private MeshRenderer _sector;
        [SerializeField] private LineRenderer _sectorBorder;
        [SerializeField] private DefenseIndicatorSurface3D _surface;
        public bool CanShowCone => IsConfigured && _sector != null && _sectorBorder != null;
        [SerializeField, Min(0.1f)] private float _arrowWidth = 1f;
        [SerializeField, Min(0f)] private float _surfaceOffset = 0.05f;
        [SerializeField] private Color _validColor = new Color(0.2f, 1f, 0.5f, 0.85f);
        [SerializeField] private Color _cancelColor = new Color(1f, 0.2f, 0.2f, 0.85f);
        public bool IsConfigured => _targetRing != null && _directionLine != null && _targetRing.positionCount >= 3 &&
            _directionLine.positionCount == 2;

        private void Awake()
        {
            _tint = new MaterialPropertyBlock();
            Hide();
        }

        public void ShowCone(DefenseResolvedAim aim, float radius, float angle, bool cancelled)
        {
            ShowAim(aim, radius, radius, cancelled);
            if (!CanShowCone || !aim.Origin.IsFinite || !aim.Direction.IsFinite || radius <= 0f) { Hide(); return; }
            Vector3 center = SurfaceAt(new Vector3(aim.Origin.X, aim.Origin.Y, aim.Origin.Z), radius);
            if (_rangeRing != null) _rangeRing.transform.position = center;
            if (_rangeFill != null) _rangeFill.transform.position = center;
            Vector3 direction = new Vector3(aim.Direction.X, 0f, aim.Direction.Z);
            if (direction.sqrMagnitude < 0.000001f) direction = Vector3.forward;
            _sector.transform.SetPositionAndRotation(center, Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0f, 0f));
            _sector.transform.localScale = Vector3.one * (radius * 2f);
            _targetRing.enabled = false;
            if (_arrow != null) _arrow.enabled = false;
            if (_tint == null) _tint = new MaterialPropertyBlock();
            _tint.Clear();
            _tint.SetFloat("_Shape", 5f);
            _tint.SetFloat("_SectorCos", Mathf.Cos(Mathf.Clamp(angle, 1f, 360f) * 0.5f * Mathf.Deg2Rad));
            _tint.SetColor(BASE_COLOR, cancelled ? _cancelColor : new Color(0.2f, 0.608f, 0.839f, 0.18f));
            _sector.SetPropertyBlock(_tint);
            _sector.enabled = true;
            _sectorBorder.transform.SetPositionAndRotation(center + Vector3.up * .002f, Quaternion.LookRotation(direction));
            _sectorBorder.transform.localScale = Vector3.one;
            const int segments = 32;
            _sectorBorder.positionCount = segments + 2;
            _sectorBorder.SetPosition(0, Vector3.zero);
            for (int i = 0; i <= segments; i++)
            {
                float a = (-angle * .5f + angle * i / segments) * Mathf.Deg2Rad;
                _sectorBorder.SetPosition(i + 1, new Vector3(Mathf.Sin(a) * radius, 0f, Mathf.Cos(a) * radius));
            }
            _sectorBorder.loop = true;
            _sectorBorder.startColor = _sectorBorder.endColor = cancelled ? _cancelColor : new Color(.475f, .812f, 1f, .95f);
            _sectorBorder.enabled = true;
        }

        /// <summary>Transient source slash at waist height; this is not a permanent ground range fan.</summary>
        public void ShowSlash(DefenseConeSnapshot cone, float progress)
        {
            Hide();
            if (_sector == null) return;
            Vector3 direction = new Vector3(cone.Forward.X, 0f, cone.Forward.Z);
            if (direction.sqrMagnitude < .000001f) direction = Vector3.forward;
            Vector3 origin = new Vector3(cone.Origin.X, cone.Origin.Y, cone.Origin.Z);
            Vector3 center = SurfaceAt(origin, 0f) + Vector3.up * .68f + direction * .35f;
            _sector.transform.SetPositionAndRotation(center, Quaternion.LookRotation(direction) *
                Quaternion.AngleAxis(progress * 4.2f * Mathf.Rad2Deg, Vector3.up) * Quaternion.Euler(90f, 0f, 0f));
            _sector.transform.localScale = Vector3.one * (cone.Radius * (1.4f / 2.05f) * 2f);
            if (_tint == null) _tint = new MaterialPropertyBlock();
            _tint.Clear();
            _tint.SetFloat("_Shape", 6f);
            _tint.SetFloat("_Progress", progress);
            _tint.SetFloat("_SectorCos", Mathf.Cos(117f * Mathf.Deg2Rad));
            _tint.SetColor(BASE_COLOR, new Color(.392f, 1f, .855f, .85f * (1f - progress)));
            _sector.SetPropertyBlock(_tint);
            _sector.enabled = true;
        }

        private Vector3 SurfaceAt(Vector3 origin, float radius)
        {
            if (_surface != null) return _surface.Place(origin, _surfaceOffset, radius);
            if (Physics.Raycast(origin + Vector3.up * 20f, Vector3.down, out var hit, 40f, 4096, QueryTriggerInteraction.Ignore))
                origin.y = hit.point.y;
            return origin + Vector3.up * _surfaceOffset;
        }

        /// <summary>Persistent basic-attack footprint without the gesture range ring or arrow.</summary>
        public void ShowFootprint(DefenseResolvedAim aim, float radius, float angle)
        {
            ShowCone(aim, radius, angle, false);
            if (!CanShowCone || !_sector.enabled) return;
            if (_rangeRing != null) _rangeRing.enabled = false;
            if (_rangeFill != null) _rangeFill.enabled = false;
            _directionLine.enabled = false;
            _sectorBorder.enabled = false;
            _tint.SetColor(BASE_COLOR, new Color(0.62f, 0.86f, 0.79f, 0.3f));
            _sector.SetPropertyBlock(_tint);
        }

        public void ShowAim(DefenseResolvedAim aim, float radius, float range, bool cancelled, bool showTarget = true)
        {
            Show(aim, radius, cancelled);
            if (_rangeRing == null || _rangeFill == null || _arrow == null || !aim.Target.IsFinite || range <= 0f) return;
            Vector3 origin = SurfaceAt(new Vector3(aim.Origin.X, aim.Origin.Y, aim.Origin.Z), range);
            if (!showTarget) _targetRing.enabled = false;
            Quaternion plane = Quaternion.Euler(90f, 0f, 0f);
            _rangeRing.transform.SetPositionAndRotation(origin, plane);
            _rangeFill.transform.SetPositionAndRotation(origin, plane);
            _rangeRing.transform.localScale = Vector3.one * (range * 2f);
            _rangeFill.transform.localScale = Vector3.one * (range * 1.988f);
            _rangeRing.enabled = _rangeFill.enabled = true;
            if (aim.Distance > 0.001f)
            {
                Vector3 direction = new Vector3(aim.Direction.X, 0f, aim.Direction.Z);
                _arrow.transform.SetPositionAndRotation(origin + direction * (aim.Distance * 0.5f),
                    Quaternion.LookRotation(direction) * plane);
                _arrow.transform.localScale = new Vector3(_arrowWidth, aim.Distance, 1f);
                _arrow.enabled = true;
            }
            else _arrow.enabled = false;
            _directionLine.enabled = false;
            SetTint(_rangeRing, cancelled ? _cancelColor : new Color(0.475f,0.812f,1f,0.65f));
            SetTint(_rangeFill, cancelled ? new Color(0.843f,0.471f,0.435f,0.055f) : new Color(0.2f,0.608f,0.839f,0.055f));
            SetTint(_arrow, cancelled ? _cancelColor : new Color(0.475f,0.812f,1f,0.95f));
        }

        private MaterialPropertyBlock _tint;
        private static readonly int BASE_COLOR = Shader.PropertyToID("_BaseColor");
        private void SetTint(Renderer renderer, Color color)
        {
            if (_tint == null) _tint = new MaterialPropertyBlock();
            _tint.Clear();
            _tint.SetColor(BASE_COLOR, color);
            renderer.SetPropertyBlock(_tint);
        }

        public void Show(DefenseResolvedAim aim, float radius, bool cancelled)
        {
            if (_sector != null) _sector.enabled = false;
            if (_sectorBorder != null) _sectorBorder.enabled = false;
            if (_rangeRing != null) _rangeRing.enabled = false;
            if (_rangeFill != null) _rangeFill.enabled = false;
            if (_arrow != null) _arrow.enabled = false;
            if (_targetRing == null || _directionLine == null || !aim.Origin.IsFinite || !aim.Target.IsFinite ||
                !DefenseDamageRules.IsFinite(radius) || radius < 0f)
            {
                Hide();
                return;
            }
            Vector3 target = SurfaceAt(new Vector3(aim.Target.X, aim.Target.Y, aim.Target.Z), radius);
            _targetRing.transform.SetPositionAndRotation(target, Quaternion.identity);
            _targetRing.transform.localScale = new Vector3(radius, 1f, radius);
            _directionLine.SetPosition(0, SurfaceAt(new Vector3(aim.Origin.X, aim.Origin.Y, aim.Origin.Z), 0f));
            _directionLine.SetPosition(1, target);
            Color color = cancelled ? _cancelColor : _validColor;
            _targetRing.startColor = _targetRing.endColor = color;
            _directionLine.startColor = _directionLine.endColor = color;
            _targetRing.enabled = radius > 0f;
            _directionLine.enabled = aim.Distance > 0f;
        }

        public void Hide()
        {
            if (_targetRing != null) _targetRing.enabled = false;
            if (_directionLine != null) _directionLine.enabled = false;
            if (_sector != null) _sector.enabled = false;
            if (_sectorBorder != null) _sectorBorder.enabled = false;
            if (_rangeRing != null) _rangeRing.enabled = false;
            if (_rangeFill != null) _rangeFill.enabled = false;
            if (_arrow != null) _arrow.enabled = false;
        }

        private void OnDisable() => Hide();
    }
}
