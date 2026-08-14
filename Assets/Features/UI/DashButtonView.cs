using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Passive View quản lý hiển thị Nút bấm Lướt (Dash Button UI).
    /// Tuân thủ chuẩn MVP (Section 12 Rules): Không truy cập Model, chỉ nhận dữ liệu đã format từ DashButtonPresenter.
    /// </summary>
    public class DashButtonView : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Button _dashButton;
        [SerializeField] private Image _cooldownRadialFill;
        [SerializeField] private TextMeshProUGUI _cooldownText;
        [SerializeField] private CanvasGroup _canvasGroup;

        public event System.Action OnButtonClicked;

        private void Awake()
        {
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }

            if (_dashButton == null) _dashButton = GetComponent<Button>();
            if (_dashButton != null)
            {
                _dashButton.onClick.AddListener(() => OnButtonClicked?.Invoke());
            }

            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// Cập nhật thanh hồi chiêu và chuỗi hiển thị số giây.
        /// </summary>
        public void SetCooldown(float remainingSeconds, float maxSeconds, string formattedText)
        {
            if (_cooldownRadialFill != null)
            {
                _cooldownRadialFill.fillAmount = maxSeconds > 0f ? Mathf.Clamp01(remainingSeconds / maxSeconds) : 0f;
            }

            if (_cooldownText != null)
            {
                _cooldownText.text = formattedText;
                _cooldownText.gameObject.SetActive(remainingSeconds > 0f);
            }
        }

        /// <summary>
        /// Bật/tắt trạng thái tương tác của nút.
        /// </summary>
        public void SetInteractable(bool isInteractable)
        {
            if (_dashButton != null)
            {
                _dashButton.interactable = isInteractable;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = isInteractable ? 1.0f : 0.4f;
            }
        }
    }
}
