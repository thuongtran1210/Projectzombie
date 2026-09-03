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
        [SerializeField] private RawImage _characterPreviewRawImage;
        [SerializeField] private Button _selectButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _prevButton;
        [SerializeField] private Button _backButton;

        [Header("Loadout Equipment Slots (Action RPG v5.0)")]
        [SerializeField] private Image _primaryWeaponIcon;
        [SerializeField] private TextMeshProUGUI _primaryWeaponNameText;
        [SerializeField] private Image[] _relicSlotIcons;
        [SerializeField] private TextMeshProUGUI[] _relicSlotNames;

        [Header("Hero Quick Selection Tabs (4 Heroes)")]
        [SerializeField] private Button[] _heroTabButtons;
        [SerializeField] private Image[] _heroTabBorders;

        [Header("Hero Combat Stat Gauges")]
        [SerializeField] private Image _atkStatFill;
        [SerializeField] private Image _spdStatFill;
        [SerializeField] private Image _defStatFill;
        [SerializeField] private TextMeshProUGUI _atkStatText;
        [SerializeField] private TextMeshProUGUI _spdStatText;
        [SerializeField] private TextMeshProUGUI _defStatText;

        public event Action OnSelectClicked;
        public event Action OnNextClicked;
        public event Action OnPrevClicked;
        public event Action OnBackClicked;
        public event Action<int> OnHeroTabClicked;

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

            if (_heroTabButtons != null)
            {
                for (int i = 0; i < _heroTabButtons.Length; i++)
                {
                    int index = i;
                    if (_heroTabButtons[i] != null)
                    {
                        _heroTabButtons[i].onClick.AddListener(() => OnHeroTabClicked?.Invoke(index));
                    }
                }
            }
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
                else if (_backButton == null && (btnName.Contains("close") || btnName.Contains("back") || btnName.Contains("dong")))
                    _backButton = btn;
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
                else if (_signatureSkillText == null && (tName.Contains("signatureskill") || tName.Contains("kynangchudong") || tName.Contains("skill")))
                    _signatureSkillText = t;
                else if (_passiveTraitText == null && (tName.Contains("passivetrait") || tName.Contains("noitai") || tName.Contains("trait")))
                    _passiveTraitText = t;
                else if (_primaryWeaponNameText == null && (tName.Contains("primaryweapon") || tName.Contains("vukhichinh")))
                    _primaryWeaponNameText = t;
            }

            var images = GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                string imgName = img.gameObject.name.ToLower();
                if (_characterAvatarImage == null && imgName.Contains("avatar"))
                    _characterAvatarImage = img;
                else if (_primaryWeaponIcon == null && (imgName.Contains("primary") || imgName.Contains("vukhi")))
                    _primaryWeaponIcon = img;
            }
        }

        public void DisplayCharacter(string charName, string formattedElement, string description, string formattedSkill, string formattedPassive, Sprite avatar, Texture renderTexture = null)
        {
            if (_characterNameText != null) _characterNameText.text = charName;
            if (_elementText != null) _elementText.text = formattedElement;
            if (_descriptionText != null) _descriptionText.text = description;
            if (_signatureSkillText != null) _signatureSkillText.text = formattedSkill;
            if (_passiveTraitText != null) _passiveTraitText.text = formattedPassive;

            if (_characterPreviewRawImage != null)
            {
                if (renderTexture != null)
                {
                    _characterPreviewRawImage.texture = renderTexture;
                    _characterPreviewRawImage.enabled = true;
                    _characterPreviewRawImage.color = Color.white;
                }
                else
                {
                    _characterPreviewRawImage.enabled = false;
                }
            }

            if (_characterAvatarImage != null)
            {
                _characterAvatarImage.sprite = avatar;
                _characterAvatarImage.enabled = (avatar != null && renderTexture == null);
                _characterAvatarImage.color = (avatar != null) ? Color.white : new Color(1f, 1f, 1f, 0f);
            }
        }

        public void UpdateActiveTab(int selectedIndex)
        {
            if (_heroTabBorders == null) return;
            for (int i = 0; i < _heroTabBorders.Length; i++)
            {
                if (_heroTabBorders[i] == null) continue;
                bool isSelected = (i == selectedIndex);
                _heroTabBorders[i].color = isSelected ? new Color(1f, 0.85f, 0.2f, 1f) : new Color(0.4f, 0.35f, 0.3f, 0.6f);
                _heroTabBorders[i].transform.localScale = isSelected ? Vector3.one * 1.1f : Vector3.one;
            }
        }

        public void DisplayStats(float atkRatio, float spdRatio, float defRatio, string atkLabel = "", string spdLabel = "", string defLabel = "")
        {
            if (_atkStatFill != null) _atkStatFill.fillAmount = Mathf.Clamp01(atkRatio);
            if (_spdStatFill != null) _spdStatFill.fillAmount = Mathf.Clamp01(spdRatio);
            if (_defStatFill != null) _defStatFill.fillAmount = Mathf.Clamp01(defRatio);

            if (_atkStatText != null && !string.IsNullOrEmpty(atkLabel)) _atkStatText.text = atkLabel;
            if (_spdStatText != null && !string.IsNullOrEmpty(spdLabel)) _spdStatText.text = spdLabel;
            if (_defStatText != null && !string.IsNullOrEmpty(defLabel)) _defStatText.text = defLabel;
        }

        public void DisplayLoadout(Weapons.WeaponData primaryWeapon, System.Collections.Generic.List<Weapons.WeaponData> relics)
        {
            // 1. Hiển thị Vũ Khí Chính
            if (_primaryWeaponNameText != null)
            {
                _primaryWeaponNameText.text = primaryWeapon != null ? primaryWeapon.weaponName : "Chưa Trang Bị";
            }

            if (_primaryWeaponIcon != null)
            {
                if (primaryWeapon != null && primaryWeapon.icon != null)
                {
                    _primaryWeaponIcon.sprite = primaryWeapon.icon;
                    _primaryWeaponIcon.enabled = true;
                    _primaryWeaponIcon.color = Color.white;
                }
                else
                {
                    _primaryWeaponIcon.enabled = false;
                }
            }

            // 2. Hiển thị các Pháp Bảo Hộ Thân
            if (_relicSlotIcons != null)
            {
                for (int i = 0; i < _relicSlotIcons.Length; i++)
                {
                    if (_relicSlotIcons[i] == null) continue;

                    if (relics != null && i < relics.Count && relics[i] != null && relics[i].icon != null)
                    {
                        _relicSlotIcons[i].sprite = relics[i].icon;
                        _relicSlotIcons[i].enabled = true;
                        _relicSlotIcons[i].color = Color.white;
                    }
                    else
                    {
                        _relicSlotIcons[i].enabled = false;
                    }
                }
            }

            if (_relicSlotNames != null)
            {
                for (int i = 0; i < _relicSlotNames.Length; i++)
                {
                    if (_relicSlotNames[i] == null) continue;

                    if (relics != null && i < relics.Count && relics[i] != null)
                    {
                        _relicSlotNames[i].text = relics[i].weaponName;
                    }
                    else
                    {
                        _relicSlotNames[i].text = "Ô Trống";
                    }
                }
            }
        }
    }
}
