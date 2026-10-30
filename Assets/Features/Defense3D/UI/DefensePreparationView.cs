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
        protected virtual bool IsLoadoutManagement => false;
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
        [Header("Courtyard presentation")]
        [SerializeField] private RawImage _courtyardHeroPreview;
        [SerializeField] private Image _courtyardHeroFallback;
        [Serializable] public struct CourtyardModel { public string Id; public GameObject Prefab; }
        [SerializeField] private CourtyardModel[] _courtyardHeroModels;
        [SerializeField] private TextMeshProUGUI _reviewStageSummary;
        [Header("Clean battle confirmation")]
        [SerializeField] private TextMeshProUGUI _reviewStageCode;
        [SerializeField] private TextMeshProUGUI _reviewStageName;
        [SerializeField] private TextMeshProUGUI _reviewStageDescription;
        [SerializeField] private TextMeshProUGUI _reviewStageDuration;
        [SerializeField] private TextMeshProUGUI _reviewStageRewards;
        [SerializeField] private Image _reviewStageArtwork;
        private GameObject _previewedHeroPrefab;

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
                _heroSlotButton.gameObject.SetActive(IsLoadoutManagement);
                _heroSlotButton.interactable = IsLoadoutManagement;
                _heroSlotButton.onClick.AddListener(OnHeroSlotClicked);
            }

            if (_relicSlotButton != null)
            {
                _relicSlotButton.gameObject.SetActive(IsLoadoutManagement);
                _relicSlotButton.interactable = IsLoadoutManagement;
                _relicSlotButton.onClick.AddListener(OnRelicSlotClicked);
            }

            if (_minionSlotButtons != null)
            {
                for (int i = 0; i < _minionSlotButtons.Length; i++)
                {
                    int index = i;
                    if (_minionSlotButtons[i] != null)
                    {
                        _minionSlotButtons[i].interactable = IsLoadoutManagement;
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
                        _towerSlotButtons[i].interactable = IsLoadoutManagement;
                        _towerSlotButtons[i].onClick.AddListener(() => OnTowerSlotClicked(index));
                    }
                }
            }
        }

        protected virtual void OnEnable()
        {
            ReleasePresenter();
            _presenter = new DefensePreparationPresenter(stage: IsLoadoutManagement ? null : ProjectZombie.Core.Architecture.AppCompositionRoot.Instance?.Flow.SelectedStage);
            _presenter.OnValidationStateChanged += UpdateValidationDisplay;
            _presenter.OnSelectionChanged += HandleSelectionChanged;
            _isStarting = false;
            ShowLoadingOverlay(false, string.Empty);
            RefreshAllDisplays();
        }

        protected virtual void OnDestroy()
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

        private void OpenLoadoutSlot(CodexTabType tab, string id)
        {
            if (!IsLoadoutManagement || _isStarting || _presenter == null) return;
            CardCodexPresenter.RequestLoadoutSlot(tab, id);
            (GetComponentInParent<MetaUIManager>(true) ?? MetaUIManager.Instance)?.OpenChildScreen(MetaScreenType.Codex);
        }

        private void OnHeroSlotClicked() => OpenLoadoutSlot(CodexTabType.HeroCards, _presenter?.Selection.HeroId);
        private void OnRelicSlotClicked() => OpenLoadoutSlot(CodexTabType.RelicFusion, _presenter?.Selection.RelicId);
        private void OnMinionSlotClicked(int index)
        {
            if (_presenter == null || index < 0) return;
            var ids = _presenter.Selection.MinionIds;
            OpenLoadoutSlot(CodexTabType.Minions, index < ids.Count ? ids[index] : null);
        }
        private void OnTowerSlotClicked(int index)
        {
            if (_presenter == null || index < 0) return;
            var ids = _presenter.Selection.TowerIds;
            OpenLoadoutSlot(CodexTabType.Towers, index < ids.Count ? ids[index] : null);
        }
        private void OnEditLoadoutClicked()
        {
            if (_isStarting || IsLoadoutManagement) return;
            (GetComponentInParent<MetaUIManager>(true) ?? MetaUIManager.Instance)?.OpenChildScreen(MetaScreenType.LoadoutManagement);
        }

        public override void OnBackPressed() => OnCloseClicked();

        private void OnCloseClicked()
        {
            if (_isStarting) return;
            if (!IsLoadoutManagement) MetaSceneTransitionController.Instance?.CancelLoadoutReview();
            var manager = GetComponentInParent<MetaUIManager>(true) ?? MetaUIManager.Instance;
            if (manager != null) manager.PopScreen();
            else gameObject.SetActive(false);
        }

        private async void OnStartBattleClicked()
        {
            if (IsLoadoutManagement) { OnCloseClicked(); return; }
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
            if (!IsLoadoutManagement) SetStageReview(_presenter.StageReview);

            if (_titleText != null) _titleText.text = IsLoadoutManagement ? "ĐỘI HÌNH • SÂN ĐÌNH" : "XÁC NHẬN XUẤT TRẬN";
            if (_subtitleText != null) _subtitleText.text = IsLoadoutManagement
                ? "Chọn Anh hùng, Thần khí, Lính và Trụ mang vào trận."
                : "Thông tin ải và lực lượng đã chọn";
            if (!IsLoadoutManagement && _reviewStageSummary != null)
                _reviewStageSummary.text = "ẢI ĐÃ CHỌN\n<color=#F1D08A>" + (stage != null ? stage.stageName : "Chưa chọn ải")
                    + "</color>\n\nKIỂM TRA LỰC LƯỢNG\n"
                    + $"Lính {selection.MinionIds.Count}/{rules.MaxMinionCount} • Trụ {selection.TowerIds.Count}/{rules.MaxTowerCount}"
                    + "\n\nChọn Chỉnh đội hình nếu cần thay đổi.\nXác nhận khi đã sẵn sàng.";

            if (_heroCatalog == null)
            {
                _heroCatalog = Resources.Load<DefenseHeroCatalogSO>("Defense3D/DefenseHeroCatalog");
            }

            int heroStar = DefenseLoadoutRuntimeAdapter.GetEffectiveHeroStarLevel(selection.HeroId);
            DefenseLoadoutRuntimeAdapter.EvaluateHeroStarBonuses(heroStar, null, out float heroHpBonus, out float heroSpdBonus);
            DefenseLoadoutRuntimeAdapter.EvaluateHeroCombatStarBonuses(heroStar, null, out float heroDamageBonus, out float signatureCooldown);
            string heroName = catalog.TryGetEntry(selection.HeroId, out var hero) ? $"{hero.DisplayName} <color=#FFD700>[{heroStar} Sao]</color>" : "Chưa chọn tướng";
            string heroStatsDesc = "Tướng điều khiển trong trận";
            Sprite heroAvatar = GetIcon(selection.HeroId);

            if (_heroCatalog != null)
            {
                var heroDef = _heroCatalog.GetHeroById(selection.HeroId);
                if (heroDef != null)
                {
                    float effectiveHp = heroDef.BaseMaxHp * (1f + heroHpBonus);
                    float effectiveSpd = heroDef.BaseMoveSpeed * (1f + heroSpdBonus);
                    heroStatsDesc = $"<color=#00FF88>Máu:</color> <b>{effectiveHp:0}</b>  <color=#FFD700>Công nền:</color> <b>{heroDef.BaseDamage:0}</b>  <color=#00E5FF>Tốc:</color> <b>{effectiveSpd:0.0}</b>\nSát thương đánh thường / bản mệnh +{heroDamageBonus * 100f:0}% • Giảm hồi chiêu bản mệnh {signatureCooldown * 100f:0}%";
                    if (heroAvatar == null && heroDef.Avatar != null)
                    {
                        heroAvatar = heroDef.Avatar;
                    }
                    ShowCourtyardHero(heroDef.HeroPrefab != null ? heroDef.HeroPrefab : GetAuthoredHeroPreview(selection.HeroId));
                }
            }

            SetHeroInfo(heroName, heroStatsDesc, heroAvatar);

            int relicStar = DefenseLoadoutRuntimeAdapter.GetEffectiveRelicStarLevel(selection.RelicId);
            DefenseLoadoutRuntimeAdapter.EvaluateRelicStarBonuses(relicStar, null, out float relicDmgBonus, out float relicCdBonus, out float relicAreaBonus);
            string relicName = catalog.TryGetEntry(selection.RelicId, out var relic) ? $"{relic.DisplayName} <color=#FFD700>[{relicStar} Sao]</color>" : "Chưa chọn pháp bảo";
            string relicDesc = relicStar > 1
                ? $"Cấp {relicStar} Sao: +{(int)(relicDmgBonus * 100)}% Công, -{(int)(relicCdBonus * 100)}% Hồi chiêu, +{(int)(relicAreaBonus * 100)}% Phạm vi."
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
            if (_minionSlotButtons == null) return;
            for (int i = 0; i < _minionSlotButtons.Length; i++)
            {
                if (_minionSlotButtons[i] != null) _minionSlotButtons[i].gameObject.SetActive(i < capacity && (IsLoadoutManagement || i < selectedIds.Count));
                if (i >= capacity) continue;
                BindSelectedRow(i, selectedIds, _minionSlotCheckmarks, _minionSlotNameTexts);
            }
        }

        private void BindTowerRows(IReadOnlyList<string> selectedIds, int capacity)
        {
            if (_towerSlotButtons == null) return;
            for (int i = 0; i < _towerSlotButtons.Length; i++)
            {
                if (_towerSlotButtons[i] != null) _towerSlotButtons[i].gameObject.SetActive(i < capacity && (IsLoadoutManagement || i < selectedIds.Count));
                if (i >= capacity) continue;
                BindSelectedRow(i, selectedIds, _towerSlotCheckmarks, _towerSlotNameTexts);
            }
        }

        private void BindSelectedRow(int index, IReadOnlyList<string> ids, Image[] icons, TextMeshProUGUI[] labels)
        {
            string id = index < ids.Count ? ids[index] : null;
            bool occupied = id != null;
            string name = occupied && _presenter.Catalog.TryGetEntry(id, out var entry) ? entry.DisplayName : "+ Thêm";
            SetRow(index, occupied, name, icons, labels);
            if (icons != null && index < icons.Length && icons[index] != null)
            {
                icons[index].sprite = occupied ? GetIcon(id) : null;
                icons[index].enabled = icons[index].sprite != null;
            }
        }

        private void ShowCourtyardHero(GameObject prefab)
        {
            if (_courtyardHeroPreview == null || !Application.isPlaying) return;
            if (!IsLoadoutManagement) { _courtyardHeroPreview.enabled = false; return; }
            if (prefab == null)
            {
                _courtyardHeroPreview.enabled = false;
                _previewedHeroPrefab = null;
                return;
            }
            var stage = CharacterPreviewStage.Instance;
            if (stage == null)
            {
                var node = new GameObject("CharacterPreviewStage");
                node.transform.position = new Vector3(2000f, 2000f, 0f);
                stage = node.AddComponent<CharacterPreviewStage>();
            }
            if (_previewedHeroPrefab != prefab || _courtyardHeroPreview.texture == null)
            {
                stage.DisplayCharacter(prefab, "Idle");
                stage.FrameCurrentCharacter();
                _previewedHeroPrefab = prefab;
            }
            _courtyardHeroPreview.texture = stage.PreviewTexture;
            _courtyardHeroPreview.enabled = stage.PreviewTexture != null;
            if (_courtyardHeroFallback != null) _courtyardHeroFallback.enabled = !_courtyardHeroPreview.enabled;
        }

        /// <summary>Returns an explicitly authored visual when the gameplay definition has no preview prefab.</summary>
        public GameObject GetAuthoredHeroPreview(string heroId)
        {
            if (_courtyardHeroModels != null)
                foreach (var item in _courtyardHeroModels)
                    if (string.Equals(item.Id, heroId, StringComparison.OrdinalIgnoreCase)) return item.Prefab;
            return null;
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
            if (_courtyardHeroFallback != null)
            {
                _courtyardHeroFallback.sprite = avatar;
                _courtyardHeroFallback.enabled = (_courtyardHeroPreview == null || !_courtyardHeroPreview.enabled) && avatar != null;
            }
        }

        public void SetRelicInfo(string relicName, string relicDesc, Sprite icon = null)
        {
            if (_relicNameText != null) _relicNameText.text = relicName;
            if (_relicDescText != null) _relicDescText.text = relicDesc;
            if (_relicIconImage != null) { _relicIconImage.sprite = icon; _relicIconImage.enabled = icon != null; }
        }

        public void SetMinionSlotState(int index, bool selected, string name)
        {
            SetRow(index, selected, name, _minionSlotCheckmarks, _minionSlotNameTexts);
            if (!IsLoadoutManagement && _minionSlotButtons != null && index < _minionSlotButtons.Length && _minionSlotButtons[index] != null)
                _minionSlotButtons[index].gameObject.SetActive(selected);
        }
        public void SetTowerSlotState(int index, bool selected, string name)
        {
            SetRow(index, selected, name, _towerSlotCheckmarks, _towerSlotNameTexts);
            if (!IsLoadoutManagement && _towerSlotButtons != null && index < _towerSlotButtons.Length && _towerSlotButtons[index] != null)
                _towerSlotButtons[index].gameObject.SetActive(selected);
        }

        /// <summary>Displays formatted stage details supplied by the presenter.</summary>
        public void SetStageReview(DefenseStageReviewPresentation presentation)
        {
            if (presentation == null || IsLoadoutManagement) return;
            if (_reviewStageCode != null) _reviewStageCode.text = presentation.Code;
            if (_reviewStageName != null) _reviewStageName.text = presentation.Name;
            if (_reviewStageDescription != null) _reviewStageDescription.text = presentation.Description;
            if (_reviewStageDuration != null) _reviewStageDuration.text = presentation.Duration;
            if (_reviewStageRewards != null) _reviewStageRewards.text = presentation.Rewards;
            if (_reviewStageArtwork != null)
            {
                _reviewStageArtwork.sprite = presentation.Artwork;
                _reviewStageArtwork.enabled = presentation.Artwork != null;
            }
        }

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
                    ? (IsLoadoutManagement ? "Chạm từng vị trí để thay trang bị." : canStart ? "Đội hình sẵn sàng xuất trận." : "Hãy hoàn thiện đội hình trước khi xuất trận.") : message;
                _validationMessageText.color = canStart ? new Color(.63f,.80f,.66f) : new Color(.94f,.65f,.56f);
            }
            if (_startBattleButton != null) _startBattleButton.interactable = IsLoadoutManagement || (canStart && !_isStarting);
            if (_editLoadoutButton != null)
            {
                _editLoadoutButton.gameObject.SetActive(!IsLoadoutManagement);
                _editLoadoutButton.interactable = !_isStarting;
            }
            if (_closeButton != null) _closeButton.interactable = !_isStarting;
            if (_startBattleButtonText != null) _startBattleButtonText.text = IsLoadoutManagement ? "XONG"
                : _isStarting ? "ĐANG TẢI..." : (_presenter != null && !_presenter.HasStageSelected ? "CHỌN ẢI XUẤT TRẬN" : "XUẤT TRẬN");
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
