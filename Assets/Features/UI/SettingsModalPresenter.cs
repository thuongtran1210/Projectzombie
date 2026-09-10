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
    public class SettingsModalPresenter : MonoBehaviour
    {
        [SerializeField] private SettingsModalView _view;

        public const string PREF_SCREEN_SHAKE = "Setting_ScreenShake";
        public const string PREF_DAMAGE_NUMBERS = "Setting_DamageNumbers";
        public const string PREF_TARGET_60FPS = "Setting_Target60FPS";

        public static bool IsScreenShakeEnabled => PlayerPrefs.GetInt(PREF_SCREEN_SHAKE, 1) == 1;
        public static bool IsDamageNumbersEnabled => PlayerPrefs.GetInt(PREF_DAMAGE_NUMBERS, 1) == 1;
        public static bool Is60FPSEnabled => PlayerPrefs.GetInt(PREF_TARGET_60FPS, 1) == 1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ApplyGlobalSettingsOnBoot()
        {
            bool fps60 = PlayerPrefs.GetInt(PREF_TARGET_60FPS, 1) == 1;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = fps60 ? 60 : 30;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Input.multiTouchEnabled = true;

            float bgm = PlayerPrefs.GetFloat("Setting_BGMVolume", 0.4f);
            float sfx = PlayerPrefs.GetFloat("Setting_SFXVolume", 0.9f);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetBGMVolume(bgm, false);
                AudioManager.Instance.SetSFXVolume(sfx, false);
            }
        }

        private void Awake()
        {
            EnsureViewAndEvents();
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
            float bgm = AudioManager.Instance != null ? AudioManager.Instance.BGMVolume : PlayerPrefs.GetFloat("Setting_BGMVolume", 0.4f);
            float sfx = AudioManager.Instance != null ? AudioManager.Instance.SFXVolume : PlayerPrefs.GetFloat("Setting_SFXVolume", 0.9f);

            bool screenShake = PlayerPrefs.GetInt(PREF_SCREEN_SHAKE, 1) == 1;
            bool damageNumbers = PlayerPrefs.GetInt(PREF_DAMAGE_NUMBERS, 1) == 1;
            bool fps60 = PlayerPrefs.GetInt(PREF_TARGET_60FPS, 1) == 1;

            // Btn_CustomizeControls chỉ hiện khi đang trong trận đấu (có Player active và không ở MainMenu)
            bool isInMatch = PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null;
            if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.MainMenu)
            {
                isInMatch = false;
            }

            if (_view != null)
            {
                _view.InitializeSettings(bgm, sfx, screenShake, damageNumbers, fps60);
                _view.SetCustomizeControlsVisible(isInMatch);
            }

            // Áp dụng FPS
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = fps60 ? 60 : 30;
        }

        private void HandleBGMVolumeChanged(float val)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetBGMVolume(val, true);
            }
            else
            {
                PlayerPrefs.SetFloat("Setting_BGMVolume", val);
                PlayerPrefs.Save();
            }
        }

        private void HandleSFXVolumeChanged(float val)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(val, true);
                AudioManager.Instance.SetUIVolume(val, true);
            }
            else
            {
                PlayerPrefs.SetFloat("Setting_SFXVolume", val);
                PlayerPrefs.Save();
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
            global::Core.Audio.AudioManager.Instance?.PlayUIClick();

            // Tìm hoặc mở Customizer Presenter
            var customizer = FindObjectOfType<ProjectZombie.Features.UI.Controls.Customization.MobileControlsCustomizerPresenter>(true);
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
                }
            }

            if (customizer != null)
            {
                // 1. Tạm ẩn Settings Modal
                Close();

                // 2. Tạm ẩn Bảng Thông Số Nhân Vật (Panel_PlayerStatsMenu) nếu đang mở
                var statsView = FindObjectOfType<ProjectZombie.Features.UI.StatsAndSkills.PlayerStatsMenuUIView>(true);
                bool wasStatsActive = statsView != null && statsView.gameObject.activeSelf;
                if (wasStatsActive)
                {
                    statsView.gameObject.SetActive(false);
                }

                // 3. Đảm bảo Panel điều khiển (Panel_MobileControls) được kích hoạt để người chơi thấy các nút và kéo chỉnh
                if (GameplayUIManager.Instance != null)
                {
                    GameplayUIManager.Instance.SetMobileControlsActive(true);
                }
                else
                {
                    var mobileControlsObj = GameObject.Find("Panel_MobileControls");
                    if (mobileControlsObj != null) mobileControlsObj.SetActive(true);
                }

                // 4. Mở Customizer Overlay
                customizer.OpenCustomizer(() => {
                    // Khi đóng Customizer, khôi phục lại Bảng Thông Số & Modal Settings
                    if (wasStatsActive && statsView != null)
                    {
                        statsView.gameObject.SetActive(true);
                        statsView.Show();
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
            global::Core.Audio.AudioManager.Instance?.PlayUIClick();
            var metaManager = MetaUIManager.Instance ?? GetComponentInParent<MetaUIManager>() ?? FindObjectOfType<MetaUIManager>(true);
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
