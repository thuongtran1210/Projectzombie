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

        private bool _isConstructed = false;

        public void Construct(HealthSystem health, PlayerStats stats, PlayerExperience experience)
        {
            if (_isConstructed)
            {
                UnsubscribeEvents();
            }

            _playerHealth = health;
            _playerStats = stats;
            _playerExp = experience;

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
            _view.SetKillCount($"💀 <color=#FF8C42>{count}</color>");
        }
    }
}
