using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectZombie.Features.UI.Controls
{
    /// <summary>
    /// Vùng Hủy Chiêu (Cancel Skill Zone [X]) ở đỉnh màn hình chuẩn MOBA Liên Quân.
    /// Tự động hiện lên khi người chơi Drag phím Skill và phát hiện ngón tay kéo vào vùng hủy.
    /// </summary>
    public class UICancelSkillZone : MonoBehaviour
    {
        public static UICancelSkillZone Instance { get; private set; }

        [Header("UI Components")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _cancelAreaRect;
        [SerializeField] private Image _backgroundGlow;
        [SerializeField] private TextMeshProUGUI _cancelText;

        [Header("Colors & Animation")]
        [SerializeField] private Color _normalGlowColor = new Color(0.8f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private Color _highlightGlowColor = new Color(1.0f, 0.1f, 0.1f, 0.95f);

        private bool _isHovered;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_cancelAreaRect == null) _cancelAreaRect = GetComponent<RectTransform>();

            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = visible ? 1f : 0f;
                _canvasGroup.blocksRaycasts = visible;
                _canvasGroup.interactable = visible;
            }
            else
            {
                gameObject.SetActive(visible);
            }

            if (!visible)
            {
                SetHovered(false);
            }
        }

        public void SetHovered(bool hovered)
        {
            _isHovered = hovered;
            if (_backgroundGlow != null)
            {
                _backgroundGlow.color = hovered ? _highlightGlowColor : _normalGlowColor;
            }

            if (_cancelText != null)
            {
                _cancelText.text = hovered ? "<color=#FF2222><b>THẢ ĐỂ HỦY</b></color>" : "Kéo vào đây để HỦY";
                _cancelText.transform.localScale = hovered ? Vector3.one * 1.15f : Vector3.one;
            }
        }

        /// <summary>
        /// Kiểm tra vị trí ngón tay chạm màn hình có nằm trong vùng Hủy Chiêu hay không.
        /// </summary>
        public bool IsPointerInsideCancelZone(Vector2 screenPosition, Camera uiCamera = null)
        {
            if (_cancelAreaRect == null) return false;
            return RectTransformUtility.RectangleContainsScreenPoint(_cancelAreaRect, screenPosition, uiCamera);
        }
    }
}
