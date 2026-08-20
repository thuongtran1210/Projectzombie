using UnityEngine;
using ProjectZombie.Features.Projectiles.Components;
using ProjectZombie.Features.Projectiles.Behaviors;

namespace ProjectZombie.Features.Projectiles.Data
{
    [CreateAssetMenu(fileName = "PeriodicHitBehaviorData", menuName = "ProjectZombie/Projectiles/Behaviors/PeriodicHit")]
    public class PeriodicHitBehaviorData : ProjectileBehaviorData
    {
        [Tooltip("Khoảng thời gian (giây) giữa 2 lần giật máu trên cùng 1 mục tiêu")]
        public float hitCooldown = 0.6f;

        [Tooltip("Tỉ lệ làm chậm kẻ địch (0.3 = 30%)")]
        public float slowPercentage = 0.3f;

        [Tooltip("Thời gian duy trì hiệu ứng làm chậm (giây)")]
        public float slowDuration = 1.0f;

        public override IProjectileBehavior CreateBehavior(ProjectileController controller)
        {
            return new PeriodicHitBehavior(controller, this);
        }
    }
}
