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
                    bool isEvolution = upgradeData is EvolutionUpgradeData || upgradeData.upgradeType == UpgradeType.EvolutionUpgrade || upgradeData.upgradeType == UpgradeType.BreakthroughUltimate;
                    bool hasSynergy = false;

                    if (isEvolution)
                    {
                        cardView.SetEvolutionMode(true);
                        cardView.SetSynergyInfo(null, "<color=#A33418><b>★ CÔNG THỨC DUNG HỢP HOÀN TẤT ★</b></color>");
                    }
                    else
                    {
                        UpgradeSynergyFormatter.FormatSynergyInfo(upgradeData, _playerWeaponManager, out Sprite synIcon, out string synText);
                        hasSynergy = synIcon != null || !string.IsNullOrEmpty(synText);
                        cardView.SetSynergyInfo(synIcon, synText);
                    }

                    // Tự động phân cấp màu khung thẻ (Gỗ Mun / Ngọc Bích / Hoàng Kim / Hổ Phách)
                    cardView.SetCardTier(upgradeData.upgradeType, isEvolution, hasSynergy);
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
