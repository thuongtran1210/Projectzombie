using UnityEngine;
using UnityEngine.Events;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Sổ tay Kiến trúc 3D - Phần 5.3: Component lắng nghe sự kiện SO_IntEvent trên GameObject.
    /// Cho phép Game Designer kéo thả hàm xử lý trên Inspector (UnityEvent) mà không cần viết code.
    /// </summary>
    public class SO_IntEventListener : MonoBehaviour
    {
        [Tooltip("Kênh sự kiện SO cần lắng nghe")]
        [SerializeField] private SO_IntEvent _eventChannel;

        [Tooltip("Sự kiện UnityEvent được kích hoạt khi Event Channel phát tín hiệu")]
        [SerializeField] private UnityEvent<int> _onEventRaised;

        private void OnEnable()
        {
            if (_eventChannel != null)
            {
                _eventChannel.RegisterListener(OnEventRaised);
            }
        }

        private void OnDisable()
        {
            if (_eventChannel != null)
            {
                _eventChannel.UnregisterListener(OnEventRaised);
            }
        }

        private void OnEventRaised(int value)
        {
            _onEventRaised?.Invoke(value);
        }
    }

    /// <summary>
    /// Component lắng nghe sự kiện SO_FloatEvent trên GameObject.
    /// </summary>
    public class SO_FloatEventListener : MonoBehaviour
    {
        [Tooltip("Kênh sự kiện SO cần lắng nghe")]
        [SerializeField] private SO_FloatEvent _eventChannel;

        [Tooltip("Sự kiện UnityEvent được kích hoạt khi Event Channel phát tín hiệu")]
        [SerializeField] private UnityEvent<float> _onEventRaised;

        private void OnEnable()
        {
            if (_eventChannel != null)
            {
                _eventChannel.RegisterListener(OnEventRaised);
            }
        }

        private void OnDisable()
        {
            if (_eventChannel != null)
            {
                _eventChannel.UnregisterListener(OnEventRaised);
            }
        }

        private void OnEventRaised(float value)
        {
            _onEventRaised?.Invoke(value);
        }
    }

    /// <summary>
    /// Component lắng nghe sự kiện SO_VoidEvent (không tham số) trên GameObject.
    /// </summary>
    public class SO_VoidEventListener : MonoBehaviour
    {
        [Tooltip("Kênh sự kiện SO cần lắng nghe")]
        [SerializeField] private SO_VoidEvent _eventChannel;

        [Tooltip("Sự kiện UnityEvent được kích hoạt khi Event Channel phát tín hiệu")]
        [SerializeField] private UnityEvent _onEventRaised;

        private void OnEnable()
        {
            if (_eventChannel != null)
            {
                _eventChannel.RegisterListener(OnEventRaised);
            }
        }

        private void OnDisable()
        {
            if (_eventChannel != null)
            {
                _eventChannel.UnregisterListener(OnEventRaised);
            }
        }

        private void OnEventRaised()
        {
            _onEventRaised?.Invoke();
        }
    }
}
