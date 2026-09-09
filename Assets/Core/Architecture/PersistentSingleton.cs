using UnityEngine;

namespace ProjectZombie.Core.Architecture
{
    /// <summary>
    /// Lớp cơ sở Generic Singleton bền vững qua các Scene (DontDestroyOnLoad).
    /// Triệt tiêu 100% mã nguồn lặp lại (Boilerplate code), bảo vệ vòng đời với cờ _isApplicationQuitting
    /// chống sinh GameObject rác lúc đóng Scene (Tear-down Phase).
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu của Component Manager</typeparam>
    public abstract class PersistentSingleton<T> : MonoBehaviour where T : Component
    {
        private static T _instance;
        private static bool _isApplicationQuitting = false;
        private static readonly object _lock = new object();

        public static T Instance
        {
            get
            {
                if (_isApplicationQuitting)
                {
                    return _instance;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<T>();
                        if (_instance == null && Application.isPlaying)
                        {
                            var go = new GameObject($"[{typeof(T).Name}]");
                            _instance = go.AddComponent<T>();
                            DontDestroyOnLoad(go);
                        }
                    }
                    return _instance;
                }
            }
        }

        public static bool HasInstance => _instance != null;

        protected virtual void Awake()
        {
            _isApplicationQuitting = false;

            if (_instance != null && _instance != this)
            {
                Debug.LogWarning($"[{typeof(T).Name}] Phát hiện bản thể trùng thừa trên '{gameObject.name}'. Đang tự động hủy...");
                Destroy(gameObject);
                return;
            }

            _instance = this as T;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _isApplicationQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
