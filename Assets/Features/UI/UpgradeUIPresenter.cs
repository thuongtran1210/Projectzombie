using UnityEngine;
using System.Collections.Generic;
using ProjectZombie.Features.Upgrades;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.UI.Formatters;
using ProjectZombie.Features.UI.Helpers;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Presenter quản lý logic lựa chọn nâng cấp và cầu nối giữa UpgradeManager (Model) và UpgradeUIView.
    /// </summary>
    public class UpgradeUIPresenter : MonoBehaviour
    {
        [Header("View Reference")]
        [SerializeField] private UpgradeUIView _view;

        [Header("Dependencies")]
        [SerializeField] private PlayerExperience _playerExperience;
        [SerializeField] private WeaponManager _playerWeaponManager;

        [Header("Roguelite Settings")]
        [SerializeField] private int _defaultChoiceCount = 3;
        [SerializeField] private int _maxRerollsPerRun = 3;

        private bool _isConstructed = false;
        private int _currentRerolls;
        private readonly IUpgradeStatFormatter _statFormatter = new UpgradeStatFormatter();

        public static UpgradeUIPresenter Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            _currentRerolls = CalculateMaxRerolls();
            if (_view == null)
            {
                _view = GetComponent<UpgradeUIView>() ?? GetComponentInChildren<UpgradeUIView>(true);
            }
        }

        public void Construct(PlayerExperience experience, WeaponManager weaponManager)
        {
            Debug.Log($"<color=#00FFFF>[DIAG_UPGRADE_PRESENTER]</color> Construct() được gọi! Exp: {(experience != null ? experience.name : "NULL")}, WeaponMgr: {(weaponManager != null ? weaponManager.name : "NULL")}");

            if (_isConstructed)
            {
                UnsubscribeEvents();
            }

            if (_view == null)
            {
                _view = GetComponent<UpgradeUIView>() ?? GetComponentInChildren<UpgradeUIView>(true);
            }

            _playerExperience = experience;
            _playerWeaponManager = weaponManager;
            _currentRerolls = CalculateMaxRerolls();

            if (_view != null)
            {
                _view.SetRerollButtonCallback(OnRerollClicked);
                _view.SetSkipButtonCallback(OnSkipClicked);
            }

            SubscribeEvents();

            _isConstructed = true;

            if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.LevelUpSelection)
            {
                Debug.Log("<color=#00FFFF>[DIAG_UPGRADE_PRESENTER]</color> Construct: Phát hiện GameState đang là LevelUpSelection -> Lập tức mở bảng nâng cấp!");
                HandleStateChanged(GameState.LevelUpSelection);
            }
        }

        private int CalculateMaxRerolls()
        {
            int baseRerolls = Mathf.Max(1, _maxRerollsPerRun);
            var saveData = ProjectZombie.Features.MetaProgression.MetaCurrencyManager.Instance != null
                ? ProjectZombie.Features.MetaProgression.MetaCurrencyManager.Instance.GetSaveData()
                : Core.Save.SaveSystem.Load();

            if (saveData != null)
            {
                var treeData = Resources.Load<ProjectZombie.Features.MetaProgression.PermanentUpgradeTreeData>("PermanentUpgradeTree");
#if UNITY_EDITOR
                if (treeData == null)
                {
                    treeData = UnityEditor.AssetDatabase.LoadAssetAtPath<ProjectZombie.Features.MetaProgression.PermanentUpgradeTreeData>("Assets/_Data/Meta/PermanentUpgradeTree.asset");
                }
#endif
                if (treeData != null)
                {
                    int rerollNodeIndex = treeData.GetNodeIndex("util_reroll");
                    if (rerollNodeIndex >= 0)
                    {
                        int bonusRerolls = saveData.GetUpgradeLevel(rerollNodeIndex);
                        baseRerolls += bonusRerolls;
                    }
                }
            }

            return baseRerolls;
        }

        private void Start()
        {
            if (_view == null)
            {
                _view = GetComponent<UpgradeUIView>() ?? GetComponentInChildren<UpgradeUIView>(true);
            }

            if (_currentRerolls <= 0)
            {
                _currentRerolls = CalculateMaxRerolls();
            }

            // Tương thích ngược: nếu chưa được Construct từ GameplayBootstrapper và đã kéo thả trong Inspector thì mới tự gọi Construct
            if (!_isConstructed && (_playerExperience != null || _playerWeaponManager != null))
            {
                Construct(_playerExperience, _playerWeaponManager);
            }

            if (_view != null)
            {
                _view.SetRerollButtonCallback(OnRerollClicked);
                _view.SetSkipButtonCallback(OnSkipClicked);
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
                GameStateManager.Instance.OnStateChanged += HandleStateChanged;
                Debug.Log($"<color=#00FFFF>[DIAG_UPGRADE_PRESENTER]</color> Start: Đã xác nhận subscribe GameStateManager.OnStateChanged (CurrentState: {GameStateManager.Instance.CurrentState})");

                if (GameStateManager.Instance.CurrentState == GameState.LevelUpSelection)
                {
                    Debug.Log("<color=#00FFFF>[DIAG_UPGRADE_PRESENTER]</color> Start: Phát hiện GameState đang là LevelUpSelection -> Lập tức kích hoạt bảng nâng cấp!");
                    HandleStateChanged(GameState.LevelUpSelection);
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            UnsubscribeEvents();

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void SubscribeEvents()
        {
            if (_playerExperience != null)
            {
                _playerExperience.OnLevelUp += HandleLevelUp;
            }
        }

        private void UnsubscribeEvents()
        {
            if (_playerExperience != null)
            {
                _playerExperience.OnLevelUp -= HandleLevelUp;
            }
        }

        private void HandleLevelUp(int newLevel)
        {
            Debug.Log($"<color=#00FFFF>[DIAG_UPGRADE_PRESENTER]</color> HandleLevelUp(newLevel: {newLevel})!");

            // Bảo vệ xung đột: Tuyệt đối không mở bảng nâng cấp nếu Game Over hoặc nhân vật đã tử trận
            if (GameStateManager.Instance != null && (GameStateManager.Instance.CurrentState == GameState.GameOver || GameStateManager.Instance.CurrentState == GameState.MainMenu))
            {
                return;
            }

            if (_playerExperience != null && _playerExperience.TryGetComponent<HealthSystem>(out var hp) && hp.CurrentHealth <= 0)
            {
                return;
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(GameState.LevelUpSelection);
            }
            else
            {
                // Fallback nếu không có GameStateManager trong scene test
                if (_view != null)
                {
                    _view.SetActive(true);
                }
                Time.timeScale = 0f;
                PopulateUpgradeScreen();
            }
        }

        private void HandleStateChanged(GameState newState)
        {
            Debug.Log($"<color=#00FFFF>[DIAG_UPGRADE_PRESENTER]</color> HandleStateChanged(newState: {newState})! _view is {(_view != null ? _view.gameObject.name : "NULL")}");

            if (newState == GameState.LevelUpSelection)
            {
                if (_view != null)
                {
                    _view.SetActive(true);
                }
                else
                {
                    _view = GetComponent<UpgradeUIView>() ?? GetComponentInChildren<UpgradeUIView>(true) ?? transform.parent?.GetComponentInChildren<UpgradeUIView>(true);
                    if (_view != null) _view.SetActive(true);
                }
                PopulateUpgradeScreen();
            }
            else
            {
                if (_view != null)
                {
                    _view.SetActive(false);
                }
            }
        }

        private void PopulateUpgradeScreen()
        {
            Debug.Log("<color=#00FFFF>[DIAG_UPGRADE_PRESENTER]</color> PopulateUpgradeScreen() ĐANG CHẠY...");

            if (_view == null)
            {
                _view = GetComponent<UpgradeUIView>() ?? GetComponentInChildren<UpgradeUIView>(true) ?? transform.parent?.GetComponentInChildren<UpgradeUIView>(true);
            }
            if (_view == null)
            {
                Debug.LogError("<color=#00FFFF>[DIAG_UPGRADE_PRESENTER]</color> LỖI: _view is null!");
                return;
            }

            if (UpgradeManager.Instance == null)
            {
                Debug.LogError("<color=#00FFFF>[DIAG_UPGRADE_PRESENTER]</color> LỖI: UpgradeManager.Instance is null!");
                return;
            }

            if (_playerWeaponManager == null)
            {
                if (PlayerProvider.HasPlayer)
                {
                    _playerWeaponManager = PlayerProvider.PlayerGameObject.GetComponent<WeaponManager>();
                }
                else
                {
                    var p = GameObject.FindWithTag("Player");
                    if (p != null) _playerWeaponManager = p.GetComponent<WeaponManager>();
                }
            }

            if (_playerWeaponManager == null)
            {
                Debug.LogError("<color=#00FFFF>[DIAG_UPGRADE_PRESENTER]</color> LỖI: _playerWeaponManager is null!");
                return;
            }

            int currentLevel = _playerExperience != null ? _playerExperience.CurrentLevel : 2;
            bool isMutation = UpgradeManager.Instance != null && UpgradeManager.Instance.IsMutationLevel(currentLevel);
            var context = Player.PlayerContext.Create(_playerWeaponManager.gameObject);

            // Cập nhật chế độ trực quan theo mốc tiến trình
            UpgradeUIVisualMode visualMode = UpgradeUIVisualMode.MicroStats;
            if (currentLevel <= 1 || (context != null && context.MythicManager != null && context.MythicManager.CurrentArchetype == MythicArchetype.None))
            {
                visualMode = UpgradeUIVisualMode.ArchetypeCore;
            }
            else if (isMutation)
            {
                visualMode = UpgradeUIVisualMode.MutationAugment;
            }

            _view.SetVisualMode(visualMode, currentLevel);

            if (isMutation && UpgradeManager.Instance != null)
            {
                _view.SetRerollCountText($"Lắc Lại ({UpgradeManager.Instance.CurrentRerollTokens}/2)");
                _view.SetRerollInteractable(UpgradeManager.Instance.CurrentRerollTokens > 0);
            }
            else
            {
                // Level thường: Khóa nút Reroll để người chơi chọn nhanh dưới 1s
                _view.SetRerollCountText("Lắc Lại (0)");
                _view.SetRerollInteractable(false);
            }

            int choiceCount = _defaultChoiceCount > 0 ? _defaultChoiceCount : 3;
            List<UpgradeData> choices = UpgradeManager.Instance.GetProgressionUpgrades(choiceCount, currentLevel, context);

            if (choices != null)
            {
                for (int c = 0; c < choices.Count; c++)
                {
                    Debug.Log($"<color=#00FFFF>[DIAG_UPGRADE_PRESENTER]</color> Choice [{c}]: {choices[c]?.upgradeName} ({choices[c]?.GetType().Name})");
                }
            }

            IReadOnlyList<UpgradeCardView> cardViews = _view.GetOrCreateCardViews(choices != null ? choices.Count : 0);
            Debug.Log($"<color=#00FFFF>[DIAG_UPGRADE_PRESENTER]</color> cardViews trả về: {cardViews?.Count ?? 0} views.");

            for (int i = 0; i < cardViews.Count; i++)
            {
                UpgradeCardView cardView = cardViews[i];
                if (cardView == null) continue;

                if (i < choices.Count)
                {
                    UpgradeData upgradeData = choices[i];

                    // Xử lý định dạng dữ liệu (Presenter format data Cổ Phong)
                    string category = FormatCategoryName(upgradeData);
                    string level = FormatLevel(upgradeData);
                    string statDiff = _statFormatter.FormatStatDiff(upgradeData);
                    string elementBadge = FormatElementAndSynergyBadge(upgradeData);

                    string desc = upgradeData.description;
                    if (upgradeData is StatMicroUpgradeData micro && !string.IsNullOrEmpty(micro.oneLineSummary))
                    {
                        desc = $"<color=#00FF88><b>{micro.oneLineSummary}</b></color>";
                    }

                    // Thiết lập card với dữ liệu đã định dạng và callback
                    cardView.Setup(
                        upgradeData.icon,
                        upgradeData.upgradeName,
                        desc,
                        category,
                        level,
                        statDiff,
                        () => OnUpgradeSelected(upgradeData),
                        () => OnBanSelected(upgradeData)
                    );

                    cardView.SetElementBadge(elementBadge);

                    // Xử lý Huy hiệu Duyên Phận & Phân loại giao diện Thẻ Nâng Cấp
                    bool isMythic = upgradeData is MythicCoreUpgradeData || upgradeData.upgradeType == UpgradeType.MythicCore;
                    bool isEvolution = upgradeData is EvolutionUpgradeData || upgradeData.upgradeType == UpgradeType.EvolutionUpgrade;
                    bool isMutationAugment = upgradeData is MutationAugmentUpgradeData;
                    bool isBreakthrough = upgradeData.upgradeType == UpgradeType.BreakthroughUltimate;
                    bool isSynergyTrait = upgradeData is SynergyTraitUpgradeData || upgradeData.upgradeType == UpgradeType.SynergyTrait;
                    bool hasSynergy = isSynergyTrait;

                    if (isMythic)
                    {
                        var mythicCore = upgradeData as MythicCoreUpgradeData;
                        string title = mythicCore != null && !string.IsNullOrEmpty(mythicCore.mythicTitle) ? mythicCore.mythicTitle : "ĐẠI LÕI KHỞI ĐẦU";
                        cardView.SetEvolutionMode(true);
                        cardView.SetSynergyInfo(null, $"<color=#FFD700><b>[ {title} ]</b></color>");
                    }
                    else if (isMutationAugment)
                    {
                        var mut = upgradeData as MutationAugmentUpgradeData;
                        cardView.SetEvolutionMode(true);
                        cardView.SetSynergyInfo(null, mut != null ? mut.GetCategoryDisplayName() : "<color=#FFD700><b>[LÕI ĐỘT BIẾN]</b></color>");
                    }
                    else if (isEvolution)
                    {
                        cardView.SetEvolutionMode(true);
                        cardView.SetSynergyInfo(null, "<color=#A33418><b>CÔNG THỨC DUNG HỢP HOÀN TẤT</b></color>");
                    }
                    else if (isBreakthrough)
                    {
                        cardView.SetEvolutionMode(true);
                        cardView.SetSynergyInfo(null, "<color=#FF7700><b>[ĐỘT PHÁ TUYỆT KỸ]</b></color>");
                    }
                    else if (isSynergyTrait)
                    {
                        var traitData = upgradeData as SynergyTraitUpgradeData;
                        string archetypeName = traitData != null ? traitData.requiredArchetype.GetDisplayName() : "LÕI";
                        cardView.SetEvolutionMode(false);
                        cardView.SetSynergyInfo(null, $"<color=#00E5FF><b>[THẦN BINH THUẬT: {archetypeName.ToUpper()}]</b></color>");
                    }
                    else
                    {
                        cardView.SetEvolutionMode(false);
                        UpgradeSynergyFormatter.FormatSynergyInfo(upgradeData, _playerWeaponManager, out Sprite synIcon, out string synText);
                        hasSynergy = synIcon != null || !string.IsNullOrEmpty(synText);
                        cardView.SetSynergyInfo(synIcon, synText);
                    }

                    // Tự động phân cấp màu khung thẻ (Gỗ Mun / Ngọc Bích / Hoàng Kim / Hổ Phách)
                    cardView.SetCardTier(upgradeData.upgradeType, isEvolution || isMythic || isBreakthrough, hasSynergy);
                }
            }
        }

        private void OnUpgradeSelected(UpgradeData selectedUpgrade)
        {
            if (selectedUpgrade == null) return;

            global::Core.Audio.AudioManager.Instance?.PlayUIConfirm();

            if (_playerWeaponManager != null)
            {
                var context = PlayerContext.Create(_playerWeaponManager.gameObject);
                selectedUpgrade.ApplyUpgrade(context);
            }

            ResumeGameplay();
        }

        private void OnRerollClicked()
        {
            if (UpgradeManager.Instance != null && UpgradeManager.Instance.TryConsumeRerollToken())
            {
                global::Core.Audio.AudioManager.Instance?.PlayUIClick();
                PopulateUpgradeScreen();
            }
            else
            {
                global::Core.Audio.AudioManager.Instance?.PlayUIError();
                Debug.LogWarning("[UpgradeUIPresenter] Đã hết lượt Reroll Token trong trận đấu!");
            }
        }

        private void OnSkipClicked()
        {
            global::Core.Audio.AudioManager.Instance?.PlayUIClick();

            if (_playerWeaponManager != null)
            {
                var healthSystem = _playerWeaponManager.GetComponent<HealthSystem>();
                if (healthSystem != null)
                {
                    float healAmount = healthSystem.MaxHealth * 0.2f;
                    healthSystem.Heal(healAmount);
                    Debug.Log($"<color=#00FF88>[UpgradeUIPresenter]</color> Bỏ qua lựa chọn nâng cấp, hồi phục {healAmount:F0} Máu (20% Max HP)!");
                }
            }

            ResumeGameplay();
        }

        private void OnBanSelected(UpgradeData upgradeToBan)
        {
            if (upgradeToBan != null && UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.BanUpgrade(upgradeToBan);
                PopulateUpgradeScreen();
            }
        }

        private void ResumeGameplay()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(GameState.Playing);
            }
            else
            {
                if (_view != null)
                {
                    _view.SetActive(false);
                }
                Time.timeScale = 1f;
            }
        }

        private string FormatCategoryName(UpgradeData data)
        {
            if (data == null) return string.Empty;
            return data.GetCategoryDisplayName();
        }

        private string FormatElementAndSynergyBadge(UpgradeData upgradeData)
        {
            string baseBadge = ElementVisualHelper.GetElementBadgeRichText(upgradeData.element);
            if (upgradeData.element == ElementType.None || _playerWeaponManager == null) return baseBadge;

            bool isSameElement = false;
            bool isGenerative = false;

            for (int i = 0; i < _playerWeaponManager.ActiveWeapons.Count; i++)
            {
                var w = _playerWeaponManager.ActiveWeapons[i];
                if (w != null && w.element != ElementType.None)
                {
                    if (w.element == upgradeData.element)
                    {
                        isSameElement = true;
                        break;
                    }
                    else if (ElementVisualHelper.IsElementGenerative(w.element, upgradeData.element))
                    {
                        isGenerative = true;
                    }
                }
            }

            if (isSameElement)
            {
                return string.IsNullOrEmpty(baseBadge) ? "<color=#0E6073><b>[ĐỒNG HỆ]</b></color>" : $"{baseBadge} <color=#0E6073><b>[ĐỒNG HỆ]</b></color>";
            }
            if (isGenerative)
            {
                return string.IsNullOrEmpty(baseBadge) ? "<color=#007A4D><b>[TƯƠNG SINH]</b></color>" : $"{baseBadge} <color=#007A4D><b>[TƯƠNG SINH]</b></color>";
            }

            return baseBadge;
        }

        private string FormatLevel(UpgradeData data)
        {
            if (data == null) return string.Empty;
            return data.GetLevelDisplayName(_playerWeaponManager != null ? _playerWeaponManager.gameObject : null);
        }
    }
}
