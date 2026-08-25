using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Passive View chọn nhân vật (Thư Sinh, Thanh Đồng, Ẩn Sĩ Sơn Lâm) tuân thủ mô hình MVP.
    /// </summary>
    public class CharacterSelectionView : BaseMetaScreenView
    {
        public override MetaScreenType ScreenType => MetaScreenType.CharacterSelect;

        [SerializeField] private TextMeshProUGUI _characterNameText;
        [SerializeField] private TextMeshProUGUI _elementText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _signatureSkillText;
        [SerializeField] private TextMeshProUGUI _passiveTraitText;
        [SerializeField] private Image _characterAvatarImage;
        [SerializeField] private Button _selectButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _prevButton;
        [SerializeField] private Button _backButton;

        public event Action OnSelectClicked;
        public event Action OnNextClicked;
        public event Action OnPrevClicked;
        public event Action OnBackClicked;

        protected override void Awake()
        {
            base.Awake();
            AutoWireComponentsIfMissing();

            if (_selectButton != null) _selectButton.onClick.AddListener(() => OnSelectClicked?.Invoke());
            if (_nextButton != null) _nextButton.onClick.AddListener(() => OnNextClicked?.Invoke());
            if (_prevButton != null) _prevButton.onClick.AddListener(() => OnPrevClicked?.Invoke());
            if (_backButton != null) _backButton.onClick.AddListener(() => {
                OnBackClicked?.Invoke();
                OnBackPressed();
            });
        }

        private void AutoWireComponentsIfMissing()
        {
            var buttons = GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                string btnName = btn.gameObject.name.ToLower();
                if (_selectButton == null && (btnName.Contains("select") || btnName.Contains("chon") || btnName.Contains("start")))
                    _selectButton = btn;
                else if (_nextButton == null && (btnName.Contains("next") || btnName.Contains("phai") || btnName.Contains("right")))
                    _nextButton = btn;
                else if (_prevButton == null && (btnName.Contains("prev") || btnName.Contains("trai") || btnName.Contains("left")))
                    _prevButton = btn;
            }

            var texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in texts)
            {
                string tName = t.gameObject.name.ToLower();
                if (_characterNameText == null && (tName.Contains("name") || tName.Contains("ten")))
                    _characterNameText = t;
                else if (_elementText == null && (tName.Contains("element") || tName.Contains("he")))
                    _elementText = t;
                else if (_descriptionText == null && (tName.Contains("desc") || tName.Contains("mota")))
                    _descriptionText = t;
                else if (_signatureSkillText == null && (tName.Contains("skill") || tName.Contains("kynang")))
                    _signatureSkillText = t;
                else if (_passiveTraitText == null && (tName.Contains("passive") || tName.Contains("bidong") || tName.Contains("trait")))
                    _passiveTraitText = t;
            }

            if (_characterAvatarImage == null)
            {
                var images = GetComponentsInChildren<Image>(true);
                foreach (var img in images)
                {
                    if (img.gameObject.name.ToLower().Contains("avatar"))
                    {
                        _characterAvatarImage = img;
                        break;
                    }
                }
            }
        }

        public void DisplayCharacter(string charName, string formattedElement, string description, string formattedSkill, string formattedPassive, Sprite avatar)
        {
            if (_characterNameText != null) _characterNameText.text = charName;
            if (_elementText != null) _elementText.text = formattedElement;
            if (_descriptionText != null) _descriptionText.text = description;
            if (_signatureSkillText != null) _signatureSkillText.text = formattedSkill;
            if (_passiveTraitText != null) _passiveTraitText.text = formattedPassive;

            if (_characterAvatarImage != null)
            {
                _characterAvatarImage.sprite = avatar;
                _characterAvatarImage.enabled = (avatar != null);
                _characterAvatarImage.color = (avatar != null) ? Color.white : new Color(1f, 1f, 1f, 0f);
            }
        }
    }
}
