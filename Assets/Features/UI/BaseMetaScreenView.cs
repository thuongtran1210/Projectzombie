using UnityEngine;

namespace ProjectZombie.Features.UI
{
    public enum MetaScreenType
    {
        MainHub,
        CharacterSelect,
        SanctuaryTree,
        Codex,
        Settings
    }

    /// <summary>
    /// Lớp cơ sở cho toàn bộ các màn hình / Popup thuộc hệ thống UI Ngoài Game (Meta Menu).
    /// Hỗ trợ quản lý hiển thị, ẩn và xử lý phím Back theo chuẩn Navigation Stack.
    /// </summary>
    public abstract class BaseMetaScreenView : MonoBehaviour
    {
        public abstract MetaScreenType ScreenType { get; }

        protected virtual void Awake()
        {
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }

            // Nếu không phải là MainHub, mặc định ẩn màn hình khi khởi động Scene
            if (ScreenType != MetaScreenType.MainHub)
            {
                Hide();
            }
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Gọi khi người dùng ấn nút Back hoặc phím Escape/Back phần cứng.
        /// </summary>
        public virtual void OnBackPressed()
        {
            if (MetaUIManager.Instance != null)
            {
                MetaUIManager.Instance.PopScreen();
            }
        }
    }
}
