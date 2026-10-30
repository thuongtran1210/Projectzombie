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
        private DefenseHeroCatalogSO _heroCatalog;
        private bool _isStarting;

        protected override void Awake()
        {
            base.Awake();
            if (_startBattleButton != null) _startBattleButton.onClick.AddListener(OnStartBattleClicked);
            if (_editLoadoutButton != null) _editLoadoutButton.onClick.AddListener(OnEditLoadoutClicked);
            if (_closeButton != null) _closeButton.onClick.AddListener(OnCloseClicked);

            if (_heroSlotButton != null)
            {
                _heroSlotButton.gameObject.SetActive(true);
                _heroSlotButton.interactable = true;
                _heroSlotButton.onClick.AddListener(OnHeroSlotClicked);
            }

            if (_relicSlotButton != null)
            {
                _relicSlotButton.gameObject.SetActive(true);
                _relicSlotButton.interactable = true;
                _relicSlotButton.onClick.AddListener(OnRelicSlotClicked);
            }

            if (_minionSlotButtons != null)
            {
                for (int i = 0; i < _minionSlotButtons.Length; i++)
                {
                    int index = i;
                    if (_minionSlotButtons[i] != null)
                    {
                        _minionSlotButtons[i].interactable = true;
                        _minionSlotButtons[i].onClick.AddListener(() => OnMinionSlotClicked(index));
                    }
                }
            }

            if (_towerSlotButtons != null)
            {
                for (int i = 0; i < _towerSlotButtons.Length; i++)
                {
                    int index = i;
                    if (_towerSlotButtons[i] != null)
                    {
                        _towerSlotButtons[i].interactable = true;
                        _towerSlotButtons[i].onClick.AddListener(() => OnTowerSlotClicked(index));
                    }
                }
            }
        }

        private void OnEnable()
        {
            ReleasePresenter();
            _presenter = new DefensePreparationPresenter(stage: ProjectZombie.Core.Architecture.AppCompositionRoot.Instance?.Flow.SelectedStage);
            _presenter.OnValidationStateChanged += UpdateValidationDisplay;
            _presenter.OnSelectionChanged += HandleSelectionChanged;
            _isStarting = false;
            ShowLoadingOverlay(false, string.Empty);
            RefreshAllDisplays();
            ApplyLegendaryTheme();
        }

        private void ApplyLegendaryTheme()
        {
            LegendaryFantasyTheme.Label(_titleText, true);
            LegendaryFantasyTheme.Label(_subtitleText);
            LegendaryFantasyTheme.Label(_heroNameText, true);
            LegendaryFantasyTheme.Label(_relicNameText, true);
            LegendaryFantasyTheme.Label(_heroRoleText);
            LegendaryFantasyTheme.Label(_relicDescText);
            LegendaryFantasyTheme.Accent(_minionCountText);
            LegendaryFantasyTheme.Accent(_towerCountText);
            LegendaryFantasyTheme.Primary(_startBattleButton, _startBattleButtonText);
            LegendaryFantasyTheme.Secondary(_editLoadoutButton);
            LegendaryFantasyTheme.Secondary(_closeButton);
            LegendaryFantasyTheme.Experience(_loadingProgressBar);
        }

        private void OnDestroy()
        {
            if (_startBattleButton != null) _startBattleButton.onClick.RemoveListener(OnStartBattleClicked);
            if (_editLoadoutButton != null) _editLoadoutButton.onClick.RemoveListener(OnEditLoadoutClicked);
            if (_closeButton != null) _closeButton.onClick.RemoveListener(OnCloseClicked);
            if (_heroSlotButton != null) _heroSlotButton.onClick.RemoveListener(OnHeroSlotClicked);
            if (_relicSlotButton != null) _relicSlotButton.onClick.RemoveListener(OnRelicSlotClicked);
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

        private void HandleSelectionChanged(LoadoutSelection selection)
        {
            RefreshAllDisplays();
            MainHubPresenter.Instance?.RefreshHubState();
        }

        private void OnHeroSlotClicked()
        {
            if (_isStarting || _presenter == null) return;
            var heroes = _presenter.Catalog.GetEntriesByType(LoadoutItemType.Hero);
            if (heroes == null || heroes.Count == 0) return;

            int currentIndex = -1;
            for (int i = 0; i < heroes.Count; i++)
            {
                if (string.Equals(heroes[i].Id, _presenter.Selection.HeroId, StringComparison.OrdinalIgnoreCase))
                {
                    currentIndex = i;
                    break;
                }
            }

            for (int step = 1; step <= heroes.Count; step++)
            {
                int nextIndex = (currentIndex + step) % heroes.Count;
                var candidate = heroes[nextIndex];
                if (_presenter.Ownership.IsOwned(candidate.Id))
                {
                    _presenter.SelectHero(candidate.Id);
                    break;
                }
            }
        }

        private void OnRelicSlotClicked()
        {
            if (_isStarting || _presenter == null) return;
            var relics = _presenter.Catalog.GetEntriesByType(LoadoutItemType.Relic);
            if (relics == null || relics.Count == 0) return;

            int currentIndex = -1;
            for (int i = 0; i < relics.Count; i++)
            {
                if (string.Equals(relics[i].Id, _presenter.Selection.RelicId, StringComparison.OrdinalIgnoreCase))
                {
                    currentIndex = i;
                    break;
                }
            }

            for (int step = 1; step <= relics.Count; step++)
            {
                int nextIndex = (currentIndex + step) % relics.Count;
                var candidate = relics[nextIndex];
                if (_presenter.Ownership.IsOwned(candidate.Id))
                {
                    _presenter.SelectRelic(candidate.Id);
                    break;
                }
            }
        }

        private void OnMinionSlotClicked(int index)
        {
            if (_isStarting || _presenter == null) return;
            var minions = _presenter.Catalog.GetEntriesByType(LoadoutItemType.Minion);
            if (minions != null && index < minions.Count)
            {
                _presenter.ToggleMinion(minions[index].Id);
            }
        }

        private void OnTowerSlotClicked(int index)
        {
            if (_isStarting || _presenter == null) return;
            var towers = _presenter.Catalog.GetEntriesByType(LoadoutItemType.Tower);
            if (towers != null && index < towers.Count)
            {
                _presenter.ToggleTower(towers[index].Id);
            }
        }

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
            if (!_presenter.HasStageSelected)
            {
                _presenter.SaveSelection();
                MetaUIManager.Instance?.OpenScreen(MetaScreenType.StageSelect);
                return;
            }
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
            if (_catalogIcons != null)
            {
                foreach (var entry in _catalogIcons)
                {
                    if (string.Equals(entry.Id, id, StringComparison.OrdinalIgnoreCase)) return entry.Icon;
                }
            }
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
            if (_subtitleText != null) _subtitleText.text = stage != null ? stage.stageName : "Kiểm tra và tùy chỉnh đội hình trước khi bắt đầu.";

            if (_heroCatalog == null)
            {
                _heroCatalog = Resources.Load<DefenseHeroCatalogSO>("Defense3D/DefenseHeroCatalog");
            }

            int heroStar = DefenseLoadoutRuntimeAdapter.GetEffectiveHeroStarLevel(selection.HeroId);
            DefenseLoadoutRuntimeAdapter.EvaluateHeroStarBonuses(heroStar, null, out float heroHpBonus, out float heroSpdBonus);
            string heroName = catalog.TryGetEntry(selection.HeroId, out var hero) ? $"{hero.DisplayName} <color=#FFD700>[{heroStar}★]</color>" : "Chưa chọn tướng";
            string heroStatsDesc = "Tướng điều khiển trong trận";
            Sprite heroAvatar = GetIcon(selection.HeroId);

            if (_heroCatalog != null)
            {
                var heroDef = _heroCatalog.GetHeroById(selection.HeroId);
                if (heroDef != null)
                {
                    float effectiveHp = heroDef.BaseMaxHp * (1f + heroHpBonus);
                    float effectiveSpd = heroDef.BaseMoveSpeed * (1f + heroSpdBonus);
                    heroStatsDesc = $"<color=#00FF88>Máu:</color> <b>{effectiveHp:0}</b>  <color=#FFD700>Công:</color> <b>{heroDef.BaseDamage:0}</b>  <color=#00E5FF>Tốc:</color> <b>{effectiveSpd:0.0}</b>";
                    if (heroAvatar == null && heroDef.Avatar != null)
                    {
                        heroAvatar = heroDef.Avatar;
                    }
                }
            }

            SetHeroInfo(heroName, heroStatsDesc, heroAvatar);

            int relicStar = DefenseLoadoutRuntimeAdapter.GetEffectiveRelicStarLevel(selection.RelicId);
            DefenseLoadoutRuntimeAdapter.EvaluateRelicStarBonuses(relicStar, null, out float relicDmgBonus, out float relicCdBonus, out float relicAreaBonus);
            string relicName = catalog.TryGetEntry(selection.RelicId, out var relic) ? $"{relic.DisplayName} <color=#FFD700>[{relicStar}★]</color>" : "Chưa chọn pháp bảo";
            string relicDesc = relicStar > 1
                ? $"Cấp {relicStar}★: +{(int)(relicDmgBonus * 100)}% Công, -{(int)(relicCdBonus * 100)}% Hồi chiêu, +{(int)(relicAreaBonus * 100)}% Phạm vi."
                : "Pháp bảo hộ thân kích hoạt kỹ năng đặc trưng khi xuất trận.";
            SetRelicInfo(relicName, relicDesc, GetIcon(selection.RelicId));


            if (_minionCountText != null) _minionCountText.text = $"{selection.MinionIds.Count}/{rules.MaxMinionCount} loại đã chọn";
            if (_towerCountText != null) _towerCountText.text = $"{selection.TowerIds.Count}/{rules.MaxTowerCount} loại đã chọn";

            BindMinionRows(selection.MinionIds, rules.MaxMinionCount);
            BindTowerRows(selection.TowerIds, rules.MaxTowerCount);
            UpdateValidationDisplay(_presenter.CanStartBattle, _presenter.LastValidationMessage);
        }

        private void BindMinionRows(IReadOnlyList<string> selectedIds, int capacity)
        {
            var allMinions = _presenter.Catalog.GetEntriesByType(LoadoutItemType.Minion);
            if (_minionSlotButtons == null) return;
            for (int i = 0; i < _minionSlotButtons.Length; i++)
            {
                if (_minionSlotButtons[i] != null) _minionSlotButtons[i].gameObject.SetActive(i < capacity);
                if (i >= capacity || i >= allMinions.Count) continue;
                var minion = allMinions[i];
                bool isSelected = ContainsId(selectedIds, minion.Id);
                SetRow(i, isSelected, minion.DisplayName, _minionSlotCheckmarks, _minionSlotNameTexts);
            }
        }

        private void BindTowerRows(IReadOnlyList<string> selectedIds, int capacity)
        {
            var allTowers = _presenter.Catalog.GetEntriesByType(LoadoutItemType.Tower);
            if (_towerSlotButtons == null) return;
            for (int i = 0; i < _towerSlotButtons.Length; i++)
            {
                if (_towerSlotButtons[i] != null) _towerSlotButtons[i].gameObject.SetActive(i < capacity);
                if (i >= capacity || i >= allTowers.Count) continue;
                var tower = allTowers[i];
                bool isSelected = ContainsId(selectedIds, tower.Id);
                SetRow(i, isSelected, tower.DisplayName, _towerSlotCheckmarks, _towerSlotNameTexts);
            }
        }

        private static bool ContainsId(IReadOnlyList<string> list, string targetId)
        {
            if (list == null || string.IsNullOrEmpty(targetId)) return false;
            for (int i = 0; i < list.Count; i++)
            {
                if (string.Equals(list[i], targetId, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
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

        public void SetMinionSlotState(int index, bool selected, string name) => SetRow(index, selected, name, _minionSlotCheckmarks, _minionSlotNameTexts);
        public void SetTowerSlotState(int index, bool selected, string name) => SetRow(index, selected, name, _towerSlotCheckmarks, _towerSlotNameTexts);

        private static void SetRow(int index, bool selected, string name, Image[] icons, TextMeshProUGUI[] labels)
        {
            if (icons != null && index < icons.Length && icons[index] != null)
            {
                icons[index].gameObject.SetActive(selected);
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
            if (_startBattleButtonText != null) _startBattleButtonText.text = _isStarting ? "ĐANG TẢI..." : (_presenter != null && !_presenter.HasStageSelected ? "CHỌN ẢI XUẤT TRẬN" : "XUẤT TRẬN");
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
