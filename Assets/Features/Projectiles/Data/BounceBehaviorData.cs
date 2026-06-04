using UnityEngine;
using ProjectZombie.Features.Projectiles.Components;
using ProjectZombie.Features.Projectiles.Behaviors;

namespace ProjectZombie.Features.Projectiles.Data
{
    [CreateAssetMenu(fileName = "BounceBehaviorData", menuName = "ProjectZombie/Projectiles/Behaviors/Bounce")]
    public class BounceBehaviorData : ProjectileBehaviorData
    {
        public int BounceCount = 3;

        public override IProjectileBehavior CreateBehavior(ProjectileController controller)
        {
            return new BounceBehavior(controller, this);
        }
    }
}
