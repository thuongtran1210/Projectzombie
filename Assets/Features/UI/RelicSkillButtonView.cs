using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Passive View quản lý Nút bấm Kỹ Năng Pháp Bảo (Relic Skill Button UI).
    /// Tuân thủ Mô hình MVP: Nhận dữ liệu đã định dạng từ RelicSkillPresenter, phát sự kiện OnButtonClicked.
    /// </summary>
    public class RelicSkillButtonView : MonoBehaviour
    {
        [Header("UI Component References")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _cooldownRadialFill;
        [SerializeField] private TextMeshProUGUI _cooldownText;
        [SerializeField] private Button _relicButton;
        [SerializeField] private CanvasGroup _canvasGroup;

        public event System.Action OnButtonClicked;

        private void Awake()
        {
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }

            if (_relicButton != null)
            {
                _relicButton.onClick.AddListener(() => OnButtonClicked?.Invoke());
            }
        }

        public void SetIcon(Sprite icon)
        {
            if (_iconImage != null)
            {
                _iconImage.sprite = icon;
                _iconImage.gameObject.SetActive(icon != null);
            }
        }

        public void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }

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

        public void SetInteractable(bool isInteractable)
        {
            if (_relicButton != null)
            {
                _relicButton.interactable = isInteractable;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = isInteractable ? 1.0f : 0.4f;
            }
        }
    }
}
