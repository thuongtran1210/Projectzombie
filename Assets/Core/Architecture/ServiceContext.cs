using System;
using System.Collections.Generic;

namespace ProjectZombie.Core.Architecture
{
    /// <summary>
    /// Service Context (Lightweight Service Registry) cung cấp khả năng Đăng ký (Register) và Lấy (Get)
    /// các Service Interfaces toàn cục mà không cần liên kết cứng (tight coupling) vào các Singleton static instances.
    /// Tuân thủ nguyên tắc Dependency Inversion (Rule 3.2 & 3.3).
    /// </summary>
    public static class ServiceContext
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetStaticDomainState()
        {
            _services.Clear();
        }

        /// <summary>
        /// Đăng ký một service instance gắn liền với Interface T.
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            if (service == null) return;
            var type = typeof(T);
            _services[type] = service;
        }

        /// <summary>
        /// Hủy đăng ký service T.
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                _services.Remove(type);
            }
        }

        /// <summary>
        /// Lấy instance service đã đăng ký theo Interface T.
        /// </summary>
        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var obj))
            {
                return obj as T;
            }
            return null;
        }

        /// <summary>
        /// Thử lấy instance service. Trả về true nếu đã đăng ký.
        /// </summary>
        public static bool TryGet<T>(out T service) where T : class
        {
            service = Get<T>();
            return service != null;
        }
    }
}
