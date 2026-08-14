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
        [SerializeField] private TextMeshProUGUI _gaugeTitleText;

        private void Awake()
        {
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
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
        /// Cập nhật giá trị thanh Slider và Text tiêu đề trạng thái.
        /// </summary>
        public void UpdateGauge(float value, string titleText)
        {
            if (_gaugeSlider != null)
            {
                _gaugeSlider.value = value;
            }

            if (_gaugeTitleText != null)
            {
                _gaugeTitleText.text = titleText;
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
