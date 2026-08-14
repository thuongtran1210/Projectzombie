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
        [Header("New Input System Binding")]
        [InputControl(layout = "Vector2")]
        [SerializeField] private string _controlPath = "<Gamepad>/leftStick";

        [Header("Joystick RectTransform References")]
        [SerializeField] private RectTransform containerRect;
        [SerializeField] private RectTransform handleRect;

        [Header("Settings")]
        [SerializeField] private float handleRange = 100f;

        private Vector2 _inputVector = Vector2.zero;
        public Vector2 InputVector => _inputVector;

        protected override string controlPathInternal
        {
            get => _controlPath;
            set => _controlPath = value;
        }

        private void Start()
        {
            if (containerRect == null) containerRect = GetComponent<RectTransform>();
            if (handleRect == null && transform.childCount > 0)
            {
                // Tự động tìm Handle nếu con đầu tiên là Image
                handleRect = transform.GetChild(0).GetComponent<RectTransform>();
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
            if (containerRect == null || handleRect == null)
            {
                Debug.LogWarning($"[{nameof(DynamicVirtualJoystick)}] Không thể nhận Touch vì containerRect hoặc handleRect bị NULL!");
                return;
            }

            // Di chuyển Joystick container đến vị trí ngón tay chạm màn hình
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                containerRect.parent as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            );

            containerRect.anchoredPosition = localPoint;
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

            _inputVector = position / handleRange;
            _inputVector = (_inputVector.magnitude > 1.0f) ? _inputVector.normalized : _inputVector;

            handleRect.anchoredPosition = _inputVector * handleRange;

            // Gửi trực tiếp giá trị vào Unity New Input System
            SendValueToControl(_inputVector);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _inputVector = Vector2.zero;
            if (handleRect != null) handleRect.anchoredPosition = Vector2.zero;

            // Trả về Vector2.zero trong New Input System khi nhấc tay
            SendValueToControl(Vector2.zero);
        }
    }
}

