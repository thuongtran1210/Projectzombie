using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>
    /// Passive View cập nhật các thông số trận đấu trên HUD theo mô hình MVP.
    /// View chỉ nhận string và float đã định dạng từ Presenter, không tự đọc Model.
    /// </summary>
    public class DefenseMatchHUDView : MonoBehaviour
    {
        [Header("Thanh Thời Gian & Tiến Trình")]
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _eventStatusText;

        [Header("Căn Cứ Nhà Chính")]
        [SerializeField] private Slider _baseHealthSlider;
        [SerializeField] private TextMeshProUGUI _baseHealthText;

        [Header("Tướng")]
        [SerializeField] private Slider _heroHealthSlider;
        [SerializeField] private TextMeshProUGUI _heroHealthText;

        [Header("Tài Nguyên & Cấp Độ")]
        [SerializeField] private TextMeshProUGUI _woodCountText;
        [SerializeField] private Slider _expSlider;
        [SerializeField] private TextMeshProUGUI _expLevelText;

        [Header("Boss Dạ Xoa")]
        [SerializeField] private GameObject _bossHealthRoot;
        [SerializeField] private Slider _bossHealthSlider;
        [SerializeField] private TextMeshProUGUI _bossHealthText;

        public void SetMatchTime(string formattedTime)
        {
            if (_timerText != null) _timerText.text = formattedTime;
        }

        public void SetEventStatus(string statusText)
        {
            if (_eventStatusText != null) _eventStatusText.text = statusText;
        }

        public void SetBaseHealth(float current, float max, string formattedText)
        {
            if (_baseHealthSlider != null)
            {
                _baseHealthSlider.maxValue = Mathf.Max(1f, max);
                _baseHealthSlider.value = Mathf.Clamp(current, 0f, max);
            }
            if (_baseHealthText != null) _baseHealthText.text = formattedText;
        }

        public void SetHeroHealth(float current, float max, string formattedText)
        {
            if (_heroHealthSlider != null)
            {
                _heroHealthSlider.maxValue = Mathf.Max(1f, max);
                _heroHealthSlider.value = Mathf.Clamp(current, 0f, max);
            }
            if (_heroHealthText != null) _heroHealthText.text = formattedText;
        }

        public void SetWoodCount(string formattedWood)
        {
            if (_woodCountText != null) _woodCountText.text = formattedWood;
        }

        public void SetExperience(float current, float max, string formattedExp)
        {
            if (_expSlider != null)
            {
                _expSlider.maxValue = Mathf.Max(1f, max);
                _expSlider.value = Mathf.Clamp(current, 0f, max);
            }
            if (_expLevelText != null) _expLevelText.text = formattedExp;
        }

        public void SetBossHealth(bool visible, float current, float max, string formattedText)
        {
            if (_bossHealthRoot != null) _bossHealthRoot.SetActive(visible);
            if (!visible) return;

            if (_bossHealthSlider != null)
            {
                _bossHealthSlider.maxValue = Mathf.Max(1f, max);
                _bossHealthSlider.value = Mathf.Clamp(current, 0f, max);
            }
            if (_bossHealthText != null) _bossHealthText.text = formattedText;
        }

        [Header("Tương Tác Xây Tháp (Mobile & PC)")]
        [Tooltip("Root GameObject chứa cụm nút bấm tương tác Xây / Nâng cấp tháp")]
        [SerializeField] private GameObject _buildPromptRoot;
        [Tooltip("Nút bấm cảm ứng để Xây hoặc Nâng cấp tháp trên di động")]
        [SerializeField] private Button _buildButton;
        [Tooltip("Nhãn chữ hiển thị chi phí và hành vi: 'Xây Tháp (50 Gỗ)' hoặc 'Nâng Cấp (40 Gỗ)'")]
        [SerializeField] private TextMeshProUGUI _buildActionText;

        private System.Action _currentBuildAction;

        private void Awake()
        {
            if (_buildButton != null)
            {
                _buildButton.onClick.AddListener(() => _currentBuildAction?.Invoke());
            }
            if (_buildPromptRoot != null)
            {
                _buildPromptRoot.SetActive(false);
            }
        }

        public void SetBuildPrompt(bool visible, string actionText = null, System.Action onAction = null)
        {
            _currentBuildAction = onAction;
            if (_buildPromptRoot != null)
            {
                _buildPromptRoot.SetActive(visible);
            }
            if (_buildActionText != null && !string.IsNullOrEmpty(actionText))
            {
                _buildActionText.text = actionText;
            }
            if (_buildButton != null)
            {
                _buildButton.interactable = onAction != null;
            }
        }
    }
}
