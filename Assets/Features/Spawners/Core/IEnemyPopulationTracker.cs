namespace ProjectZombie.Features.Spawners.Core
{
    /// <summary>
    /// Giao diện theo dõi sĩ số quái vật và điều phối áp lực (Pacing / Adaptive Catchup).
    /// </summary>
    public interface IEnemyPopulationTracker
    {
        int CurrentEnemyCount { get; }
        int MaxEnemyCap { get; set; }
        int MinEnemyFloor { get; set; }
        bool CanSpawnMore { get; }

        void RegisterSpawn(int count = 1);
        void OnEnemyDied(int count = 1);
        void ResetCount();
        float CalculateAdaptiveMultiplier(float catchupRate);
    }
}
