using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.UI.Controls.Customization
{
    /// <summary>
    /// Presenter điều phối toàn bộ luồng tùy chỉnh phím ảo giữa Model (LayoutManager) và View (CustomizerView).
    /// </summary>
    public class MobileControlsCustomizerPresenter : MonoBehaviour
    {
        [SerializeField] private MobileControlsCustomizerView _view;

        private CustomizableControlButton _currentSelectedControl;
        private readonly Dictionary<CustomizableControlButton, ControlButtonLayoutData> _backupBeforeEdit = new Dictionary<CustomizableControlButton, ControlButtonLayoutData>();
        private bool _isOpen = false;

        public bool IsOpen => _isOpen;

        private void Awake()
        {
            if (_view == null) _view = GetComponent<MobileControlsCustomizerView>();
        }

        private void Start()
        {
            if (_view != null)
            {
                _view.OnScaleChanged += HandleScaleChanged;
                _view.OnOpacityChanged += HandleOpacityChanged;
                _view.OnSaveClicked += HandleSaveClicked;
                _view.OnResetDefaultClicked += HandleResetDefaultClicked;
                _view.OnCancelClicked += HandleCancelClicked;
            }
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.OnScaleChanged -= HandleScaleChanged;
                _view.OnOpacityChanged -= HandleOpacityChanged;
                _view.OnSaveClicked -= HandleSaveClicked;
                _view.OnResetDefaultClicked -= HandleResetDefaultClicked;
                _view.OnCancelClicked -= HandleCancelClicked;
            }
        }

        private System.Action _onClosedCallback;
        public event System.Action OnCustomizerClosed;

        public void OpenCustomizer(System.Action onClosed = null)
        {
            if (_isOpen) return;
            _isOpen = true;
            _onClosedCallback = onClosed;

            if (MobileControlsLayoutManager.Instance != null)
            {
                MobileControlsLayoutManager.Instance.ScanAndRegisterControls();
            }

            // Backup trạng thái trước khi chỉnh sửa để có thể Hủy (Cancel)
            _backupBeforeEdit.Clear();
            var controls = MobileControlsLayoutManager.Instance != null 
                ? MobileControlsLayoutManager.Instance.RegisteredControls 
                : FindObjectsOfType<CustomizableControlButton>(true);

            foreach (var c in controls)
            {
                if (c == null) continue;
                _backupBeforeEdit[c] = c.ExportCurrentLayout();
                c.SetEditMode(true);
                c.OnSelectedInEditMode += HandleControlSelected;
            }

            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            if (_view == null) _view = GetComponent<MobileControlsCustomizerView>();
            if (_view == null) _view = GetComponentInChildren<MobileControlsCustomizerView>(true);

            if (_view != null)
            {
                _view.gameObject.SetActive(true);
                _view.EnsureSetupListeners();
                _view.SetVisible(true);
                _view.transform.SetAsLastSibling();
            }

            // Mặc định chọn nút đầu tiên nếu có
            if (controls.Count > 0)
            {
                SelectControl(controls[0]);
            }
            else
            {
                SelectControl(null);
            }
        }

        public void CloseCustomizer()
        {
            if (!_isOpen) return;
            _isOpen = false;

            var controls = MobileControlsLayoutManager.Instance != null 
                ? MobileControlsLayoutManager.Instance.RegisteredControls 
                : FindObjectsOfType<CustomizableControlButton>(true);

            foreach (var c in controls)
            {
                if (c == null) continue;
                c.SetEditMode(false);
                c.SetSelected(false);
                c.OnSelectedInEditMode -= HandleControlSelected;
            }

            _currentSelectedControl = null;

            if (_view != null)
            {
                _view.SetVisible(false);
            }

            var callback = _onClosedCallback;
            _onClosedCallback = null;
            callback?.Invoke();
            OnCustomizerClosed?.Invoke();
        }

        private void HandleControlSelected(CustomizableControlButton control)
        {
            SelectControl(control);
        }

        private void SelectControl(CustomizableControlButton control)
        {
            if (_currentSelectedControl != null)
            {
                _currentSelectedControl.SetSelected(false);
            }

            _currentSelectedControl = control;

            if (_currentSelectedControl != null)
            {
                _currentSelectedControl.SetSelected(true);
                _view.SetSelectedControlTitle($"Đang chọn: <color=#00FF88>{_currentSelectedControl.DisplayName}</color>");
                _view.SetSlidersInteractable(true);

                float scaleVal = _currentSelectedControl.CurrentScale.x;
                float opacityVal = _currentSelectedControl.CurrentOpacity;

                _view.SetScaleSliderValue(scaleVal, $"{Mathf.RoundToInt(scaleVal * 100f)}%");
                _view.SetOpacitySliderValue(opacityVal, $"{Mathf.RoundToInt(opacityVal * 100f)}%");
            }
            else
            {
                _view.SetSelectedControlTitle("Chạm vào một nút để chỉnh sửa");
                _view.SetSlidersInteractable(false);
                _view.SetScaleSliderValue(1.0f, "100%");
                _view.SetOpacitySliderValue(1.0f, "100%");
            }
        }

        private void HandleScaleChanged(float newScale)
        {
            if (_currentSelectedControl != null)
            {
                _currentSelectedControl.SetScale(newScale);
                _view.SetScaleSliderValue(newScale, $"{Mathf.RoundToInt(newScale * 100f)}%");
            }
        }

        private void HandleOpacityChanged(float newOpacity)
        {
            if (_currentSelectedControl != null)
            {
                _currentSelectedControl.SetOpacity(newOpacity);
                _view.SetOpacitySliderValue(newOpacity, $"{Mathf.RoundToInt(newOpacity * 100f)}%");
            }
        }

        private void HandleSaveClicked()
        {
            global::Core.Audio.AudioManager.Instance?.PlayUIClick();
            if (MobileControlsLayoutManager.Instance != null)
            {
                MobileControlsLayoutManager.Instance.SaveCurrentLayout();
            }
            CloseCustomizer();
        }

        private void HandleResetDefaultClicked()
        {
            global::Core.Audio.AudioManager.Instance?.PlayUIClick();
            if (MobileControlsLayoutManager.Instance != null)
            {
                MobileControlsLayoutManager.Instance.ResetToDefault();
            }
            if (_currentSelectedControl != null)
            {
                SelectControl(_currentSelectedControl);
            }
        }

        private void HandleCancelClicked()
        {
            global::Core.Audio.AudioManager.Instance?.PlayUIClick();
            // Khôi phục lại trạng thái backup trước khi mở edit
            foreach (var kvp in _backupBeforeEdit)
            {
                if (kvp.Key != null && kvp.Value != null)
                {
                    kvp.Key.ApplyLayout(kvp.Value);
                }
            }
            CloseCustomizer();
        }
    }
}
