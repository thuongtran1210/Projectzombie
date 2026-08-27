using UnityEngine;
using Core.Audio;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Presenter điều phối toàn bộ logic của Modal Cài Đặt (Settings Modal).
    /// Tích hợp trực tiếp với AudioManager và PlayerPrefs.
    /// </summary>
    public class SettingsModalPresenter : MonoBehaviour
    {
        [SerializeField] private SettingsModalView _view;

        private const string PREF_SCREEN_SHAKE = "Setting_ScreenShake";
        private const string PREF_DAMAGE_NUMBERS = "Setting_DamageNumbers";
        private const string PREF_TARGET_60FPS = "Setting_Target60FPS";

        private void Awake()
        {
            if (_view == null)
            {
                _view = GetComponent<SettingsModalView>();
            }

            if (_view != null)
            {
                _view.OnBGMVolumeChanged += HandleBGMVolumeChanged;
                _view.OnSFXVolumeChanged += HandleSFXVolumeChanged;
                _view.OnScreenShakeToggled += HandleScreenShakeToggled;
                _view.OnDamageNumbersToggled += HandleDamageNumbersToggled;
                _view.On60FPSToggled += Handle60FPSToggled;
                _view.OnCloseClicked += HandleCloseClicked;
            }
        }

        private void Start()
        {
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
                _view.OnCloseClicked -= HandleCloseClicked;
            }
        }

        public void Open()
        {
            LoadAndApplyInitialSettings();
            if (_view != null)
            {
                _view.SetVisible(true);
            }
        }

        public void Close()
        {
            if (_view != null)
            {
                _view.SetVisible(false);
            }
        }

        private void LoadAndApplyInitialSettings()
        {
            float bgm = AudioManager.Instance != null ? AudioManager.Instance.BGMVolume : PlayerPrefs.GetFloat("Setting_BGMVolume", 0.4f);
            float sfx = AudioManager.Instance != null ? AudioManager.Instance.SFXVolume : PlayerPrefs.GetFloat("Setting_SFXVolume", 0.9f);

            bool screenShake = PlayerPrefs.GetInt(PREF_SCREEN_SHAKE, 1) == 1;
            bool damageNumbers = PlayerPrefs.GetInt(PREF_DAMAGE_NUMBERS, 1) == 1;
            bool fps60 = PlayerPrefs.GetInt(PREF_TARGET_60FPS, 1) == 1;

            if (_view != null)
            {
                _view.InitializeSettings(bgm, sfx, screenShake, damageNumbers, fps60);
            }

            // Áp dụng FPS
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
            Application.targetFrameRate = isOn ? 60 : 30;
        }

        private void HandleCloseClicked()
        {
            var metaManager = MetaUIManager.Instance ?? GetComponentInParent<MetaUIManager>() ?? FindObjectOfType<MetaUIManager>(true);
            if (metaManager != null)
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
