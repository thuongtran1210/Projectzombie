using System;
using UnityEngine;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Singleton theo dõi các thống kê trong một lượt chơi (run).
    /// Reset tự động khi scene được load lại.
    /// Được HUD, GameOverScreen và GameManager đọc để hiển thị kết quả và tính Currency Meta.
    /// </summary>
    public class RunStatsTracker : MonoBehaviour
    {
        // ====================================================================
        // SINGLETON
        // ====================================================================
        public static RunStatsTracker Instance { get; private set; }

        // ====================================================================
        // THỐNG KÊ TRONG RUN
        // ====================================================================

        /// <summary>Thời gian sống sót (giây).</summary>
        public float ElapsedTime { get; private set; } = 0f;

        /// <summary>Tổng số kẻ địch đã tiêu diệt.</summary>
        public int KillCount { get; private set; } = 0;

        /// <summary>Tổng sát thương đã gây ra.</summary>
        public float TotalDamageDealt { get; private set; } = 0f;

        /// <summary>Cấp độ cao nhất đạt được trong run này.</summary>
        public int MaxLevelReached { get; private set; } = 1;

        // ====================================================================
        // EVENTS
        // ====================================================================

        /// <summary>Kích hoạt khi Kill Count thay đổi. Truyền tổng kill count mới.</summary>
        public event Action<int> OnKillCountChanged;

        /// <summary>Kích hoạt mỗi giây để HUD cập nhật Timer.</summary>
        public event Action<float> OnTimerTick;

        // ====================================================================
        // UNITY LIFECYCLE
        // ====================================================================

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private float _timerTickAccumulator = 0f;
        private bool _isTracking = false;

        private void Update()
        {
            if (!_isTracking) return;

            ElapsedTime += Time.deltaTime;

            // Kích hoạt event Timer mỗi giây để HUD cập nhật (tránh gọi mỗi frame)
            _timerTickAccumulator += Time.deltaTime;
            if (_timerTickAccumulator >= 1f)
            {
                _timerTickAccumulator -= 1f;
                OnTimerTick?.Invoke(ElapsedTime);
            }
        }

        // ====================================================================
        // PUBLIC API — Được gọi bởi các hệ thống khác
        // ====================================================================

        /// <summary>
        /// Bắt đầu đếm thời gian và ghi nhận chỉ số khi trận đấu chính thức bắt đầu.
        /// </summary>
        public void StartTracking()
        {
            ResetStats();
            _isTracking = true;
        }

        /// <summary>
        /// Reset toàn bộ chỉ số về 0.
        /// </summary>
        public void ResetStats()
        {
            ElapsedTime = 0f;
            KillCount = 0;
            TotalDamageDealt = 0f;
            MaxLevelReached = 1;
            _timerTickAccumulator = 0f;
            OnTimerTick?.Invoke(0f);
            OnKillCountChanged?.Invoke(0);
        }

        /// <summary>
        /// Gọi bởi Enemy khi chết để cộng dồn kill count.
        /// </summary>
        public void RegisterKill()
        {
            KillCount++;
            OnKillCountChanged?.Invoke(KillCount);
        }

        /// <summary>
        /// Gọi bởi hệ thống sát thương để cộng dồn damage đã gây.
        /// </summary>
        public void RegisterDamage(float amount)
        {
            TotalDamageDealt += amount;
        }

        /// <summary>
        /// Gọi bởi PlayerExperience khi lên cấp.
        /// </summary>
        public void RegisterLevelUp(int newLevel)
        {
            if (newLevel > MaxLevelReached)
                MaxLevelReached = newLevel;
        }

        /// <summary>
        /// Dừng đếm thời gian (gọi khi player chết hoặc game over).
        /// </summary>
        public void StopTracking()
        {
            _isTracking = false;
        }

        // ====================================================================
        // TÍNH TOÁN CURRENCY META
        // ====================================================================

        /// <summary>
        /// Tính toán lượng Currency Meta nhận được sau run này.
        /// Công thức: (Kill * 1) + (Minute sống sót * 10) + (MaxLevel * 5).
        /// Có thể điều chỉnh theo GDD sau playtest.
        /// </summary>
        public int CalculateMetaCurrency()
        {
            int fromKills    = KillCount * 1;
            int fromTime     = Mathf.FloorToInt(ElapsedTime / 60f) * 10;
            int fromLevel    = MaxLevelReached * 5;
            return fromKills + fromTime + fromLevel;
        }

        /// <summary>Định dạng ElapsedTime thành chuỗi MM:SS.</summary>
        public string GetFormattedTime()
        {
            int minutes = Mathf.FloorToInt(ElapsedTime / 60f);
            int seconds = Mathf.FloorToInt(ElapsedTime % 60f);
            return $"{minutes:00}:{seconds:00}";
        }
    }
}
