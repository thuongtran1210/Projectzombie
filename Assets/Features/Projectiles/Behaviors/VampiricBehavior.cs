using UnityEngine;
using ProjectZombie.Features.Projectiles.Components;
using ProjectZombie.Features.Projectiles.Data;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Projectiles.Behaviors
{
    public class VampiricBehavior : IProjectileBehavior
    {
        private ProjectileController _controller;
        private VampiricBehaviorData _data;
        private Transform _target;

        public VampiricBehavior(ProjectileController controller, VampiricBehaviorData data)
        {
            _controller = controller;
            _data = data;
        }

        public void OnSpawn()
        {
            FindNewTarget();
        }

        public void OnUpdate()
        {
            // Homing logic: Dơi bay lượn tìm mục tiêu
            if (_target == null || !_target.gameObject.activeInHierarchy)
            {
                FindNewTarget();
            }

            if (_target != null)
            {
                Vector2 directionToTarget = ((Vector2)_target.position - (Vector2)_controller.transform.position).normalized;
                
                // Từ từ xoay hướng bay về phía mục tiêu thay vì snap ngay lập tức (tạo cảm giác dơi bay lượn)
                float angle = Vector2.SignedAngle(_controller.CurrentDirection, directionToTarget);
                float maxRotation = _data.RotationSpeed * Time.deltaTime;
                
                float angleToRotate = Mathf.Clamp(angle, -maxRotation, maxRotation);
                _controller.CurrentDirection = Quaternion.Euler(0, 0, angleToRotate) * _controller.CurrentDirection;
            }
        }

        public BehaviorHitResult OnHit(Core.ProjectileEventContext context)
        {
            // Tính toán tỉ lệ hút máu
            if (Random.value <= _data.VampiricChance)
            {
                if (_controller.Owner != null)
                {
                    // Lấy IHealable của người chơi (Owner) để hồi máu
                    if (_controller.Owner.TryGetComponent(out IHealable healable))
                    {
                        healable.Heal(_data.HealAmount);
                    }
                }
            }

            // Dơi thường cắn 1 cái là biến mất, nên yêu cầu despawn
            return BehaviorHitResult.RequireDespawn;
        }

        public void OnDespawn()
        {
            // Không có logic đặc biệt khi despawn
        }

        private void FindNewTarget()
        {
            int mask = _data != null && _data.EnemyLayer != 0 
                ? (int)_data.EnemyLayer 
                : TargetingUtility.EnemyLayerMask;

            _target = TargetingUtility.FindNearestEnemy(
                _controller.transform.position, 
                _data.SearchRadius, 
                mask
            );
        }
    }
}
