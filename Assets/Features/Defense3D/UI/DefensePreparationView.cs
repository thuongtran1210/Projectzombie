using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProjectZombie.Features.UI;
using ProjectZombie.Features.Defense3D.Loadout;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>Read-only review of the saved Defense loadout. Equipment is edited in CardCodex.</summary>
    public class DefensePreparationView : BaseMetaScreenView
    {
        public override MetaScreenType ScreenType => MetaScreenType.DefensePreparation;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _subtitleText;
        [SerializeField] private TextMeshProUGUI _validationMessageText;
        [SerializeField] private TextMeshProUGUI _heroNameText;
        [SerializeField] private TextMeshProUGUI _heroRoleText;
        [SerializeField] private Image _heroAvatarImage;
        [SerializeField] private Button _heroSlotButton;
        [SerializeField] private TextMeshProUGUI _relicNameText;
        [SerializeField] private TextMeshProUGUI _relicDescText;
        [SerializeField] private Image _relicIconImage;
        [SerializeField] private Button _relicSlotButton;
        [Header("Review rows; legacy Checkmarks fields now hold item icons")]
        [SerializeField] private Button[] _minionSlotButtons;
        [SerializeField] private Image[] _minionSlotCheckmarks;
        [SerializeField] private TextMeshProUGUI[] _minionSlotNameTexts;
        [SerializeField] private Button[] _towerSlotButtons;
        [SerializeField] private Image[] _towerSlotCheckmarks;
        [SerializeField] private TextMeshProUGUI[] _towerSlotNameTexts;
        [SerializeField] private TextMeshProUGUI _minionCountText;
        [SerializeField] private TextMeshProUGUI _towerCountText;
        [SerializeField] private Button _startBattleButton;
        [SerializeField] private TextMeshProUGUI _startBattleButtonText;
        [SerializeField] private Button _editLoadoutButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private GameObject _loadingOverlay;
        [SerializeField] private Slider _loadingProgressBar;
        [SerializeField] private TextMeshProUGUI _loadingStatusText;
        [Serializable] public struct ReviewIcon { public string Id; public Sprite Icon; }
        [SerializeField] private ReviewIcon[] _catalogIcons;

        private DefensePreparationPresenter _presenter;
        private bool _isStarting;
        protected override void Awake()
        {
            base.Awake();
            if (_startBattleButton != null) _startBattleButton.onClick.AddListener(OnStartBattleClicked);
            if (_editLoadoutButton != null) _editLoadoutButton.onClick.AddListener(OnEditLoadoutClicked);
            if (_closeButton != null) _closeButton.onClick.AddListener(OnCloseClicked);
            if (_heroSlotButton != null) _heroSlotButton.gameObject.SetActive(false);
            if (_relicSlotButton != null) _relicSlotButton.gameObject.SetActive(false);
            MakeReadOnly(_minionSlotButtons);
            MakeReadOnly(_towerSlotButtons);
        }
        private static void MakeReadOnly(Button[] buttons)
        {
            if (buttons == null) return;
            foreach (var button in buttons)
            {
                if (button == null) continue;
                button.transition = Selectable.Transition.None;
                button.interactable = false;
                if (button.targetGraphic != null) button.targetGraphic.raycastTarget = false;
            }
        }
        private void OnEnable()
        {
            ReleasePresenter();
            _presenter = new DefensePreparationPresenter();
            _presenter.OnValidationStateChanged += UpdateValidationDisplay;
            _presenter.OnSelectionChanged += HandleSelectionChanged;
            _isStarting = false;
            ShowLoadingOverlay(false, string.Empty);
            RefreshAllDisplays();
        }
        private void OnDestroy()
        {
            if (_startBattleButton != null) _startBattleButton.onClick.RemoveListener(OnStartBattleClicked);
            if (_editLoadoutButton != null) _editLoadoutButton.onClick.RemoveListener(OnEditLoadoutClicked);
            if (_closeButton != null) _closeButton.onClick.RemoveListener(OnCloseClicked);
            ReleasePresenter();
        }
        private void ReleasePresenter()
        {
            if (_presenter == null) return;
            _presenter.OnValidationStateChanged -= UpdateValidationDisplay;
            _presenter.OnSelectionChanged -= HandleSelectionChanged;
            _presenter.Dispose();
            _presenter = null;
        }
        private void HandleSelectionChanged(LoadoutSelection selection) => RefreshAllDisplays();
        private void OnEditLoadoutClicked()
        {
            if (!_isStarting) MetaUIManager.Instance?.OpenScreen(MetaScreenType.Codex);
        }
        public override void OnBackPressed() => OnCloseClicked();
        private void OnCloseClicked()
        {
            if (_isStarting) return;
            MetaSceneTransitionController.Instance?.CancelLoadoutReview();
            if (MetaUIManager.Instance != null) MetaUIManager.Instance.PopScreen();
            else gameObject.SetActive(false);
        }
        private async void OnStartBattleClicked()
        {
            if (_isStarting || _presenter == null || !_presenter.CanStartBattle) return;
            _isStarting = true;
            UpdateValidationDisplay(true, string.Empty);
            ShowLoadingOverlay(true, "Đang khởi tạo trận địa...");
            bool success = false;
            try
            {
                success = await _presenter.StartBattleAsync((progress, message) =>
                {
                    if (this != null) UpdateLoadingProgress(progress, message);
                });
            }
            catch (Exception ex) { Debug.LogException(ex); }
            if (this != null && !success)
            {
                _isStarting = false;
                ShowLoadingOverlay(false, string.Empty);
                RefreshAllDisplays();
                UpdateValidationDisplay(_presenter.CanStartBattle, "Không thể tải trận. Vui lòng thử lại.");
                if (_validationMessageText != null) _validationMessageText.color = new Color(.94f,.65f,.56f);
            }
        }
        private Sprite GetIcon(string id)
        {
            if (_catalogIcons != null) foreach (var entry in _catalogIcons)
                if (string.Equals(entry.Id, id, StringComparison.OrdinalIgnoreCase)) return entry.Icon;
            return null;
        }
        public void RefreshAllDisplays()
        {
            if (_presenter == null) return;
            var selection = _presenter.Selection;
            var catalog = _presenter.Catalog;
            var rules = LoadoutRules.Default;
            var stage = MetaSceneTransitionController.Instance?.SelectedStage;
            if (_titleText != null) _titleText.text = "ĐỘI HÌNH XUẤT TRẬN";
            if (_subtitleText != null) _subtitleText.text = stage != null ? stage.stageName : "Kiểm tra đội hình trước khi bắt đầu.";
            SetHeroInfo(catalog.TryGetEntry(selection.HeroId, out var hero) ? hero.DisplayName : "Chưa chọn tướng",
                "Tướng điều khiển trong trận", GetIcon(selection.HeroId));
            SetRelicInfo(catalog.TryGetEntry(selection.RelicId, out var relic) ? relic.DisplayName : "Chưa chọn pháp bảo",
                "Pháp bảo đã trang bị cho tướng khi xuất trận.", GetIcon(selection.RelicId));
            if (_minionCountText != null) _minionCountText.text = $"{selection.MinionIds.Count}/{rules.MaxMinionCount} loại đã chọn";
            if (_towerCountText != null) _towerCountText.text = $"{selection.TowerIds.Count}/{rules.MaxTowerCount} loại đã chọn";
            BindRows(selection.MinionIds, rules.MaxMinionCount, _minionSlotButtons, _minionSlotCheckmarks, _minionSlotNameTexts);
            BindRows(selection.TowerIds, rules.MaxTowerCount, _towerSlotButtons, _towerSlotCheckmarks, _towerSlotNameTexts);
            UpdateValidationDisplay(_presenter.CanStartBattle, _presenter.LastValidationMessage);
        }
        private void BindRows(IReadOnlyList<string> ids, int capacity, Button[] buttons, Image[] icons, TextMeshProUGUI[] labels)
        {
            if (buttons == null) return;
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null) buttons[i].gameObject.SetActive(i < capacity);
                if (i >= capacity) continue;
                bool selected = i < ids.Count;
                string id = selected ? ids[i] : null;
                string name = selected && _presenter.Catalog.TryGetEntry(id, out var entry) ? entry.DisplayName : "Chưa chọn";
                SetRow(i, selected, name, selected ? GetIcon(id) : null, icons, labels);
            }
        }
        public void SetHeroInfo(string heroName, string roleDesc, Sprite avatar = null)
        {
            if (_heroNameText != null) _heroNameText.text = heroName;
            if (_heroRoleText != null) _heroRoleText.text = roleDesc;
            if (_heroAvatarImage != null) { _heroAvatarImage.sprite = avatar; _heroAvatarImage.enabled = avatar != null; }
        }
        public void SetRelicInfo(string relicName, string relicDesc, Sprite icon = null)
        {
            if (_relicNameText != null) _relicNameText.text = relicName;
            if (_relicDescText != null) _relicDescText.text = relicDesc;
            if (_relicIconImage != null) { _relicIconImage.sprite = icon; _relicIconImage.enabled = icon != null; }
        }
        public void SetMinionSlotState(int index, bool selected, string name) => SetRow(index, selected, name, null, _minionSlotCheckmarks, _minionSlotNameTexts);
        public void SetTowerSlotState(int index, bool selected, string name) => SetRow(index, selected, name, null, _towerSlotCheckmarks, _towerSlotNameTexts);
        private static void SetRow(int index, bool selected, string name, Sprite icon, Image[] icons, TextMeshProUGUI[] labels)
        {
            if (icons != null && index < icons.Length && icons[index] != null)
            {
                icons[index].sprite = icon;
                icons[index].gameObject.SetActive(selected && icon != null);
            }
            if (labels != null && index < labels.Length && labels[index] != null)
            {
                labels[index].text = name;
                labels[index].color = selected ? new Color(.91f,.87f,.77f) : new Color(.56f,.54f,.48f);
            }
        }
        public void UpdateValidationDisplay(bool canStart, string message)
        {
            if (_validationMessageText != null)
            {
                _validationMessageText.text = string.IsNullOrEmpty(message)
                    ? (canStart ? "Đội hình sẵn sàng xuất trận." : "Hãy hoàn thiện đội hình tại Bách Bảo Các.") : message;
                _validationMessageText.color = canStart ? new Color(.63f,.80f,.66f) : new Color(.94f,.65f,.56f);
            }
            if (_startBattleButton != null) _startBattleButton.interactable = canStart && !_isStarting;
            if (_editLoadoutButton != null) _editLoadoutButton.interactable = !_isStarting;
            if (_closeButton != null) _closeButton.interactable = !_isStarting;
            if (_startBattleButtonText != null) _startBattleButtonText.text = _isStarting ? "ĐANG TẢI..." : "XUẤT TRẬN";
        }
        public void ShowLoadingOverlay(bool show, string initialStatus)
        {
            if (_loadingOverlay != null) _loadingOverlay.SetActive(show);
            if (_loadingStatusText != null) _loadingStatusText.text = initialStatus;
            if (_loadingProgressBar != null) _loadingProgressBar.value = 0;
        }
        public void UpdateLoadingProgress(float progress, string status)
        {
            if (_loadingProgressBar != null) _loadingProgressBar.value = Mathf.Clamp01(progress);
            if (_loadingStatusText != null && !string.IsNullOrEmpty(status)) _loadingStatusText.text = status;
        }
    }
}
