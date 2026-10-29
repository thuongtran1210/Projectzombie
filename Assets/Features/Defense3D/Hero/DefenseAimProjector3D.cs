using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Authored camera/ground adapter. Validates the clamped endpoint, not a dash traversal path.</summary>
    public sealed class DefenseAimProjector3D : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _origin;
        [SerializeField] private LayerMask _groundMask;
        [SerializeField, Min(0.1f)] private float _surfaceProbeHeight = 20f;
        [SerializeField, Min(0.1f)] private float _screenRayDistance = 1000f;
        [SerializeField, Range(0f, 89f)] private float _maximumSlope = 50f;

        public bool IsConfigured => _camera != null && _origin != null && _groundMask.value != 0 &&
            DefenseDamageRules.IsFinite(_surfaceProbeHeight) && _surfaceProbeHeight > 0f &&
            DefenseDamageRules.IsFinite(_screenRayDistance) && _screenRayDistance > 0f &&
            DefenseDamageRules.IsFinite(_maximumSlope) && _maximumSlope >= 0f && _maximumSlope < 90f;

        public bool TryDrag(float x, float y, float pull, float range, out DefenseResolvedAim result, float minimumDistance = 0f)
        {
            return TryPreviewDrag(x, y, pull, range, out result, out bool validSurface, minimumDistance) && validSurface;
        }

        /// <summary>Retains finite aim geometry for a red preview when the strict surface query fails.</summary>
        public bool TryPreviewDrag(float x, float y, float pull, float range, out DefenseResolvedAim result,
            out bool validSurface, float minimumDistance = 0f)
        {
            result = default;
            validSurface = false;
            if (!IsConfigured) return false;
            if (!DefenseDamageRules.IsFinite(minimumDistance) || minimumDistance < 0f || range <= 0f) return false;
            Vector3 right = Vector3.ProjectOnPlane(_camera.transform.right, Vector3.up);
            Vector3 up = Vector3.ProjectOnPlane(_camera.transform.up, Vector3.up);
            if (up.sqrMagnitude < 0.000001f) up = Vector3.ProjectOnPlane(_camera.transform.forward, Vector3.up);
            if (!DefenseAimResolver.TryResolveDrag(Point(_origin.position), Point(right), Point(up),
                x, y, pull, range, Point(_origin.forward), out var geometry, minimumDistance)) return false;
            validSurface = TrySurface(geometry, out result);
            if (!validSurface) result = geometry;
            return true;
        }

        public bool TryPreviewDefault(float range, float initialDistance, out DefenseResolvedAim result, out bool validSurface)
        {
            result = default;
            validSurface = false;
            if (!IsConfigured) return false;
            Vector3 desired = _origin.position + Vector3.ProjectOnPlane(_origin.forward, Vector3.up).normalized *
                Mathf.Min(initialDistance, range * 0.7f);
            if (!DefenseAimResolver.TryResolve(Point(_origin.position), Point(desired), range, out var geometry)) return false;
            validSurface = TrySurface(geometry, out result);
            if (!validSurface) result = geometry;
            return true;
        }

        public bool TryScreenPoint(Vector2 screenPoint, float range, out DefenseResolvedAim result)
        {
            result = default;
            if (!IsConfigured || !DefenseDamageRules.IsFinite(screenPoint.x) || !DefenseDamageRules.IsFinite(screenPoint.y)) return false;
            Ray ray = _camera.ScreenPointToRay(screenPoint);
            if (!Physics.Raycast(ray, out var hit, _screenRayDistance, _groundMask, QueryTriggerInteraction.Ignore) ||
                !DefenseAimResolver.TryResolve(Point(_origin.position), Point(hit.point), range, out var geometry)) return false;
            return TrySurface(geometry, out result);
        }

        private bool TrySurface(DefenseResolvedAim geometry, out DefenseResolvedAim result)
        {
            result = default;
            Vector3 start = new Vector3(geometry.Target.X, geometry.Origin.Y + _surfaceProbeHeight, geometry.Target.Z);
            if (!Physics.Raycast(start, Vector3.down, out var hit, _surfaceProbeHeight * 2f, _groundMask,
                QueryTriggerInteraction.Ignore) || Vector3.Angle(hit.normal, Vector3.up) > _maximumSlope) return false;
            result = new DefenseResolvedAim(geometry.Origin, Point(hit.point), geometry.Direction, geometry.Distance);
            return result.Target.IsFinite;
        }

        public bool TrySelf(out DefenseResolvedAim result)
        {
            result = default;
            if (!IsConfigured) return false;
            var origin = Point(_origin.position);
            return TrySurface(new DefenseResolvedAim(origin, origin, default, 0f), out result);
        }

        public bool TryTap(IDefenseTargetQuery targets, float range, bool autoTarget, out DefenseResolvedAim result,
            DefenseTargetMask mask = DefenseTargetMask.All)
        {
            result = default;
            if (!IsConfigured) return false;
            DefensePoint3 origin = Point(_origin.position);
            DefensePoint3 desired;
            if (autoTarget)
            {
                var target = targets?.FindNearest(origin, range, mask);
                desired = target != null ? target.Position : Point(_origin.position +
                    Vector3.ProjectOnPlane(_origin.forward, Vector3.up).normalized * Mathf.Min(5f, range * 0.85f));
            }
            else
            {
                Vector3 forward = Vector3.ProjectOnPlane(_origin.forward, Vector3.up).normalized;
                if (forward.sqrMagnitude < 0.000001f) return false;
                desired = Point(_origin.position + forward * range);
            }
            return DefenseAimResolver.TryResolve(origin, desired, range, out var geometry) && TrySurface(geometry, out result);
        }

        private static DefensePoint3 Point(Vector3 value) => new DefensePoint3(value.x, value.y, value.z);
    }
}
