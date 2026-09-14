using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Player.Core;

namespace ProjectZombie.Features.Enemies.Behaviors
{
    /// <summary>
    /// Chiến lược chọn người chơi còn sống ở cự ly gần nhất (Proximity Targeting Strategy).
    /// Triệt tiêu 100% GC Allocations trong vòng lặp chiến đấu.
    /// </summary>
    public class ProximityTargetSelector : ITargetSelector
    {
        public static readonly ProximityTargetSelector SharedInstance = new ProximityTargetSelector();

        public PlayerContext SelectTarget(Vector2 enemyPosition, IPlayerRegistry registry)
        {
            if (registry == null) return null;
            return registry.GetNearestLivingPlayer(enemyPosition);
        }
    }
}
