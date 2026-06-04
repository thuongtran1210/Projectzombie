using UnityEngine;

namespace ProjectZombie.Features.Projectiles.Core
{
    public class ProjectileRuntimeState
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

        public ProjectileRuntimeState(int generation = 0)
        {
            Generation = generation;
            HitCount = 0;
            DistanceTraveled = 0f;
            CurrentTarget = null;
        }
    }
}
