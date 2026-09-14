using System.Threading.Tasks;
using UnityEngine;

namespace ProjectZombie.Features.Spawners
{
    /// <summary>
    /// Giao diện dịch vụ điều phối spawn và quản lý tiến trình trận đấu (Spawn & Match Pacing Service).
    /// </summary>
    public interface ISpawnService
    {
        float MatchTime { get; }
        int CurrentEnemyCount { get; }
        bool IsMatchActive { get; }
        float MatchProgress { get; }
        int CurrentWaveIndex { get; }
        int TotalWaves { get; }
        
        void StartMatch();
        Task StartMatchAsync();
        void StopMatch();
        void StopMatchAndClearAllEnemies();
        GameObject SpawnAtPosition(GameObject prefab, Vector3 position, string poolKey = null);
    }
}
