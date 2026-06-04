using UnityEngine;
using ProjectZombie.Features.Projectiles.Components;

namespace ProjectZombie.Features.Projectiles.Behaviors
{
    public class StraightBehavior : IProjectileBehavior
    {
        private ProjectileController _controller;

        public StraightBehavior(ProjectileController controller)
        {
            _controller = controller;
        }

        public void OnSpawn() { }
        public void OnUpdate() { }
        public bool OnHit(Core.ProjectileEventContext context) => true; // Default behavior is to allow despawn
        public void OnDespawn() { }
    }
}
