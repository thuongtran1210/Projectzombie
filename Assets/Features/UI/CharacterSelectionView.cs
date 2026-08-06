using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Passive View chọn nhân vật (Thư Sinh, Thanh Đồng, Ẩn Sĩ Sơn Lâm) tuân thủ mô hình MVP.
    /// </summary>
    public class CharacterSelectionView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _characterNameText;
        [SerializeField] private TextMeshProUGUI _elementText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _signatureSkillText;
        [SerializeField] private Image _characterAvatarImage;
        [SerializeField] private Button _selectButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _prevButton;

        public event Action OnSelectClicked;
        public event Action OnNextClicked;
        public event Action OnPrevClicked;

        private void Awake()
        {
            if (_selectButton != null) _selectButton.onClick.AddListener(() => OnSelectClicked?.Invoke());
            if (_nextButton != null) _nextButton.onClick.AddListener(() => OnNextClicked?.Invoke());
            if (_prevButton != null) _prevButton.onClick.AddListener(() => OnPrevClicked?.Invoke());
        }

        public void DisplayCharacter(string charName, string formattedElement, string description, string formattedSkill, Sprite avatar)
        {
            if (_characterNameText != null) _characterNameText.text = charName;
            if (_elementText != null) _elementText.text = formattedElement;
            if (_descriptionText != null) _descriptionText.text = description;
            if (_signatureSkillText != null) _signatureSkillText.text = formattedSkill;
            if (_characterAvatarImage != null && avatar != null) _characterAvatarImage.sprite = avatar;
        }
    }
}
