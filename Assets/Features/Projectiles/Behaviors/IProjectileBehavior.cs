using UnityEngine;

namespace ProjectZombie.Features.Projectiles.Behaviors
{
    public enum BehaviorHitResult
    {
        Neutral = 0,
        KeepAlive = 1,
        RequireDespawn = 2
    }

    public interface IProjectileBehavior
    {
        void OnSpawn();
        void OnUpdate();
        
        /// <summary>
        /// Called when the projectile hits a target.
        /// Returns BehaviorHitResult to determine whether the projectile should be despawned, kept alive, or neutral.
        /// </summary>
        BehaviorHitResult OnHit(Core.ProjectileEventContext context);
        
        void OnDespawn();
    }
}
