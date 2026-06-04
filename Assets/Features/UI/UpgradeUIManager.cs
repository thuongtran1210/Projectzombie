using UnityEngine;
using System.Collections.Generic;
using ProjectZombie.Features.Upgrades;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Weapons;

namespace ProjectZombie.Features.UI
{
    public class UpgradeUIManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject upgradePanel;
        [SerializeField] private UpgradeCardUI[] upgradeCards; // Usually 3 cards

        [Header("Dependencies")]
        [SerializeField] private PlayerExperience playerExperience;
        [SerializeField] private WeaponManager playerWeaponManager;

        private void Start()
        {
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(false);
            }

            if (playerExperience != null)
            {
                playerExperience.OnLevelUp += HandleLevelUp;
            }
        }

        private void OnDestroy()
        {
            if (playerExperience != null)
            {
                playerExperience.OnLevelUp -= HandleLevelUp;
            }
        }

        private void HandleLevelUp(int newLevel)
        {
            ShowUpgradeScreen();
        }

        public void ShowUpgradeScreen()
        {
            Time.timeScale = 0f; // Pause the game
            upgradePanel.SetActive(true);

            // Get random upgrades
            if (UpgradeManager.Instance != null)
            {
                // Truyền gameObject của player thay vì WeaponManager
                List<UpgradeData> choices = UpgradeManager.Instance.GetRandomUpgrades(upgradeCards.Length, playerWeaponManager.gameObject);

                for (int i = 0; i < upgradeCards.Length; i++)
                {
                    if (i < choices.Count)
                    {
                        upgradeCards[i].gameObject.SetActive(true);
                        upgradeCards[i].Setup(choices[i], OnUpgradeSelected);
                    }
                    else
                    {
                        upgradeCards[i].gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                Debug.LogError("[UpgradeUIManager] UpgradeManager.Instance is null!");
            }
        }

        private void OnUpgradeSelected(UpgradeData selectedUpgrade)
        {
            if (playerWeaponManager != null)
            {
                // Gọi thẳng hàm ApplyUpgrade của thẻ, truyền playerGameObject vào
                selectedUpgrade.ApplyUpgrade(playerWeaponManager.gameObject);
            }

            // Hide UI and resume game
            upgradePanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}
