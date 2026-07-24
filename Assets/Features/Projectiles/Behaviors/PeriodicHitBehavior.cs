using UnityEngine;
using System.Collections.Generic;
using ProjectZombie.Features.Projectiles.Components;
using ProjectZombie.Features.Projectiles.Data;
using ProjectZombie.Features.Projectiles.Core;

namespace ProjectZombie.Features.Projectiles.Behaviors
{
    public class PeriodicHitBehavior : IProjectileBehavior
    {
        private readonly ProjectileController _controller;
        private readonly PeriodicHitBehaviorData _data;
        private readonly Dictionary<Collider2D, float> _lastHitTimes = new Dictionary<Collider2D, float>();

        public PeriodicHitBehavior(ProjectileController controller, PeriodicHitBehaviorData data)
        {
            _controller = controller;
            _data = data;
        }

        public void OnSpawn()
        {
            _lastHitTimes.Clear();
        }

        public void OnUpdate() { }

        public bool OnHit(ProjectileEventContext context)
        {
            if (context.TargetCollider == null) return false;

            float cooldown = _data != null ? _data.hitCooldown : 0.5f;

            if (_lastHitTimes.TryGetValue(context.TargetCollider, out float lastTime))
            {
                if (Time.time < lastTime + cooldown)
                {
                    return false; // Chưa hết cooldown, không gây sát thương thêm & không despawn
                }
            }

            _lastHitTimes[context.TargetCollider] = Time.time;

            if (context.TargetCollider.TryGetComponent(out Shared.HealthSystem health))
            {
                health.TakeDamage(context.Damage);
            }

            // Trả về false để đạn không bị despawn
            return false;
        }

        public void OnDespawn()
        {
            _lastHitTimes.Clear();
        }
    }
}
