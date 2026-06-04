using UnityEngine;
using ProjectZombie.Features.Projectiles.Components;
using ProjectZombie.Features.Projectiles.Behaviors;

namespace ProjectZombie.Features.Projectiles.Data
{
    [CreateAssetMenu(fileName = "StraightBehaviorData", menuName = "ProjectZombie/Projectiles/Behaviors/Straight")]
    public class StraightBehaviorData : ProjectileBehaviorData
    {
        public override IProjectileBehavior CreateBehavior(ProjectileController controller)
        {
            return new StraightBehavior(controller);
        }
    }
}
