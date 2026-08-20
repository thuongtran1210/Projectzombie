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
            _currentRerolls = _maxRerollsPerRun;

            if (_view != null)
            {
                _view.SetRerollButtonCallback(OnRerollClicked);
                _view.SetSkipButtonCallback(OnSkipClicked);
            }

            SubscribeEvents();

            _isConstructed = true;
        }

        private void Start()
        {
            if (_view == null)
            {
                _view = GetComponent<UpgradeUIView>();
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
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(GameState.LevelUpSelection);
            }
            else
            {
                if (_view != null)
                {
                    _view.SetActive(true);
                }
                PopulateUpgradeScreen();
            }
        }

        private void HandleStateChanged(GameState newState)
        {
            if (_view == null) return;

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

            _view.SetRerollCountText($"Reroll ({_currentRerolls})");
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

                    // Xử lý định dạng dữ liệu (Presenter format data)
                    string category = FormatCategoryName(upgradeData.upgradeType);
                    string level = FormatLevel(upgradeData);
                    string statDiff = _statFormatter.FormatStatDiff(upgradeData);
                    string elementBadge = ElementVisualHelper.GetElementBadgeRichText(upgradeData.element);

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
                }
            }
        }

        private void OnUpgradeSelected(UpgradeData selectedUpgrade)
        {
            if (_playerWeaponManager != null && selectedUpgrade != null)
            {
                selectedUpgrade.ApplyUpgrade(_playerWeaponManager.gameObject);
            }

            ResumeGameplay();
        }

        private void OnRerollClicked()
        {
            if (_currentRerolls > 0)
            {
                _currentRerolls--;
                PopulateUpgradeScreen();
            }
        }

        private void OnSkipClicked()
        {
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

        private string FormatCategoryName(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.WeaponUpgrade: return "Weapon Upgrade";
                case UpgradeType.SignatureSkillUpgrade: return "Signature Skill";
                case UpgradeType.CommonUpgrade: return "Common Upgrade";
                case UpgradeType.FactionCounterUpgrade: return "Faction Counter";
                case UpgradeType.RareUpgrade: return "Rare Upgrade";
                case UpgradeType.EvolutionUpgrade: return "Evolution Upgrade";
                default: return type.ToString();
            }
        }

        private string FormatLevel(UpgradeData data)
        {
            if (data is WeaponUpgradeData weaponData)
            {
                if (weaponData.requiredCurrentLevel == 0)
                    return "NEW!";
                else
                    return $"Lv.{weaponData.requiredCurrentLevel + 1}";
            }
            else if (data is EvolutionUpgradeData)
            {
                return "EVOLUTION";
            }
            else if (data is CommonUpgradeData commonData && _playerWeaponManager != null)
            {
                var playerPassives = _playerWeaponManager.GetComponent<PlayerPassives>();
                int count = playerPassives != null ? playerPassives.GetUpgradeCount(commonData.upgradeName) : 0;
                int nextLevel = count + 1;
                if (commonData.maxLevel > 0)
                {
                    return $"Lv.{nextLevel}/{commonData.maxLevel}";
                }
                return $"Lv.{nextLevel}";
            }
            return "";
        }
    }
}
