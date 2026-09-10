using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Dynamic Virtual Joystick cho màn hình cảm ứng di động Android.
    /// Cung cấp dữ liệu Vector2 (InputVector) trực tiếp cho PlayerInputReader.
    /// Cho phép tự động xuất hiện tại vị trí chạm tay của người chơi.
    /// </summary>
    public class DynamicVirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public static DynamicVirtualJoystick Instance { get; private set; }

        [Header("Joystick Mode")]
        [Tooltip("Nếu tích chọn: Joystick sẽ nhảy đến điểm chạm tay. Nếu bỏ tích: Joystick đứng yên tại vị trí đã đặt.")]
        [SerializeField] private bool _isFloatingJoystick = false;

        [Header("Joystick RectTransform References")]
        [SerializeField] private RectTransform containerRect;
        [SerializeField] private RectTransform handleRect;

        [Header("Joystick Settings")]
        [SerializeField] private float handleRange = 100f;
        [Tooltip("Vùng chết (Deadzone) để loại bỏ rung lắc ngón tay cảm ứng.")]
        [Range(0f, 0.5f)]
        [SerializeField] private float deadZone = 0.1f;
        [Tooltip("Nếu bật: khi kéo ngón tay vượt quá handleRange, gốc Joystick sẽ trượt theo (chỉ tác dụng khi bật Floating Joystick).")]
        [SerializeField] private bool _dynamicFollowDrag = false;

        [Header("Fade Visual (Optional)")]
        [Tooltip("Ẩn Joystick khi không chạm vào (chỉ hiện khi chạm).")]
        [SerializeField] private bool _hideWhenInactive = false;
        [SerializeField] private CanvasGroup _joystickCanvasGroup;

        private Vector2 _inputVector = Vector2.zero;
        public Vector2 InputVector => _inputVector;

        private Vector2 _defaultPosition;
        private int _activePointerId = -999;
        private bool _isPointerDown = false;

        public bool IsPointerDown => _isPointerDown;

        /// <summary>
        /// Cập nhật toạ độ mặc định (Home/Default Position) khi thay đổi qua trình tùy biến UI.
        /// </summary>
        public void SetDefaultPosition(Vector2 newPos)
        {
            _defaultPosition = newPos;
        }

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            Instance = this;
            AutoResolveReferences();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            AutoResolveReferences();

            if (_joystickCanvasGroup == null)
            {
                _joystickCanvasGroup = GetComponent<CanvasGroup>();
                if (_joystickCanvasGroup == null && containerRect != null)
                {
                    _joystickCanvasGroup = containerRect.GetComponent<CanvasGroup>();
                }
            }

            if (containerRect != null)
            {
                _defaultPosition = containerRect.anchoredPosition;
            }

            if (_hideWhenInactive && _joystickCanvasGroup != null)
            {
                _joystickCanvasGroup.alpha = 0f;
            }

            // Kiểm tra EventSystem trong Scene
            if (EventSystem.current == null)
            {
                Debug.LogError($"[{nameof(DynamicVirtualJoystick)}] Thiếu GameObject 'EventSystem' trong Scene! Vui lòng tạo EventSystem (GameObject > UI > Event System) để nhận sự kiện chạm/click.");
            }

            if (containerRect == null || handleRect == null)
            {
                Debug.LogWarning($"[{nameof(DynamicVirtualJoystick)}] containerRect hoặc handleRect chưa được gán trên Inspector trên GameObject: {gameObject.name}.");
            }
        }

        private void AutoResolveReferences()
        {
            if (containerRect == null)
            {
                // 1. Thử tìm trong con của chính GameObject
                Transform visual = transform.Find("Joystick_Visual") ?? transform.Find("Joystick") ?? transform.Find("Container");
                
                // 2. Thử tìm trong anh em (siblings) cùng parent
                if (visual == null && transform.parent != null)
                {
                    visual = transform.parent.Find("Joystick_Visual") 
                          ?? transform.parent.Find("DynamicVirtualJoystick") 
                          ?? transform.parent.Find("Joystick") 
                          ?? transform.parent.Find("Container");
                }

                // 3. Thử tìm sâu trong toàn bộ Panel_MobileControls / Canvas cha
                if (visual == null && transform.root != null)
                {
                    var allTransforms = transform.root.GetComponentsInChildren<Transform>(true);
                    foreach (var t in allTransforms)
                    {
                        if (t != transform && (t.name == "Joystick_Visual" || t.name == "DynamicVirtualJoystick"))
                        {
                            visual = t;
                            break;
                        }
                    }
                }

                if (visual != null)
                {
                    containerRect = visual.GetComponent<RectTransform>();
                }
                else
                {
                    containerRect = GetComponent<RectTransform>();
                }
            }

            if (handleRect == null)
            {
                // Tìm handle bên trong containerRect hoặc chính transform
                Transform searchRoot = containerRect != null ? containerRect.transform : transform;
                Transform handle = searchRoot.Find("JoystickHandle") 
                                   ?? searchRoot.Find("Handle") 
                                   ?? searchRoot.Find("Knob") 
                                   ?? searchRoot.Find("Knob_Visual")
                                   ?? searchRoot.Find("Handle_Visual");

                if (handle == null && searchRoot.childCount > 0)
                {
                    handle = searchRoot.GetChild(0);
                }

                // Nếu vẫn chưa có, thử tìm sâu hơn trong tất cả children của searchRoot hoặc parent
                if (handle == null)
                {
                    var allChildren = searchRoot.GetComponentsInChildren<RectTransform>(true);
                    foreach (var child in allChildren)
                    {
                        if (child != searchRoot && (child.name.ToLower().Contains("handle") || child.name.ToLower().Contains("knob")))
                        {
                            handle = child.transform;
                            break;
                        }
                    }
                }

                if (handle != null)
                {
                    handleRect = handle.GetComponent<RectTransform>();
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isActiveAndEnabled || Controls.Customization.CustomizableControlButton.IsAnyInEditMode) return;
            if (containerRect == null || handleRect == null) return;

            // Đa điểm chạm (Multi-touch Isolation): Nếu Joystick đã có ngón tay điều khiển thì bỏ qua ngón tay khác
            if (_isPointerDown) return;

            _activePointerId = eventData.pointerId;
            _isPointerDown = true;

            // Chế độ Floating Joystick: Chỉ nhảy container khi được cấu hình rõ ràng
            if (_isFloatingJoystick)
            {
                RectTransform parentRect = containerRect.parent as RectTransform;
                if (parentRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint))
                {
                    containerRect.anchoredPosition = localPoint;
                }
            }

            if (_hideWhenInactive && _joystickCanvasGroup != null)
            {
                _joystickCanvasGroup.alpha = 1f;
            }

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isActiveAndEnabled || Controls.Customization.CustomizableControlButton.IsAnyInEditMode) return;
            if (!_isPointerDown || eventData.pointerId != _activePointerId) return;
            if (containerRect == null || handleRect == null) return;

            // Tính vị trí ngón tay so với tâm của containerRect
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                containerRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 position))
            {
                return;
            }

            // Nếu bật Dynamic Follow Drag và là Floating Joystick: khi kéo quá xa thì container mới trượt nhẹ theo
            if (_dynamicFollowDrag && _isFloatingJoystick && position.magnitude > handleRange)
            {
                RectTransform parentRect = containerRect.parent as RectTransform;
                if (parentRect != null)
                {
                    Vector2 excess = position - (position.normalized * handleRange);
                    containerRect.anchoredPosition += excess;
                    position = position.normalized * handleRange;
                }
            }

            float distance = position.magnitude;
            Vector2 rawDir = distance > 0.001f ? position / handleRange : Vector2.zero;

            // Xử lý Deadzone mượt mà (Continuous re-scaled response curve)
            float mag = rawDir.magnitude;
            if (mag < deadZone)
            {
                _inputVector = Vector2.zero;
            }
            else
            {
                float normalizedMag = Mathf.Clamp01((mag - deadZone) / (1f - deadZone));
                _inputVector = rawDir.normalized * normalizedMag;
            }

            // Cập nhật vị trí hiển thị của cần gạt (Knob / Handle)
            Vector2 clampedHandlePos = (distance > handleRange) ? (position / distance) * handleRange : position;
            handleRect.anchoredPosition = clampedHandlePos;
        }

        private void OnDisable()
        {
            ResetJoystick();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                ResetJoystick();
            }
        }

        public void ResetJoystick()
        {
            _activePointerId = -999;
            _isPointerDown = false;
            _inputVector = Vector2.zero;
            if (handleRect != null) handleRect.anchoredPosition = Vector2.zero;

            if (_isFloatingJoystick && containerRect != null)
            {
                containerRect.anchoredPosition = _defaultPosition;
            }

            if (_hideWhenInactive && _joystickCanvasGroup != null)
            {
                _joystickCanvasGroup.alpha = 0f;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != _activePointerId) return;
            ResetJoystick();
        }
    }
}

