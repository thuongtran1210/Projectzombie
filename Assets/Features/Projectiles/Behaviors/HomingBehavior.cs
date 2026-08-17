using UnityEngine;
using ProjectZombie.Features.Projectiles.Components;

namespace ProjectZombie.Features.Projectiles.Behaviors
{
    public class HomingBehavior : IProjectileBehavior
    {
        private ProjectileController _controller;
        private Data.HomingBehaviorData _data;

        public HomingBehavior(ProjectileController controller, Data.HomingBehaviorData data)
        {
            _controller = controller;
            _data = data;
        }

        public void OnSpawn()
        {
            _controller.State.CurrentTarget = null;
            FindTarget();
        }

        public void OnUpdate()
        {
            if (_controller.State.CurrentTarget == null || !_controller.State.CurrentTarget.gameObject.activeInHierarchy)
            {
                FindTarget();
            }

            if (_controller.State.CurrentTarget != null)
            {
                Vector2 targetDirection = ((Vector2)_controller.State.CurrentTarget.position - (Vector2)_controller.transform.position).normalized;
                
                // Steer current direction towards target direction
                _controller.CurrentDirection = Vector3.RotateTowards(
                    _controller.CurrentDirection, 
                    targetDirection, 
                    _data.HomingStrength * Time.deltaTime, 
                    0f
                ).normalized;
            }
        }

        public BehaviorHitResult OnHit(Core.ProjectileEventContext context) => BehaviorHitResult.Neutral; // Does not alter despawn decision

        public void OnDespawn()
        {
            _controller.State.CurrentTarget = null;
        }

        private void FindTarget()
        {
            int mask = _controller.Data != null && _controller.Data.HitLayer != 0 
                ? (int)_controller.Data.HitLayer 
                : Shared.TargetingUtility.EnemyLayerMask;

            _controller.State.CurrentTarget = Shared.TargetingUtility.FindNearestEnemy(
                _controller.transform.position, 
                _data.HomingRadius, 
                mask
            );
        }
    }
}
