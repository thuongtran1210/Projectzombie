using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Dynamic Virtual Joystick cho màn hình cảm ứng di động Android.
    /// Tích hợp trực tiếp với Unity New Input System (OnScreenControl), phát dữ liệu vào controlPath <Gamepad>/leftStick.
    /// Cho phép tự động xuất hiện tại vị trí chạm tay của người chơi.
    /// </summary>
    public class DynamicVirtualJoystick : OnScreenControl, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public static DynamicVirtualJoystick Instance { get; private set; }

        [Header("New Input System Binding")]
        [InputControl(layout = "Vector2")]
        [SerializeField] private string _controlPath = "<Gamepad>/leftStick";

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
        [Tooltip("Nếu bật: khi kéo ngón tay vượt quá handleRange, gốc Joystick sẽ tự động trượt theo ngón tay.")]
        [SerializeField] private bool _dynamicFollowDrag = true;

        [Header("Fade Visual (Optional)")]
        [Tooltip("Ẩn Joystick khi không chạm vào (chỉ hiện khi chạm).")]
        [SerializeField] private bool _hideWhenInactive = false;
        [SerializeField] private CanvasGroup _joystickCanvasGroup;

        private Vector2 _inputVector = Vector2.zero;
        public Vector2 InputVector => _inputVector;

        private Vector2 _defaultPosition;

        protected override string controlPathInternal
        {
            get => _controlPath;
            set => _controlPath = value;
        }

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (containerRect == null) containerRect = GetComponent<RectTransform>();
            if (handleRect == null && transform.childCount > 0)
            {
                // Tự động tìm Handle nếu con đầu tiên là Image
                handleRect = transform.GetChild(0).GetComponent<RectTransform>();
            }

            if (_joystickCanvasGroup == null)
            {
                _joystickCanvasGroup = GetComponent<CanvasGroup>();
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
                Debug.LogWarning($"[{nameof(DynamicVirtualJoystick)}] containerRect hoặc handleRect chưa được gán trên Inspector.");
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (containerRect == null || handleRect == null) return;

            if (_isFloatingJoystick && containerRect.parent is RectTransform parentRect)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint))
                {
                    containerRect.localPosition = localPoint;
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
            if (containerRect == null || handleRect == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                containerRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 position
            );

            // Dynamic follow: khi ngón tay vượt quá handleRange, gốc Joystick trượt theo
            if (_dynamicFollowDrag && _isFloatingJoystick && position.magnitude > handleRange && containerRect.parent is RectTransform parentRect)
            {
                Vector2 excess = position - (position.normalized * handleRange);
                containerRect.localPosition += (Vector3)excess;
                position = position.normalized * handleRange;
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
            Vector2 clampedHandlePos = (rawDir.magnitude > 1.0f) ? rawDir.normalized * handleRange : position;
            handleRect.anchoredPosition = clampedHandlePos;

            // Gửi trực tiếp giá trị vào Unity New Input System
            SendValueToControl(_inputVector);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
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

            // Trả về Vector2.zero trong New Input System khi nhấc tay
            SendValueToControl(Vector2.zero);
        }
    }
}

