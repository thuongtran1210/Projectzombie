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

            // Cơ chế Cản Đạn (Heavy Armor Bullet Sponge) cho Quỷ Nhập Tràng (E_QUYNHAPTRANG) - GDD 5.1
            int pierceCost = 1;
            if (context.TargetCollider != null && 
               (context.TargetCollider.CompareTag("HeavyArmor") || context.TargetCollider.name.Contains("E_QUYNHAPTRANG")))
            {
                pierceCost = 2; // Trừ 2 Pierce Charge đối với Quỷ Nhập Tràng
            }

            if (_controller.State.RemainingPierce >= pierceCost)
            {
                _controller.State.RemainingPierce -= pierceCost;
                return false; // Còn Pierce charge, không tiêu hủy đạn
            }

            _controller.State.RemainingPierce = 0;
            return true; // Hết Pierce charge, tiêu hủy đạn ngay lập tức
        }

        public void OnDespawn()
        {
            _hitTargets.Clear();
        }
    }
}
