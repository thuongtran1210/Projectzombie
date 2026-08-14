using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Core.Services
{
    /// <summary>
    /// Service Locator đơn giản, an toàn cho phép đăng ký và truy xuất service qua Interface.
    /// Giúp loại bỏ phụ thuộc cứng vào Singleton tĩnh.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// Đăng ký một Service triển khai interface T.
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            Type type = typeof(T);
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Service {type.Name} đã được đăng ký trước đó. Sẽ ghi đè reference mới.");
                _services[type] = service;
            }
            else
            {
                _services.Add(type, service);
            }
        }

        /// <summary>
        /// Hủy đăng ký Service T.
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            Type type = typeof(T);
            if (_services.ContainsKey(type))
            {
                _services.Remove(type);
            }
        }

        /// <summary>
        /// Truy xuất Service T. Trả về null nếu chưa đăng ký.
        /// </summary>
        public static T Get<T>() where T : class
        {
            Type type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return (T)service;
            }

            Debug.LogWarning($"[ServiceLocator] Service {type.Name} chưa được đăng ký trong hệ thống!");
            return null;
        }

        /// <summary>
        /// Thử truy xuất Service T.
        /// </summary>
        public static bool TryGet<T>(out T service) where T : class
        {
            Type type = typeof(T);
            if (_services.TryGetValue(type, out var obj))
            {
                service = (T)obj;
                return true;
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Xóa sạch mọi đăng ký Service (khi đổi scene hoặc kết thúc game).
        /// </summary>
        public static void Reset()
        {
            _services.Clear();
        }
    }
}
