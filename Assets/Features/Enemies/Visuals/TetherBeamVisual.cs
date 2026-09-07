using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Enemies.Special
{
    /// <summary>
    /// Component hiển thị hiệu ứng Dây Xích Oán Khí chuẩn Cổ Phong Đông Sơn / Anime URP.
    /// TÍNH NĂNG NÂNG CAO:
    /// - Multi-segment Dynamic Sine Wave & Noise (Dây uốn lượn giật điện sống động).
    /// - Breathing Width & Pulse (Nhấp nháy phát sáng theo nhịp thở oán khí).
    /// - Cảnh báo màu sắc khi kéo căng sắp đứt (> 8.0m).
    /// - Tối ưu 100% Zero-GC Alloc trên Mobile.
    /// </summary>
    public class TetherBeamVisual : MonoBehaviour
    {
        [Header("Rendering Settings")]
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private float _baseLineWidth = 0.09f; // Giảm xuống 0.09f để sợi xích thanh mảnh, sắc nét
        [SerializeField] private Color _glowColor = new Color(1.5f, 0.25f, 0.15f, 0.95f); // Đỏ Chu Sa HDR rực sáng
        [SerializeField] private Color _coreColor = new Color(2.0f, 1.2f, 0.5f, 1.0f);     // Lõi Vàng Hoàng Kim HDR
        [SerializeField] private Color _dangerColor = new Color(2.0f, 0.1f, 0.1f, 1.0f);   // Đỏ rực cảnh báo căng dây

        [Header("Wave & Dynamic Motion Settings")]
        [Range(4, 20)]
        [SerializeField] private int _segmentCount = 14;
        [SerializeField] private float _waveFrequency = 3.5f;
        [SerializeField] private float _waveSpeed = 12.0f;
        [SerializeField] private float _waveAmplitude = 0.08f;
        [SerializeField] private float _jitterSpeed = 24.0f;
        [SerializeField] private float _pulseSpeed = 6.0f;

        private Transform _startPoint;
        private Transform _endPoint;
        private bool _isActive = false;

        // Bộ đệm tọa độ cố định - Tránh hoàn toàn Garbage Collection (0 KB GC Alloc)
        private Vector3[] _positions;
        private Gradient _normalGradient;
        private Gradient _dangerGradient;

        public bool IsActive => _isActive;
        public Transform EndPoint => _endPoint;

        private void Awake()
        {
            InitializeBuffers();
            EnsureLineRenderer();
        }

        private void InitializeBuffers()
        {
            if (_segmentCount < 4) _segmentCount = 4;
            _positions = new Vector3[_segmentCount];

            // 1. Gradient trạng thái bình thường (Chu Sa - Hoàng Kim)
            _normalGradient = new Gradient();
            _normalGradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(_glowColor, 0.0f), 
                    new GradientColorKey(_coreColor, 0.5f), 
                    new GradientColorKey(_glowColor, 1.0f) 
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(0.85f, 0.0f), 
                    new GradientAlphaKey(1.0f, 0.5f), 
                    new GradientAlphaKey(0.85f, 1.0f) 
                }
            );

            // 2. Gradient trạng thái kéo căng sắp đứt (Cảnh báo Đỏ Rực)
            _dangerGradient = new Gradient();
            _dangerGradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(_dangerColor, 0.0f), 
                    new GradientColorKey(Color.white, 0.5f), 
                    new GradientColorKey(_dangerColor, 1.0f) 
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(0.95f, 0.0f), 
                    new GradientAlphaKey(1.0f, 0.5f), 
                    new GradientAlphaKey(0.95f, 1.0f) 
                }
            );
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

            _lineRenderer.positionCount = _segmentCount;
            _lineRenderer.startWidth = _baseLineWidth;
            _lineRenderer.endWidth = _baseLineWidth;
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.sortingLayerName = "Skill";
            _lineRenderer.sortingOrder = 500;
            _lineRenderer.colorGradient = _normalGradient;

            // Load Material Sprite-Default hoặc Universal 2D Unlit nếu chưa có
            if (_lineRenderer.sharedMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") 
                             ?? Shader.Find("Sprites/Default") 
                             ?? Shader.Find("Unlit/Color");
                
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
            if (_lineRenderer == null || _startPoint == null || _endPoint == null) return;

            Vector3 p0 = _startPoint.position;
            Vector3 p1 = _endPoint.position;
            p0.z = 0f;
            p1.z = 0f;

            Vector3 delta = p1 - p0;
            float currentDist = delta.magnitude;
            if (currentDist < 0.001f) return;

            // Vector pháp tuyến 2D (vuông góc với hướng nối giữa 2 quái)
            Vector3 normal = new Vector3(-delta.y, delta.x, 0f) / currentDist;

            float time = Time.time;
            float wavePhase = time * _waveSpeed;
            float jitter = Mathf.Sin(time * _jitterSpeed) * 0.03f;

            // 1. Tính toán vị trí từng phân đoạn (Wave Motion)
            int n = _segmentCount;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / (n - 1);
                Vector3 basePos = Vector3.Lerp(p0, p1, t);

                // Dập tắt dao động ở 2 đầu mút (gắn chặt vào cơ thể quái)
                float envelope = Mathf.Sin(t * Mathf.PI);

                // Sóng uốn lượn kết hợp sóng chính và rung giật
                float wave = Mathf.Sin(t * _waveFrequency * Mathf.PI * 2f + wavePhase) * (_waveAmplitude + jitter);

                _positions[i] = basePos + normal * (wave * envelope);
            }

            _lineRenderer.SetPositions(_positions);

            // 2. Breathing Pulse (Nhấp nháy bề dày tạo cảm giác luồng khí thở sống động)
            float pulse = Mathf.Sin(time * _pulseSpeed) * 0.015f;
            float targetWidth = Mathf.Max(0.04f, _baseLineWidth + pulse);
            _lineRenderer.startWidth = targetWidth;
            _lineRenderer.endWidth = targetWidth;

            // 3. Đổi màu nhấp nháy cảnh báo khi dây bị căng ra xa (> 8.0m)
            if (currentDist > 8.0f)
            {
                bool isBlink = Mathf.Sin(time * 16f) > 0f;
                _lineRenderer.colorGradient = isBlink ? _dangerGradient : _normalGradient;
            }
            else
            {
                _lineRenderer.colorGradient = _normalGradient;
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
