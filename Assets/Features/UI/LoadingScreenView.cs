using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Passive View quản lý hiển thị Màn hình Loading / Chuyển Cảnh (Loading Screen).
    /// Chuẩn mỹ thuật Cổ Phong Đông Sơn: Bánh xe Bát Quái xoay, Thanh linh lực ngũ hành, Tips dân gian.
    /// Tuân thủ mô hình MVP và xử lý mượt mà với UnscaledTime.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class LoadingScreenView : MonoBehaviour
    {
        [Header("Root & CanvasGroup")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private GameObject _panelRoot;

        [Header("Progress Elements")]
        [SerializeField] private Image _progressBarFill;
        [SerializeField] private TextMeshProUGUI _progressPercentText;
        [SerializeField] private TextMeshProUGUI _statusMessageText;

        [Header("Lore & Gameplay Tips")]
        [SerializeField] private TextMeshProUGUI _tipTitleText;
        [SerializeField] private TextMeshProUGUI _tipBodyText;

        [Header("Animated Spinner / Totem")]
        [SerializeField] private RectTransform _yinYangSpinner;
        [SerializeField] private float _spinSpeed = -180f; // Độ/giây (xoay theo chiều kim đồng hồ)

        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_panelRoot == null) _panelRoot = gameObject;

            // Đảm bảo ban đầu ẩn bằng CanvasGroup để GameObject vẫn active sẵn sàng chạy Coroutine
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }
        }

        private void Update()
        {
            // Xoay biểu tượng Bát Quái / Trống Đồng liên tục khi màn hình đang hiển thị
            if (_yinYangSpinner != null && _canvasGroup != null && _canvasGroup.alpha > 0.01f)
            {
                _yinYangSpinner.Rotate(0f, 0f, _spinSpeed * Time.unscaledDeltaTime);
            }
        }

        public void SetProgress(float normalizedProgress)
        {
            float clamped = Mathf.Clamp01(normalizedProgress);
            if (_progressBarFill != null)
            {
                _progressBarFill.fillAmount = clamped;
            }

            if (_progressPercentText != null)
            {
                _progressPercentText.text = $"{Mathf.RoundToInt(clamped * 100f)}%";
            }
        }

        public void SetStatusMessage(string message)
        {
            if (_statusMessageText != null)
            {
                _statusMessageText.text = message;
            }
        }

        public void SetTip(string title, string body)
        {
            if (_tipTitleText != null) _tipTitleText.text = title;
            if (_tipBodyText != null) _tipBodyText.text = body;
        }

        public void SetVisible(bool isVisible)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = isVisible ? 1f : 0f;
                _canvasGroup.blocksRaycasts = isVisible;
                _canvasGroup.interactable = isVisible;
            }
        }

        public void FadeIn(float duration, System.Action onComplete = null)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(RoutineFade(0f, 1f, duration, () =>
            {
                SetVisible(true);
                onComplete?.Invoke();
            }));
        }

        public void FadeOut(float duration, System.Action onComplete = null)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(RoutineFade(1f, 0f, duration, () =>
            {
                SetVisible(false);
                onComplete?.Invoke();
            }));
        }

        private IEnumerator RoutineFade(float fromAlpha, float toAlpha, float duration, System.Action onFinished)
        {
            SetVisible(true);
            if (_canvasGroup == null)
            {
                onFinished?.Invoke();
                yield break;
            }

            float elapsed = 0f;
            _canvasGroup.alpha = fromAlpha;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration);
                yield return null;
            }

            _canvasGroup.alpha = toAlpha;
            onFinished?.Invoke();
        }
    }
}
