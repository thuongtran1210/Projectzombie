using UnityEngine;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Chuẩn hóa định nghĩa & thuật toán di chuyển tức thời / cơ động cao trong game:
    /// 1. DASH (Lướt Thân Pháp):
    ///    - Quét liên tục trên đường đi.
    ///    - BỊ VẬT CẢN (Obstacle / Wall / Water) CHẶN LẠI ngay lập tức tại điểm tiếp xúc.
    /// 2. TELEPORT / BLINK (Dịch Chuyển Không Gian):
    ///    - Xuyên qua mọi vật cản ở giữa đường (Tường, ao nước, bầy quái).
    ///    - NHƯNG KHÔNG ĐƯỢC PHÉP ĐÁP NGOÀI MAP HOẶC ĐÁP BÊN TRONG VẬT CẢN (Clamp về sàn Walkable gần nhất).
    /// </summary>
    public static class MovementPhysicsUtility
    {
        private static int _obstacleMask = -1;

        public static int ObstacleMask
        {
            get
            {
                if (_obstacleMask == -1)
                {
                    int mask = LayerMask.GetMask("Obstacle", "Water");
                    _obstacleMask = mask != 0 ? mask : LayerMask.GetMask("Obstacle");
                }
                return _obstacleMask;
            }
        }

        private static readonly RaycastHit2D[] _hitBuffer = new RaycastHit2D[1];
        private static UnityEngine.Tilemaps.Tilemap _cachedGroundTilemap;
        private static bool _searchedTilemap = false;

        public static UnityEngine.Tilemaps.Tilemap GroundTilemap
        {
            get
            {
                if (_cachedGroundTilemap == null && !_searchedTilemap)
                {
                    var obj = GameObject.Find("Tilemap_Ground");
                    if (obj != null) _cachedGroundTilemap = obj.GetComponent<UnityEngine.Tilemaps.Tilemap>();

                    if (_cachedGroundTilemap == null)
                    {
                        var grid = Object.FindObjectOfType<Grid>();
                        if (grid != null)
                        {
                            var maps = grid.GetComponentsInChildren<UnityEngine.Tilemaps.Tilemap>();
                            foreach (var map in maps)
                            {
                                if (map.name.ToLower().Contains("ground") || map.name.ToLower().Contains("floor"))
                                {
                                    _cachedGroundTilemap = map;
                                    break;
                                }
                            }
                            if (_cachedGroundTilemap == null && maps.Length > 0) _cachedGroundTilemap = maps[0];
                        }
                    }

                    if (_cachedGroundTilemap == null)
                    {
                        _cachedGroundTilemap = Object.FindObjectOfType<UnityEngine.Tilemaps.Tilemap>();
                    }

                    _searchedTilemap = true;
                }
                return _cachedGroundTilemap;
            }
        }

        public static void ResetTilemapCache()
        {
            _cachedGroundTilemap = null;
            _searchedTilemap = false;
        }

        /// <summary>
        /// Kiểm tra xem toàn bộ diện tích cơ thể (4 góc/hướng với bán kính bodyRadius) có hoàn toàn nằm trên sàn gạch Tilemap hay không.
        /// Ngăn chặn hiện tượng tâm điểm nằm ở rìa gạch nhưng nửa thân người bị chìa ra ngoài mép biển/vực.
        /// </summary>
        public static bool IsPositionFullyOnGround(Vector3 pos, float bodyRadius = 0.35f)
        {
            var tilemap = GroundTilemap;
            if (tilemap == null) return true;

            // 1. Kiểm tra tâm điểm
            if (!tilemap.HasTile(tilemap.WorldToCell(pos))) return false;

            // 2. Kiểm tra 4 hướng đệm an toàn (Trái, Phải, Trên, Dưới)
            float padding = Mathf.Max(0.2f, bodyRadius);
            if (!tilemap.HasTile(tilemap.WorldToCell(new Vector3(pos.x - padding, pos.y, pos.z)))) return false;
            if (!tilemap.HasTile(tilemap.WorldToCell(new Vector3(pos.x + padding, pos.y, pos.z)))) return false;
            if (!tilemap.HasTile(tilemap.WorldToCell(new Vector3(pos.x, pos.y - padding, pos.z)))) return false;
            if (!tilemap.HasTile(tilemap.WorldToCell(new Vector3(pos.x, pos.y + padding, pos.z)))) return false;

            return true;
        }

        /// <summary>
        /// [GROUND CLAMP]: Đảm bảo tọa độ chỉ định luôn nằm hoàn toàn trong vùng sàn gạch hợp lệ (cách mép vực/tường tối thiểu bodyRadius) và không nằm trong vật cản.
        /// </summary>
        public static Vector3 ClampToWalkableGround(Vector3 targetPos, Vector3 fallbackOrigin, float maxClampStep = 6.0f, float bodyRadius = 0.45f)
        {
            int mask = ObstacleMask;

            // 1. Nếu vị trí đã hoàn toàn hợp lệ (không đè Obstacle và 4 hướng đều trên sàn gạch) thì giữ nguyên
            bool isInsideObstacle = mask != 0 && Physics2D.OverlapCircle(targetPos, bodyRadius, mask) != null;
            bool isFullyGrounded = IsPositionFullyOnGround(targetPos, bodyRadius);

            if (!isInsideObstacle && isFullyGrounded)
            {
                return targetPos;
            }

            // 2. Kéo dần về phía fallbackOrigin để tìm ô sàn an toàn hoàn toàn (Safe Margin Inset)
            Vector2 fromTargetToOrigin = ((Vector2)fallbackOrigin - (Vector2)targetPos).normalized;
            if (fromTargetToOrigin == Vector2.zero) fromTargetToOrigin = Vector2.down;

            for (float step = 0.2f; step <= maxClampStep; step += 0.2f)
            {
                Vector3 candidate = targetPos + (Vector3)(fromTargetToOrigin * step);
                bool candObstacle = mask != 0 && Physics2D.OverlapCircle(candidate, bodyRadius, mask) != null;
                bool candGrounded = IsPositionFullyOnGround(candidate, bodyRadius);

                if (!candObstacle && candGrounded)
                {
                    return candidate;
                }
            }

            return fallbackOrigin;
        }

        /// <summary>
        /// [DASH]: Tính toán điểm đến tối đa của cú lướt vật lý.
        /// Bị cản lại ngay khi va chạm vào bất kỳ vật cản nào trên đường đi HOẶC khi sắp chạm tới mép sàn Tilemap.
        /// Zero GC Alloc.
        /// </summary>
        public static Vector3 CalculateDashDestination(Vector3 startPos, Vector2 direction, float maxDistance, float bodyRadius = 0.35f)
        {
            if (direction == Vector2.zero || maxDistance <= 0.01f) return startPos;

            Vector2 dir = direction.normalized;
            int mask = ObstacleMask;

            // 1. Quét va chạm vật cản bằng CircleCastNonAlloc (Zero GC Alloc)
            float allowedDistance = maxDistance;
            if (mask != 0)
            {
                int hitCount = Physics2D.CircleCastNonAlloc(startPos, bodyRadius, dir, _hitBuffer, maxDistance, mask);
                if (hitCount > 0 && _hitBuffer[0].collider != null)
                {
                    allowedDistance = Mathf.Min(allowedDistance, Mathf.Max(0f, _hitBuffer[0].distance - 0.1f));
                }
            }

            // 2. Quét từng bước chân dọc theo đường lướt với kiểm tra đệm an toàn 4 hướng (Full Ground Margin)
            var tilemap = GroundTilemap;
            if (tilemap != null)
            {
                float stepSize = 0.2f;
                float currentDist = 0f;
                Vector3 lastValidFloorPos = startPos;

                while (currentDist < allowedDistance)
                {
                    currentDist += stepSize;
                    if (currentDist > allowedDistance) currentDist = allowedDistance;

                    Vector3 samplePoint = startPos + (Vector3)(dir * currentDist);

                    if (IsPositionFullyOnGround(samplePoint, bodyRadius))
                    {
                        lastValidFloorPos = samplePoint;
                    }
                    else
                    {
                        // Gặp mép vực/hết sàn gạch: Dừng lại ngay tại vị trí có đệm an toàn cuối cùng
                        return lastValidFloorPos;
                    }
                }
                return lastValidFloorPos;
            }

            return startPos + (Vector3)(dir * allowedDistance);
        }

        /// <summary>
        /// [TELEPORT / BLINK]: Tính toán điểm đến dịch chuyển không gian.
        /// Xuyên qua mọi vật cản ở giữa, nhưng nếu điểm đáp rơi vào trong vật cản hoặc ngoài map thì tự kéo về vị trí sàn an toàn gần nhất.
        /// </summary>
        public static Vector3 ValidateTeleportDestination(Vector3 targetPos, Vector3 originPos, float bodyRadius = 0.35f)
        {
            return ClampToWalkableGround(targetPos, originPos, 8.0f, bodyRadius);
        }
    }
}
