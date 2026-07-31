using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Passive View quản lý Nút bấm Kỹ năng Chủ động (Signature Skill Button UI).
    /// Tuân thủ Mô hình MVP (Section 12 Rules): Không tự đọc Model, chỉ nhận dữ liệu đã định dạng từ Presenter.
    /// </summary>
    public class SignatureSkillButtonView : MonoBehaviour
    {
        [Header("UI Component References")]
        [SerializeField] private Image _cooldownRadialFill;
        [SerializeField] private TextMeshProUGUI _cooldownText;
        [SerializeField] private Button _skillButton;
        [SerializeField] private CanvasGroup _canvasGroup;

        public event System.Action OnButtonClicked;

        private void Awake()
        {
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }

            if (_skillButton != null)
            {
                _skillButton.onClick.AddListener(() => OnButtonClicked?.Invoke());
            }
        }

        /// <summary>
        /// Cập nhật thời gian hồi chiêu và hiển thị Radial Fill.
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
        /// Bật/tắt trạng thái tương tác của nút (được bấm hay bị mờ/khóa).
        /// </summary>
        public void SetInteractable(bool isInteractable)
        {
            if (_skillButton != null)
            {
                _skillButton.interactable = isInteractable;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = isInteractable ? 1.0f : 0.4f;
            }
        }
    }
}
