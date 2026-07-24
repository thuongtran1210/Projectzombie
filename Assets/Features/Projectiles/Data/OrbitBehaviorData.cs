using UnityEngine;
using ProjectZombie.Features.Projectiles.Components;
using ProjectZombie.Features.Projectiles.Behaviors;

namespace ProjectZombie.Features.Projectiles.Data
{
    [CreateAssetMenu(fileName = "OrbitBehaviorData", menuName = "ProjectZombie/Projectiles/Behaviors/Orbit")]
    public class OrbitBehaviorData : ProjectileBehaviorData
    {
        [Tooltip("Bán kính xoay mặc định")]
        public float radius = 2f;

        [Tooltip("Tốc độ xoay (độ/giây)")]
        public float orbitSpeed = 180f;

        public override IProjectileBehavior CreateBehavior(ProjectileController controller)
        {
            return new OrbitBehavior(controller, this);
        }
    }
}
