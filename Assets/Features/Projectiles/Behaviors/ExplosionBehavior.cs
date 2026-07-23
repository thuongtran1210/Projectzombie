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

        public bool OnHit(Core.ProjectileEventContext context)
        {
            if (_data.TriggerOnHit && !_hasExploded)
            {
                Explode(context.HitPoint);
            }
            return true;
        }

        public void OnDespawn()
        {
            if (_data.TriggerOnDespawn && !_hasExploded)
            {
                Explode(_controller.transform.position);
            }
        }

        private void Explode(Vector2 center)
        {
            _hasExploded = true;

            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, _data.ExplosionRadius, _controller.Data.HitLayer);
            
            float damageAmount = _controller.Damage.BaseDamage * _data.ExplosionDamageMultiplier;
            DamageContext explosionDamage = new DamageContext(_controller.Owner, damageAmount);

            foreach (var col in hitColliders)
            {
                if (col.gameObject == _controller.Owner) continue;

                if (col.TryGetComponent(out IDamageable target))
                {
                    target.TakeDamage(explosionDamage);
                }
            }

            // Có thể spawn VFX nổ ở đây thông qua Event/Visual System sau này.
        }
    }
}
