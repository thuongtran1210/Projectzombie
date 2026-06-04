using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Shared;
using UnityEngine.InputSystem;

namespace ProjectZombie.Features.UI.StatsAndSkills
{
    public class PlayerInfoUIPresenter : MonoBehaviour
    {
        [Header("Models / Logic")]
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private HealthSystem playerHealth;
        [SerializeField] private PlayerExperience playerExperience;
        [SerializeField] private WeaponManager weaponManager;

        [Header("Views")]
        [SerializeField] private PlayerHUDView hudView;
        [SerializeField] private PlayerStatsMenuUIView statsMenuView;

        private PlayerInputActions inputActions;
        private bool isMenuOpen = false;

        private void Awake()
        {
            inputActions = new PlayerInputActions();
            inputActions.UI.TogglePauseMenu.performed += OnToggleMenuPressed;
        }

        private void OnEnable()
        {
            inputActions.UI.Enable();
        }

        private void OnDisable()
        {
            inputActions.UI.Disable();
        }

        private void Start()
        {
            // Subscribe to events
            if (playerStats != null)
                playerStats.OnStatsUpdated += HandleStatsUpdated;

            if (playerHealth != null)
                playerHealth.OnHealthChanged += HandleHealthChanged;

            if (playerExperience != null)
                playerExperience.OnExpChanged += HandleExpChanged;

            if (weaponManager != null)
                weaponManager.OnWeaponsChanged += HandleWeaponsChanged;

            // Initial Update
            ForceUpdateAll();
            
            // Ensure menu is closed on start
            if (statsMenuView != null)
            {
                statsMenuView.gameObject.SetActive(false);
            }
            isMenuOpen = false;
        }

        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks
            if (playerStats != null)
                playerStats.OnStatsUpdated -= HandleStatsUpdated;

            if (playerHealth != null)
                playerHealth.OnHealthChanged -= HandleHealthChanged;

            if (playerExperience != null)
                playerExperience.OnExpChanged -= HandleExpChanged;

            if (weaponManager != null)
                weaponManager.OnWeaponsChanged -= HandleWeaponsChanged;
        }

        public void ForceUpdateAll()
        {
            if (playerStats != null) HandleStatsUpdated();
            if (playerHealth != null) HandleHealthChanged(playerHealth.CurrentHealth, playerStats != null ? playerStats.MaxHealth : 100f);
            if (playerExperience != null) HandleExpChanged(playerExperience.CurrentExp, playerExperience.MaxExp);
            if (weaponManager != null) HandleWeaponsChanged();
        }

        private void OnToggleMenuPressed(InputAction.CallbackContext context)
        {
            if (statsMenuView == null) return;

            isMenuOpen = !isMenuOpen;
            statsMenuView.gameObject.SetActive(isMenuOpen);
            
            if (isMenuOpen)
            {
                Time.timeScale = 0f;
                
                // Cập nhật data khi vừa mở menu để tránh tình trạng hiển thị Prefab mẫu 
                // (do Awake của Panel bị gọi trễ khi gameObject tắt ở đầu game).
                HandleStatsUpdated();
                
                // Nếu muốn tắt phím điều khiển khi mở menu:
                // inputActions.Gameplay.Disable();
            }
            else
            {
                Time.timeScale = 1f;
                // Bật lại điều khiển:
                // inputActions.Gameplay.Enable();
            }
        }

        private void HandleStatsUpdated()
        {
            if (statsMenuView != null && playerStats != null)
            {
                statsMenuView.UpdateStats(playerStats);
            }
        }

        private void HandleHealthChanged(float currentHealth, float maxHealth)
        {
            if (hudView != null)
            {
                hudView.UpdateHealth(currentHealth, maxHealth);
            }
        }

        private void HandleExpChanged(float currentExp, float maxExp)
        {
            if (hudView != null)
            {
                hudView.UpdateExp(currentExp, maxExp);
            }
        }

        private void HandleWeaponsChanged()
        {
            if (hudView != null && weaponManager != null)
            {
                hudView.UpdateSkills(weaponManager.ActiveWeapons);
            }
        }
    }
}
