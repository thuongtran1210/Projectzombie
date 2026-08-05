using UnityEngine;
using ProjectZombie.Features.Projectiles.Components;

namespace ProjectZombie.Features.Projectiles.Behaviors
{
    public class BounceBehavior : IProjectileBehavior
    {
        private ProjectileController _controller;
        private Data.BounceBehaviorData _data;

        public BounceBehavior(ProjectileController controller, Data.BounceBehaviorData data)
        {
            _controller = controller;
            _data = data;
        }

        public void OnSpawn()
        {
            _controller.State.RemainingBounce = _data.BounceCount;
        }

        public void OnUpdate() { }

        public BehaviorHitResult OnHit(Core.ProjectileEventContext context)
        {
            if (_controller.State.RemainingBounce > 0)
            {
                _controller.State.RemainingBounce--;
                
                // Reflect direction across the normal
                _controller.CurrentDirection = Vector2.Reflect(_controller.CurrentDirection, context.HitNormal).normalized;
                
                return BehaviorHitResult.KeepAlive; // Prevent despawn
            }
            
            return BehaviorHitResult.RequireDespawn; // No bounces left, despawn
        }

        public void OnDespawn() { }
    }
}
