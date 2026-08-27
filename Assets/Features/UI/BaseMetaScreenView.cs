using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectZombie.Features.UI
{
    public enum MetaScreenType
    {
        MainHub,
        CharacterSelect,
        WeaponLoadout,
        SanctuaryTree,
        Codex,
        Settings
    }

    /// <summary>
    /// Lớp cơ sở chuẩn hoá cho toàn bộ các màn hình / Popup thuộc hệ thống UI Ngoài Game (Meta Menu).
    /// Quản lý tối ưu hiển thị (Canvas/CanvasGroup không rebuild layout), hiệu ứng Pop-in mượt mà và cơ chế Click Outside to Close.
    /// </summary>
    public abstract class BaseMetaScreenView : MonoBehaviour
    {
        public abstract MetaScreenType ScreenType { get; }

        [Header("Modal & Animation Components")]
        [Tooltip("Khung hộp thoại trung tâm (được scale nảy khi mở)")]
        [SerializeField] protected RectTransform _modalContainer;

        [Tooltip("Lớp nền đen mờ phía sau (nhấn vào để đóng popup)")]
        [SerializeField] protected Button _dimBackgroundButton;

        [Tooltip("CanvasGroup điều khiển Alpha và Raycast")]
        [SerializeField] protected CanvasGroup _screenCanvasGroup;

        [Tooltip("Sub-Canvas tối ưu draw call (nếu có)")]
        [SerializeField] protected Canvas _screenCanvas;

        private Coroutine _animCoroutine;

        protected virtual void Awake()
        {
            if (_screenCanvasGroup == null) _screenCanvasGroup = GetComponent<CanvasGroup>();
            if (_screenCanvas == null) _screenCanvas = GetComponent<Canvas>();

            // Auto-detect modal container if not explicitly wired
            if (_modalContainer == null)
            {
                foreach (Transform child in transform)
                {
                    if (child.name.StartsWith("Modal_") || child.name.Contains("Modal") || child.name.Contains("MainModal"))
                    {
                        _modalContainer = child.GetComponent<RectTransform>();
                        break;
                    }
                }
            }

            // Auto-detect Dim background button
            if (_dimBackgroundButton == null)
            {
                foreach (Transform child in transform)
                {
                    if (child.name.StartsWith("Dim_") || child.name.Contains("Dim") || child.name.Contains("BackgroundDim"))
                    {
                        _dimBackgroundButton = child.GetComponent<Button>();
                        break;
                    }
                }
            }

            // Lắng nghe sự kiện chạm ngoài nền để đóng Modal
            if (_dimBackgroundButton != null)
            {
                _dimBackgroundButton.onClick.RemoveListener(OnBackPressed);
                _dimBackgroundButton.onClick.AddListener(OnBackPressed);
            }

            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);

            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }

            if (_screenCanvas != null) _screenCanvas.enabled = true;

            if (_screenCanvasGroup != null)
            {
                _screenCanvasGroup.interactable = true;
                _screenCanvasGroup.blocksRaycasts = true;
            }

            if (gameObject.activeInHierarchy)
            {
                if (_animCoroutine != null) StopCoroutine(_animCoroutine);
                _animCoroutine = StartCoroutine(PlayOpenAnimationRoutine());
            }
            else
            {
                if (_screenCanvasGroup != null) _screenCanvasGroup.alpha = 1f;
                if (_modalContainer != null) _modalContainer.localScale = Vector3.one;
            }

            Debug.Log($"[{GetType().Name}] -> Show() được gọi mượt mà!");
        }

        public virtual void Hide()
        {
            if (_screenCanvasGroup != null)
            {
                _screenCanvasGroup.interactable = false;
                _screenCanvasGroup.blocksRaycasts = false;
            }

            if (gameObject.activeInHierarchy)
            {
                if (_animCoroutine != null) StopCoroutine(_animCoroutine);
                _animCoroutine = StartCoroutine(PlayCloseAnimationRoutine());
            }
            else
            {
                if (_screenCanvasGroup != null) _screenCanvasGroup.alpha = 0f;
                if (_screenCanvas != null) _screenCanvas.enabled = false;
            }

            Debug.Log($"[{GetType().Name}] -> Hide() được gọi mượt mà!");
        }

        /// <summary>
        /// Hiệu ứng mở Modal mượt mà: Fade Alpha (0 -> 1) + Scale Pop-In (0.92 -> 1.0) trong 0.15s
        /// </summary>
        protected virtual IEnumerator PlayOpenAnimationRoutine()
        {
            float duration = 0.15f;
            float elapsed = 0f;

            if (_modalContainer != null) _modalContainer.localScale = new Vector3(0.92f, 0.92f, 1f);
            if (_screenCanvasGroup != null) _screenCanvasGroup.alpha = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easeOut = Mathf.Sin(t * Mathf.PI * 0.5f); // Smooth Ease Out

                if (_screenCanvasGroup != null) _screenCanvasGroup.alpha = easeOut;
                if (_modalContainer != null) _modalContainer.localScale = Vector3.Lerp(new Vector3(0.92f, 0.92f, 1f), Vector3.one, easeOut);

                yield return null;
            }

            if (_screenCanvasGroup != null) _screenCanvasGroup.alpha = 1f;
            if (_modalContainer != null) _modalContainer.localScale = Vector3.one;
            _animCoroutine = null;
        }

        /// <summary>
        /// Hiệu ứng đóng Modal: Fade Alpha (1 -> 0) + Scale (1.0 -> 0.95) trong 0.1s
        /// </summary>
        protected virtual IEnumerator PlayCloseAnimationRoutine()
        {
            float duration = 0.1f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (_screenCanvasGroup != null) _screenCanvasGroup.alpha = 1f - t;
                if (_modalContainer != null) _modalContainer.localScale = Vector3.Lerp(Vector3.one, new Vector3(0.95f, 0.95f, 1f), t);

                yield return null;
            }

            if (_screenCanvasGroup != null) _screenCanvasGroup.alpha = 0f;
            if (_screenCanvas != null) _screenCanvas.enabled = false;
            gameObject.SetActive(false);
            _animCoroutine = null;
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
