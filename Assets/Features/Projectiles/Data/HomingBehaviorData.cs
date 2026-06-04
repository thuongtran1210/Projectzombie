using UnityEngine;
using ProjectZombie.Features.Projectiles.Components;
using ProjectZombie.Features.Projectiles.Behaviors;

namespace ProjectZombie.Features.Projectiles.Data
{
    [CreateAssetMenu(fileName = "HomingBehaviorData", menuName = "ProjectZombie/Projectiles/Behaviors/Homing")]
    public class HomingBehaviorData : ProjectileBehaviorData
    {
        public float HomingStrength = 5f;
        public float HomingRadius = 10f;

        public override IProjectileBehavior CreateBehavior(ProjectileController controller)
        {
            return new HomingBehavior(controller, this);
        }
    }
}
