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

        private int _currentRerolls;
        private bool _isConstructed = false;
        private readonly IUpgradeStatFormatter _statFormatter = new UpgradeStatFormatter();

        private void Awake()
        {
            _currentRerolls = CalculateMaxRerolls();
            if (_view == null)
            {
                _view = GetComponent<UpgradeUIView>();
            }
        }

        public void Construct(PlayerExperience experience, WeaponManager weaponManager)
        {
            if (_isConstructed)
            {
                UnsubscribeEvents();
            }

            if (_view == null)
            {
                _view = GetComponent<UpgradeUIView>();
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
                _view = GetComponent<UpgradeUIView>();
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
                GameStateManager.Instance.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDestroy()
        {
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
            if (newState == GameState.LevelUpSelection)
            {
                _view.SetActive(true);
                PopulateUpgradeScreen();
            }
            else
            {
                _view.SetActive(false);
            }
        }

        private void PopulateUpgradeScreen()
        {
            if (_view == null) return;

            if (UpgradeManager.Instance == null)
            {
                Debug.LogError("[UpgradeUIPresenter] UpgradeManager.Instance is null!");
                return;
            }

            if (_playerWeaponManager == null)
            {
                Debug.LogError("[UpgradeUIPresenter] _playerWeaponManager is null!");
                return;
            }

            _view.SetRerollCountText($"Lắc Lại ({_currentRerolls})");
            _view.SetRerollInteractable(_currentRerolls > 0);

            int choiceCount = _defaultChoiceCount > 0 ? _defaultChoiceCount : 3;
            List<UpgradeData> choices = UpgradeManager.Instance.GetRandomUpgrades(choiceCount, _playerWeaponManager.gameObject);
            IReadOnlyList<UpgradeCardView> cardViews = _view.GetOrCreateCardViews(choices.Count);

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

                    // Thiết lập card với dữ liệu đã định dạng và callback
                    cardView.Setup(
                        upgradeData.icon,
                        upgradeData.upgradeName,
                        upgradeData.description,
                        category,
                        level,
                        statDiff,
                        () => OnUpgradeSelected(upgradeData),
                        () => OnBanSelected(upgradeData)
                    );

                    cardView.SetElementBadge(elementBadge);

                    // Xử lý Huy hiệu Duyên Phận & Chế độ Thần Khí Tiến Hóa
                    if (upgradeData is EvolutionUpgradeData)
                    {
                        cardView.SetEvolutionMode(true);
                        cardView.SetSynergyInfo(null, "<color=#FFD700>★ CÔNG THỨC DUNG HỢP HOÀN TẤT ★</color>");
                    }
                    else
                    {
                        FormatSynergyInfo(upgradeData, out Sprite synIcon, out string synText);
                        cardView.SetSynergyInfo(synIcon, synText);
                    }
                }
            }
        }

        private void OnUpgradeSelected(UpgradeData selectedUpgrade)
        {
            global::Core.Audio.AudioManager.Instance?.PlayUIConfirm();

            if (_playerWeaponManager != null && selectedUpgrade != null)
            {
                selectedUpgrade.ApplyUpgrade(_playerWeaponManager.gameObject);
            }

            ResumeGameplay();
        }

        private void OnRerollClicked()
        {
            Debug.Log($"<color=#00FF88>[UpgradeUIPresenter]</color> OnRerollClicked! Lượt còn lại: {_currentRerolls}");
            if (_currentRerolls > 0)
            {
                _currentRerolls--;
                global::Core.Audio.AudioManager.Instance?.PlayUIClick();
                PopulateUpgradeScreen();
            }
            else
            {
                global::Core.Audio.AudioManager.Instance?.PlayUIError();
                Debug.LogWarning("[UpgradeUIPresenter] Đã hết số lần Lắc Lại trong lượt chạy!");
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
            if (data is EvolutionUpgradeData) return "<color=#A33418><b>[THẦN PHÁP TIẾN HÓA]</b></color>";
            if (data is BreakthroughUpgradeData) return "<color=#6B2D82><b>[ĐỘT PHÁ TUYỆT KỸ]</b></color>";
            if (data is ComboAugmentUpgradeData) return "<color=#B85D00><b>[BÍ KÍP ĐÒN CHÉM]</b></color>";
            if (data is DashTraitUpgradeData) return "<color=#0E6073><b>[CƯỜNG HÓA LƯỚT]</b></color>";
            if (data is WeaponUpgradeData) return "<color=#007A4D><b>[CƯỜNG HÓA PHÁP BẢO]</b></color>";
            if (data is FallbackRewardUpgradeData) return "<color=#A33418><b>[THƯỞNG CỨU MỆNH]</b></color>";
            if (data is RareUpgradeData) return "<color=#6B2D82><b>[BÍ THUẬT HIẾM]</b></color>";
            return "<color=#1B4D7E><b>[BỔ TRỢ KHÍ VẬN]</b></color>";
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
                    else if (IsElementGenerative(w.element, upgradeData.element))
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

        private bool IsElementGenerative(ElementType parent, ElementType child)
        {
            switch (parent)
            {
                case ElementType.Kim: return child == ElementType.Thuy;
                case ElementType.Thuy: return child == ElementType.Moc;
                case ElementType.Moc: return child == ElementType.Hoa;
                case ElementType.Hoa: return child == ElementType.Tho;
                case ElementType.Tho: return child == ElementType.Kim;
                default: return false;
            }
        }

        private string FormatLevel(UpgradeData data)
        {
            if (data is WeaponUpgradeData weaponData)
            {
                if (weaponData.requiredCurrentLevel == 0)
                    return "MỚI!";
                else
                    return $"Cấp {weaponData.requiredCurrentLevel + 1}";
            }
            else if (data is EvolutionUpgradeData)
            {
                return "TIẾN HÓA";
            }
            else if (data is BreakthroughUpgradeData)
            {
                return "ĐỘT PHÁ";
            }
            else if (data is ComboAugmentUpgradeData)
            {
                return "BÍ KÍP";
            }
            else if (data is DashTraitUpgradeData)
            {
                return "LƯỚT";
            }
            else if (data is FallbackRewardUpgradeData)
            {
                return "THƯỞNG";
            }
            else if (data is CommonUpgradeData commonData && _playerWeaponManager != null)
            {
                var playerPassives = _playerWeaponManager.GetComponent<PlayerPassives>();
                int count = playerPassives != null ? playerPassives.GetUpgradeCount(commonData.upgradeName) : 0;
                int nextLevel = count + 1;
                if (commonData.maxLevel > 0)
                {
                    return $"Cấp {nextLevel}/{commonData.maxLevel}";
                }
                return $"Cấp {nextLevel}";
            }
            return "";
        }

        private void FormatSynergyInfo(UpgradeData data, out Sprite icon, out string formattedText)
        {
            icon = null;
            formattedText = null;

            if (WeaponEvolutionManager.Instance == null || _playerWeaponManager == null) return;

            var playerPassives = _playerWeaponManager.GetComponent<PlayerPassives>();

            // 1. Trường hợp thẻ là Vũ Khí (WeaponUpgradeData / Base Weapon Unlock)
            if (data is WeaponUpgradeData weaponData)
            {
                if (WeaponEvolutionManager.Instance.TryGetRecipeByWeaponId(weaponData.weaponId, out var recipe))
                {
                    bool hasPassive = playerPassives != null && playerPassives.HasPassive(recipe.requiredPassiveId);
                    if (hasPassive)
                    {
                        formattedText = $"<color=#00FF88>★ Duyên Phận: Đã có {recipe.requiredPassiveId} ✓ (Sẵn sàng)</color>";
                    }
                    else
                    {
                        formattedText = $"<color=#AAAAAA>Duyên Phận: Cần {recipe.requiredPassiveId} (Chưa có)</color>";
                    }
                }
            }
            // 2. Trường hợp thẻ là Thẻ Bị Động (Common / Passive Upgrade)
            else if (data is CommonUpgradeData commonData)
            {
                var recipes = WeaponEvolutionManager.Instance.GetRecipesByPassiveId(commonData.id);
                if (recipes != null && recipes.Count > 0)
                {
                    List<string> weaponNames = new List<string>();
                    bool anyWeaponOwned = false;

                    foreach (var r in recipes)
                    {
                        bool hasWeapon = _playerWeaponManager.GetWeaponById(r.baseWeaponId) != null;
                        if (hasWeapon)
                        {
                            anyWeaponOwned = true;
                            weaponNames.Add($"<color=#00FF88>{r.baseWeaponId} ✓</color>");
                        }
                        else
                        {
                            weaponNames.Add($"<color=#888888>{r.baseWeaponId}</color>");
                        }
                    }

                    if (anyWeaponOwned)
                    {
                        formattedText = $"<color=#FFD700>★ Hợp Thể:</color> {string.Join(", ", weaponNames)}";
                    }
                    else
                    {
                        formattedText = $"<color=#AAAAAA>Ghép Cùng:</color> {string.Join(", ", weaponNames)}";
                    }
                }
            }
        }
    }
}
