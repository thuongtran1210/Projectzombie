using UnityEngine;
using System.Collections.Generic;
using ProjectZombie.Features.Upgrades;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Shared;

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

        private bool _isConstructed = false;

        public void Construct(PlayerExperience experience, WeaponManager weaponManager)
        {
            if (_isConstructed)
            {
                UnsubscribeEvents();
            }

            _playerExperience = experience;
            _playerWeaponManager = weaponManager;

            SubscribeEvents();

            _isConstructed = true;
        }

        private void Start()
        {
            if (_view == null)
            {
                _view = GetComponent<UpgradeUIView>();
            }

            // Tương thích ngược: nếu đã kéo thả trong Inspector thì tự động Construct luôn
            if (_playerExperience != null || _playerWeaponManager != null)
            {
                Construct(_playerExperience, _playerWeaponManager);
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

            int cardsCount = _view.GetCardsLength();
            List<UpgradeData> choices = UpgradeManager.Instance.GetRandomUpgrades(cardsCount, _playerWeaponManager.gameObject);

            for (int i = 0; i < cardsCount; i++)
            {
                UpgradeCardView cardView = _view.GetCardView(i);
                if (cardView == null) continue;

                if (i < choices.Count)
                {
                    UpgradeData upgradeData = choices[i];
                    cardView.gameObject.SetActive(true);

                    // Xử lý định dạng dữ liệu (Presenter format data)
                    string category = FormatCategoryName(upgradeData.upgradeType);
                    string level = FormatLevel(upgradeData);

                    // Thiết lập card với dữ liệu đã định dạng và callback ẩn danh
                    cardView.Setup(
                        upgradeData.icon,
                        upgradeData.upgradeName,
                        upgradeData.description,
                        category,
                        level,
                        () => OnUpgradeSelected(upgradeData)
                    );
                }
                else
                {
                    cardView.gameObject.SetActive(false);
                }
            }
        }

        private void OnUpgradeSelected(UpgradeData selectedUpgrade)
        {
            if (_playerWeaponManager != null && selectedUpgrade != null)
            {
                selectedUpgrade.ApplyUpgrade(_playerWeaponManager.gameObject);
            }

            // Chuyển lại trạng thái chơi để tiếp tục game
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
            return "";
        }
    }
}
