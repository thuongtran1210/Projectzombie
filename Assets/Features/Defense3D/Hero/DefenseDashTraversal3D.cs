using UnityEngine;
using UnityEngine.AI;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Allocation-free straight dash validation; clips against blockers, ledges and baked NavMesh boundaries.</summary>
    public sealed class DefenseDashTraversal3D : MonoBehaviour
    {
        [SerializeField] private CapsuleCollider _capsule;
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private LayerMask _obstacleMask;
        [SerializeField, Min(0.05f)] private float _sampleStep = 0.15f;
        [SerializeField, Min(0.01f)] private float _skin = 0.03f;
        [SerializeField, Min(0.01f)] private float _maximumFloorStep = 0.4f;
        [SerializeField, Range(0f, 89f)] private float _maximumSlope = 50f;
        [SerializeField] private bool _requireNavMesh = true;
        private readonly Collider[] _overlaps = new Collider[8];
        private const int MAX_SAMPLES = 128;

        public bool IsConfigured => _capsule != null && _capsule.enabled && !_capsule.isTrigger && _capsule.direction == 1 &&
            _groundMask.value != 0 && _obstacleMask.value != 0 && (_obstacleMask.value & (1 << _capsule.gameObject.layer)) == 0 &&
            DefenseDamageRules.IsFinite(_sampleStep) && _sampleStep >= 0.05f && DefenseDamageRules.IsFinite(_skin) && _skin > 0f &&
            DefenseDamageRules.IsFinite(_maximumFloorStep) && _maximumFloorStep > 0f &&
            DefenseDamageRules.IsFinite(_maximumSlope) && _maximumSlope >= 0f && _maximumSlope < 90f;

        public bool TryResolve(Vector3 start, Vector3 desired, out Vector3 destination)
        {
            destination = start;
            if (!IsConfigured || !Finite(start) || !Finite(desired)) return false;
            Vector3 delta = desired - start;
            delta.y = 0f;
            float distance = delta.magnitude;
            if (!DefenseDamageRules.IsFinite(distance) || distance <= _skin || distance / _sampleStep > MAX_SAMPLES) return false;
            Vector3 direction = delta / distance;
            Vector3 scale = _capsule.transform.lossyScale;
            float radius = _capsule.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            float height = Mathf.Max(_capsule.height * Mathf.Abs(scale.y), radius * 2f);
            Vector3 centerOffset = _capsule.transform.TransformVector(_capsule.center);
            float halfSegment = height * 0.5f - radius;
            Vector3 center = start + centerOffset;
            Vector3 top = center + Vector3.up * halfSegment, bottom = center - Vector3.up * halfSegment;
            if (Physics.OverlapCapsuleNonAlloc(top, bottom, Mathf.Max(0.01f, radius - _skin), _overlaps,
                _obstacleMask, QueryTriggerInteraction.Ignore) != 0) return false;
            if (Physics.CapsuleCast(top, bottom, Mathf.Max(0.01f, radius - _skin), direction, out var obstacle,
                distance, _obstacleMask, QueryTriggerInteraction.Ignore)) distance = Mathf.Max(0f, obstacle.distance - _skin);
            if (!Floor(start, out var initialFloor)) return false;
            float rootClearance = height * 0.5f - centerOffset.y;
            if (_requireNavMesh)
            {
                if (!NavMesh.SamplePosition(initialFloor, out var navStart, radius, NavMesh.AllAreas) ||
                    (navStart.position - initialFloor).sqrMagnitude > _skin * _skin * 4f) return false;
                Vector3 navEnd = initialFloor + direction * distance;
                if (NavMesh.Raycast(navStart.position, navEnd, out var edge, NavMesh.AllAreas))
                    distance = Mathf.Min(distance, Mathf.Max(0f, Vector3.Dot(edge.position - initialFloor, direction) - radius));
            }
            Vector3 side = new Vector3(-direction.z, 0f, direction.x) * Mathf.Max(0f, radius - _skin);
            Vector3 lastFloor = initialFloor;
            for (float travelled = Mathf.Min(_sampleStep, distance); travelled <= distance && travelled > 0f;)
            {
                Vector3 candidate = start + direction * travelled;
                if (!Floor(candidate, out var floor) || Mathf.Abs(floor.y - lastFloor.y) > _maximumFloorStep ||
                    !Floor(candidate + side, out var left) || !Floor(candidate - side, out var right) ||
                    Mathf.Abs(left.y - floor.y) > _maximumFloorStep || Mathf.Abs(right.y - floor.y) > _maximumFloorStep) break;
                Vector3 candidateRoot = new Vector3(candidate.x, floor.y + rootClearance, candidate.z);
                Vector3 candidateCenter = candidateRoot + centerOffset;
                if (Physics.OverlapCapsuleNonAlloc(candidateCenter + Vector3.up * halfSegment, candidateCenter - Vector3.up * halfSegment,
                    Mathf.Max(0.01f, radius - _skin), _overlaps, _obstacleMask, QueryTriggerInteraction.Ignore) != 0) break;
                destination = candidateRoot;
                lastFloor = floor;
                if (travelled >= distance) break;
                travelled = Mathf.Min(distance, travelled + _sampleStep);
            }
            Vector3 accepted = destination - start;
            accepted.y = 0f;
            return accepted.sqrMagnitude > _skin * _skin;
        }

        private bool Floor(Vector3 point, out Vector3 floor)
        {
            floor = default;
            if (!Physics.Raycast(point + Vector3.up * 2f, Vector3.down, out var hit, 4f, _groundMask,
                QueryTriggerInteraction.Ignore) || Vector3.Angle(hit.normal, Vector3.up) > _maximumSlope) return false;
            floor = hit.point;
            return Finite(floor);
        }

        private static bool Finite(Vector3 point) => DefenseDamageRules.IsFinite(point.x) &&
            DefenseDamageRules.IsFinite(point.y) && DefenseDamageRules.IsFinite(point.z);
    }
}
