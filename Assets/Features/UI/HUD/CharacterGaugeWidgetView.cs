using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectZombie.Features.UI.HUD
{
    /// <summary>
    /// View thụ động (Passive View) cho thanh cơ chế đặc thù nhân vật trên HUD.
    /// Tuân thủ MVP: Chỉ hiển thị dữ liệu do CharacterGaugeWidgetPresenter truyền vào.
    /// </summary>
    public class CharacterGaugeWidgetView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Slider _gaugeSlider;
        [SerializeField] private Image _gaugeFillImage;
        [SerializeField] private TextMeshProUGUI _gaugeTitleText;

        private void Awake()
        {
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }

            if (_gaugeSlider != null && _gaugeFillImage == null)
            {
                var fillRect = _gaugeSlider.fillRect;
                if (fillRect != null)
                {
                    _gaugeFillImage = fillRect.GetComponent<Image>();
                }
            }
        }

        /// <summary>
        /// Cấu hình dải giá trị Min/Max cho thanh Slider.
        /// </summary>
        public void Setup(float minValue, float maxValue)
        {
            if (_gaugeSlider != null)
            {
                _gaugeSlider.minValue = minValue;
                _gaugeSlider.maxValue = maxValue;
            }
        }

        /// <summary>
        /// Cập nhật giá trị thanh Slider, Text tiêu đề trạng thái và màu sắc Fill Bar.
        /// </summary>
        public void UpdateGauge(float value, string titleText, Color? fillColor = null)
        {
            if (_gaugeSlider != null)
            {
                _gaugeSlider.value = value;
            }

            if (_gaugeTitleText != null)
            {
                _gaugeTitleText.text = titleText;
            }

            if (_gaugeFillImage != null && fillColor.HasValue)
            {
                _gaugeFillImage.color = fillColor.Value;
            }
        }

        /// <summary>
        /// Bật/Tắt hiển thị toàn bộ widget thanh cơ chế.
        /// </summary>
        public void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }
    }
}
