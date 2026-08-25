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
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            var cg = GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
            Debug.Log($"[{GetType().Name}] -> Show() được gọi! (activeSelf={gameObject.activeSelf})");
        }

        public virtual void Hide()
        {
            var cg = GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
            gameObject.SetActive(false);
            Debug.Log($"[{GetType().Name}] -> Hide() được gọi! (activeSelf={gameObject.activeSelf})");
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
