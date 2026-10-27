using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Ground collider plus editor-baked visible terrain; visual placement never changes gameplay collisions.</summary>
    public sealed class DefenseIndicatorSurface3D : MonoBehaviour
    {
        [SerializeField] private Vector3[] _triangles = new Vector3[0];
        [SerializeField] private LayerMask _groundMask = 4096;
        [SerializeField] private float _probeHeight = 20f;
        public Vector3 Place(Vector3 origin, float lift, float radius = 0f)
        {
            float height = Sample(origin);
            if (radius > 0f)
            {
                const int samples = 8;
                for (int i = 0; i < samples; i++)
                {
                    float angle = i * Mathf.PI * 2f / samples;
                    height = Mathf.Max(height, Sample(origin + new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius)));
                }
            }
            if (float.IsNegativeInfinity(height)) height = origin.y;
            return new Vector3(origin.x, height + lift, origin.z);
        }
        private float Sample(Vector3 point)
        {
            float height = float.NegativeInfinity;
            if (Physics.Raycast(point + Vector3.up * _probeHeight, Vector3.down, out var hit,
                _probeHeight * 2f, _groundMask, QueryTriggerInteraction.Ignore)) height = hit.point.y;
            var p = Point(point);
            for (int i = 0; i + 2 < _triangles.Length; i += 3)
            {
                if (DefenseBasicAttackRules.TryTriangleHeight(p, Point(_triangles[i]), Point(_triangles[i + 1]), Point(_triangles[i + 2]), out float y)
                    && y <= point.y + 2f) height = Mathf.Max(height, y);
            }
            // A missing surface must not silently place an indicator on the world origin plane.
            return height;
        }
        private static DefensePoint3 Point(Vector3 p) => new DefensePoint3(p.x, p.y, p.z);
    }
}
