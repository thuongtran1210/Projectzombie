// ============================================================================
// FILE: CoopReviveWorldView.cs — TẦNG VIEW (MVP)
// Trách nhiệm DUY NHẤT: Hiển thị giao diện SOS và thanh tiến độ hồi sinh
// dạng World-Space nổi trên đầu nhân vật bị gục ngã (Downed).
// Tuân thủ triệt để Passive View, 0-GC và quy chuẩn Android 60 FPS.
// ============================================================================

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectZombie.Features.UI.HUD
{
    /// <summary>
    /// View thụ động (Passive View) cho World-Space Revive UI trên đầu Player.
    /// Không chứa bất kỳ logic tính toán mạng hay gameplay.
    /// </summary>
    public class CoopReviveWorldView : MonoBehaviour
    {
        [Header("Canvas & Container")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _badgeRoot;

        [Header("UI Elements")]
        [SerializeField] private Slider _reviveProgressBar;
        [SerializeField] private Image _progressBarFill;
        [SerializeField] private TextMeshProUGUI _statusLabel;
        [SerializeField] private TextMeshProUGUI _promptSubLabel;

        [Header("Colors (Cổ Phong Đông Sơn)")]
        [SerializeField] private Color _sosColor = new Color(1f, 0.25f, 0.25f, 1f);       // Đỏ chu sa cảnh báo
        [SerializeField] private Color _revivingColor = new Color(0f, 1f, 0.55f, 1f);      // Xanh ngọc hồi sinh
        [SerializeField] private Color _idlePromptColor = new Color(1f, 0.85f, 0.2f, 1f);   // Vàng hoàng kim

        private Transform _parentEntityTransform;

        private void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }

            _parentEntityTransform = transform.parent;
            EnsureDefaultHierarchy();
            SetVisible(false);
        }

        private void LateUpdate()
        {
            // Đảm bảo World-Space UI luôn giữ hướng thẳng, không bị lật ngược (flipped) khi nhân vật quay trái/phải
            if (_parentEntityTransform != null && _badgeRoot != null)
            {
                _badgeRoot.rotation = Quaternion.identity;
                float parentSignX = Mathf.Sign(_parentEntityTransform.lossyScale.x);
                Vector3 currentScale = _badgeRoot.localScale;
                if (Mathf.Sign(currentScale.x) * parentSignX < 0)
                {
                    _badgeRoot.localScale = new Vector3(-currentScale.x, currentScale.y, currentScale.z);
                }
            }
        }

        /// <summary>
        /// Bật/tắt hiển thị toàn bộ World-Space Revive UI (Tối ưu Canvas Group Alpha).
        /// </summary>
        public void SetVisible(bool isVisible)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = isVisible ? 1f : 0f;
                _canvasGroup.blocksRaycasts = false;
            }
            else
            {
                gameObject.SetActive(isVisible);
            }
        }

        /// <summary>
        /// Cập nhật nhãn trạng thái và gợi ý hành động.
        /// </summary>
        public void SetStatusText(string mainText, string subText = "")
        {
            if (_statusLabel != null)
            {
                _statusLabel.text = mainText;
            }

            if (_promptSubLabel != null)
            {
                if (string.IsNullOrEmpty(subText))
                {
                    _promptSubLabel.gameObject.SetActive(false);
                }
                else
                {
                    _promptSubLabel.gameObject.SetActive(true);
                    _promptSubLabel.text = subText;
                }
            }
        }

        /// <summary>
        /// Cập nhật thanh tiến độ nạp hồi sinh (0.0f -> 1.0f).
        /// </summary>
        public void SetProgress(float normalizedProgress, bool isReviving)
        {
            if (_reviveProgressBar != null)
            {
                _reviveProgressBar.value = Mathf.Clamp01(normalizedProgress);
                _reviveProgressBar.gameObject.SetActive(isReviving || normalizedProgress > 0f);
            }

            if (_progressBarFill != null)
            {
                _progressBarFill.color = isReviving ? _revivingColor : _sosColor;
            }
        }

        /// <summary>
        /// Tự sinh cấu trúc UI tối ưu nếu View được AddComponent tại runtime mà chưa gắn Prefab qua Inspector.
        /// </summary>
        private void EnsureDefaultHierarchy()
        {
            if (_badgeRoot == null)
            {
                var rootGo = new GameObject("ReviveBadgeRoot");
                rootGo.transform.SetParent(transform, false);
                _badgeRoot = rootGo.AddComponent<RectTransform>();
                _badgeRoot.localPosition = new Vector3(0f, 1.85f, 0f);
                _badgeRoot.sizeDelta = new Vector2(280f, 75f);
                _badgeRoot.localScale = Vector3.one * 0.012f;
            }

            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }

            var canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.sortingLayerName = "UI";
                canvas.sortingOrder = 120;
            }

            if (_statusLabel == null)
            {
                var labelGo = new GameObject("StatusLabel");
                labelGo.transform.SetParent(_badgeRoot, false);
                var rt = labelGo.AddComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0f, 16f);
                rt.sizeDelta = new Vector2(280f, 32f);

                _statusLabel = labelGo.AddComponent<TextMeshProUGUI>();
                _statusLabel.alignment = TextAlignmentOptions.Center;
                _statusLabel.fontSize = 24;
                _statusLabel.richText = true;
                _statusLabel.raycastTarget = false;
                _statusLabel.text = "<color=#FF4444><b>[CẦN CỨU VIỆN!]</b></color>";
            }

            if (_promptSubLabel == null)
            {
                var subGo = new GameObject("PromptSubLabel");
                subGo.transform.SetParent(_badgeRoot, false);
                var rt = subGo.AddComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0f, -8f);
                rt.sizeDelta = new Vector2(280f, 24f);

                _promptSubLabel = subGo.AddComponent<TextMeshProUGUI>();
                _promptSubLabel.alignment = TextAlignmentOptions.Center;
                _promptSubLabel.fontSize = 18;
                _promptSubLabel.richText = true;
                _promptSubLabel.raycastTarget = false;
                _promptSubLabel.text = "<color=#FFD700>Đứng gần để Cứu</color>";
            }

            if (_reviveProgressBar == null)
            {
                var sliderGo = new GameObject("ReviveSlider");
                sliderGo.transform.SetParent(_badgeRoot, false);
                var sliderRt = sliderGo.AddComponent<RectTransform>();
                sliderRt.anchoredPosition = new Vector2(0f, -22f);
                sliderRt.sizeDelta = new Vector2(220f, 10f);

                _reviveProgressBar = sliderGo.AddComponent<Slider>();
                _reviveProgressBar.minValue = 0f;
                _reviveProgressBar.maxValue = 1f;
                _reviveProgressBar.interactable = false;

                // Background
                var bgGo = new GameObject("Background");
                bgGo.transform.SetParent(sliderGo.transform, false);
                var bgRt = bgGo.AddComponent<RectTransform>();
                bgRt.anchorMin = Vector2.zero;
                bgRt.anchorMax = Vector2.one;
                bgRt.sizeDelta = Vector2.zero;
                var bgImg = bgGo.AddComponent<Image>();
                bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.75f);
                bgImg.raycastTarget = false;

                // Fill Area
                var fillAreaGo = new GameObject("FillArea");
                fillAreaGo.transform.SetParent(sliderGo.transform, false);
                var fillAreaRt = fillAreaGo.AddComponent<RectTransform>();
                fillAreaRt.anchorMin = Vector2.zero;
                fillAreaRt.anchorMax = Vector2.one;
                fillAreaRt.sizeDelta = Vector2.zero;

                // Fill Image
                var fillGo = new GameObject("Fill");
                fillGo.transform.SetParent(fillAreaGo.transform, false);
                var fillRt = fillGo.AddComponent<RectTransform>();
                fillRt.anchorMin = Vector2.zero;
                fillRt.anchorMax = Vector2.one;
                fillRt.sizeDelta = Vector2.zero;
                _progressBarFill = fillGo.AddComponent<Image>();
                _progressBarFill.color = _revivingColor;
                _progressBarFill.raycastTarget = false;

                _reviveProgressBar.fillRect = fillRt;
                _reviveProgressBar.gameObject.SetActive(false);
            }
        }
    }
}
