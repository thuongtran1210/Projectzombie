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
        [SerializeField] private UpgradeUIView view;

        [Header("Dependencies")]
        [SerializeField] private PlayerExperience playerExperience;
        [SerializeField] private WeaponManager playerWeaponManager;

        private void Start()
        {
            if (view == null)
            {
                view = GetComponent<UpgradeUIView>();
            }

            if (playerExperience != null)
            {
                playerExperience.OnLevelUp += HandleLevelUp;
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (playerExperience != null)
            {
                playerExperience.OnLevelUp -= HandleLevelUp;
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
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
                view.SetActive(true);
                PopulateUpgradeScreen();
            }
        }

        private void HandleStateChanged(GameState newState)
        {
            if (view == null) return;

            if (newState == GameState.LevelUpSelection)
            {
                view.SetActive(true);
                PopulateUpgradeScreen();
            }
            else
            {
                view.SetActive(false);
            }
        }

        private void PopulateUpgradeScreen()
        {
            if (view == null) return;

            if (UpgradeManager.Instance == null)
            {
                Debug.LogError("[UpgradeUIPresenter] UpgradeManager.Instance is null!");
                return;
            }

            int cardsCount = view.GetCardsLength();
            List<UpgradeData> choices = UpgradeManager.Instance.GetRandomUpgrades(cardsCount, playerWeaponManager.gameObject);

            for (int i = 0; i < cardsCount; i++)
            {
                UpgradeCardView cardView = view.GetCardView(i);
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
            if (playerWeaponManager != null && selectedUpgrade != null)
            {
                selectedUpgrade.ApplyUpgrade(playerWeaponManager.gameObject);
            }

            // Chuyển lại trạng thái chơi để tiếp tục game
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(GameState.Playing);
            }
            else
            {
                if (view != null)
                {
                    view.SetActive(false);
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
