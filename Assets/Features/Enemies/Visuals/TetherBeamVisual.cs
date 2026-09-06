using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Enemies.Special
{
    /// <summary>
    /// Component hiển thị hiệu ứng Dây Xích Oán Khí bằng LineRenderer 2D URP.
    /// Tự động cập nhật 2 đầu mút và kiểm tra va chạm cắt ngang với Player.
    /// Hoàn toàn độc lập (Autonomous / No Singleton).
    /// </summary>
    public class TetherBeamVisual : MonoBehaviour
    {
        [Header("Rendering Settings")]
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private float _lineWidth = 0.18f;
        [SerializeField] private Color _glowColor = new Color(1f, 0.2f, 0.15f, 0.95f); // Đỏ Chu Sa rực sáng
        [SerializeField] private Color _coreColor = new Color(1f, 0.85f, 0.4f, 1f);    // Lõi Vàng Hoàng Kim

        private Transform _startPoint;
        private Transform _endPoint;
        private bool _isActive = false;

        public bool IsActive => _isActive;
        public Transform EndPoint => _endPoint;

        private void Awake()
        {
            EnsureLineRenderer();
        }

        private void EnsureLineRenderer()
        {
            if (_lineRenderer == null)
            {
                _lineRenderer = GetComponent<LineRenderer>();
            }
            if (_lineRenderer == null)
            {
                _lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            _lineRenderer.positionCount = 2;
            _lineRenderer.startWidth = _lineWidth;
            _lineRenderer.endWidth = _lineWidth;
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.sortingLayerName = "VFX_World";
            _lineRenderer.sortingOrder = 100;

            // Thiết lập Gradient màu sắc Cổ Phong Đông Sơn
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(_glowColor, 0.0f), 
                    new GradientColorKey(_coreColor, 0.5f), 
                    new GradientColorKey(_glowColor, 1.0f) 
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(0.9f, 0.0f), 
                    new GradientAlphaKey(1.0f, 0.5f), 
                    new GradientAlphaKey(0.9f, 1.0f) 
                }
            );
            _lineRenderer.colorGradient = gradient;

            // Load Material Sprite-Default nếu chưa có
            if (_lineRenderer.sharedMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    _lineRenderer.material = new Material(shader);
                }
            }

            _lineRenderer.enabled = false;
        }

        /// <summary>
        /// Kích hoạt hiển thị dây xích nối giữa 2 điểm Transform.
        /// </summary>
        public void Connect(Transform start, Transform end)
        {
            _startPoint = start;
            _endPoint = end;
            _isActive = true;

            EnsureLineRenderer();
            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = true;
                UpdatePositions();
            }
        }

        /// <summary>
        /// Ngắt và ẩn dây xích.
        /// </summary>
        public void Disconnect()
        {
            _isActive = false;
            _startPoint = null;
            _endPoint = null;

            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = false;
            }
        }

        private void LateUpdate()
        {
            if (!_isActive) return;

            if (_startPoint == null || !_startPoint.gameObject.activeInHierarchy ||
                _endPoint == null || !_endPoint.gameObject.activeInHierarchy)
            {
                Disconnect();
                return;
            }

            UpdatePositions();
        }

        private void UpdatePositions()
        {
            if (_lineRenderer != null && _startPoint != null && _endPoint != null)
            {
                Vector3 p0 = _startPoint.position;
                Vector3 p1 = _endPoint.position;
                p0.z = 0f;
                p1.z = 0f;

                _lineRenderer.SetPosition(0, p0);
                _lineRenderer.SetPosition(1, p1);
            }
        }

        /// <summary>
        /// Kiểm tra xem một vị trí (ví dụ vị trí Player) có nằm cắt ngang sợi xích trong phạm vi bán kính radius không.
        /// Sử dụng công thức khoảng cách từ điểm đến đoạn thẳng (Zero-Alloc).
        /// </summary>
        public bool IsPointIntersecting(Vector2 point, float thresholdRadius)
        {
            if (!_isActive || _startPoint == null || _endPoint == null) return false;

            Vector2 a = _startPoint.position;
            Vector2 b = _endPoint.position;

            float dist = DistancePointToSegment(point, a, b);
            return dist <= thresholdRadius;
        }

        private float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float sqrLen = ab.sqrMagnitude;
            if (sqrLen < 0.0001f) return Vector2.Distance(p, a);

            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / sqrLen);
            Vector2 projection = a + t * ab;
            return Vector2.Distance(p, projection);
        }
    }
}
