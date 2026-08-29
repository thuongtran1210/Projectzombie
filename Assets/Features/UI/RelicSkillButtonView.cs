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
            else if (_relicButton != null)
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

        [SerializeField] private Image _recastGlowBorder;

        private Coroutine _pulseRoutine;

        public void SetRecastGlow(bool isRecastActive)
        {
            if (_recastGlowBorder == null)
            {
                // Tự động tìm hoặc tạo một image viền sáng xung quanh nút
                var glowObj = transform.Find("RecastGlowBorder");
                if (glowObj != null)
                {
                    _recastGlowBorder = glowObj.GetComponent<Image>();
                }
                else
                {
                    var newGlow = new GameObject("RecastGlowBorder");
                    newGlow.transform.SetParent(transform, false);
                    newGlow.transform.SetAsFirstSibling();
                    _recastGlowBorder = newGlow.AddComponent<Image>();
                    _recastGlowBorder.color = new Color(1f, 0.85f, 0.2f, 0.8f); // Màu Vàng Kim phát sáng
                    var rt = _recastGlowBorder.rectTransform;
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.sizeDelta = new Vector2(16f, 16f); // Nới rộng hơn nút bấm 16px
                }
            }

            if (_recastGlowBorder != null)
            {
                _recastGlowBorder.gameObject.SetActive(isRecastActive);
                if (isRecastActive)
                {
                    if (_pulseRoutine != null) StopCoroutine(_pulseRoutine);
                    _pulseRoutine = StartCoroutine(RoutineGlowPulse());
                }
                else if (_pulseRoutine != null)
                {
                    StopCoroutine(_pulseRoutine);
                    _pulseRoutine = null;
                }
            }
        }

        private System.Collections.IEnumerator RoutineGlowPulse()
        {
            while (true)
            {
                float alpha = 0.4f + Mathf.PingPong(Time.unscaledTime * 4f, 0.6f);
                if (_recastGlowBorder != null)
                {
                    var c = _recastGlowBorder.color;
                    c.a = alpha;
                    _recastGlowBorder.color = c;
                }
                yield return null;
            }
        }

        public void SetInteractable(bool isInteractable)
        {
            if (_relicButton != null)
            {
                _relicButton.interactable = isInteractable;
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
