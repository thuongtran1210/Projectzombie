using UnityEngine;
using ProjectZombie.Features.Spawners.Core;

namespace ProjectZombie.Features.Spawners.Spatial
{
    /// <summary>
    /// Thuật toán tìm kiếm vị trí spawn ngoài tầm nhìn Camera,
    /// kết hợp kiểm tra sàn hợp lệ và cản trở chướng ngại vật (0 GC Alloc).
    /// </summary>
    public class CameraAwareSpawnLocator : ISpawnPositionLocator
    {
        private readonly ArenaBoundaryContext _boundaryContext;
        private readonly Camera _camera;
        private readonly float _cameraPadding;

        // Bộ đệm tĩnh cấp phát sẵn để triệt tiêu GC Alloc trong Physics2D NonAlloc
        private static readonly Collider2D[] _spawnObstacleBuffer = new Collider2D[1];

        public CameraAwareSpawnLocator(ArenaBoundaryContext boundaryContext, Camera camera = null, float cameraPadding = 1.5f)
        {
            _boundaryContext = boundaryContext;
            _camera = camera != null ? camera : Camera.main;
            _cameraPadding = cameraPadding;
        }

        public Vector3 GetSpawnPosition(Transform centerTarget, float minRadius, float maxRadius)
        {
            Vector3 center = centerTarget != null ? centerTarget.position : Vector3.zero;
            Camera cam = (_camera != null && _camera.isActiveAndEnabled) ? _camera : Camera.main;

            int obstacleMask = LayerMask.GetMask("Obstacle", "Water");
            if (obstacleMask == 0) obstacleMask = LayerMask.GetMask("Obstacle");

            float effectiveMin = minRadius > 0 ? minRadius : 10f;
            float effectiveMax = maxRadius > effectiveMin ? maxRadius : effectiveMin + 6f;

            // Thích ứng bán kính theo kích thước Camera orthographic
            if (cam != null && cam.orthographic)
            {
                float camHalfH = cam.orthographicSize + _cameraPadding;
                float camHalfW = (cam.orthographicSize * cam.aspect) + _cameraPadding;
                float camDiagonal = Mathf.Sqrt((camHalfW * camHalfW) + (camHalfH * camHalfH));

                // Bán kính spawn tối thiểu phải lớn hơn hoặc bằng đường chéo nửa màn hình để nằm ngoài tầm nhìn
                effectiveMin = Mathf.Max(camDiagonal, effectiveMin);
                effectiveMax = Mathf.Max(effectiveMin + 4f, maxRadius);
            }

            // Giai đoạn 1: Lấy mẫu Polar ngoài Camera (16 lần thử)
            for (int i = 0; i < 16; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = Random.Range(effectiveMin, effectiveMax);
                Vector3 candidatePos = center + new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0f);

                if (_boundaryContext != null && !_boundaryContext.IsInsideWalkableArea(candidatePos))
                    continue;

                if (obstacleMask != 0)
                {
                    int hits = Physics2D.OverlapCircleNonAlloc(candidatePos, 0.5f, _spawnObstacleBuffer, obstacleMask);
                    if (hits > 0) continue;
                }

                if (!IsOutsideCameraViewport(cam, candidatePos))
                    continue;

                return candidatePos;
            }

            // Giai đoạn 2: Smart Math Clamping Fallback (Khi Player đứng sát góc chết mép tường)
            for (int i = 0; i < 16; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = Random.Range(effectiveMin, effectiveMax);
                Vector3 rawPos = center + new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0f);

                Vector3 clampedPos = _boundaryContext != null ? _boundaryContext.ClampToSafeBounds(rawPos) : rawPos;

                // Đảm bảo sau khi Clamp không bị ép ngược lại sát sạt Player hoặc trong viewport camera
                if (Vector3.Distance(clampedPos, center) < 8.5f || !IsOutsideCameraViewport(cam, clampedPos))
                    continue;

                if (_boundaryContext != null && _boundaryContext.IsInsideWalkableArea(clampedPos))
                {
                    if (obstacleMask != 0 && Physics2D.OverlapCircleNonAlloc(clampedPos, 0.5f, _spawnObstacleBuffer, obstacleMask) > 0)
                        continue;

                    return clampedPos;
                }
            }

            // Giai đoạn 3: Fallback an toàn tuyệt đối - Giữ khoảng cách an toàn tối thiểu 10m với Player
            float fallbackDist = Mathf.Max(effectiveMin, 11f);
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            if (randomDir.sqrMagnitude < 0.01f) randomDir = Vector2.up;
            Vector3 fallbackPos = center + (Vector3)(randomDir * fallbackDist);

            if (_boundaryContext != null)
            {
                Vector3 clamped = _boundaryContext.ClampToSafeBounds(fallbackPos);
                if (Vector3.Distance(clamped, center) < 8.5f)
                {
                    fallbackPos = center - (Vector3)(randomDir * fallbackDist);
                    clamped = _boundaryContext.ClampToSafeBounds(fallbackPos);
                }
                return clamped;
            }

            return fallbackPos;
        }

        private bool IsOutsideCameraViewport(Camera cam, Vector3 position)
        {
            if (cam == null) return true;
            Vector3 vp = cam.WorldToViewportPoint(position);
            return vp.x < -0.05f || vp.x > 1.05f || vp.y < -0.05f || vp.y > 1.05f;
        }
    }
}
