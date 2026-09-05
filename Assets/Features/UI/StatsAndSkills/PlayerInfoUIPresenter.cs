using UnityEngine;
using System.Collections.Generic;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.UI.HUD;
using ProjectZombie.Features.UI.Helpers;
using ProjectZombie.Features.MetaProgression;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ProjectZombie.Features.UI.StatsAndSkills
{
    /// <summary>
    /// Presenter điều phối dữ liệu từ Player, WeaponManager, Passives và RunStatsTracker sang PlayerStatsMenuUIView.
    /// Quản lý mở/đóng Pause Menu trong trận, cập nhật 8 chỉ số RPG, thông tin tướng và trang bị.
    /// </summary>
    public class PlayerInfoUIPresenter : MonoBehaviour
    {
        [Header("Models / Logic")]
        [SerializeField] private PlayerStats _playerStats;
        [SerializeField] private HealthSystem _playerHealth;
        [SerializeField] private PlayerExperience _playerExperience;
        [SerializeField] private WeaponManager _weaponManager;
        [SerializeField] private PlayerPassives _playerPassives;

        [Header("Views")]
        [SerializeField] private RunHUDView _hudView;
        [SerializeField] private PlayerStatsMenuUIView _statsMenuView;

        private PlayerInputActions _inputActions;
        private bool _isMenuOpen = false;
        private bool _isConstructed = false;

        public bool IsMenuOpen => _isMenuOpen;

        private void Awake()
        {
            _inputActions = new PlayerInputActions();
            _inputActions.UI.TogglePauseMenu.performed += OnToggleMenuPressed;

            if (_statsMenuView == null)
            {
                _statsMenuView = GetComponent<PlayerStatsMenuUIView>();
                if (_statsMenuView == null) _statsMenuView = GetComponentInChildren<PlayerStatsMenuUIView>(true);
            }
        }

        private void OnEnable()
        {
            _inputActions?.UI.Enable();
        }

        private void OnDisable()
        {
            _inputActions?.UI.Disable();
        }

        public void Construct(PlayerStats stats, HealthSystem health, PlayerExperience experience, WeaponManager weaponManager, PlayerPassives passives = null)
        {
            if (_isConstructed)
            {
                UnsubscribeEvents();
            }

            _playerStats = stats;
            _playerHealth = health;
            _playerExperience = experience;
            _weaponManager = weaponManager;
            _playerPassives = passives;

            if (_playerPassives == null && _playerStats != null)
            {
                _playerPassives = _playerStats.GetComponent<PlayerPassives>();
            }

            SetupViewCallbacks();
            SubscribeEvents();
            ForceUpdateAll();

            _isConstructed = true;
        }

        private void Start()
        {
            if (_statsMenuView == null)
            {
                _statsMenuView = GetComponent<PlayerStatsMenuUIView>();
                if (_statsMenuView == null) _statsMenuView = GetComponentInChildren<PlayerStatsMenuUIView>(true);
            }

            // Tương thích ngược: nếu đã kéo thả trong Inspector thì tự động Construct luôn
            if (!_isConstructed && (_playerStats != null || _playerHealth != null || _playerExperience != null || _weaponManager != null))
            {
                Construct(_playerStats, _playerHealth, _playerExperience, _weaponManager, _playerPassives);
            }
            
            SetupViewCallbacks();
        }

        private void SetupViewCallbacks()
        {
            if (_statsMenuView != null)
            {
                _statsMenuView.SetCallbacks(
                    onClose: CloseMenu,
                    onResume: CloseMenu,
                    onSettings: HandleSettingsClicked,
                    onQuit: HandleQuitClicked
                );
            }
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
            if (_inputActions != null)
            {
                _inputActions.UI.TogglePauseMenu.performed -= OnToggleMenuPressed;
                _inputActions.Dispose();
            }
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

            if (_playerPassives != null)
                _playerPassives.OnPassivesChanged += HandlePassivesChanged;

            if (RunStatsTracker.Instance != null)
            {
                RunStatsTracker.Instance.OnTimerTick += HandleTimerTick;
                RunStatsTracker.Instance.OnKillCountChanged += HandleKillCountChanged;
                RunStatsTracker.Instance.OnCoinsChanged += HandleCoinsChanged;
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

            if (_playerPassives != null)
                _playerPassives.OnPassivesChanged -= HandlePassivesChanged;

            if (RunStatsTracker.Instance != null)
            {
                RunStatsTracker.Instance.OnTimerTick -= HandleTimerTick;
                RunStatsTracker.Instance.OnKillCountChanged -= HandleKillCountChanged;
                RunStatsTracker.Instance.OnCoinsChanged -= HandleCoinsChanged;
            }
        }

        private void HandleCoinsChanged(int coins)
        {
            UpdateCurrencyDisplay();
        }

        public void ForceUpdateAll()
        {
            UpdateHeroDisplay();
            UpdateWeaponDisplay();
            UpdateCurrencyDisplay();
            HandleStatsUpdated();
            HandlePassivesChanged();

            if (RunStatsTracker.Instance != null)
            {
                HandleTimerTick(RunStatsTracker.Instance.ElapsedTime);
                HandleKillCountChanged(RunStatsTracker.Instance.KillCount);
            }
        }

        private void OnToggleMenuPressed(InputAction.CallbackContext context)
        {
            ToggleMenu();
        }

        public void ToggleMenu()
        {
            if (_isMenuOpen) CloseMenu();
            else OpenMenu();
        }

        public void OpenMenu()
        {
            // Tuyệt đối không cho phép mở Pause Menu khi đang chọn nâng cấp Level Up
            if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.LevelUpSelection)
            {
                return;
            }

            if (_statsMenuView == null)
            {
                _statsMenuView = GetComponent<PlayerStatsMenuUIView>();
                if (_statsMenuView == null) _statsMenuView = GetComponentInChildren<PlayerStatsMenuUIView>(true);
            }
            if (_statsMenuView == null) return;

            _isMenuOpen = true;
            _statsMenuView.gameObject.SetActive(true);
            _statsMenuView.transform.SetAsLastSibling();
            SetupViewCallbacks();

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(GameState.Paused);
            }
            else
            {
                Time.timeScale = 0f;
            }

            ForceUpdateAll();
        }

        public void CloseMenu()
        {
            global::Core.Audio.AudioManager.Instance?.PlayUIClick();

            if (_statsMenuView == null) return;

            _isMenuOpen = false;
            _statsMenuView.gameObject.SetActive(false);

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(GameState.Playing);
            }
            else
            {
                Time.timeScale = 1f;
            }
        }

        private void HandleSettingsClicked()
        {
            global::Core.Audio.AudioManager.Instance?.PlayUIClick();

            var settingsPresenter = FindObjectOfType<SettingsModalPresenter>(true);
            if (settingsPresenter == null)
            {
                // Thử tìm trong Canvas cha hoặc Resources / Prefab
                var settingsPrefab = Resources.Load<GameObject>("UI/SettingsModalUI");
                if (settingsPrefab == null)
                {
#if UNITY_EDITOR
                    settingsPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/UI/SettingsModalUI.prefab");
#endif
                }

                if (settingsPrefab != null)
                {
                    Transform canvasTransform = transform.root;
                    var canvas = GetComponentInParent<Canvas>();
                    if (canvas != null) canvasTransform = canvas.transform;

                    GameObject settingsObj = Instantiate(settingsPrefab, canvasTransform);
                    settingsObj.name = "Modal_Settings";
                    settingsPresenter = settingsObj.GetComponent<SettingsModalPresenter>();
                }
            }

            if (settingsPresenter != null)
            {
                settingsPresenter.Open();
            }
            else
            {
                Debug.LogWarning("[PlayerInfoUIPresenter] SettingsModalPresenter not found and could not be loaded.");
            }
        }

        private void HandleQuitClicked()
        {
            global::Core.Audio.AudioManager.Instance?.PlayUIClick();

            // Kết thúc trận, lưu ngân lượng và về Sảnh Chính
            Time.timeScale = 1f;

            if (RunStatsTracker.Instance != null)
            {
                int earned = RunStatsTracker.Instance.CalculateMetaCurrency(false);
                if (ProjectZombie.Core.Save.GameManager.Instance != null)
                {
                    ProjectZombie.Core.Save.GameManager.Instance.OnRunCompleted(RunStatsTracker.Instance.ElapsedTime, RunStatsTracker.Instance.KillCount, earned);
                }
                else if (MetaCurrencyManager.Instance != null)
                {
                    MetaCurrencyManager.Instance.AddCurrency(earned);
                }
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(GameState.MainMenu);
            }

            SceneManager.LoadScene("SampleScene");
        }

        private void UpdateHeroDisplay()
        {
            if (_statsMenuView == null) return;

            var hero = RunLoadoutState.SelectedCharacter;
            if (hero == null)
            {
                var charDb = Resources.Load<CharacterDatabaseSO>("CharacterDatabase");
#if UNITY_EDITOR
                if (charDb == null)
                {
                    charDb = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterDatabaseSO>("Assets/_Data/CharacterDatabase.asset");
                }
#endif
                if (charDb != null && charDb.Characters != null && charDb.Characters.Count > 0)
                {
                    var so = charDb.GetCharacterByIndex(0);
                    if (so != null) hero = so.ToEntry();
                }
            }

            if (hero != null)
            {
                Sprite avatarSprite = hero.avatar;
                if (avatarSprite == null && hero.basicAttackConfig != null && hero.basicAttackConfig.attackIcon != null)
                {
                    avatarSprite = hero.basicAttackConfig.attackIcon;
                }
                if (avatarSprite == null && hero.playerPrefab != null)
                {
                    var sr = hero.playerPrefab.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null) avatarSprite = sr.sprite;
                }

                string elemBadge = ElementVisualHelper.GetElementBadgeRichText(hero.element);
                _statsMenuView.SetHeroInfo(avatarSprite, hero.characterName, elemBadge);
            }
            else
            {
                _statsMenuView.SetHeroInfo(null, "ĐẠO SĨ", "<color=#4DEEEA>[Mộc]</color>");
            }
        }

        private void UpdateWeaponDisplay()
        {
            if (_statsMenuView == null) return;

            WeaponBase primary = _weaponManager != null ? _weaponManager.PrimaryWeapon : null;
            if (primary != null)
            {
                string levelStr = primary.WeaponLevel >= primary.MaxLevel ? "<color=#FFD700>MAX CẤP</color>" : $"Cấp {primary.WeaponLevel}/{primary.MaxLevel}";
                string dpsStr = $"Sát thương: <color=#FF5722>{primary.GetDamage():F0}</color>";
                _statsMenuView.SetWeaponInfo(primary.icon, primary.displayName, levelStr, dpsStr);
            }
            else
            {
                _statsMenuView.SetWeaponInfo(null, "Chưa Trang Bị", "Cấp 0", "0 DPS");
            }
        }

        private void UpdateCurrencyDisplay()
        {
            if (_statsMenuView == null) return;

            int runGold = 0;
            if (RunStatsTracker.Instance != null)
            {
                runGold = RunStatsTracker.Instance.CoinsCollected;
            }
            _statsMenuView.SetCurrency($"Cổ Tiền: <color=#FFD700>{runGold}</color>");
        }

        private void HandleStatsUpdated()
        {
            if (_statsMenuView == null || _playerStats == null) return;

            // 1. Sinh Lực
            float curHp = _playerHealth != null ? _playerHealth.CurrentHealth : _playerStats.MaxHealth;
            float maxHp = _playerStats.MaxHealth;
            string hpStr = $"<color=#00FF88>{curHp:F0}</color> / <color=#FFFFFF>{maxHp:F0}</color>";
            _statsMenuView.UpdateHealth(hpStr);

            // 2. Công Kích
            float dmg = _playerStats.GetTotalDamage();
            float dmgMult = _playerStats.DamageMultiplier;
            string dmgStr = $"<color=#FF7043>{dmg:F1}</color> <size=80%><color=#F2D88C>(x{dmgMult:F2})</color></size>";
            _statsMenuView.UpdateDamage(dmgStr);

            // 3. Bạo Kích
            float crit = _playerStats.CritChance;
            float critMult = _playerStats.CritDamageMultiplier;
            string critStr = $"<color=#FFD700>{crit * 100f:F1}%</color> <size=80%><color=#F2D88C>(x{critMult:F1})</color></size>";
            _statsMenuView.UpdateCrit(critStr);

            // 4. Tốc Đánh
            float atkSpd = _playerStats.AttackSpeed;
            string atkSpdStr = $"<color=#4DEEEA>{atkSpd:F2}</color> đòn/s";
            _statsMenuView.UpdateAttackSpeed(atkSpdStr);

            // 5. Thân Pháp
            float spd = _playerStats.MoveSpeed;
            string spdStr = $"<color=#00FF88>{spd:F1}</color> m/s";
            _statsMenuView.UpdateSpeed(spdStr);

            // 6. Phi Vân (Hồi chiêu Lướt)
            float dash = _playerStats.DashCooldown;
            string dashStr = $"<color=#4DEEEA>{dash:F1}s</color>";
            _statsMenuView.UpdateDashCooldown(dashStr);

            // 7. Thu Hút (Bán kính nam châm)
            float pickup = _playerStats.PickupRange;
            string pickupStr = $"<color=#FFD700>{pickup:F1}m</color>";
            _statsMenuView.UpdatePickupRange(pickupStr);

            // 8. Ngộ Tính (Exp Multiplier)
            float exp = _playerStats.ExpMultiplier;
            string expStr = $"<color=#00FF88>+{(exp - 1f) * 100f:F0}%</color>";
            _statsMenuView.UpdateExpMultiplier(expStr);
        }

        private void HandlePassivesChanged()
        {
            if (_statsMenuView == null || _playerPassives == null) return;

            var passivesList = new List<(Sprite icon, string name, string description, int level)>();
            var dataMap = _playerPassives.PassiveDataMap;

            foreach (var kvp in dataMap)
            {
                var upgrade = kvp.Value;
                if (upgrade != null)
                {
                    int lvl = _playerPassives.GetUpgradeCount(kvp.Key);
                    if (lvl <= 0) lvl = 1;
                    passivesList.Add((upgrade.icon, upgrade.upgradeName, upgrade.description, lvl));
                }
            }

            _statsMenuView.SetPassives(passivesList);
        }

        private void HandleHealthChanged(float current, float max)
        {
            if (_isMenuOpen) HandleStatsUpdated();
        }

        private void HandleExpChanged(float current, float max)
        {
            // Exp cập nhật
        }

        private void HandleWeaponsChanged()
        {
            if (_isMenuOpen) UpdateWeaponDisplay();
        }

        private void HandleTimerTick(float time)
        {
            if (_statsMenuView == null) return;
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            string timerFormatted = $"Thời Gian: <color=#00FF88>{minutes:00}:{seconds:00}</color>";
            
            int kills = RunStatsTracker.Instance != null ? RunStatsTracker.Instance.KillCount : 0;
            string killsFormatted = $"Diệt Quái: <color=#FF5722>{kills}</color>";

            _statsMenuView.SetRunStats(timerFormatted, killsFormatted);
            UpdateCurrencyDisplay();
        }

        private void HandleKillCountChanged(int count)
        {
            if (_statsMenuView == null) return;
            string killsFormatted = $"Diệt Quái: <color=#FF5722>{count}</color>";
            _statsMenuView.SetRunStats(null, killsFormatted);
            UpdateCurrencyDisplay();
        }
    }
}
