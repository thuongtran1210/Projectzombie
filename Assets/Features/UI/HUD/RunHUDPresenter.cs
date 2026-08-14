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

        private bool _isConstructed = false;

        public void Construct(HealthSystem health, PlayerStats stats, PlayerExperience experience, ProjectZombie.Features.Weapons.WeaponManager weaponManager = null)
        {
            if (_isConstructed)
            {
                UnsubscribeEvents();
            }

            _playerHealth = health;
            _playerStats = stats;
            _playerExp = experience;
            _weaponManager = weaponManager;

            SubscribeEvents();
            ForceRefreshAll();

            _isConstructed = true;
        }

        // ====================================================================
        // UNITY LIFECYCLE
        // ====================================================================

        private void Start()
        {
            // Tương thích ngược: nếu đã kéo thả trong Inspector thì tự động Construct luôn
            if (_playerHealth != null || _playerStats != null || _playerExp != null)
            {
                Construct(_playerHealth, _playerStats, _playerExp);
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

            // Subscribe Âm Dương Manager
            if (ProjectZombie.Features.YinYang.YinYangManager.Instance != null)
            {
                var yinyang = ProjectZombie.Features.YinYang.YinYangManager.Instance;
                yinyang.OnYinYangValueChanged += OnYinYangValueChanged;
                yinyang.OnTrackerActiveChanged += OnYinYangTrackerActiveChanged;
                
                if (_view != null)
                {
                    _view.SetYinYangActive(yinyang.IsTrackerActive);
                }
                OnYinYangValueChanged(yinyang.CurrentValue, yinyang.GetState());
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

            if (ProjectZombie.Features.YinYang.YinYangManager.Instance != null)
            {
                var yinyang = ProjectZombie.Features.YinYang.YinYangManager.Instance;
                yinyang.OnYinYangValueChanged -= OnYinYangValueChanged;
                yinyang.OnTrackerActiveChanged -= OnYinYangTrackerActiveChanged;
            }
        }

        private void OnYinYangTrackerActiveChanged(bool isActive)
        {
            if (_view != null)
            {
                _view.SetYinYangActive(isActive);
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
                _weaponManager.OnWeaponsChanged += OnWeaponsChanged;
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
                _weaponManager.OnWeaponsChanged -= OnWeaponsChanged;
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

            if (_weaponManager != null)
            {
                OnWeaponsChanged();
            }

            if (RunStatsTracker.Instance != null)
            {
                OnTimerTick(RunStatsTracker.Instance.ElapsedTime);
                OnKillCountChanged(RunStatsTracker.Instance.KillCount);
            }

            if (ProjectZombie.Features.YinYang.YinYangManager.Instance != null)
            {
                OnYinYangValueChanged(ProjectZombie.Features.YinYang.YinYangManager.Instance.CurrentValue, ProjectZombie.Features.YinYang.YinYangManager.Instance.GetState());
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
            // Presenter định dạng string TRƯỚC khi truyền cho View
            _view.SetLevel($"<b>Lv.{level}</b>");
        }

        private void OnTimerTick(float elapsedSeconds)
        {
            int m = Mathf.FloorToInt(elapsedSeconds / 60f);
            int s = Mathf.FloorToInt(elapsedSeconds % 60f);
            _view.SetTimer($"{m:00}:{s:00}");
        }

        private void OnKillCountChanged(int count)
        {
            // Dùng TMP Rich Text để tô màu số, không dùng _text.color = Color.red
            _view.SetKillCount($"Kills: <color=#FF8C42>{count}</color>");
        }

        private void OnWeaponsChanged()
        {
            if (_view == null || _weaponManager == null) return;

            var displaySkills = new System.Collections.Generic.List<RunHUDView.SkillDisplayData>();
            foreach (var weapon in _weaponManager.ActiveWeapons)
            {
                displaySkills.Add(new RunHUDView.SkillDisplayData
                {
                    Icon = null,
                    Level = weapon.WeaponLevel,
                    Name = $"Weapon {weapon.weaponId}",
                    Description = $"Description for {weapon.weaponId}"
                });
            }
            _view.UpdateSkills(displaySkills);
        }

        // ====================================================================
        // VONG XUYEN (v4.0) EVENT HANDLERS
        // ====================================================================

        public void OnYinYangValueChanged(float val, ProjectZombie.Features.YinYang.YinYangState state)
        {
            if (_view == null) return;

            string stateName = state switch
            {
                ProjectZombie.Features.YinYang.YinYangState.YinDominant => "<color=#4A90E2>Âm Thịnh</color>",
                ProjectZombie.Features.YinYang.YinYangState.YangDominant => "<color=#FF4444>Dương Thịnh</color>",
                _ => "<color=#FFD700>Thái Cực Cân Bằng</color>"
            };

            _view.SetYinYangBalance(val, stateName);
        }

        public void OnYinYangStateChanged(ProjectZombie.Features.YinYang.YinYangState state)
        {
            float val = ProjectZombie.Features.YinYang.YinYangManager.Instance != null 
                ? ProjectZombie.Features.YinYang.YinYangManager.Instance.CurrentValue 
                : 50f;
            OnYinYangValueChanged(val, state);
        }

        public void OnBossElementChanged(ElementType element)
        {
            if (_view == null) return;
            string elemName = element switch
            {
                ElementType.Kim => "<color=#FFD700>[BOSS: KIM]</color>",
                ElementType.Moc => "<color=#4CAF50>[BOSS: MỘC]</color>",
                ElementType.Thuy => "<color=#2196F3>[BOSS: THỦY]</color>",
                ElementType.Hoa => "<color=#FF5722>[BOSS: HỎA]</color>",
                ElementType.Tho => "<color=#795548>[BOSS: THỔ]</color>",
                _ => ""
            };

            _view.SetBossElement(elemName);
        }
    }
}
