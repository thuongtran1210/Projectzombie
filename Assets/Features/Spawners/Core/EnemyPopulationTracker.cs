using UnityEngine;

namespace ProjectZombie.Features.Spawners.Core
{
    /// <summary>
    /// Quản lý kiểm soát trần số lượng quái vật (Enemy Cap),
    /// và thuật toán bù quái thích ứng (Adaptive Catchup) tránh khoảng lặng trận đấu.
    /// </summary>
    public class EnemyPopulationTracker : IEnemyPopulationTracker
    {
        private int _currentEnemyCount;
        private int _maxEnemyCap;
        private int _minEnemyFloor;

        public int CurrentEnemyCount => _currentEnemyCount;
        public int MaxEnemyCap
        {
            get => _maxEnemyCap;
            set => _maxEnemyCap = Mathf.Max(1, value);
        }

        public int MinEnemyFloor
        {
            get => _minEnemyFloor;
            set => _minEnemyFloor = Mathf.Max(0, value);
        }

        public bool CanSpawnMore => _currentEnemyCount < _maxEnemyCap;

        public EnemyPopulationTracker(int maxCap = 50, int minFloor = 8)
        {
            _maxEnemyCap = maxCap;
            _minEnemyFloor = minFloor;
            _currentEnemyCount = 0;
        }

        public void RegisterSpawn(int count = 1)
        {
            _currentEnemyCount += count;
        }

        public void OnEnemyDied(int count = 1)
        {
            _currentEnemyCount = Mathf.Max(0, _currentEnemyCount - count);
        }

        public void ResetCount()
        {
            _currentEnemyCount = 0;
        }

        /// <summary>
        /// Tính toán hệ số gia tốc spawn khi số quái trên sân tụt xuống dưới ngưỡng minEnemyFloor.
        /// </summary>
        public float CalculateAdaptiveMultiplier(float catchupRate)
        {
            if (_currentEnemyCount >= _minEnemyFloor || _minEnemyFloor <= 0)
            {
                return 1.0f;
            }

            // Khi quái = 0, deficitRatio = 1.0 -> timeMultiplier đạt tối đa (1 + catchupRate)
            float deficitRatio = 1f - ((float)_currentEnemyCount / _minEnemyFloor);
            return 1.0f + (deficitRatio * catchupRate);
        }
    }
}
