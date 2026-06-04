using UnityEngine;

namespace ProjectZombie.Features.Projectiles.Behaviors
{
    public interface IProjectileBehavior
    {
        void OnSpawn();
        void OnUpdate();
        
        /// <summary>
        /// Called when the projectile hits a target.
        /// Return true to allow despawn, false to prevent despawn (e.g. for piercing).
        /// </summary>
        bool OnHit(Core.ProjectileEventContext context);
        
        void OnDespawn();
    }
}
