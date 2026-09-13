using UnityEngine;

namespace ProjectZombie.Features.UI.Common
{
    /// <summary>
    /// Tự động co giãn RectTransform theo vùng an toàn (Screen.safeArea) của thiết bị di động (Android / iOS).
    /// Áp dụng cơ chế Normalized Anchors (0.0 -> 1.0) độc lập với CanvasScaler và độ phân giải màn hình.
    /// Tối ưu 0 GC Allocation trong Gameplay Loop.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class SafeAreaFitter : MonoBehaviour
    {
        [Header("Áp Dụng Cạnh (Safe Margins)")]
        [SerializeField] private bool _applyLeft = true;
        [SerializeField] private bool _applyRight = true;
        [SerializeField] private bool _applyTop = true;
        [SerializeField] private bool _applyBottom = true;

#if UNITY_EDITOR
        public enum EditorSimType
        {
            None,
            TopNotch_And_BottomBar,    // Notch trên đỉnh (iPhone / Android dọc)
            Landscape_LeftPunchHole,   // Nốt ruồi camera bên trái (Android Landscape)
            Landscape_RightPunchHole,  // Nốt ruồi camera bên phải (Android Landscape)
            Landscape_BothSides        // Cả 2 bên (Dynamic Island hoặc viền bo cong dày)
        }

        [Header("Giả Lập Kiểm Thử Trong Editor")]
        [SerializeField] private EditorSimType _editorSimulation = EditorSimType.None;
#endif

        private RectTransform _rectTransform;
        private Rect _lastSafeArea = Rect.zero;
        private Vector2Int _lastScreenSize = Vector2Int.zero;
        private ScreenOrientation _lastOrientation = ScreenOrientation.AutoRotation;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void OnEnable()
        {
            ApplySafeArea();
        }

        private void Update()
        {
            // Kiểm tra thay đổi kích thước/safeArea hoặc xoay màn hình (So sánh primitive value, 0 GC allocation)
            if (HasScreenChanged())
            {
                ApplySafeArea();
            }
        }

        private bool HasScreenChanged()
        {
            if (_lastSafeArea != GetCurrentSafeArea()) return true;
            if (_lastScreenSize.x != Screen.width || _lastScreenSize.y != Screen.height) return true;
            if (_lastOrientation != Screen.orientation) return true;
            return false;
        }

        /// <summary>
        /// Tính toán và áp dụng vùng an toàn vào Anchor của RectTransform.
        /// </summary>
        public void ApplySafeArea()
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
                if (_rectTransform == null) return;
            }

            Rect safeArea = GetCurrentSafeArea();
            int screenWidth = Screen.width;
            int screenHeight = Screen.height;

            if (screenWidth <= 0 || screenHeight <= 0) return;

            // Lưu lại cache để kiểm tra thay đổi ở các frame tiếp theo
            _lastSafeArea = safeArea;
            _lastScreenSize = new Vector2Int(screenWidth, screenHeight);
            _lastOrientation = Screen.orientation;

            // Chuyển đổi tọa độ pixel vật lý thành tỉ lệ chuẩn hóa từ 0.0f đến 1.0f
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x = _applyLeft ? (anchorMin.x / screenWidth) : 0f;
            anchorMin.y = _applyBottom ? (anchorMin.y / screenHeight) : 0f;
            anchorMax.x = _applyRight ? (anchorMax.x / screenWidth) : 1f;
            anchorMax.y = _applyTop ? (anchorMax.y / screenHeight) : 1f;

            // Đảm bảo anchor nằm trong khoảng an toàn [0, 1]
            anchorMin.x = Mathf.Clamp01(anchorMin.x);
            anchorMin.y = Mathf.Clamp01(anchorMin.y);
            anchorMax.x = Mathf.Clamp01(anchorMax.x);
            anchorMax.y = Mathf.Clamp01(anchorMax.y);

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
        }

        private Rect GetCurrentSafeArea()
        {
            Rect area = Screen.safeArea;

#if UNITY_EDITOR
            if (_editorSimulation != EditorSimType.None)
            {
                float w = Screen.width;
                float h = Screen.height;

                switch (_editorSimulation)
                {
                    case EditorSimType.TopNotch_And_BottomBar:
                        area = new Rect(0, h * 0.04f, w, h * 0.90f);
                        break;
                    case EditorSimType.Landscape_LeftPunchHole:
                        area = new Rect(w * 0.06f, 0, w * 0.94f, h);
                        break;
                    case EditorSimType.Landscape_RightPunchHole:
                        area = new Rect(0, 0, w * 0.94f, h);
                        break;
                    case EditorSimType.Landscape_BothSides:
                        area = new Rect(w * 0.05f, h * 0.03f, w * 0.90f, h * 0.94f);
                        break;
                }
            }
#endif
            return area;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }
#endif
    }
}
