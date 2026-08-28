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
        [SerializeField] private Controls.SmartSkillDragHandler _dragHandler;

        public event System.Action OnButtonPressed;
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

            if (_attackButton == null) _attackButton = GetComponent<Button>();
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();

            if (_dragHandler == null) _dragHandler = GetComponent<Controls.SmartSkillDragHandler>() ?? gameObject.AddComponent<Controls.SmartSkillDragHandler>();
            if (_dragHandler != null)
            {
                // [LOẠI 2: CHỈ DẤU TẤN CÔNG / SKILL MOBA]
                // Bật cờ RequireHoldOrDrag = true để khi người chơi NHẤP NHANH (Tap) chém thường liên tục thì KHÔNG chớp chỉ dấu.
                // Chỉ hiển thị Mũi tên / Hình quạt quét khi người chơi ĐÈ (>0.12s) hoặc KÉO TAY (Drag) định hướng.
                _dragHandler.RequireHoldOrDrag = true;
                _dragHandler.OnAimStarted += () => OnAimStarted?.Invoke();
                _dragHandler.OnAimUpdated += (dir, pull, isCancel) => OnAimUpdated?.Invoke(dir, pull, isCancel);
                _dragHandler.OnAimReleased += (dir, isTap) => OnAimReleased?.Invoke(dir, isTap);
                _dragHandler.OnAimCancelled += () => OnAimCancelled?.Invoke();
            }
            else if (_attackButton != null)
            {
                _attackButton.onClick.AddListener(() => OnButtonPressed?.Invoke());
            }
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

            if (_dragHandler != null)
            {
                _dragHandler.SetInteractable(isInteractable);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = isInteractable ? 1.0f : 0.6f;
            }
        }
    }
}
