using System;
using System.Collections.Generic;

namespace ProjectZombie.Core.Events
{
    /// <summary>
    /// Event Bus tĩnh gọn nhẹ, type-safe phục vụ Event-Driven Architecture.
    /// Giúp Decouple hoàn toàn giữa Game Logic và Presentation (UI, VFX, Audio).
    /// </summary>
    public static class GameEventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();

        /// <summary>
        /// Đăng ký nhận sự kiện với kiểu dữ liệu T.
        /// </summary>
        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null) return;

            Type type = typeof(T);
            if (!_subscribers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                _subscribers[type] = list;
            }

            if (!list.Contains(handler))
            {
                list.Add(handler);
            }
        }

        /// <summary>
        /// Hủy đăng ký nhận sự kiện.
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null) return;

            Type type = typeof(T);
            if (_subscribers.TryGetValue(type, out var list))
            {
                list.Remove(handler);
                if (list.Count == 0)
                {
                    _subscribers.Remove(type);
                }
            }
        }

        /// <summary>
        /// Phát sự kiện tới toàn bộ subscriber đã đăng ký.
        /// </summary>
        public static void Publish<T>(T eventData) where T : struct
        {
            Type type = typeof(T);
            if (_subscribers.TryGetValue(type, out var list))
            {
                // Duyệt ngược để an toàn khi subscriber tự unsubscribe trong lúc callback
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (i < list.Count && list[i] is Action<T> action)
                    {
                        action.Invoke(eventData);
                    }
                }
            }
        }

        /// <summary>
        /// Xóa sạch tất cả các subscription (dùng khi reset scene hoặc shutdown game).
        /// </summary>
        public static void ClearAll()
        {
            _subscribers.Clear();
        }
    }
}
