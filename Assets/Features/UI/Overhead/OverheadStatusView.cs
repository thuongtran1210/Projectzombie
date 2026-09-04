// ============================================================================
// FILE: OverheadStatusView.cs — TẦNG VIEW (MVP)
// Trách nhiệm: Render thanh máu, vệt máu trễ (Delay Bar), Level Text trên đầu nhân vật.
// KHÔNG xử lý logic gameplay hay trực tiếp truy cập Model.
// ============================================================================

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectZombie.Features.UI.Overhead
{
    /// <summary>
    /// View component cho World-Space UI trên đầu Entity (Player / Enemy / Boss).
    /// Hỗ trợ thanh máu, vệt máu trễ (juice effect), và level badge.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class OverheadStatusView : MonoBehaviour
    {
        [Header("Health Bar Visuals")]
        [SerializeField] private Image _healthFillImage;
        [SerializeField] private Image _healthDelayFillImage;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Level & EXP Display (Optional)")]
        [SerializeField] private GameObject _levelBadgeRoot;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private Image _expRingFillImage;
        [SerializeField] private string _levelFormat = "{0}";

        [Header("Juice & Animation Settings")]
        [SerializeField] private float _delayDuration = 0.4f;
        [SerializeField] private float _delayCatchupSpeed = 2.5f;
        [SerializeField] private bool _autoHideWhenFull = false;
        [SerializeField] private float _autoHideDelay = 3f;
        [SerializeField] private float _fadeDuration = 0.3f;

        [Header("Orientation Settings")]
        [Tooltip("Nếu true, View sẽ tự động giữ nguyên tỷ lệ scale dương trên trục X để tránh bị lật ngược chữ khi nhân vật flip scale.")]
        [SerializeField] private bool _keepUpright = true;

        private Coroutine _delayBarCoroutine;
        private Coroutine _fadeCoroutine;
        private float _lastHealthRatio = 1f;
        private float _currentAlpha = 1f;
        private Transform _parentTransform;

        private void Awake()
        {
            var canvas = GetComponent<Canvas>();
            if (canvas != null && canvas.worldCamera == null)
            {
                canvas.worldCamera = Camera.main;
            }

            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }

            _parentTransform = transform.parent;

            if (_autoHideWhenFull)
            {
                SetCanvasAlpha(0f);
            }
        }

        private void LateUpdate()
        {
            if (_keepUpright && _parentTransform != null)
            {
                // Đảm bảo Canvas World-Space không bị lật ngược khi parent đổi hướng scale X (vd: localScale.x = -1)
                Vector3 currentScale = transform.localScale;
                float parentSignX = Mathf.Sign(_parentTransform.lossyScale.x);
                if (Mathf.Sign(currentScale.x) * parentSignX < 0)
                {
                    transform.localScale = new Vector3(-currentScale.x, currentScale.y, currentScale.z);
                }
            }
        }

        /// <summary>
        /// Cập nhật thanh máu và kích hoạt hiệu ứng trễ.
        /// </summary>
        public void SetHealth(float currentHealth, float maxHealth)
        {
            if (maxHealth <= 0f) return;

            float targetRatio = Mathf.Clamp01(currentHealth / maxHealth);

            if (_healthFillImage != null)
            {
                _healthFillImage.fillAmount = targetRatio;
            }

            // Xử lý hiệu ứng Delay Bar khi nhận damage
            if (targetRatio < _lastHealthRatio)
            {
                if (_delayBarCoroutine != null) StopCoroutine(_delayBarCoroutine);
                _delayBarCoroutine = StartCoroutine(AnimateDelayBarRoutine(targetRatio));

                // Hiện lại bar nếu đang ở chế độ auto-hide
                if (_autoHideWhenFull)
                {
                    ShowWithAutoHide();
                }
            }
            else
            {
                // Khi hồi máu, thanh trễ lên cùng ngay lập tức
                if (_healthDelayFillImage != null)
                {
                    _healthDelayFillImage.fillAmount = targetRatio;
                }
            }

            _lastHealthRatio = targetRatio;
        }

        /// <summary>
        /// Cập nhật cấp độ nhân vật.
        /// </summary>
        public void SetLevel(int level)
        {
            if (_levelBadgeRoot != null)
            {
                _levelBadgeRoot.SetActive(true);
            }

            if (_levelText != null)
            {
                _levelText.text = string.Format(_levelFormat, level);
            }
        }

        /// <summary>
        /// Cập nhật vòng tròn tiến trình kinh nghiệm (EXP Ring).
        /// </summary>
        public void SetExp(float currentExp, float maxExp)
        {
            if (_expRingFillImage != null && maxExp > 0f)
            {
                _expRingFillImage.fillAmount = Mathf.Clamp01(currentExp / maxExp);
            }
        }

        /// <summary>
        /// Ẩn hoàn toàn cụm level (dùng khi gắn cho Enemy hoặc Quái không có level).
        /// </summary>
        public void SetLevelVisible(bool isVisible)
        {
            if (_levelBadgeRoot != null)
            {
                _levelBadgeRoot.SetActive(isVisible);
            }
        }

        /// <summary>
        /// Bật/tắt toàn bộ Overhead UI.
        /// </summary>
        public void SetVisibility(bool isVisible)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = isVisible ? 1f : 0f;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }
            else
            {
                gameObject.SetActive(isVisible);
            }
        }

        private IEnumerator AnimateDelayBarRoutine(float targetRatio)
        {
            yield return new WaitForSeconds(_delayDuration);

            if (_healthDelayFillImage == null) yield break;

            while (_healthDelayFillImage.fillAmount > targetRatio)
            {
                _healthDelayFillImage.fillAmount = Mathf.MoveTowards(
                    _healthDelayFillImage.fillAmount,
                    targetRatio,
                    _delayCatchupSpeed * Time.deltaTime);
                yield return null;
            }

            _healthDelayFillImage.fillAmount = targetRatio;
        }

        private void ShowWithAutoHide()
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(AutoHideRoutine());
        }

        private IEnumerator AutoHideRoutine()
        {
            SetCanvasAlpha(1f);
            yield return new WaitForSeconds(_autoHideDelay);

            // Chỉ mờ đi nếu đang đầy máu
            if (_lastHealthRatio >= 0.999f)
            {
                float t = 0f;
                while (t < _fadeDuration)
                {
                    t += Time.deltaTime;
                    SetCanvasAlpha(Mathf.Lerp(1f, 0f, t / _fadeDuration));
                    yield return null;
                }
                SetCanvasAlpha(0f);
            }
        }

        private void SetCanvasAlpha(float alpha)
        {
            _currentAlpha = alpha;
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = alpha;
            }
        }
    }
}
