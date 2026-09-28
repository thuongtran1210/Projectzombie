using UnityEngine;
using Core.Audio;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Presenter điều phối toàn bộ logic của Modal Cài Đặt (Settings Modal).
    /// Tích hợp trực tiếp với AudioManager và PlayerPrefs.
    /// </summary>
    public class SettingsModalPresenter : MonoBehaviour, ProjectZombie.Core.Audio.IAudioServiceConsumer
    {
        private ProjectZombie.Core.Audio.IAudioService _audioService;

        public void InjectAudioService(ProjectZombie.Core.Audio.IAudioService audioService)
        {
            _audioService = audioService;
            ApplySavedAudioSettings();
        }

        public static SettingsModalPresenter Instance { get; private set; }

        [SerializeField] private SettingsModalView _view;
        [SerializeField] private GameObject _cachedMobileControlsPanel;

        public const string PREF_SCREEN_SHAKE = "Setting_ScreenShake";
        public const string PREF_DAMAGE_NUMBERS = "Setting_DamageNumbers";
        public const string PREF_TARGET_60FPS = "Setting_Target60FPS";

        public static bool IsScreenShakeEnabled => PlayerPrefs.GetInt(PREF_SCREEN_SHAKE, 1) == 1;
        public static bool IsDamageNumbersEnabled => PlayerPrefs.GetInt(PREF_DAMAGE_NUMBERS, 1) == 1;
        public static bool Is60FPSEnabled => PlayerPrefs.GetInt(PREF_TARGET_60FPS, 1) == 1;

        public static void ApplyGlobalSettingsOnBoot()
        {
            bool fps60 = PlayerPrefs.GetInt(PREF_TARGET_60FPS, 1) == 1;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = fps60 ? 60 : 30;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Input.multiTouchEnabled = true;

        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            ApplyGlobalSettingsOnBoot();
            EnsureViewAndEvents();
        }

        private void ApplySavedAudioSettings()
        {
            if (_audioService == null) return;
            _audioService.SetBGMVolume(PlayerPrefs.GetFloat("Setting_BGMVolume", 0.4f), false);
            _audioService.SetSFXVolume(PlayerPrefs.GetFloat("Setting_SFXVolume", 0.9f), false);
        }

        private void OnEnable()
        {
            EnsureViewAndEvents();
        }

        private void EnsureViewAndEvents()
        {
            if (_view == null)
            {
                _view = GetComponent<SettingsModalView>();
                if (_view == null) _view = GetComponentInChildren<SettingsModalView>(true);
            }

            if (_view != null)
            {
                _view.OnBGMVolumeChanged -= HandleBGMVolumeChanged;
                _view.OnSFXVolumeChanged -= HandleSFXVolumeChanged;
                _view.OnScreenShakeToggled -= HandleScreenShakeToggled;
                _view.OnDamageNumbersToggled -= HandleDamageNumbersToggled;
                _view.On60FPSToggled -= Handle60FPSToggled;
                _view.OnCustomizeControlsClicked -= HandleCustomizeControlsClicked;
                _view.OnCloseClicked -= HandleCloseClicked;

                _view.OnBGMVolumeChanged += HandleBGMVolumeChanged;
                _view.OnSFXVolumeChanged += HandleSFXVolumeChanged;
                _view.OnScreenShakeToggled += HandleScreenShakeToggled;
                _view.OnDamageNumbersToggled += HandleDamageNumbersToggled;
                _view.On60FPSToggled += Handle60FPSToggled;
                _view.OnCustomizeControlsClicked += HandleCustomizeControlsClicked;
                _view.OnCloseClicked += HandleCloseClicked;
            }
        }

        private bool _isOpen;

        private void Start()
        {
            EnsureViewAndEvents();
            LoadAndApplyInitialSettings();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;

            if (_view != null)
            {
                _view.OnBGMVolumeChanged -= HandleBGMVolumeChanged;
                _view.OnSFXVolumeChanged -= HandleSFXVolumeChanged;
                _view.OnScreenShakeToggled -= HandleScreenShakeToggled;
                _view.OnDamageNumbersToggled -= HandleDamageNumbersToggled;
                _view.On60FPSToggled -= Handle60FPSToggled;
                _view.OnCustomizeControlsClicked -= HandleCustomizeControlsClicked;
                _view.OnCloseClicked -= HandleCloseClicked;
            }
        }

        public void Open()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            EnsureViewAndEvents();
            LoadAndApplyInitialSettings();
            if (_view == null)
            {
                _view = GetComponent<SettingsModalView>();
                if (_view == null) _view = GetComponentInChildren<SettingsModalView>(true);
            }
            if (_view != null)
            {
                _view.Show();
            }
        }

        public void Close()
        {
            if (_view != null)
            {
                _view.Hide();
            }
            else
            {
                gameObject.SetActive(false);
            }

            // Nếu đang trong trận: Khôi phục lại cụm điều khiển Mobile Controls nếu GameState là Playing
            if (GameStateManager.Instance == null || GameStateManager.Instance.CurrentState == GameState.Playing)
            {
                if (GameplayUIManager.Instance != null)
                {
                    GameplayUIManager.Instance.SetMobileControlsActive(true);
                }
            }
        }

        private void LoadAndApplyInitialSettings()
        {
            if (_view == null) return;

            float bgm = _audioService != null ? _audioService.BGMVolume : PlayerPrefs.GetFloat("Setting_BGMVolume", 0.4f);
            float sfx = _audioService != null ? _audioService.SFXVolume : PlayerPrefs.GetFloat("Setting_SFXVolume", 0.9f);

            bool shake = IsScreenShakeEnabled;
            bool dmgNum = IsDamageNumbersEnabled;
            bool fps60 = Is60FPSEnabled;

            _view.InitializeSettings(bgm, sfx, shake, dmgNum, fps60);

            // Btn_CustomizeControls chỉ hiện khi đang trong trận đấu (có Player active và không ở MainMenu)
            bool isInMatch = PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null;
            if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.MainMenu)
            {
                isInMatch = false;
            }
            _view.SetCustomizeControlsVisible(isInMatch);

            if (_audioService != null)
            {
                _audioService.SetBGMVolume(bgm, false);
                _audioService.SetSFXVolume(sfx, false);
            }
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = fps60 ? 60 : 30;
        }

        private void HandleBGMVolumeChanged(float val)
        {
            PlayerPrefs.SetFloat("Setting_BGMVolume", val);
            PlayerPrefs.Save();
            if (_audioService != null)
            {
                _audioService.SetBGMVolume(val, false);
            }
        }

        private void HandleSFXVolumeChanged(float val)
        {
            PlayerPrefs.SetFloat("Setting_SFXVolume", val);
            PlayerPrefs.Save();
            if (_audioService != null)
            {
                _audioService.SetSFXVolume(val, false);
            }
        }

        private void HandleScreenShakeToggled(bool isOn)
        {
            PlayerPrefs.SetInt(PREF_SCREEN_SHAKE, isOn ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void HandleDamageNumbersToggled(bool isOn)
        {
            PlayerPrefs.SetInt(PREF_DAMAGE_NUMBERS, isOn ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void Handle60FPSToggled(bool isOn)
        {
            PlayerPrefs.SetInt(PREF_TARGET_60FPS, isOn ? 1 : 0);
            PlayerPrefs.Save();
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = isOn ? 60 : 30;
        }

        private void HandleCustomizeControlsClicked()
        {
            _audioService?.PlayUIClick();

            // Tìm hoặc mở Customizer Presenter
            var customizer = ProjectZombie.Features.UI.Controls.Customization.MobileControlsCustomizerPresenter.Instance;
            if (customizer == null)
            {
                var customizerPrefab = Resources.Load<GameObject>("UI/MobileControlsCustomizerUI");
                if (customizerPrefab == null)
                {
#if UNITY_EDITOR
                    customizerPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/UI/MobileControlsCustomizerUI.prefab");
#endif
                }

                if (customizerPrefab != null)
                {
                    var canvas = GetComponentInParent<Canvas>();
                    var inst = Instantiate(customizerPrefab, canvas != null ? canvas.transform : transform.root);
                    inst.name = "MobileControlsCustomizerUI";
                    customizer = inst.GetComponent<ProjectZombie.Features.UI.Controls.Customization.MobileControlsCustomizerPresenter>();
                    if (customizer is ProjectZombie.Core.Audio.IAudioServiceConsumer consumer)
                        consumer.InjectAudioService(_audioService);
                }
            }

            if (customizer != null)
            {
                // 1. Tạm ẩn Settings Modal
                Close();

                // 2. Tạm ẩn Bảng Thông Số Nhân Vật (Panel_PlayerStatsMenu) nếu đang mở
                var statsPresenter = ProjectZombie.Features.UI.StatsAndSkills.PlayerInfoUIPresenter.Instance;
                bool wasStatsActive = statsPresenter != null && statsPresenter.IsMenuOpen;
                if (wasStatsActive)
                {
                    statsPresenter.StatsMenuView?.gameObject.SetActive(false);
                }

                // 3. Đảm bảo Panel điều khiển (Panel_MobileControls) được kích hoạt để người chơi thấy các nút và kéo chỉnh
                if (GameplayUIManager.Instance != null)
                {
                    GameplayUIManager.Instance.SetMobileControlsActive(true);
                }
                else if (_cachedMobileControlsPanel != null)
                {
                    _cachedMobileControlsPanel.SetActive(true);
                }

                // 4. Mở Customizer Overlay
                customizer.OpenCustomizer(() => {
                    // Khi đóng Customizer, khôi phục lại Bảng Thông Số & Modal Settings
                    if (wasStatsActive && statsPresenter != null && statsPresenter.StatsMenuView != null)
                    {
                        statsPresenter.StatsMenuView.gameObject.SetActive(true);
                        statsPresenter.StatsMenuView.Show();
                    }
                    Open();
                });
            }
            else
            {
                Debug.LogWarning("[SettingsModalPresenter] MobileControlsCustomizerPresenter not found!");
            }
        }

        private void HandleCloseClicked()
        {
            _audioService?.PlayUIClick();
            var metaManager = MetaUIManager.Instance ?? GetComponentInParent<MetaUIManager>();
            if (metaManager != null && metaManager.IsInMetaMenu)
            {
                metaManager.PopScreen();
            }
            else
            {
                Close();
            }
        }
    }
}
