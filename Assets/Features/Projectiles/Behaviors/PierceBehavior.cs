using UnityEngine;
using ProjectZombie.Features.Projectiles.Components;
using ProjectZombie.Features.Enemies;
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
            int basePierce = _data != null ? _data.PierceCount : 0;
            _controller.State.RemainingPierce = basePierce + _controller.State.BonusPierce;
            _hitTargets.Clear();
        }

        public void OnUpdate() { }

        public BehaviorHitResult OnHit(Core.ProjectileEventContext context)
        {
            if (_hitTargets.Contains(context.TargetCollider))
            {
                return BehaviorHitResult.KeepAlive; // Already hit this target, do not count again, do not despawn
            }

            _hitTargets.Add(context.TargetCollider);

            // Cơ chế Cản Đạn (Heavy Armor Bullet Sponge) cho Quỷ Nhập Tràng (E_QUYNHAPTRANG) - GDD 5.1
            int pierceCost = 1;
            if (context.TargetCollider != null)
            {
                var enemy = context.TargetCollider.GetComponentInParent<Enemy>();
                bool isHeavyArmor = false;

                if (enemy != null)
                {
                    isHeavyArmor = enemy.IsHeavyArmor;
                }
                else
                {
                    string targetName = context.TargetCollider.name;
                    isHeavyArmor = targetName.Contains("E_QUYNHAPTRANG") || targetName.Contains("QUYNHAPTRANG");
                }

                if (isHeavyArmor)
                {
                    pierceCost = 2; // Trừ 2 Pierce Charge đối với Yêu Ma có Heavy Armor
                }
            }

            if (_controller.State.RemainingPierce >= pierceCost)
            {
                _controller.State.RemainingPierce -= pierceCost;
                return BehaviorHitResult.KeepAlive; // Còn Pierce charge, không tiêu hủy đạn
            }

            _controller.State.RemainingPierce = 0;
            return BehaviorHitResult.RequireDespawn; // Hết Pierce charge, tiêu hủy đạn ngay lập tức
        }

        public void OnDespawn()
        {
            _hitTargets.Clear();
        }
    }
}
