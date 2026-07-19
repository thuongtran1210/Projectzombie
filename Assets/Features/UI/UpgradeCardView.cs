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
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI categoryText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Button selectButton;

        private Action _onClicked;

        private void Awake()
        {
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(OnButtonClicked);
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

            if (iconImage != null) iconImage.sprite = icon;
            if (nameText != null) nameText.text = cardName;
            if (descriptionText != null) descriptionText.text = description;
            if (categoryText != null) categoryText.text = category;
            if (levelText != null) levelText.text = level;
        }

        private void OnButtonClicked()
        {
            _onClicked?.Invoke();
        }
    }
}
