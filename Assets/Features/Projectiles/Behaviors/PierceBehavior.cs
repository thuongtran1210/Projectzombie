using UnityEngine;
using ProjectZombie.Features.Projectiles.Components;
using System.Collections.Generic;

namespace ProjectZombie.Features.Projectiles.Behaviors
{
    public class PierceBehavior : IProjectileBehavior
    {
        private ProjectileController _controller;
        private Data.PierceBehaviorData _data;
        private HashSet<Collider2D> _hitTargets = new HashSet<Collider2D>();

        public PierceBehavior(ProjectileController controller, Data.PierceBehaviorData data)
        {
            _controller = controller;
            _data = data;
        }

        public void OnSpawn()
        {
            _controller.State.RemainingPierce = _data.PierceCount;
            _hitTargets.Clear();
        }

        public void OnUpdate() { }

        public bool OnHit(Core.ProjectileEventContext context)
        {
            if (_hitTargets.Contains(context.TargetCollider))
            {
                return false; // Already hit this target, do not count again, do not despawn
            }

            _hitTargets.Add(context.TargetCollider);

            if (_controller.State.RemainingPierce > 0)
            {
                _controller.State.RemainingPierce--;
                return false; // Prevent despawn
            }
            return true; // No pierce left, allow despawn
        }

        public void OnDespawn()
        {
            _hitTargets.Clear();
        }
    }
}
