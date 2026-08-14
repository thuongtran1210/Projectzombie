using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// View hiển thị thụ động cho một thẻ nâng cấp (Upgrade Card) theo mô hình MVP.
    /// </summary>
    public class UpgradeCardView : MonoBehaviour
    {
        [Header("Display Elements")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _categoryText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _statDiffText;
        [SerializeField] private TextMeshProUGUI _elementBadgeText;

        [Header("Buttons")]
        [SerializeField] private Button _selectButton;
        [SerializeField] private Button _banButton;

        private Action _onClicked;
        private Action _onBanClicked;

        private void Awake()
        {
            if (_selectButton != null)
            {
                _selectButton.onClick.AddListener(OnButtonClicked);
            }
            if (_banButton != null)
            {
                _banButton.onClick.AddListener(OnBanButtonClicked);
            }

            // Đảm bảo Animator chạy bình thường ngay cả khi Time.timeScale = 0 (khi pause chọn nâng cấp)
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }
        }

        /// <summary>
        /// Thiết lập hiển thị toàn diện của thẻ nâng cấp kèm chuỗi Stat Diff thay đổi chỉ số.
        /// </summary>
        public void Setup(
            Sprite icon,
            string cardName,
            string description,
            string category,
            string level,
            string statDiff,
            Action onClicked,
            Action onBanClicked = null)
        {
            _onClicked = onClicked;
            _onBanClicked = onBanClicked;

            if (_banButton != null)
            {
                _banButton.gameObject.SetActive(onBanClicked != null);
            }

            if (_iconImage != null) _iconImage.sprite = icon;
            if (_nameText != null) _nameText.text = cardName;
            if (_descriptionText != null) _descriptionText.text = description;
            if (_categoryText != null) _categoryText.text = category;
            if (_levelText != null) _levelText.text = level;

            SetStatDiff(statDiff);
        }

        /// <summary>
        /// Thiết lập hiển thị chuỗi so sánh chỉ số (Stat Diff) với TMP Rich Text.
        /// </summary>
        public void SetStatDiff(string statDiffFormattedText)
        {
            if (_statDiffText == null) return;

            if (string.IsNullOrEmpty(statDiffFormattedText))
            {
                _statDiffText.gameObject.SetActive(false);
            }
            else
            {
                _statDiffText.gameObject.SetActive(true);
                _statDiffText.text = statDiffFormattedText;
            }
        }

        /// <summary>
        /// Hiển thị thuộc tính Ngũ Hành trên thẻ nâng cấp với TMP Rich Text màu sắc.
        /// </summary>
        public void SetElementBadge(string badgeFormattedText)
        {
            if (_elementBadgeText == null) return;

            if (string.IsNullOrEmpty(badgeFormattedText))
            {
                _elementBadgeText.gameObject.SetActive(false);
            }
            else
            {
                _elementBadgeText.gameObject.SetActive(true);
                _elementBadgeText.text = badgeFormattedText;
            }
        }

        private void OnButtonClicked()
        {
            _onClicked?.Invoke();
        }

        private void OnBanButtonClicked()
        {
            _onBanClicked?.Invoke();
        }
    }
}
