using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Passive View quản lý hiển thị Sảnh Hoàng Tuyền (Main Hub Panel).
    /// Tuân thủ mô hình MVP: Tách biệt độc lập các khu vực Anh Hùng, Tàng Bảo Các, Miếu Cổ và Xuất Trận.
    /// </summary>
    public class MainHubView : BaseMetaScreenView
    {
        public override MetaScreenType ScreenType => MetaScreenType.MainHub;

        [Header("Header Info")]
        [SerializeField] private TextMeshProUGUI _coTienText;
        [SerializeField] private TextMeshProUGUI _linhHonText;
        [SerializeField] private Button _settingsButton;

        [Header("Main Action")]
        [SerializeField] private Button _startRunButton;

        [Header("Navigation Buttons")]
        [SerializeField] private Button _heroSelectButton;
        [SerializeField] private Button _armoryButton;
        [SerializeField] private Button _sanctuaryTreeButton;
        [SerializeField] private Button _codexButton;

        [Header("Current Hero Stage & Preview")]
        [SerializeField] private TextMeshProUGUI _currentHeroNameText;
        [SerializeField] private TextMeshProUGUI _currentHeroElementText;
        [SerializeField] private Image _currentHeroAvatarImage;

        [Header("Equipped Loadout Summary Card (Bottom-Left)")]
        [SerializeField] private Button _loadoutCardButton;
        [SerializeField] private TextMeshProUGUI _primaryWeaponNameText;
        [SerializeField] private Image _primaryWeaponIcon;
        [SerializeField] private Image[] _relicIcons;

        public event Action OnStartRunClicked;
        public event Action OnHeroSelectClicked;
        public event Action OnArmoryClicked;
        public event Action OnSanctuaryTreeClicked;
        public event Action OnCodexClicked;
        public event Action OnSettingsClicked;

        protected override void Awake()
        {
            base.Awake();

            if (_startRunButton != null) _startRunButton.onClick.AddListener(() => OnStartRunClicked?.Invoke());
            if (_heroSelectButton != null) _heroSelectButton.onClick.AddListener(() => OnHeroSelectClicked?.Invoke());
            if (_armoryButton != null) _armoryButton.onClick.AddListener(() => OnArmoryClicked?.Invoke());
            if (_loadoutCardButton != null) _loadoutCardButton.onClick.AddListener(() => OnArmoryClicked?.Invoke());
            if (_sanctuaryTreeButton != null) _sanctuaryTreeButton.onClick.AddListener(() => OnSanctuaryTreeClicked?.Invoke());
            if (_codexButton != null) _codexButton.onClick.AddListener(() => OnCodexClicked?.Invoke());
            if (_settingsButton != null) _settingsButton.onClick.AddListener(() => OnSettingsClicked?.Invoke());
        }

        public void SetCoTienBalance(string formattedText)
        {
            if (_coTienText != null) _coTienText.text = formattedText;
        }

        public void SetLinhHonBalance(string formattedText)
        {
            if (_linhHonText != null) _linhHonText.text = formattedText;
        }

        public void SetSelectedHeroPreview(string heroName, string elementText, Sprite heroAvatar)
        {
            if (_currentHeroNameText != null) _currentHeroNameText.text = heroName;
            if (_currentHeroElementText != null) _currentHeroElementText.text = elementText;

            if (_currentHeroAvatarImage != null)
            {
                _currentHeroAvatarImage.sprite = heroAvatar;
                _currentHeroAvatarImage.enabled = heroAvatar != null;
            }
        }

        public void SetEquippedLoadoutSummary(string primaryName, Sprite primaryIcon, List<Sprite> relicSprites)
        {
            if (_primaryWeaponNameText != null)
            {
                _primaryWeaponNameText.text = !string.IsNullOrEmpty(primaryName) ? primaryName : "Chưa Chọn";
            }

            if (_primaryWeaponIcon != null)
            {
                _primaryWeaponIcon.sprite = primaryIcon;
                _primaryWeaponIcon.enabled = primaryIcon != null;
            }

            if (_relicIcons != null)
            {
                for (int i = 0; i < _relicIcons.Length; i++)
                {
                    if (_relicIcons[i] == null) continue;
                    bool hasRelic = relicSprites != null && i < relicSprites.Count && relicSprites[i] != null;
                    _relicIcons[i].sprite = hasRelic ? relicSprites[i] : null;
                    _relicIcons[i].enabled = hasRelic;
                }
            }
        }
    }
}
