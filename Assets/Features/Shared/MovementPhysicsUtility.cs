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

        private static UnityEngine.Tilemaps.Tilemap _cachedGroundTilemap;

        public static UnityEngine.Tilemaps.Tilemap GroundTilemap
        {
            get
            {
                if (_cachedGroundTilemap == null)
                {
                    var obj = GameObject.Find("Tilemap_Ground");
                    if (obj != null) _cachedGroundTilemap = obj.GetComponent<UnityEngine.Tilemaps.Tilemap>();
                    if (_cachedGroundTilemap == null) _cachedGroundTilemap = Object.FindObjectOfType<UnityEngine.Tilemaps.Tilemap>();
                }
                return _cachedGroundTilemap;
            }
        }

        /// <summary>
        /// [DASH]: Tính toán điểm đến tối đa của cú lướt vật lý.
        /// Bị cản lại ngay khi va chạm vào bất kỳ vật cản nào trên đường đi HOẶC khi sắp vượt ra khỏi sàn Tilemap_Ground.
        /// </summary>
        public static Vector3 CalculateDashDestination(Vector3 startPos, Vector2 direction, float maxDistance, float bodyRadius = 0.35f)
        {
            if (direction == Vector2.zero || maxDistance <= 0.01f) return startPos;

            Vector2 dir = direction.normalized;
            int mask = ObstacleMask;

            // 1. Quét va chạm vật cản bằng CircleCast
            float allowedDistance = maxDistance;
            if (mask != 0)
            {
                RaycastHit2D hit = Physics2D.CircleCast(startPos, bodyRadius, dir, maxDistance, mask);
                if (hit.collider != null)
                {
                    allowedDistance = Mathf.Min(allowedDistance, Mathf.Max(0f, hit.distance - 0.1f));
                }
            }

            // 2. Quét từng bước chân dọc theo đường lướt để đảm bảo không bước ra ngoài sàn gạch Tilemap_Ground
            var tilemap = GroundTilemap;
            if (tilemap != null)
            {
                float stepSize = 0.3f;
                float currentDist = 0f;
                Vector3 lastValidFloorPos = startPos;

                while (currentDist < allowedDistance)
                {
                    currentDist += stepSize;
                    if (currentDist > allowedDistance) currentDist = allowedDistance;

                    Vector3 samplePoint = startPos + (Vector3)(dir * currentDist);
                    Vector3Int cellPos = tilemap.WorldToCell(samplePoint);

                    if (tilemap.HasTile(cellPos))
                    {
                        lastValidFloorPos = samplePoint;
                    }
                    else
                    {
                        // Gặp mép vực/hết sàn gạch: Dừng lại ngay tại ô gạch cuối cùng
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
        /// <param name="targetPos">Tọa độ mục tiêu muốn dịch chuyển tới</param>
        /// <param name="originPos">Vị trí ban đầu của người chơi để fallback nếu kẹt</param>
        /// <param name="bodyRadius">Bán kính cơ thể</param>
        /// <returns>Tọa độ đích hợp lệ 100% nằm trong map</returns>
        public static Vector3 ValidateTeleportDestination(Vector3 targetPos, Vector3 originPos, float bodyRadius = 0.35f)
        {
            int mask = ObstacleMask;
            if (mask == 0) return targetPos;

            // 1. Kiểm tra xem điểm đích có đang nằm đè bên trong vật thể cản hay không
            Collider2D insideHit = Physics2D.OverlapCircle(targetPos, bodyRadius, mask);
            if (insideHit == null)
            {
                return targetPos; // Điểm đáp hoàn toàn thông thoáng
            }

            // 2. Nếu điểm đáp bị kẹt trong tường: Tìm điểm thông thoáng gần nhất ngược về hướng xuất phát
            Vector2 fromTargetToOrigin = ((Vector2)originPos - (Vector2)targetPos).normalized;
            for (float step = 0.5f; step <= 8.0f; step += 0.5f)
            {
                Vector3 candidate = targetPos + (Vector3)(fromTargetToOrigin * step);
                if (Physics2D.OverlapCircle(candidate, bodyRadius, mask) == null)
                {
                    return candidate;
                }
            }

            return originPos; // Fallback an toàn tuyệt đối
        }
    }
}
