using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Passive View quản lý hiển thị Nút Tấn Công Chủ Động (Attack Button UI).
    /// Tuân thủ chuẩn MVP: Không truy cập Model, chỉ nhận dữ liệu đã format từ AttackButtonPresenter và phát event bấm nút.
    /// </summary>
    public class AttackButtonView : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Button _attackButton;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _cooldownRadialFill;
        [SerializeField] private CanvasGroup _canvasGroup;

        public event System.Action OnButtonPressed;

        private void Awake()
        {
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }

            if (_attackButton == null) _attackButton = GetComponent<Button>();
            if (_attackButton != null)
            {
                _attackButton.onClick.AddListener(() => OnButtonPressed?.Invoke());
            }

            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void SetIcon(Sprite icon)
        {
            if (_iconImage != null && icon != null)
            {
                _iconImage.sprite = icon;
                _iconImage.enabled = true;
            }
        }

        public void SetCooldown(float remainingSeconds, float maxSeconds)
        {
            if (_cooldownRadialFill != null)
            {
                _cooldownRadialFill.fillAmount = maxSeconds > 0f ? Mathf.Clamp01(remainingSeconds / maxSeconds) : 0f;
            }
        }

        public void SetInteractable(bool isInteractable)
        {
            if (_attackButton != null)
            {
                _attackButton.interactable = isInteractable;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = isInteractable ? 1.0f : 0.6f;
            }
        }
    }
}
