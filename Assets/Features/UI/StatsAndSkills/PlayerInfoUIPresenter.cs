using UnityEngine;
using System.Collections.Generic;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Shared;
using UnityEngine.InputSystem;

namespace ProjectZombie.Features.UI.StatsAndSkills
{
    public class PlayerInfoUIPresenter : MonoBehaviour
    {
        [Header("Models / Logic")]
        [SerializeField] private PlayerStats _playerStats;
        [SerializeField] private HealthSystem _playerHealth;
        [SerializeField] private PlayerExperience _playerExperience;
        [SerializeField] private WeaponManager _weaponManager;

        [Header("Views")]
        [SerializeField] private PlayerHUDView _hudView;
        [SerializeField] private PlayerStatsMenuUIView _statsMenuView;

        private PlayerInputActions _inputActions;
        private bool _isMenuOpen = false;

        private void Awake()
        {
            _inputActions = new PlayerInputActions();
            _inputActions.UI.TogglePauseMenu.performed += OnToggleMenuPressed;
        }

        private void OnEnable()
        {
            _inputActions.UI.Enable();
        }

        private void OnDisable()
        {
            _inputActions.UI.Disable();
        }

        private bool _isConstructed = false;

        public void Construct(PlayerStats stats, HealthSystem health, PlayerExperience experience, WeaponManager weaponManager)
        {
            if (_isConstructed)
            {
                UnsubscribeEvents();
            }

            _playerStats = stats;
            _playerHealth = health;
            _playerExperience = experience;
            _weaponManager = weaponManager;

            SubscribeEvents();
            ForceUpdateAll();

            _isConstructed = true;
        }

        private void Start()
        {
            // Tương thích ngược: nếu đã kéo thả trong Inspector thì tự động Construct luôn
            if (_playerStats != null || _playerHealth != null || _playerExperience != null || _weaponManager != null)
            {
                Construct(_playerStats, _playerHealth, _playerExperience, _weaponManager);
            }
            
            // Ensure menu is closed on start
            if (_statsMenuView != null)
            {
                _statsMenuView.gameObject.SetActive(false);
            }
            _isMenuOpen = false;
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (_playerStats != null)
                _playerStats.OnStatsUpdated += HandleStatsUpdated;

            if (_playerHealth != null)
                _playerHealth.OnHealthChanged += HandleHealthChanged;

            if (_playerExperience != null)
                _playerExperience.OnExpChanged += HandleExpChanged;

            if (_weaponManager != null)
                _weaponManager.OnWeaponsChanged += HandleWeaponsChanged;

            if (RunStatsTracker.Instance != null)
            {
                RunStatsTracker.Instance.OnTimerTick += HandleTimerTick;
                RunStatsTracker.Instance.OnKillCountChanged += HandleKillCountChanged;
            }
        }

        private void UnsubscribeEvents()
        {
            if (_playerStats != null)
                _playerStats.OnStatsUpdated -= HandleStatsUpdated;

            if (_playerHealth != null)
                _playerHealth.OnHealthChanged -= HandleHealthChanged;

            if (_playerExperience != null)
                _playerExperience.OnExpChanged -= HandleExpChanged;

            if (_weaponManager != null)
                _weaponManager.OnWeaponsChanged -= HandleWeaponsChanged;

            if (RunStatsTracker.Instance != null)
            {
                RunStatsTracker.Instance.OnTimerTick -= HandleTimerTick;
                RunStatsTracker.Instance.OnKillCountChanged -= HandleKillCountChanged;
            }
        }

        public void ForceUpdateAll()
        {
            if (_playerStats != null) HandleStatsUpdated();
            if (_playerHealth != null) HandleHealthChanged(_playerHealth.CurrentHealth, _playerStats != null ? _playerStats.MaxHealth : 100f);
            if (_playerExperience != null) HandleExpChanged(_playerExperience.CurrentExp, _playerExperience.MaxExp);
            if (_weaponManager != null) HandleWeaponsChanged();
            if (RunStatsTracker.Instance != null)
            {
                HandleTimerTick(RunStatsTracker.Instance.ElapsedTime);
                HandleKillCountChanged(RunStatsTracker.Instance.KillCount);
            }
        }

