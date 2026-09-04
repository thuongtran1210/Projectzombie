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
        public event System.Action<Vector2, float, bool> OnAimDetailedReleased;
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
                _dragHandler.OnAimDetailedReleased += (dir, pull, isTap) => OnAimDetailedReleased?.Invoke(dir, pull, isTap);
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

        private static readonly string[] s_CooldownSecondsCache = GenerateCooldownStringCache(120);

        private static string[] GenerateCooldownStringCache(int maxSeconds)
        {
            var cache = new string[maxSeconds + 1];
            for (int i = 0; i <= maxSeconds; i++)
            {
                cache[i] = $"{i}s";
            }
            return cache;
        }

        public static string GetCachedCooldownText(float remainingSeconds)
        {
            if (remainingSeconds <= 0f) return string.Empty;
            int sec = Mathf.CeilToInt(remainingSeconds);
            if (sec >= 0 && sec < s_CooldownSecondsCache.Length)
            {
                return s_CooldownSecondsCache[sec];
            }
            return $"{sec}s";
        }

        public void SetCooldown(float remainingSeconds, float maxSeconds, string formattedText = null)
        {
            if (_cooldownRadialFill != null)
            {
                _cooldownRadialFill.fillAmount = maxSeconds > 0f ? Mathf.Clamp01(remainingSeconds / maxSeconds) : 0f;
            }

            if (_cooldownText != null)
            {
                if (remainingSeconds > 0f)
                {
                    _cooldownText.text = formattedText ?? GetCachedCooldownText(remainingSeconds);
                    _cooldownText.gameObject.SetActive(true);
                }
                else
                {
                    _cooldownText.gameObject.SetActive(false);
                }
            }
        }

        [Header("Stack / Progress Badge")]
        [SerializeField] private GameObject _stackBadgeRoot;
        [SerializeField] private Image _stackBadgeBg;
        [SerializeField] private TextMeshProUGUI _stackBadgeText;
        private Coroutine _stackBadgePunchRoutine;

        public void SetStackBadge(string badgeText)
        {
            if (string.IsNullOrEmpty(badgeText))
            {
                if (_stackBadgeRoot != null) _stackBadgeRoot.SetActive(false);
                return;
            }

            EnsureStackBadgeUI();

            if (_stackBadgeRoot != null)
            {
                _stackBadgeRoot.SetActive(true);
            }

            if (_stackBadgeText != null)
            {
                _stackBadgeText.text = badgeText;
            }

            if (gameObject.activeInHierarchy)
            {
                if (_stackBadgePunchRoutine != null) StopCoroutine(_stackBadgePunchRoutine);
                _stackBadgePunchRoutine = StartCoroutine(RoutinePunchBadge());
            }
        }

        private void EnsureStackBadgeUI()
        {
            if (_stackBadgeRoot != null && _stackBadgeText != null) return;

            var badgeFind = transform.Find("RelicStackBadge");
            if (badgeFind != null)
            {
                _stackBadgeRoot = badgeFind.gameObject;
                _stackBadgeBg = badgeFind.GetComponent<Image>();
                _stackBadgeText = badgeFind.GetComponentInChildren<TextMeshProUGUI>();
            }
            else
            {
                _stackBadgeRoot = new GameObject("RelicStackBadge");
                _stackBadgeRoot.transform.SetParent(transform, false);
                _stackBadgeRoot.transform.SetAsLastSibling();

                var rt = _stackBadgeRoot.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, -4f);
                rt.sizeDelta = new Vector2(46f, 18f);

                _stackBadgeBg = _stackBadgeRoot.AddComponent<Image>();
                _stackBadgeBg.color = new Color(0.12f, 0.12f, 0.15f, 0.92f);
                _stackBadgeBg.raycastTarget = false;

                var textObj = new GameObject("Text_Count");
                textObj.transform.SetParent(_stackBadgeRoot.transform, false);
                var textRt = textObj.AddComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.sizeDelta = Vector2.zero;

                _stackBadgeText = textObj.AddComponent<TextMeshProUGUI>();
                _stackBadgeText.alignment = TextAlignmentOptions.Center;
                _stackBadgeText.fontSize = 12f;
                _stackBadgeText.fontStyle = FontStyles.Bold;
                _stackBadgeText.color = new Color(1f, 0.9f, 0.35f, 1f); // Màu Vàng Kim
                _stackBadgeText.raycastTarget = false;
            }
        }

        private System.Collections.IEnumerator RoutinePunchBadge()
        {
            if (_stackBadgeRoot == null) yield break;
            Transform t = _stackBadgeRoot.transform;
            float elapsed = 0f;
            float dur = 0.2f;
            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float scale = 1f + 0.35f * Mathf.Sin((elapsed / dur) * Mathf.PI);
                t.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            t.localScale = Vector3.one;
            _stackBadgePunchRoutine = null;
        }

        [SerializeField] private Image _recastGlowBorder;
        [SerializeField] private CanvasGroup _recastGlowCanvasGroup;
        private Coroutine _pulseRoutine;
        private static Sprite _cachedCircleGlowSprite;

        private static Sprite GetOrCreateCircleSprite()
        {
            if (_cachedCircleGlowSprite != null) return _cachedCircleGlowSprite;

            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.48f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    float a = Mathf.Clamp01((radius - d) / 1.5f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            _cachedCircleGlowSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return _cachedCircleGlowSprite;
        }

        private void OnDisable()
        {
            if (_pulseRoutine != null)
            {
                StopCoroutine(_pulseRoutine);
                _pulseRoutine = null;
            }
            if (_stackBadgePunchRoutine != null)
            {
                StopCoroutine(_stackBadgePunchRoutine);
                _stackBadgePunchRoutine = null;
            }
        }

        public void SetRecastGlow(bool isRecastActive)
        {
            if (_recastGlowBorder == null)
            {
                // Tự động tìm hoặc tạo một image viền sáng hình tròn xung quanh nút
                var glowObj = transform.Find("RecastGlowBorder");
                if (glowObj != null)
                {
                    _recastGlowBorder = glowObj.GetComponent<Image>();
                    _recastGlowCanvasGroup = glowObj.GetComponent<CanvasGroup>();
                }
                else
                {
                    var newGlow = new GameObject("RecastGlowBorder");
                    newGlow.transform.SetParent(transform, false);
                    newGlow.transform.SetAsFirstSibling();
                    _recastGlowBorder = newGlow.AddComponent<Image>();
                    _recastGlowCanvasGroup = newGlow.AddComponent<CanvasGroup>();
                    _recastGlowBorder.color = new Color(1f, 0.85f, 0.2f, 0.85f); // Màu Vàng Kim phát sáng
                    _recastGlowBorder.raycastTarget = false;

                    var rt = _recastGlowBorder.rectTransform;
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.sizeDelta = new Vector2(14f, 14f); // Nới rộng hơn nút bấm 14px
                }
            }

            if (_recastGlowBorder != null)
            {
                if (_recastGlowBorder.sprite == null)
                {
                    if (_cooldownRadialFill != null && _cooldownRadialFill.sprite != null)
                        _recastGlowBorder.sprite = _cooldownRadialFill.sprite;
                    else if (_relicButton != null && _relicButton.image != null && _relicButton.image.sprite != null)
                        _recastGlowBorder.sprite = _relicButton.image.sprite;
                    else
                        _recastGlowBorder.sprite = GetOrCreateCircleSprite();
                }

                _recastGlowBorder.raycastTarget = false;
            }

            if (_recastGlowCanvasGroup == null && _recastGlowBorder != null)
            {
                _recastGlowCanvasGroup = _recastGlowBorder.GetComponent<CanvasGroup>();
                if (_recastGlowCanvasGroup == null) _recastGlowCanvasGroup = _recastGlowBorder.gameObject.AddComponent<CanvasGroup>();
            }

            if (_recastGlowBorder != null)
            {
                _recastGlowBorder.gameObject.SetActive(isRecastActive);
                if (isRecastActive && isActiveAndEnabled)
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
                if (_recastGlowCanvasGroup != null)
                {
                    _recastGlowCanvasGroup.alpha = alpha; // Không kích hoạt Canvas Mesh Dirty Rebuild
                }
                else if (_recastGlowBorder != null)
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
