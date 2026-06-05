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

        public bool OnHit(Core.ProjectileEventContext context)
        {
            // Tính toán tỉ lệ hút máu
            if (Random.value <= _data.VampiricChance)
            {
                if (_controller.Owner != null)
                {
                    // Lấy HealthSystem của người chơi (Owner) để hồi máu
                    var healthSystem = _controller.Owner.GetComponent<HealthSystem>();
                    if (healthSystem != null)
                    {
                        healthSystem.Heal(_data.HealAmount);
                    }
                }
            }

            // Dơi thường cắn 1 cái là biến mất, nên trả về true để projectile bị xoá
            return true;
        }

        public void OnDespawn()
        {
            // Không có logic đặc biệt khi despawn
        }

        private void FindNewTarget()
        {
            // Tìm quái vật gần nhất trong bán kính
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(_controller.transform.position, _data.SearchRadius, _data.EnemyLayer);
            float closestDistance = float.MaxValue;
            Transform closestEnemy = null;

            foreach (var hitCollider in hitColliders)
            {
                // Kiểm tra xem kẻ địch có còn sống không
                var enemyHealth = hitCollider.GetComponent<HealthSystem>();
                if (enemyHealth != null && enemyHealth.CurrentHealth <= 0) continue;

                float distance = Vector2.Distance(_controller.transform.position, hitCollider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = hitCollider.transform;
                }
            }

            _target = closestEnemy;
        }
    }
}
