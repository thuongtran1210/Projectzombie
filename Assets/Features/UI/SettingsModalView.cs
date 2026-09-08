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

            if (_screenCanvasGroup != null)
            {
                _screenCanvasGroup.alpha = 0f;
                _screenCanvasGroup.interactable = false;
                _screenCanvasGroup.blocksRaycasts = false;
            }

            // Auto-detect missing references
            if (_bgmSlider == null || _sfxSlider == null)
            {
                foreach (var slider in GetComponentsInChildren<Slider>(true))
                {
                    string nameLower = slider.name.ToLower();
                    if (_bgmSlider == null && (nameLower.Contains("bgm") || nameLower.Contains("music") || nameLower.Contains("nhac")))
                        _bgmSlider = slider;
                    else if (_sfxSlider == null && (nameLower.Contains("sfx") || nameLower.Contains("sound") || nameLower.Contains("hieuung")))
                        _sfxSlider = slider;
                }
            }

            if (_screenShakeToggle == null || _damageNumbersToggle == null || _fps60Toggle == null)
            {
                foreach (var toggle in GetComponentsInChildren<Toggle>(true))
                {
                    string nameLower = toggle.name.ToLower();
                    if (_screenShakeToggle == null && (nameLower.Contains("shake") || nameLower.Contains("rung")))
                        _screenShakeToggle = toggle;
                    else if (_damageNumbersToggle == null && (nameLower.Contains("damage") || nameLower.Contains("satthuong")))
                        _damageNumbersToggle = toggle;
                    else if (_fps60Toggle == null && (nameLower.Contains("fps") || nameLower.Contains("60fps") || nameLower.Contains("muot")))
                        _fps60Toggle = toggle;
                }
            }

            if (_closeButton == null || _overlayCloseButton == null)
            {
                foreach (var btn in GetComponentsInChildren<Button>(true))
                {
                    string nameLower = btn.name.ToLower();
                    if (_closeButton == null && (nameLower.Contains("close") || nameLower.Contains("btn_close") || nameLower.Contains("dong")))
                        _closeButton = btn;
                    else if (_overlayCloseButton == null && (nameLower.Contains("overlay") || nameLower.Contains("dim") || nameLower.Contains("dark")))
                        _overlayCloseButton = btn;
                }
            }

            if (_bgmValText == null || _sfxValText == null)
            {
                if (_bgmSlider != null) _bgmValText = _bgmSlider.transform.parent != null ? _bgmSlider.transform.parent.GetComponentInChildren<TextMeshProUGUI>() : null;
                if (_sfxSlider != null) _sfxValText = _sfxSlider.transform.parent != null ? _sfxSlider.transform.parent.GetComponentInChildren<TextMeshProUGUI>() : null;
            }

            if (_bgmSlider != null)
            {
                _bgmSlider.onValueChanged.RemoveAllListeners();
                _bgmSlider.onValueChanged.AddListener(val => {
                    UpdateBGMValueText(val);
                    OnBGMVolumeChanged?.Invoke(val);
                });
            }

            if (_sfxSlider != null)
            {
                _sfxSlider.onValueChanged.RemoveAllListeners();
                _sfxSlider.onValueChanged.AddListener(val => {
                    UpdateSFXValueText(val);
                    OnSFXVolumeChanged?.Invoke(val);
                });
            }

            if (_screenShakeToggle != null)
            {
                _screenShakeToggle.onValueChanged.RemoveAllListeners();
                _screenShakeToggle.onValueChanged.AddListener(isOn => OnScreenShakeToggled?.Invoke(isOn));
            }

            if (_damageNumbersToggle != null)
            {
                _damageNumbersToggle.onValueChanged.RemoveAllListeners();
                _damageNumbersToggle.onValueChanged.AddListener(isOn => OnDamageNumbersToggled?.Invoke(isOn));
            }

            if (_fps60Toggle != null)
            {
                _fps60Toggle.onValueChanged.RemoveAllListeners();
                _fps60Toggle.onValueChanged.AddListener(isOn => On60FPSToggled?.Invoke(isOn));
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
            }

            if (_overlayCloseButton != null)
            {
                _overlayCloseButton.onClick.RemoveAllListeners();
                _overlayCloseButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
            }
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

        public override void Show()
        {
            if (_screenCanvasGroup == null) _screenCanvasGroup = GetComponent<CanvasGroup>();
            if (_modalContainer == null)
            {
                foreach (Transform child in transform)
                {
                    string cn = child.name.ToLower();
                    if (cn.Contains("frame") || cn.Contains("modal") || cn.Contains("content") || cn.Contains("container"))
                    {
                        _modalContainer = child.GetComponent<RectTransform>();
                        break;
                    }
                }
            }

            gameObject.SetActive(true);

            if (_screenCanvasGroup != null)
            {
                _screenCanvasGroup.alpha = 1f;
                _screenCanvasGroup.interactable = true;
                _screenCanvasGroup.blocksRaycasts = true;
            }

            if (_modalContainer != null)
            {
                _modalContainer.localScale = Vector3.one;
            }

            base.Show();
        }

        public void SetVisible(bool isVisible)
        {
            if (isVisible)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        public override void OnBackPressed()
        {
            OnCloseClicked?.Invoke();
        }
    }
}
