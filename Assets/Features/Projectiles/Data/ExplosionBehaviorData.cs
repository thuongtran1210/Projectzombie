using UnityEngine;
using ProjectZombie.Features.Projectiles.Components;
using ProjectZombie.Features.Projectiles.Behaviors;

namespace ProjectZombie.Features.Projectiles.Data
{
    [CreateAssetMenu(fileName = "ExplosionBehaviorData", menuName = "ProjectZombie/Projectiles/Behaviors/Explosion")]
    public class ExplosionBehaviorData : ProjectileBehaviorData
    {
        public float ExplosionRadius = 2.5f;
        public float ExplosionDamageMultiplier = 1f;
        public bool TriggerOnHit = true;
        public bool TriggerOnDespawn = false;

        public override IProjectileBehavior CreateBehavior(ProjectileController controller)
        {
            return new ExplosionBehavior(controller, this);
        }
    }
}
