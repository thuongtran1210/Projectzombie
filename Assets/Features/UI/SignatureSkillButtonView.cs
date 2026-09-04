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
                    OnAimReleased?.Invoke(dir, isTap);
                };
                _dragHandler.OnAimCancelled += () => OnAimCancelled?.Invoke();
            }
            else if (_skillButton != null)
            {
                _skillButton.onClick.AddListener(() => OnButtonClicked?.Invoke());
            }
        }

        private void EnsureCooldownComponents()
        {
            if (_cooldownRadialFill == null)
            {
                // Thử tìm Image CooldownFill trong children
                var fillFind = transform.Find("CooldownFill");
                if (fillFind != null)
                {
                    _cooldownRadialFill = fillFind.GetComponent<Image>();
                }
                else
                {
                    // Tự tạo một lớp Cooldown Radial Fill bán trong suốt phủ lên nút
                    var fillObj = new GameObject("CooldownFill");
                    fillObj.transform.SetParent(transform, false);
                    fillObj.transform.SetAsLastSibling();

                    var rt = fillObj.AddComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.sizeDelta = Vector2.zero;

                    _cooldownRadialFill = fillObj.AddComponent<Image>();
                    _cooldownRadialFill.color = new Color(0f, 0f, 0f, 0.65f); // Lớp phủ tối đếm lùi
                    _cooldownRadialFill.type = Image.Type.Filled;
                    _cooldownRadialFill.fillMethod = Image.FillMethod.Radial360;
                    _cooldownRadialFill.fillOrigin = (int)Image.Origin360.Top;
                    _cooldownRadialFill.fillClockwise = false;
                    _cooldownRadialFill.raycastTarget = false;

                    // Lấy sprite Mask_Circle nếu có
                    if (GetComponent<Image>() != null && GetComponent<Image>().sprite != null)
                    {
                        _cooldownRadialFill.sprite = GetComponent<Image>().sprite;
                    }
                }
            }

            if (_cooldownText == null)
            {
                _cooldownText = GetComponentInChildren<TextMeshProUGUI>(true);
                if (_cooldownText == null)
                {
                    var textObj = new GameObject("Txt_Cooldown");
                    textObj.transform.SetParent(transform, false);
                    textObj.transform.SetAsLastSibling();

                    var rt = textObj.AddComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.sizeDelta = Vector2.zero;

                    _cooldownText = textObj.AddComponent<TextMeshProUGUI>();
                    _cooldownText.alignment = TextAlignmentOptions.Center;
                    _cooldownText.fontSize = 28f;
                    _cooldownText.fontStyle = FontStyles.Bold;
                    _cooldownText.color = Color.white;
                    _cooldownText.raycastTarget = false;
                }
            }
        }

        /// <summary>
        /// Cập nhật thời gian hồi chiêu và hiển thị Radial Fill.
        /// </summary>
        public void SetCooldown(float remainingSeconds, float maxSeconds, string formattedText)
        {
            EnsureCooldownComponents();

            bool isCoolingDown = remainingSeconds > 0f;

            if (_cooldownRadialFill != null)
            {
                _cooldownRadialFill.fillAmount = maxSeconds > 0f ? Mathf.Clamp01(remainingSeconds / maxSeconds) : 0f;
                // Nếu radial fill là đối tượng riêng (không phải chính nút), chỉ hiển thị khi đang hồi chiêu
                if (_cooldownRadialFill.gameObject != this.gameObject)
                {
                    _cooldownRadialFill.gameObject.SetActive(isCoolingDown);
                }
            }

            if (_cooldownText != null)
            {
                _cooldownText.text = isCoolingDown ? (string.IsNullOrEmpty(formattedText) ? $"{Mathf.CeilToInt(remainingSeconds)}s" : formattedText) : string.Empty;
                _cooldownText.gameObject.SetActive(isCoolingDown);
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
                // Giữ alpha = 1.0f để UI đếm lùi hồi chiêu và icon luôn sáng rõ
                _canvasGroup.alpha = isInteractable ? 1.0f : 0.85f;
            }
        }
    }
}
