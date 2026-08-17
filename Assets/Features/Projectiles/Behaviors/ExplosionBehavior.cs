using UnityEngine;
using ProjectZombie.Features.Projectiles.Components;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Projectiles.Behaviors
{
    public class ExplosionBehavior : IProjectileBehavior
    {
        private ProjectileController _controller;
        private Data.ExplosionBehaviorData _data;
        private bool _hasExploded = false;

        public ExplosionBehavior(ProjectileController controller, Data.ExplosionBehaviorData data)
        {
            _controller = controller;
            _data = data;
        }

        public void OnSpawn()
        {
            _hasExploded = false;
        }

        public void OnUpdate() { }

        public BehaviorHitResult OnHit(Core.ProjectileEventContext context)
        {
            if (_data.TriggerOnHit && !_hasExploded)
            {
                Explode(context.HitPoint);
            }
            return BehaviorHitResult.RequireDespawn;
        }

        public void OnDespawn()
        {
            if (_data.TriggerOnDespawn && !_hasExploded)
            {
                Explode(_controller.transform.position);
            }
        }

        private static readonly Collider2D[] _explosionBuffer = new Collider2D[60];

        private void Explode(Vector2 center)
        {
            _hasExploded = true;

            int mask = _controller.Data != null && _controller.Data.HitLayer != 0 
                ? (int)_controller.Data.HitLayer 
                : TargetingUtility.EnemyLayerMask;

            int numHits = Physics2D.OverlapCircleNonAlloc(center, _data.ExplosionRadius, _explosionBuffer, mask);
            if (numHits <= 0) return;

            float damageAmount = _controller.Damage.BaseDamage * _data.ExplosionDamageMultiplier;
            DamageContext explosionDamage = new DamageContext(_controller.Owner, damageAmount);

            for (int i = 0; i < numHits; i++)
            {
                var col = _explosionBuffer[i];
                if (col == null || col.gameObject == _controller.Owner) continue;

                if (col.TryGetComponent(out IDamageable target))
                {
                    target.TakeDamage(explosionDamage);
                }
            }

            // Có thể spawn VFX nổ ở đây thông qua Event/Visual System sau này.
        }
    }
}
