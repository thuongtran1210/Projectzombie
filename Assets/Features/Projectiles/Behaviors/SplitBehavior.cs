using UnityEngine;
using ProjectZombie.Features.Projectiles.Components;
using ProjectZombie.Features.Projectiles.Core;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Projectiles.Behaviors
{
    public class SplitBehavior : IProjectileBehavior
    {
        private ProjectileController _controller;
        private Data.SplitBehaviorData _data;
        private bool _hasSplit = false;

        public SplitBehavior(ProjectileController controller, Data.SplitBehaviorData data)
        {
            _controller = controller;
            _data = data;
        }

        public void OnSpawn()
        {
            _hasSplit = false;
        }

        public void OnUpdate() { }

        public BehaviorHitResult OnHit(Core.ProjectileEventContext context)
        {
            if (_data.TriggerOnHit && !_hasSplit)
            {
                Split(context.HitPoint);
            }
            return BehaviorHitResult.RequireDespawn;
        }

        public void OnDespawn()
        {
            if (_data.TriggerOnDespawn && !_hasSplit)
            {
                Split(_controller.transform.position);
            }
        }

        private void Split(Vector2 position)
        {
            if (_data.ChildProjectileData == null) return;

            _hasSplit = true;

            float angleStep = _data.SplitCount > 1 ? _data.SpreadAngle / (_data.SplitCount - 1) : 0;
            float startAngle = -_data.SpreadAngle / 2f;
            
            float childDamage = _data.DivideDamage ? (_controller.Damage.BaseDamage / _data.SplitCount) : _controller.Damage.BaseDamage;
            DamageData childDamageOverride = new DamageData(childDamage);

            for (int i = 0; i < _data.SplitCount; i++)
            {
                float currentAngle = startAngle + (angleStep * i);
                Vector2 spreadDirection = Quaternion.Euler(0, 0, currentAngle) * _controller.CurrentDirection;
                
                // Spawn with next generation
                ProjectileSystem.Instance.Spawn(
                    _data.ChildProjectileData, 
                    position, 
                    spreadDirection, 
                    _controller.Owner, 
                    childDamageOverride, 
                    _controller.State.Generation + 1
                );
            }
        }
    }
}
