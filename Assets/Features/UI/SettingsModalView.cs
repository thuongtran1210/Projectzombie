using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Passive View quản lý hiển thị Modal Cài Đặt (Settings Modal).
    /// Tuân thủ MVP: Kế thừa BaseMetaScreenView để tích hợp vào MetaUIManager navigation stack.
    /// </summary>
    public class SettingsModalView : BaseMetaScreenView
    {
        public override MetaScreenType ScreenType => MetaScreenType.Settings;

        [Header("Audio Controls")]
        [SerializeField] private Slider _bgmSlider;
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private TextMeshProUGUI _bgmValText;
        [SerializeField] private TextMeshProUGUI _sfxValText;

        [Header("Game Feel / Graphics Toggles")]
        [SerializeField] private Toggle _screenShakeToggle;
        [SerializeField] private Toggle _damageNumbersToggle;
        [SerializeField] private Toggle _fps60Toggle;

        [Header("Modal Action Buttons")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _overlayCloseButton;

        public event Action<float> OnBGMVolumeChanged;
        public event Action<float> OnSFXVolumeChanged;
        public event Action<bool> OnScreenShakeToggled;
        public event Action<bool> OnDamageNumbersToggled;
        public event Action<bool> On60FPSToggled;
        public event Action OnCloseClicked;

        protected override void Awake()
        {
            base.Awake();

            if (_bgmSlider != null)
            {
                _bgmSlider.onValueChanged.AddListener(val => {
                    UpdateBGMValueText(val);
                    OnBGMVolumeChanged?.Invoke(val);
                });
            }

            if (_sfxSlider != null)
            {
                _sfxSlider.onValueChanged.AddListener(val => {
                    UpdateSFXValueText(val);
                    OnSFXVolumeChanged?.Invoke(val);
                });
            }

            if (_screenShakeToggle != null)
            {
                _screenShakeToggle.onValueChanged.AddListener(isOn => OnScreenShakeToggled?.Invoke(isOn));
            }

            if (_damageNumbersToggle != null)
            {
                _damageNumbersToggle.onValueChanged.AddListener(isOn => OnDamageNumbersToggled?.Invoke(isOn));
            }

            if (_fps60Toggle != null)
            {
                _fps60Toggle.onValueChanged.AddListener(isOn => On60FPSToggled?.Invoke(isOn));
            }

            if (_closeButton != null) _closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
            if (_overlayCloseButton != null) _overlayCloseButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
        }

        public void InitializeSettings(float bgmVol, float sfxVol, bool screenShake, bool damageNumbers, bool fps60)
        {
            if (_bgmSlider != null)
            {
                _bgmSlider.value = bgmVol;
                UpdateBGMValueText(bgmVol);
            }

            if (_sfxSlider != null)
            {
                _sfxSlider.value = sfxVol;
                UpdateSFXValueText(sfxVol);
            }

            if (_screenShakeToggle != null) _screenShakeToggle.isOn = screenShake;
            if (_damageNumbersToggle != null) _damageNumbersToggle.isOn = damageNumbers;
            if (_fps60Toggle != null) _fps60Toggle.isOn = fps60;
        }

        private void UpdateBGMValueText(float val)
        {
            if (_bgmValText != null) _bgmValText.text = $"{Mathf.RoundToInt(val * 100)}%";
        }

        private void UpdateSFXValueText(float val)
        {
            if (_sfxValText != null) _sfxValText.text = $"{Mathf.RoundToInt(val * 100)}%";
        }

        public void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }
    }
}
