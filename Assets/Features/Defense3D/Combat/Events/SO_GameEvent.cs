using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Sổ tay Kiến trúc 3D - Phần 5.3: Hệ thống sự kiện có tham số dạng ScriptableObject (Generic Event Channels).
    /// Giúp truyền tải dữ liệu giữa các hệ thống (Máu Nhà chính, Cổ Mộc, Đợt sóng) mà không cần tham chiếu trực tiếp.
    /// </summary>
    public abstract class SO_GameEvent<T> : ScriptableObject
    {
        private readonly List<Action<T>> _listeners = new List<Action<T>>();

        public void Raise(T value)
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i]?.Invoke(value);
            }
        }

        public void RegisterListener(Action<T> listener)
        {
            if (listener != null && !_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        public void UnregisterListener(Action<T> listener)
        {
            if (listener != null)
            {
                _listeners.Remove(listener);
            }
        }
    }

    /// <summary>
    /// Kênh sự kiện truyền số nguyên int (Máu Nhà Chính, Số lượng Gỗ, Đợt sóng hiện tại...).
    /// </summary>
    [CreateAssetMenu(fileName = "Evt_IntEvent", menuName = "TD/Events/Int Event")]
    public class SO_IntEvent : SO_GameEvent<int> { }

    /// <summary>
    /// Kênh sự kiện truyền số thực float (Thời gian còn lại, Thanh tiến trình...).
    /// </summary>
    [CreateAssetMenu(fileName = "Evt_FloatEvent", menuName = "TD/Events/Float Event")]
    public class SO_FloatEvent : SO_GameEvent<float> { }

    /// <summary>
    /// Kênh sự kiện không có tham số (Trận đấu bắt đầu, Chiến thắng, Thất bại, Boss xuất hiện...).
    /// </summary>
    [CreateAssetMenu(fileName = "Evt_VoidEvent", menuName = "TD/Events/Void Event")]
    public class SO_VoidEvent : ScriptableObject
    {
        private readonly List<Action> _listeners = new List<Action>();

        public void Raise()
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i]?.Invoke();
            }
        }

        public void RegisterListener(Action listener)
        {
            if (listener != null && !_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        public void UnregisterListener(Action listener)
        {
            if (listener != null)
            {
                _listeners.Remove(listener);
            }
        }
    }
}
