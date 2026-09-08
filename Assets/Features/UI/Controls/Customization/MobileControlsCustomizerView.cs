using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectZombie.Features.UI.Controls.Customization
{
    /// <summary>
    /// Passive View quản lý hiển thị Overlay Tùy Chỉnh Phím Ảo (Mobile Controls Customizer).
    /// Tuân thủ nghiêm ngặt MVP: Không chứa logic nghiệp vụ, chỉ bắt event bấm nút/kéo slider và hiển thị text đã format.
    /// </summary>
    public class MobileControlsCustomizerView : MonoBehaviour
    {
        [Header("Header Bar Info")]
        [SerializeField] private TextMeshProUGUI _selectedControlTitleText;

        [Header("Sliders & Percent Texts")]
        [SerializeField] private Slider _scaleSlider;
        [SerializeField] private TextMeshProUGUI _scaleValueText;
        [SerializeField] private Slider _opacitySlider;
        [SerializeField] private TextMeshProUGUI _opacityValueText;

        [Header("Action Buttons")]
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _resetDefaultButton;
        [SerializeField] private Button _cancelButton;

        [Header("Root Panel")]
        [SerializeField] private GameObject _panelRoot;

        public event Action<float> OnScaleChanged;
        public event Action<float> OnOpacityChanged;
        public event Action OnSaveClicked;
        public event Action OnResetDefaultClicked;
        public event Action OnCancelClicked;

        private void Awake()
        {
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }

            EnsureSetupListeners();
        }

        public void EnsureSetupListeners()
        {
            SetupListeners();
        }

        private void SetupListeners()
        {
            if (_scaleSlider != null)
            {
                _scaleSlider.onValueChanged.RemoveAllListeners();
                _scaleSlider.onValueChanged.AddListener(val => OnScaleChanged?.Invoke(val));
            }

            if (_opacitySlider != null)
            {
                _opacitySlider.onValueChanged.RemoveAllListeners();
                _opacitySlider.onValueChanged.AddListener(val => OnOpacityChanged?.Invoke(val));
            }

            if (_saveButton != null)
            {
                _saveButton.onClick.RemoveAllListeners();
                _saveButton.onClick.AddListener(() => OnSaveClicked?.Invoke());
            }

            if (_resetDefaultButton != null)
            {
                _resetDefaultButton.onClick.RemoveAllListeners();
                _resetDefaultButton.onClick.AddListener(() => OnResetDefaultClicked?.Invoke());
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveAllListeners();
                _cancelButton.onClick.AddListener(() => OnCancelClicked?.Invoke());
            }
        }

        public void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(isVisible);
            }
        }

        public void SetSelectedControlTitle(string title)
        {
            if (_selectedControlTitleText != null)
            {
                _selectedControlTitleText.text = title;
            }
        }

        public void SetScaleSliderValue(float value, string formattedText)
        {
            if (_scaleSlider != null)
            {
                _scaleSlider.SetValueWithoutNotify(value);
            }
            if (_scaleValueText != null)
            {
                _scaleValueText.text = formattedText;
            }
        }

        public void SetOpacitySliderValue(float value, string formattedText)
        {
            if (_opacitySlider != null)
            {
                _opacitySlider.SetValueWithoutNotify(value);
            }
            if (_opacityValueText != null)
            {
                _opacityValueText.text = formattedText;
            }
        }

        public void SetSlidersInteractable(bool isInteractable)
        {
            if (_scaleSlider != null) _scaleSlider.interactable = isInteractable;
            if (_opacitySlider != null) _opacitySlider.interactable = isInteractable;
        }
    }
}
