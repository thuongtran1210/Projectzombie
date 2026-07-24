using UnityEngine;
using ProjectZombie.Features.Projectiles.Components;
using ProjectZombie.Features.Projectiles.Behaviors;

namespace ProjectZombie.Features.Projectiles.Data
{
    [CreateAssetMenu(fileName = "PeriodicHitBehaviorData", menuName = "ProjectZombie/Projectiles/Behaviors/PeriodicHit")]
    public class PeriodicHitBehaviorData : ProjectileBehaviorData
    {
        [Tooltip("Khoảng thời gian (giây) giữa 2 lần giật máu trên cùng 1 mục tiêu")]
        public float hitCooldown = 0.5f;

        public override IProjectileBehavior CreateBehavior(ProjectileController controller)
        {
            return new PeriodicHitBehavior(controller, this);
        }
    }
}
