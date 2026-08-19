// ============================================================================
// FILE: RunHUDPresenter.cs — TẦNG PRESENTER (MVP)
// Trách nhiệm: Subscribe Model events, format dữ liệu, gọi View để render.
// KHÔNG tự render UI. KHÔNG chứa dữ liệu game.
// ============================================================================

using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.UI.HUD
{
    /// <summary>
    /// Presenter điều phối dữ liệu từ các Model (PlayerStats, RunStatsTracker, PlayerExperience)
    /// sang RunHUDView để hiển thị.
    ///
    /// HƯỚNG DẪN SỬ DỤNG:
    /// 1. Gắn script này vào cùng GameObject với RunHUDView (hoặc Parent của nó).
    /// 2. Kéo RunHUDView vào trường _view trong Inspector.
    /// 3. Kéo PlayerStats, HealthSystem, PlayerExperience từ Player vào Inspector.
    /// 4. RunStatsTracker được lấy qua Singleton vì nó quản lý run-level data.
    /// </summary>
    public class RunHUDPresenter : MonoBehaviour
    {
        // ====================================================================
        // [INSPECTOR] — View
        // ====================================================================

        [Header("View")]
        [SerializeField] private RunHUDView _view;

        // ====================================================================
        // [INSPECTOR] — Model References (inject qua Inspector, không dùng Find)
        // ====================================================================

        [Header("Model References")]
        [SerializeField] private HealthSystem _playerHealth;
        [SerializeField] private PlayerStats _playerStats;
        [SerializeField] private PlayerExperience _playerExp;
        [SerializeField] private ProjectZombie.Features.Weapons.WeaponManager _weaponManager;
        [SerializeField] private PlayerPassives _playerPassives;

        private bool _isConstructed = false;

        public void Construct(HealthSystem health, PlayerStats stats, PlayerExperience experience, ProjectZombie.Features.Weapons.WeaponManager weaponManager = null, PlayerPassives passives = null)
        {
            if (_isConstructed)
            {
                UnsubscribeEvents();
            }

            _playerHealth = health;
            _playerStats = stats;
            _playerExp = experience;
            _weaponManager = weaponManager;
            _playerPassives = passives;

            SubscribeEvents();
            ForceRefreshAll();

            _isConstructed = true;

            if (_view != null)
            {
                _view.gameObject.SetActive(true);
            }
        }

        // ====================================================================
        // UNITY LIFECYCLE
        // ====================================================================

        private void Start()
        {
            // Tương thích ngược: nếu đã kéo thả trong Inspector thì tự động Construct luôn
            if (_playerHealth != null || _playerStats != null || _playerExp != null)
            {
                Construct(_playerHealth, _playerStats, _playerExp, _weaponManager, _playerPassives);
            }
            else if (!_isConstructed && _view != null)
            {
                _view.gameObject.SetActive(false);
            }

            // RunStatsTracker là Singleton toàn run — subscribe nếu tồn tại
            if (RunStatsTracker.Instance != null)
            {
                RunStatsTracker.Instance.OnTimerTick        += OnTimerTick;
                RunStatsTracker.Instance.OnKillCountChanged += OnKillCountChanged;
            }
            else
            {
                Debug.LogWarning("[RunHUDPresenter] RunStatsTracker.Instance không tìm thấy. Timer/Kill sẽ không cập nhật.");
            }

            // Subscribe trạng thái Game
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();

            if (RunStatsTracker.Instance != null)
            {
                RunStatsTracker.Instance.OnTimerTick        -= OnTimerTick;
                RunStatsTracker.Instance.OnKillCountChanged -= OnKillCountChanged;
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void SubscribeEvents()
        {
            if (_playerHealth != null)
                _playerHealth.OnHealthChanged += OnHealthChanged;

            if (_playerExp != null)
            {
                _playerExp.OnExpChanged += OnExpChanged;
                _playerExp.OnLevelUp    += OnLevelUp;
            }

            if (_weaponManager != null)
            {
                _weaponManager.OnWeaponsChanged += OnSkillsOrPassivesChanged;
            }

            if (_playerPassives != null)
            {
                _playerPassives.OnPassivesChanged += OnSkillsOrPassivesChanged;
            }
        }

        private void UnsubscribeEvents()
        {
            if (_playerHealth != null)
                _playerHealth.OnHealthChanged -= OnHealthChanged;

            if (_playerExp != null)
            {
                _playerExp.OnExpChanged -= OnExpChanged;
                _playerExp.OnLevelUp    -= OnLevelUp;
            }

            if (_weaponManager != null)
            {
                _weaponManager.OnWeaponsChanged -= OnSkillsOrPassivesChanged;
            }

            if (_playerPassives != null)
            {
                _playerPassives.OnPassivesChanged -= OnSkillsOrPassivesChanged;
            }
        }

        // ====================================================================
        // PUBLIC API
        // ====================================================================

        /// <summary>
        /// Buộc View cập nhật lại toàn bộ các giá trị từ Model hiện tại.
        /// Gọi sau khi áp dụng Upgrade hoặc khi mở màn hình Stats.
        /// </summary>
        public void ForceRefreshAll()
        {
            if (_playerHealth != null && _playerStats != null)
                OnHealthChanged(_playerHealth.CurrentHealth, _playerStats.MaxHealth);

            if (_playerExp != null)
            {
                OnExpChanged(_playerExp.CurrentExp, _playerExp.MaxExp);
                OnLevelUp(_playerExp.CurrentLevel);
            }

            OnSkillsOrPassivesChanged();

            if (RunStatsTracker.Instance != null)
            {
                OnTimerTick(RunStatsTracker.Instance.ElapsedTime);
                OnKillCountChanged(RunStatsTracker.Instance.KillCount);
            }
        }

        private void HandleStateChanged(GameState newState)
        {
            if (_view == null) return;
            // Chỉ hiển thị HUD khi đang chơi, tạm dừng hoặc đang chọn nâng cấp
            bool shouldShow = (newState == GameState.Playing || newState == GameState.Paused || newState == GameState.LevelUpSelection);
            _view.gameObject.SetActive(shouldShow);
        }

        // ====================================================================
        // EVENT HANDLERS — Format dữ liệu rồi đẩy sang View
        // ====================================================================

        private void OnHealthChanged(float current, float max)
        {
            // View nhận giá trị thô — View tự render "75 / 100"
            _view.SetHealth(current, max);
        }

        private void OnExpChanged(float current, float max)
        {
            _view.SetExp(current, max);
        }

        private void OnLevelUp(int level)
        {
            // Presenter định dạng string TRƯỚC khi truyền cho View (Chuẩn Hoàng Kim #FFD700)
            _view.SetLevel($"<color=#FFD700><b>Lv.{level}</b></color>");
        }

        private void OnTimerTick(float elapsedSeconds)
        {
            int m = Mathf.FloorToInt(elapsedSeconds / 60f);
            int s = Mathf.FloorToInt(elapsedSeconds % 60f);
            _view.SetTimer($"{m:00}:{s:00}");
        }

        private void OnKillCountChanged(int count)
        {
            // Hiển thị icon skull và số lượng quái hạ với màu cam lửa (#FF8C42)
            _view.SetKillCount($"💀 <color=#FF8C42>{count}</color>");
        }

        private void OnSkillsOrPassivesChanged()
        {
            if (_view == null) return;

            var displaySkills = new System.Collections.Generic.List<RunHUDView.SkillDisplayData>();

            // 1. Thu thập danh sách Vũ khí (Active Weapons)
            if (_weaponManager != null)
            {
                foreach (var weapon in _weaponManager.ActiveWeapons)
                {
                    if (weapon == null) continue;

                    string weaponName = !string.IsNullOrEmpty(weapon.displayName) ? weapon.displayName : weapon.weaponId;
                    string desc = !string.IsNullOrEmpty(weapon.description) ? weapon.description : $"Pháp Bảo cấp {weapon.WeaponLevel}";

                    displaySkills.Add(new RunHUDView.SkillDisplayData
                    {
                        Icon = weapon.icon,
                        Level = weapon.WeaponLevel,
                        Name = weaponName,
                        Description = desc
                    });
                }
            }

            // 2. Thu thập danh sách Thẻ Bị Động (Passives)
            if (_playerPassives != null)
            {
                foreach (var passiveId in _playerPassives.ActivePassives)
                {
                    int level = _playerPassives.GetUpgradeCount(passiveId);
                    if (level <= 0) level = 1;

                    Sprite icon = null;
                    string name = passiveId;
                    string desc = $"Bị động: {passiveId}";

                    if (_playerPassives.PassiveDataMap.TryGetValue(passiveId, out var data) && data != null)
                    {
                        icon = data.icon;
                        if (!string.IsNullOrEmpty(data.upgradeName)) name = data.upgradeName;
                        if (!string.IsNullOrEmpty(data.description)) desc = data.description;
                    }

                    displaySkills.Add(new RunHUDView.SkillDisplayData
                    {
                        Icon = icon,
                        Level = level,
                        Name = name,
                        Description = desc
                    });
                }
            }

            _view.UpdateSkills(displaySkills);
        }
    }
}
