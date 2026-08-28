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
        [SerializeField] private Controls.SmartSkillDragHandler _dragHandler;

        public event System.Action OnButtonClicked;
        public event System.Action OnAimStarted;
        public event System.Action<Vector2, float, bool> OnAimUpdated;
        public event System.Action<Vector2, bool> OnAimReleased;
        public event System.Action OnAimCancelled;

        private void Awake()
        {
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }

            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_dragHandler == null) _dragHandler = GetComponent<Controls.SmartSkillDragHandler>() ?? gameObject.AddComponent<Controls.SmartSkillDragHandler>();

            if (_dragHandler != null)
            {
                _dragHandler.OnAimStarted += () => OnAimStarted?.Invoke();
                _dragHandler.OnAimUpdated += (dir, pull, isCancel) => OnAimUpdated?.Invoke(dir, pull, isCancel);
                _dragHandler.OnAimReleased += (dir, isTap) => {
                    if (isTap) OnButtonClicked?.Invoke();
                    OnAimReleased?.Invoke(dir, isTap);
                };
                _dragHandler.OnAimCancelled += () => OnAimCancelled?.Invoke();
            }
            else if (_skillButton != null)
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

            if (_dragHandler != null)
            {
                _dragHandler.SetInteractable(isInteractable);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = isInteractable ? 1.0f : 0.4f;
            }
        }
    }
}
