using UnityEngine;

namespace ProjectZombie.Features.Projectiles.Core
{
    public struct ProjectileRuntimeState
    {
        public int HitCount;
        public float DistanceTraveled;
        public Vector2 SpawnPosition;
        
        public int RemainingPierce;
        public int RemainingBounce;
        public int RemainingSplit;
        
        public Transform CurrentTarget;
        
        /// <summary>
        /// Thế hệ của đạn (dùng cho SplitBehavior để tránh đẻ đạn vô hạn).
        /// Đạn gốc = 0, Đạn vỡ lần 1 = 1, ...
        /// </summary>
        public int Generation;

        public void Reset(int generation = 0, Vector2 spawnPosition = default)
        {
            Generation = generation;
            SpawnPosition = spawnPosition;
            HitCount = 0;
            DistanceTraveled = 0f;
            RemainingPierce = 0;
            RemainingBounce = 0;
            RemainingSplit = 0;
            CurrentTarget = null;
        }
    }
}
