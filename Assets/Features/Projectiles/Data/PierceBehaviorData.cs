using UnityEngine;
using ProjectZombie.Features.Projectiles.Components;
using ProjectZombie.Features.Projectiles.Behaviors;

namespace ProjectZombie.Features.Projectiles.Data
{
    [CreateAssetMenu(fileName = "PierceBehaviorData", menuName = "ProjectZombie/Projectiles/Behaviors/Pierce")]
    public class PierceBehaviorData : ProjectileBehaviorData
    {
        public int PierceCount = 1;

        public override IProjectileBehavior CreateBehavior(ProjectileController controller)
        {
            return new PierceBehavior(controller, this);
        }
    }
}
