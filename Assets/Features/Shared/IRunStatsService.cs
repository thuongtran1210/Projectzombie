using System;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Contract cung cấp các số liệu thống kê trong một lượt chơi (Run).
    /// Giúp UI Presenters (như RunHUDPresenter, GameOverPresenter) lấy dữ liệu mà không phụ thuộc vào Singleton RunStatsTracker.
    /// Tuân thủ nguyên tắc Dependency Inversion Principle (DIP).
    /// </summary>
    public interface IRunStatsService
    {
        float ElapsedTime { get; }
        int KillCount { get; }
        float TotalDamageDealt { get; }
        int MaxLevelReached { get; }
        int CoinsCollected { get; }

        event Action<int> OnKillCountChanged;
        event Action<int> OnCoinsChanged;
        event Action<float> OnTimerTick;

        void StartTracking();
        void StopTracking();
        void ResetStats();

        void RegisterKill();
        void RegisterCoinCollected(int amount);
        void DeductCoins(int amount);
        void AddCoins(int amount);
        void RegisterDamage(float amount);
        void RegisterLevelUp(int newLevel);

        int CalculateMetaCurrency(bool isVictory = false);
        string GetFormattedTime();
    }
}
