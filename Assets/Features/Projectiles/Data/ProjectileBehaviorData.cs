using UnityEngine;
using ProjectZombie.Features.Projectiles.Components;

namespace ProjectZombie.Features.Projectiles.Data
{
    public abstract class ProjectileBehaviorData : ScriptableObject
    {
        [Tooltip("Thứ tự chạy của Behavior này. Số nhỏ chạy trước (VD: Homing=0, Straight=10)")]
        public int ExecutionOrder = 0;

        public abstract Behaviors.IProjectileBehavior CreateBehavior(ProjectileController controller);
    }
}
