using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// View hiển thị thụ động cho một thẻ nâng cấp (Upgrade Card).
    /// </summary>
    public class UpgradeCardView : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _categoryText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Button _selectButton;

        private Action _onClicked;

        private void Awake()
        {
            if (_selectButton != null)
            {
                _selectButton.onClick.AddListener(OnButtonClicked);
            }

            // Đảm bảo Animator chạy bình thường ngay cả khi Time.timeScale = 0
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }
        }

        /// <summary>
        /// Thiết lập hiển thị của thẻ nâng cấp.
        /// </summary>
        public void Setup(Sprite icon, string cardName, string description, string category, string level, Action onClicked)
        {
            _onClicked = onClicked;

            if (_iconImage == null || _nameText == null || _descriptionText == null || _categoryText == null || _levelText == null)
            {
                Debug.LogWarning($"[{nameof(UpgradeCardView)}] Một hoặc nhiều component UI chưa được gán trong Inspector.");
                return;
            }

            _iconImage.sprite = icon;
            _nameText.text = cardName;
            _descriptionText.text = description;
            _categoryText.text = category;
            _levelText.text = level;
        }

        private void OnButtonClicked()
        {
            _onClicked?.Invoke();
        }
    }
}
