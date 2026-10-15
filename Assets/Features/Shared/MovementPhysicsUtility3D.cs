using UnityEngine;
using UnityEngine.AI;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Tiện ích chuẩn hóa thuật toán di chuyển tức thời & va chạm trong không gian 3D (X-Z plane, Y là cao độ):
    /// 1. DASH 3D (Lướt Thân Pháp):
    ///    - Quét liên tục trên đường đi theo mặt phẳng X-Z bằng SphereCastNonAlloc.
    ///    - Bị vật cản (Layer Obstacle / Wall) chặn lại ngay lập tức tại điểm tiếp xúc.
    ///    - Tự động bám theo cao độ mặt sàn 3D.
    /// 2. TELEPORT / BLINK 3D (Dịch Chuyển Không Gian):
    ///    - Xuyên qua mọi vật cản ở giữa đường.
    ///    - Clamp về vị trí sàn Walkable an toàn gần nhất (sử dụng Raycast xuống sàn hoặc NavMesh.SamplePosition).
    /// Tuân thủ quy tắc 6.1 AGENTS.md: Zero GC Allocations trong Gameplay Loop.
    /// </summary>
    public static class MovementPhysicsUtility3D
    {
        private const int BUFFER_SIZE = 10;
        private static readonly RaycastHit[] _sphereCastBuffer = new RaycastHit[BUFFER_SIZE];
        private static readonly RaycastHit[] _groundRaycastBuffer = new RaycastHit[BUFFER_SIZE];
        private static readonly Collider[] _overlapBuffer = new Collider[BUFFER_SIZE];

        private static int _groundMask = -1;
        private static int _obstacleMask = -1;

        public static int GroundMask
        {
            get
            {
                if (_groundMask == -1)
                {
                    int mask = LayerMask.GetMask("Ground", "Default");
                    _groundMask = mask != 0 ? mask : LayerMask.GetMask("Default");
                }
                return _groundMask;
            }
        }

        public static int ObstacleMask
        {
            get
            {
                if (_obstacleMask == -1)
                {
                    int mask = LayerMask.GetMask("Obstacle", "Wall", "Water");
                    _obstacleMask = mask != 0 ? mask : LayerMask.GetMask("Obstacle");
                }
                return _obstacleMask;
            }
        }

        /// <summary>
        /// Bắn tia thẳng đứng từ trên xuống để tìm cao độ mặt sàn Y tại tọa độ chỉ định.
        /// </summary>
        /// <param name="pos">Tọa độ cần kiểm tra (X, Z)</param>
        /// <param name="groundY">Cao độ mặt sàn tìm được</param>
        /// <param name="maxRayDistance">Khoảng cách quét tối đa</param>
        /// <returns>True nếu tìm thấy mặt sàn hợp lệ bên dưới</returns>
        public static bool TryGetGroundHeight(Vector3 pos, out float groundY, float maxRayDistance = 10.0f)
        {
            groundY = pos.y;
            Vector3 rayOrigin = new Vector3(pos.x, pos.y + 2.0f, pos.z);
            int hitCount = Physics.RaycastNonAlloc(rayOrigin, Vector3.down, _groundRaycastBuffer, maxRayDistance, GroundMask);

            if (hitCount > 0)
            {
                float closestDist = float.MaxValue;
                int bestIdx = -1;

                for (int i = 0; i < hitCount; i++)
                {
                    if (_groundRaycastBuffer[i].collider != null && _groundRaycastBuffer[i].distance < closestDist)
                    {
                        closestDist = _groundRaycastBuffer[i].distance;
                        bestIdx = i;
                    }
                }

                if (bestIdx >= 0)
                {
                    groundY = _groundRaycastBuffer[bestIdx].point.y;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Kiểm tra xem vị trí chỉ định có nằm trên bề mặt sàn Walkable và không bị kẹt trong tường 3D không.
        /// </summary>
        public static bool IsPositionWalkable(Vector3 pos, float bodyRadius = 0.45f)
        {
            // 1. Kiểm tra va chạm với vật cản / tường
            int obstacleCount = Physics.OverlapSphereNonAlloc(pos + Vector3.up * 0.5f, bodyRadius, _overlapBuffer, ObstacleMask);
            if (obstacleCount > 0) return false;

            // 2. Kiểm tra có sàn bên dưới không
            if (!TryGetGroundHeight(pos, out _)) return false;

            // 3. Nếu có NavMesh, kiểm tra vị trí hợp lệ trên NavMesh
            if (NavMesh.SamplePosition(pos, out NavMeshHit navHit, bodyRadius * 1.5f, NavMesh.AllAreas))
            {
                return Vector3.Distance(pos, navHit.position) <= bodyRadius;
            }

            return true;
        }

        /// <summary>
        /// [DASH 3D]: Tính toán điểm đến tối đa của cú lướt trong không gian 3D (X-Z).
        /// Bị cản lại ngay khi va chạm vào tường/vật cản hoặc sắp rời khỏi mép sàn.
        /// Zero GC Allocations.
        /// </summary>
        /// <param name="startPos">Vị trí bắt đầu lướt</param>
        /// <param name="horizontalDir">Hướng lướt trên mặt phẳng X-Z (đã chuẩn hóa)</param>
        /// <param name="maxDistance">Cự ly lướt tối đa</param>
        /// <param name="bodyRadius">Bán kính cơ thể nhân vật</param>
        public static Vector3 CalculateDashDestination(Vector3 startPos, Vector3 horizontalDir, float maxDistance, float bodyRadius = 0.35f)
        {
            horizontalDir.y = 0f;
            if (horizontalDir.sqrMagnitude < 0.001f || maxDistance <= 0.01f) return startPos;

            Vector3 dir = horizontalDir.normalized;
            float allowedDistance = maxDistance;

            // 1. Quét va chạm chướng ngại vật bằng SphereCastNonAlloc
            Vector3 castOrigin = startPos + Vector3.up * (bodyRadius + 0.1f);
            int hitCount = Physics.SphereCastNonAlloc(castOrigin, bodyRadius, dir, _sphereCastBuffer, maxDistance, ObstacleMask);

            if (hitCount > 0)
            {
                float closestHitDist = float.MaxValue;
                for (int i = 0; i < hitCount; i++)
                {
                    if (_sphereCastBuffer[i].collider != null && _sphereCastBuffer[i].distance < closestHitDist)
                    {
                        closestHitDist = _sphereCastBuffer[i].distance;
                    }
                }

                if (closestHitDist < float.MaxValue)
                {
                    allowedDistance = Mathf.Min(allowedDistance, Mathf.Max(0f, closestHitDist - 0.1f));
                }
            }

            // 2. Quét bước chân bám sàn và kiểm tra mép vực
            float stepSize = 0.3f;
            float currentDist = 0f;
            Vector3 lastValidPos = startPos;

            while (currentDist < allowedDistance)
            {
                currentDist += stepSize;
                if (currentDist > allowedDistance) currentDist = allowedDistance;

                Vector3 candidate = startPos + dir * currentDist;

                if (TryGetGroundHeight(candidate, out float floorY))
                {
                    candidate.y = floorY;
                    lastValidPos = candidate;
                }
                else
                {
                    // Gặp mép vực không có sàn: Dừng lại ngay tại vị trí có sàn hợp lệ cuối cùng
                    return lastValidPos;
                }
            }

            return lastValidPos;
        }

        /// <summary>
        /// [GROUND CLAMP 3D]: Kéo một tọa độ chỉ định về vị trí sàn hợp lệ gần nhất.
        /// </summary>
        public static Vector3 ClampToWalkableGround(Vector3 targetPos, Vector3 fallbackOrigin, float maxClampStep = 8.0f, float bodyRadius = 0.45f)
        {
            if (IsPositionWalkable(targetPos, bodyRadius))
            {
                if (TryGetGroundHeight(targetPos, out float groundY))
                {
                    targetPos.y = groundY;
                }
                return targetPos;
            }

            // Dùng NavMesh SamplePosition để tìm vị trí gần nhất nếu có
            if (NavMesh.SamplePosition(targetPos, out NavMeshHit navHit, maxClampStep, NavMesh.AllAreas))
            {
                return navHit.position;
            }

            // Kéo dần về phía fallbackOrigin theo vector ngang
            Vector3 fromTargetToOrigin = fallbackOrigin - targetPos;
            fromTargetToOrigin.y = 0f;
            Vector3 dir = fromTargetToOrigin.normalized;

            for (float step = 0.3f; step <= maxClampStep; step += 0.3f)
            {
                Vector3 candidate = targetPos + dir * step;
                if (IsPositionWalkable(candidate, bodyRadius))
                {
                    if (TryGetGroundHeight(candidate, out float floorY))
                    {
                        candidate.y = floorY;
                    }
                    return candidate;
                }
            }

            return fallbackOrigin;
        }

        /// <summary>
        /// [TELEPORT 3D]: Tính toán điểm đến dịch chuyển không gian 3D.
        /// </summary>
        public static Vector3 ValidateTeleportDestination(Vector3 targetPos, Vector3 originPos, float bodyRadius = 0.35f)
        {
            return ClampToWalkableGround(targetPos, originPos, 8.0f, bodyRadius);
        }
    }
}
