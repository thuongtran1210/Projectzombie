using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProjectZombie.Features.Spawners.Spatial
{
    /// <summary>
    /// Quản lý ngữ cảnh không gian sàn đấu, kiểm tra ô đi được (Walkable)
    /// và tính toán khung an toàn (Safe Map Bounds) để Clamping O(1).
    /// Hỗ trợ nạp động bản đồ qua Addressables một cách độc lập.
    /// </summary>
    public class ArenaBoundaryContext
    {
        private Collider2D _walkableAreaCollider;
        private Tilemap _groundTilemap;
        private Tilemap _obstacleTilemap;
        private Bounds _safeMapBounds;
        private bool _hasCalculatedBounds;
        private readonly bool _autoFindGroundTilemap;
        private readonly float _safeMargin;

        public Bounds SafeMapBounds
        {
            get
            {
                if (!_hasCalculatedBounds) CalculateSafeMapBounds();
                return _safeMapBounds;
            }
        }

        public ArenaBoundaryContext(
            Collider2D walkableCollider = null,
            Tilemap ground = null,
            Tilemap obstacle = null,
            bool autoFindGround = true,
            float safeMargin = 1.0f)
        {
            _walkableAreaCollider = walkableCollider;
            _groundTilemap = ground;
            _obstacleTilemap = obstacle;
            _autoFindGroundTilemap = autoFindGround;
            _safeMargin = safeMargin;
            _hasCalculatedBounds = false;
        }

        /// <summary>
        /// Tìm kiếm tự động các Tilemap hoặc Collider trong Scene nếu chưa được gán thủ công.
        /// </summary>
        public void EnsureDependencies()
        {
#if UNITY_EDITOR
            if (_autoFindGroundTilemap && _groundTilemap == null && _walkableAreaCollider == null)
            {
                var groundObj = GameObject.Find("Tilemap_Ground");
                if (groundObj != null)
                {
                    _groundTilemap = groundObj.GetComponent<Tilemap>();
                }
                else
                {
                    _groundTilemap = Object.FindObjectOfType<Tilemap>();
                }
            }

            if (_obstacleTilemap == null)
            {
                var obsObj = GameObject.Find("Tilemap_Obstacles");
                if (obsObj != null)
                {
                    _obstacleTilemap = obsObj.GetComponent<Tilemap>();
                }
            }
#endif

            CalculateSafeMapBounds();
        }

        /// <summary>
        /// Gán trực tiếp tham chiếu Tilemap từ Map Instance vừa sinh ra, triệt tiêu hoàn toàn Find trong runtime.
        /// </summary>
        public void AssignExplicitTilemaps(Tilemap ground, Tilemap obstacle, Collider2D walkable = null)
        {
            _groundTilemap = ground;
            _obstacleTilemap = obstacle;
            _walkableAreaCollider = walkable;
            _hasCalculatedBounds = false;
            ProjectZombie.Features.Shared.MovementPhysicsUtility.SetTilemaps(ground, obstacle);
            CalculateSafeMapBounds();
        }

        /// <summary>
        /// Làm mới tham chiếu Tilemap & ranh giới sàn đấu khi nạp một Map mới từ Addressables.
        /// </summary>
        public void RefreshMapReferences()
        {
            _groundTilemap = null;
            _obstacleTilemap = null;
            _walkableAreaCollider = null;
            _hasCalculatedBounds = false;
            ProjectZombie.Features.Shared.MovementPhysicsUtility.ResetTilemapCache();
            EnsureDependencies();
        }

        /// <summary>
        /// Tính toán khung hình chữ nhật an toàn (Safe Map Bounds) để hỗ trợ Clamping O(1).
        /// </summary>
        public void CalculateSafeMapBounds()
        {
            if (_hasCalculatedBounds) return;

            if (_walkableAreaCollider != null)
            {
                _safeMapBounds = _walkableAreaCollider.bounds;
                _safeMapBounds.Expand(-_safeMargin * 2f);
                _hasCalculatedBounds = true;
                return;
            }

            if (_groundTilemap != null)
            {
                _groundTilemap.CompressBounds();
                Bounds localB = _groundTilemap.localBounds;
                Vector3 worldMin = _groundTilemap.transform.TransformPoint(localB.min);
                Vector3 worldMax = _groundTilemap.transform.TransformPoint(localB.max);

                Vector3 center = (worldMin + worldMax) * 0.5f;
                Vector3 size = new Vector3(
                    Mathf.Max(2f, (worldMax.x - worldMin.x) - _safeMargin * 2f),
                    Mathf.Max(2f, (worldMax.y - worldMin.y) - _safeMargin * 2f),
                    10f);

                _safeMapBounds = new Bounds(center, size);
                _hasCalculatedBounds = true;
                return;
            }

            // Fallback nếu chưa nạp Tilemap/Collider: Khung 100x100 rộng rãi
            _safeMapBounds = new Bounds(Vector3.zero, new Vector3(100f, 100f, 10f));
            _hasCalculatedBounds = true;
        }

        /// <summary>
        /// Giới hạn một tọa độ bất kỳ nằm trọn vẹn bên trong Safe Map Bounds.
        /// </summary>
        public Vector3 ClampToSafeBounds(Vector3 targetPos)
        {
            if (!_hasCalculatedBounds) CalculateSafeMapBounds();

            float clampedX = Mathf.Clamp(targetPos.x, _safeMapBounds.min.x, _safeMapBounds.max.x);
            float clampedY = Mathf.Clamp(targetPos.y, _safeMapBounds.min.y, _safeMapBounds.max.y);
            return new Vector3(clampedX, clampedY, 0f);
        }

        /// <summary>
        /// Kiểm tra một điểm tọa độ có nằm trên sàn đi được và không bị cản bởi tường/vật cản.
        /// </summary>
        public bool IsInsideWalkableArea(Vector3 position)
        {
            // 1. Phải nằm trong Safe Bounds của sàn đấu
            if (_hasCalculatedBounds && !_safeMapBounds.Contains(position))
            {
                return false;
            }

            // 2. Nếu có Walkable Collider thì điểm phải nằm bên trong collider
            if (_walkableAreaCollider != null)
            {
                if (!_walkableAreaCollider.gameObject.name.Contains("Obstacle") && !_walkableAreaCollider.OverlapPoint(position))
                {
                    return false;
                }
            }

            // 3. Phải có sàn gạch trên Ground Tilemap
            if (_groundTilemap != null)
            {
                Vector3Int cellPos = _groundTilemap.WorldToCell(position);
                if (!_groundTilemap.HasTile(cellPos)) return false;
            }

            // 4. Không được trùng với ô tường trên Obstacle Tilemap
            if (_obstacleTilemap != null)
            {
                Vector3Int obsCell = _obstacleTilemap.WorldToCell(position);
                if (_obstacleTilemap.HasTile(obsCell)) return false;
            }

            return true;
        }
    }
}