        private void OnToggleMenuPressed(InputAction.CallbackContext context)
        {
            if (_statsMenuView == null) return;

            _isMenuOpen = !_isMenuOpen;
            _statsMenuView.gameObject.SetActive(_isMenuOpen);
            
            if (_isMenuOpen)
            {
                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.ChangeState(GameState.Paused);
                }
                else
                {
                    Time.timeScale = 0f;
                }
                HandleStatsUpdated();
            }
            else
            {
                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.ChangeState(GameState.Playing);
                }
                else
                {
                    Time.timeScale = 1f;
                }
            }
        }

        private void HandleStatsUpdated()
        {
            if (_statsMenuView != null && _playerStats != null)
            {
                // Format Damage
                float dmg = _playerStats.GetTotalDamage();
                string dmgStr = RarityColorUtility.FormatText(dmg.ToString("F1"), GetDamageRarity(dmg));
                _statsMenuView.UpdateDamage(dmgStr);

                // Format Move Speed
                float spd = _playerStats.MoveSpeed;
                string spdStr = RarityColorUtility.FormatText(spd.ToString("F1"), GetSpeedRarity(spd));
                _statsMenuView.UpdateSpeed(spdStr);

                // Format Crit Chance
                float crit = _playerStats.CritChance;
                string critStr = RarityColorUtility.FormatText((crit * 100f).ToString("F1") + "%", GetCritRarity(crit));
                _statsMenuView.UpdateCrit(critStr);

                // Format Attack Speed
                float atkSpd = _playerStats.AttackSpeed;
                string atkSpdStr = RarityColorUtility.FormatText(atkSpd.ToString("F2"), GetAttackSpeedRarity(atkSpd));
                _statsMenuView.UpdateAttackSpeed(atkSpdStr);

                // Format Max Health
                float hp = _playerStats.MaxHealth;
                string hpStr = RarityColorUtility.FormatText(hp.ToString("F0"), GetHealthRarity(hp));
                _statsMenuView.UpdateMaxHealth(hpStr);

                // Format Dash Cooldown
                float dash = _playerStats.DashCooldown;
                string dashStr = RarityColorUtility.FormatText(dash.ToString("F1") + "s", GetDashCooldownRarity(dash));
                _statsMenuView.UpdateDashCooldown(dashStr);

                // Format Pickup Range
                float pickup = _playerStats.PickupRange;
                string pickupStr = RarityColorUtility.FormatText(pickup.ToString("F1"), GetPickupRangeRarity(pickup));
                _statsMenuView.UpdatePickupRange(pickupStr);

                // Format Exp Multiplier
                float exp = _playerStats.ExpMultiplier;
                string expStr = RarityColorUtility.FormatText((exp * 100f).ToString("F0") + "%", GetExpRarity(exp));
                _statsMenuView.UpdateExpMultiplier(expStr);
            }
        }

        private void HandleHealthChanged(float currentHealth, float maxHealth)
        {
            if (_hudView != null)
            {
                _hudView.UpdateHealth(currentHealth, maxHealth);
            }
        }

        private void HandleExpChanged(float currentExp, float maxExp)
        {
            if (_hudView != null)
            {
                _hudView.UpdateExp(currentExp, maxExp);
            }
        }

        private void HandleWeaponsChanged()
        {
            if (_hudView != null && _weaponManager != null)
            {
                var displaySkills = new List<PlayerHUDView.SkillDisplayData>();
                foreach (var weapon in _weaponManager.ActiveWeapons)
                {
                    displaySkills.Add(new PlayerHUDView.SkillDisplayData
                    {
                        Icon = null, // Có thể lấy icon thực tế từ config/database của weapon nếu có
                        Level = weapon.WeaponLevel,
                        Name = $"Weapon {weapon.weaponId}",
                        Description = $"Description for {weapon.weaponId}"
                    });
                }
                _hudView.UpdateSkills(displaySkills);
            }
        }

        private void HandleTimerTick(float elapsedSeconds)
        {
            if (_hudView != null)
            {
                int minutes = Mathf.FloorToInt(elapsedSeconds / 60f);
                int seconds = Mathf.FloorToInt(elapsedSeconds % 60f);
                _hudView.SetTimer($"{minutes:00}:{seconds:00}");
            }
        }

        private void HandleKillCountChanged(int count)
        {
            if (_hudView != null)
            {
                _hudView.SetKillCount($"💀 {count}");
            }
        }

        #region Thresholds - Điều kiện Rarity
        private Rarity GetDamageRarity(float value)
        {
            if (value >= 200f) return Rarity.Mythic;
            if (value >= 100f) return Rarity.Legendary;
            if (value >= 50f) return Rarity.Epic;
            if (value >= 30f) return Rarity.Rare;
            if (value >= 15f) return Rarity.Uncommon;
            return Rarity.Common;
        }

        private Rarity GetSpeedRarity(float value)
        {
            if (value >= 10f) return Rarity.Mythic;
            if (value >= 8f) return Rarity.Legendary;
            if (value >= 6f) return Rarity.Epic;
            if (value >= 5f) return Rarity.Rare;
            if (value >= 4f) return Rarity.Uncommon;
            return Rarity.Common;
        }

        private Rarity GetCritRarity(float value)
        {
            if (value >= 0.5f) return Rarity.Mythic;
            if (value >= 0.3f) return Rarity.Legendary;
            if (value >= 0.2f) return Rarity.Epic;
            if (value >= 0.1f) return Rarity.Rare;
            if (value >= 0.05f) return Rarity.Uncommon;
            return Rarity.Common;
        }

        private Rarity GetAttackSpeedRarity(float value)
        {
            if (value >= 3f) return Rarity.Mythic;
            if (value >= 2f) return Rarity.Legendary;
            if (value >= 1.5f) return Rarity.Epic;
            if (value >= 1.2f) return Rarity.Rare;
            if (value >= 1.1f) return Rarity.Uncommon;
            return Rarity.Common;
        }

        private Rarity GetHealthRarity(float value)
        {
            if (value >= 500f) return Rarity.Mythic;
            if (value >= 300f) return Rarity.Legendary;
            if (value >= 200f) return Rarity.Epic;
            if (value >= 150f) return Rarity.Rare;
            if (value >= 120f) return Rarity.Uncommon;
            return Rarity.Common;
        }

        private Rarity GetDashCooldownRarity(float value)
        {
            if (value <= 0.5f) return Rarity.Mythic;
            if (value <= 1.0f) return Rarity.Legendary;
            if (value <= 1.5f) return Rarity.Epic;
            if (value <= 2.0f) return Rarity.Rare;
            if (value <= 2.5f) return Rarity.Uncommon;
            return Rarity.Common;
        }

        private Rarity GetPickupRangeRarity(float value)
        {
            if (value >= 10f) return Rarity.Mythic;
            if (value >= 7f) return Rarity.Legendary;
            if (value >= 5f) return Rarity.Epic;
            if (value >= 3f) return Rarity.Rare;
            if (value >= 2f) return Rarity.Uncommon;
            return Rarity.Common;
        }

        private Rarity GetExpRarity(float value)
        {
            if (value >= 3f) return Rarity.Mythic;
            if (value >= 2f) return Rarity.Legendary;
            if (value >= 1.5f) return Rarity.Epic;
            if (value >= 1.2f) return Rarity.Rare;
            if (value >= 1.1f) return Rarity.Uncommon;
            return Rarity.Common;
        }
        #endregion
    }
}
