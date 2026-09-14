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
                effectiveMax = Mathf.Max(effectiveMin + 8f, maxRadius);
            }

            var registry = Player.PlayerProvider.Registry;
            var activePlayers = registry != null ? registry.ActivePlayers : null;
            Bounds safeBounds = _boundaryContext != null ? _boundaryContext.SafeMapBounds : new Bounds(Vector3.zero, new Vector3(100f, 100f, 10f));

            // GIAI ĐOẠN 1: Lấy mẫu Polar 360 độ quanh Player (32 lần thử)
            for (int i = 0; i < 32; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = Random.Range(effectiveMin, effectiveMax);
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

            // GIAI ĐOẠN 2: Hướng vào vùng mở của bản đồ (Inward Cone Sampling khi Player đứng gần góc/tường)
            Vector3 mapCenter = safeBounds.center;
            Vector2 dirToCenter = ((Vector2)(mapCenter - center)).normalized;
            if (dirToCenter.sqrMagnitude < 0.01f) dirToCenter = Vector2.up;
            float baseAngle = Mathf.Atan2(dirToCenter.y, dirToCenter.x) * Mathf.Rad2Deg;

            float distToCenter = Vector3.Distance(center, mapCenter);
            float coneMaxDist = Mathf.Max(effectiveMax, distToCenter + 15f);

            for (int i = 0; i < 32; i++)
            {
                float offsetAngle = Random.Range(-80f, 80f);
                float angle = (baseAngle + offsetAngle) * Mathf.Deg2Rad;
                float distance = Random.Range(effectiveMin, coneMaxDist);
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

            // GIAI ĐOẠN 3: Lấy mẫu ngẫu nhiên trên toàn bộ sàn đấu (Global Arena Uniform Sampling)
            // Tìm bất kỳ điểm hợp lệ nào trên bản đồ nằm ngoài màn hình Camera và xa Player
            for (int i = 0; i < 32; i++)
            {
                float rx = Random.Range(safeBounds.min.x + 1.5f, safeBounds.max.x - 1.5f);
                float ry = Random.Range(safeBounds.min.y + 1.5f, safeBounds.max.y - 1.5f);
                Vector3 candidatePos = new Vector3(rx, ry, 0f);

                if (_boundaryContext != null && !_boundaryContext.IsInsideWalkableArea(candidatePos))
                    continue;

                if (obstacleMask != 0 && Physics2D.OverlapCircleNonAlloc(candidatePos, 0.5f, _spawnObstacleBuffer, obstacleMask) > 0)
                    continue;

                if (!IsFarFromAllPlayers(candidatePos, activePlayers, cam))
                    continue;

                return candidatePos;
            }

            // GIAI ĐOẠN 4: Fallback - Chọn góc xa nhất của sàn đấu so với Player
            Vector3[] corners = new Vector3[]
            {
                new Vector3(safeBounds.min.x + 2f, safeBounds.min.y + 2f, 0f),
                new Vector3(safeBounds.max.x - 2f, safeBounds.min.y + 2f, 0f),
                new Vector3(safeBounds.min.x + 2f, safeBounds.max.y - 2f, 0f),
                new Vector3(safeBounds.max.x - 2f, safeBounds.max.y - 2f, 0f),
                new Vector3(safeBounds.center.x, safeBounds.max.y - 2f, 0f),
                new Vector3(safeBounds.center.x, safeBounds.min.y + 2f, 0f),
                new Vector3(safeBounds.min.x + 2f, safeBounds.center.y, 0f),
                new Vector3(safeBounds.max.x - 2f, safeBounds.center.y, 0f)
            };

            Vector3 bestCorner = center + Vector3.up * effectiveMin;
            float maxDist = -1f;

            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 p = corners[i];
                if (_boundaryContext != null && !_boundaryContext.IsInsideWalkableArea(p)) continue;
                if (obstacleMask != 0 && Physics2D.OverlapCircleNonAlloc(p, 0.5f, _spawnObstacleBuffer, obstacleMask) > 0) continue;

                float d = Vector3.Distance(p, center);
                if (d > maxDist)
                {
                    maxDist = d;
                    bestCorner = p;
                }
            }

            return bestCorner;
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
                        if (Vector2.Distance(position, p.Transform.position) < 11.5f)
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
            // Mở rộng viewport threshold ra -0.08f .. 1.08f để quái sinh hoàn toàn ngoài mép màn hình
            return vp.x < -0.08f || vp.x > 1.08f || vp.y < -0.08f || vp.y > 1.08f;
        }
    }
}
