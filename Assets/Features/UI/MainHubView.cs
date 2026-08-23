using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Passive View quản lý hiển thị Sảnh Hoàng Tuyền (Main Hub Panel).
    /// Tuân thủ mô hình MVP: Không chứa logic game, chỉ nhận dữ liệu đã format và phát event bấm nút.
    /// </summary>
    public class MainHubView : BaseMetaScreenView
    {
        public override MetaScreenType ScreenType => MetaScreenType.MainHub;

        [Header("Header Info")]
        [SerializeField] private TextMeshProUGUI _coTienText;
        [SerializeField] private Button _settingsButton;

        [Header("Main Action")]
        [SerializeField] private Button _startRunButton;

        [Header("Navigation Buttons")]
        [SerializeField] private Button _heroSelectButton;
        [SerializeField] private Button _sanctuaryTreeButton;
        [SerializeField] private Button _codexButton;

        [Header("Current Hero Preview")]
        [SerializeField] private TextMeshProUGUI _currentHeroNameText;
        [SerializeField] private Image _currentHeroAvatarImage;

        public event Action OnStartRunClicked;
        public event Action OnHeroSelectClicked;
        public event Action OnSanctuaryTreeClicked;
        public event Action OnCodexClicked;
        public event Action OnSettingsClicked;

        protected override void Awake()
        {
            base.Awake();

            if (_startRunButton != null) _startRunButton.onClick.AddListener(() => OnStartRunClicked?.Invoke());
            if (_heroSelectButton != null) _heroSelectButton.onClick.AddListener(() => OnHeroSelectClicked?.Invoke());
            if (_sanctuaryTreeButton != null) _sanctuaryTreeButton.onClick.AddListener(() => OnSanctuaryTreeClicked?.Invoke());
            if (_codexButton != null) _codexButton.onClick.AddListener(() => OnCodexClicked?.Invoke());
            if (_settingsButton != null) _settingsButton.onClick.AddListener(() => OnSettingsClicked?.Invoke());
        }

        public void SetCoTienBalance(string formattedText)
        {
            if (_coTienText != null)
            {
                _coTienText.text = formattedText;
            }
        }

        public void SetSelectedHeroPreview(string heroName, Sprite heroAvatar)
        {
            if (_currentHeroNameText != null)
            {
                _currentHeroNameText.text = heroName;
            }

            if (_currentHeroAvatarImage != null && heroAvatar != null)
            {
                _currentHeroAvatarImage.sprite = heroAvatar;
                _currentHeroAvatarImage.enabled = true;
            }
        }
    }
}
