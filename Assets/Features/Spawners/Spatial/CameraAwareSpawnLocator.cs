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

        public CameraAwareSpawnLocator(ArenaBoundaryContext boundaryContext, Camera camera = null, float cameraPadding = 2.0f)
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

            float effectiveMin = minRadius > 0 ? minRadius : 12f;
            float effectiveMax = maxRadius > effectiveMin ? maxRadius : effectiveMin + 8f;

            // Thích ứng bán kính theo kích thước Camera orthographic
            if (cam != null && cam.orthographic)
            {
                float camHalfH = cam.orthographicSize + _cameraPadding;
                float camHalfW = (cam.orthographicSize * cam.aspect) + _cameraPadding;
                float camDiagonal = Mathf.Sqrt((camHalfW * camHalfW) + (camHalfH * camHalfH));

                // Bán kính spawn tối thiểu phải lớn hơn hoặc bằng đường chéo nửa màn hình để chắc chắn nằm ngoài tầm nhìn
                effectiveMin = Mathf.Max(camDiagonal, effectiveMin);
                effectiveMax = Mathf.Max(effectiveMin + 6f, maxRadius);
            }

            var registry = Player.PlayerProvider.Registry;
            var activePlayers = registry != null ? registry.ActivePlayers : null;
            Bounds safeBounds = _boundaryContext != null ? _boundaryContext.SafeMapBounds : new Bounds(Vector3.zero, new Vector3(100f, 100f, 10f));

            // Giai đoạn 1: Lấy mẫu Polar 360 độ ngoài tầm nhìn tất cả Players (24 lần thử)
            for (int i = 0; i < 24; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = Random.Range(effectiveMin, effectiveMax);
                Vector3 candidatePos = center + new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0f);

                // Điểm candidate PHẢI nằm trọn vẹn trong sàn đấu an toàn (Không tự tiện clamp ép giật lùi vào màn hình)
                if (!safeBounds.Contains(candidatePos))
                    continue;

                if (_boundaryContext != null && !_boundaryContext.IsInsideWalkableArea(candidatePos))
                    continue;

                if (obstacleMask != 0 && Physics2D.OverlapCircleNonAlloc(candidatePos, 0.5f, _spawnObstacleBuffer, obstacleMask) > 0)
                    continue;

                if (!IsFarFromAllPlayers(candidatePos, activePlayers, cam))
                    continue;

                return candidatePos;
            }

            // Giai đoạn 2: Hướng về tâm bản đồ / vùng mở (Center-Biased Inward Sampling) khi Player đứng sát góc/mép tường
            Vector3 mapCenter = safeBounds.center;
            Vector2 dirToCenter = ((Vector2)(mapCenter - center)).normalized;
            if (dirToCenter.sqrMagnitude < 0.01f) dirToCenter = Vector2.up;
            float baseAngle = Mathf.Atan2(dirToCenter.y, dirToCenter.x) * Mathf.Rad2Deg;

            for (int i = 0; i < 24; i++)
            {
                float offsetAngle = Random.Range(-75f, 75f);
                float angle = (baseAngle + offsetAngle) * Mathf.Deg2Rad;
                float distance = Random.Range(effectiveMin, Mathf.Min(effectiveMax, Vector3.Distance(center, mapCenter) + 20f));
                Vector3 candidatePos = center + new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0f);

                if (!safeBounds.Contains(candidatePos))
                    continue;

                if (_boundaryContext != null && !_boundaryContext.IsInsideWalkableArea(candidatePos))
                    continue;

                if (obstacleMask != 0 && Physics2D.OverlapCircleNonAlloc(candidatePos, 0.5f, _spawnObstacleBuffer, obstacleMask) > 0)
                    continue;

                if (!IsFarFromAllPlayers(candidatePos, activePlayers, cam))
                    continue;

                return candidatePos;
            }

            // Giai đoạn 3: Fallback an toàn tuyệt đối - Lựa chọn điểm biên map xa Player nhất & ngoài tầm nhìn
            Vector3[] cornerCandidates = new Vector3[]
            {
                new Vector3(safeBounds.min.x + 1f, safeBounds.min.y + 1f, 0f),
                new Vector3(safeBounds.max.x - 1f, safeBounds.min.y + 1f, 0f),
                new Vector3(safeBounds.min.x + 1f, safeBounds.max.y - 1f, 0f),
                new Vector3(safeBounds.max.x - 1f, safeBounds.max.y - 1f, 0f),
                new Vector3(safeBounds.center.x, safeBounds.min.y + 1f, 0f),
                new Vector3(safeBounds.center.x, safeBounds.max.y - 1f, 0f),
                new Vector3(safeBounds.min.x + 1f, safeBounds.center.y, 0f),
                new Vector3(safeBounds.max.x - 1f, safeBounds.center.y, 0f)
            };

            Vector3 bestPos = center + Vector3.up * effectiveMin;
            float maxPlayerDist = -1f;

            for (int i = 0; i < cornerCandidates.Length; i++)
            {
                Vector3 p = cornerCandidates[i];
                if (_boundaryContext != null && !_boundaryContext.IsInsideWalkableArea(p))
                    continue;

                if (obstacleMask != 0 && Physics2D.OverlapCircleNonAlloc(p, 0.5f, _spawnObstacleBuffer, obstacleMask) > 0)
                    continue;

                float dist = Vector3.Distance(p, center);
                bool isFar = IsFarFromAllPlayers(p, activePlayers, cam);

                if (isFar && dist > maxPlayerDist)
                {
                    maxPlayerDist = dist;
                    bestPos = p;
                }
            }

            if (maxPlayerDist > 0f)
            {
                return bestPos;
            }

            // Fallback cuối cùng: Hướng xa nhất từ Player
            Vector3 fallbackPos = center + (Vector3)(-dirToCenter * effectiveMin);
            if (_boundaryContext != null)
            {
                return _boundaryContext.ClampToSafeBounds(fallbackPos);
            }

            return fallbackPos;
        }

        private bool IsFarFromAllPlayers(Vector3 position, System.Collections.Generic.IReadOnlyList<Player.PlayerContext> players, Camera cam)
        {
            // 1. Kiểm tra Viewport của Camera Host
            if (cam != null && !IsOutsideCameraViewport(cam, position))
            {
                return false;
            }

            // 2. Kiểm tra cự ly với tất cả người chơi trong phòng (Né màn hình Client)
            if (players != null && players.Count > 0)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    var p = players[i];
                    if (p != null && p.Transform != null && p.IsAlive)
                    {
                        if (Vector2.Distance(position, p.Transform.position) < 11.0f)
                        {
                            return false; // Quá gần 1 người chơi -> Loại bỏ
                        }
                    }
                }
            }

            return true;
        }

        private bool IsOutsideCameraViewport(Camera cam, Vector3 position)
        {
            if (cam == null) return true;
            Vector3 vp = cam.WorldToViewportPoint(position);
            return vp.x < -0.05f || vp.x > 1.05f || vp.y < -0.05f || vp.y > 1.05f;
        }
    }
}
