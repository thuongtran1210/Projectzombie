using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Vùng cảm ứng màn hình rộng (Touch Zone) cho Joystick trên thiết bị di động Android.
    /// Cho phép người chơi chạm vào bất kỳ điểm nào ở nửa trái màn hình để điều khiển cần gạt.
    /// Chuyển tiếp (forward) các sự kiện PointerDown, Drag, PointerUp sang DynamicVirtualJoystick.
    /// </summary>
    public class DynamicJoystickTouchZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Tooltip("Tham chiếu tới DynamicVirtualJoystick để chuyển tiếp sự kiện cảm ứng")]
        [SerializeField] private DynamicVirtualJoystick _targetJoystick;

        private void Awake()
        {
            if (_targetJoystick == null)
            {
                _targetJoystick = DynamicVirtualJoystick.Instance ?? FindObjectOfType<DynamicVirtualJoystick>();
            }

            // Đảm bảo có Graphic trong suốt để nhận Touch Event
            if (!TryGetComponent<UnityEngine.UI.Image>(out var touchGraphic))
            {
                touchGraphic = gameObject.AddComponent<UnityEngine.UI.Image>();
                touchGraphic.color = new Color(0f, 0f, 0f, 0f);
            }
            touchGraphic.raycastTarget = true;
        }

        public void BindJoystick(DynamicVirtualJoystick joystick)
        {
            _targetJoystick = joystick;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_targetJoystick == null) _targetJoystick = DynamicVirtualJoystick.Instance;
            _targetJoystick?.OnPointerDown(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_targetJoystick == null) _targetJoystick = DynamicVirtualJoystick.Instance;
            _targetJoystick?.OnDrag(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_targetJoystick == null) _targetJoystick = DynamicVirtualJoystick.Instance;
            _targetJoystick?.OnPointerUp(eventData);
        }
    }
}
