using UnityEngine;
using ProjectZombie.Features.Projectiles.Components;
using ProjectZombie.Features.Projectiles.Data;
using ProjectZombie.Features.Projectiles.Core;

namespace ProjectZombie.Features.Projectiles.Behaviors
{
    public class OrbitBehavior : IProjectileBehavior
    {
        private readonly ProjectileController _controller;
        private readonly OrbitBehaviorData _data;
        private float _currentAngle;

        public OrbitBehavior(ProjectileController controller, OrbitBehaviorData data)
        {
            _controller = controller;
            _data = data;
        }

        public void OnSpawn()
        {
            if (_controller.CurrentDirection != Vector2.zero)
            {
                _currentAngle = Mathf.Atan2(_controller.CurrentDirection.y, _controller.CurrentDirection.x) * Mathf.Rad2Deg;
            }
            else
            {
                _currentAngle = 0f;
            }
        }

        public void OnUpdate()
        {
            if (_controller.Owner == null) return;

            float speed = _data != null ? _data.orbitSpeed : 180f;
            float radius = _data != null ? _data.radius : 2f;

            _currentAngle += speed * Time.deltaTime;
            if (_currentAngle > 360f) _currentAngle -= 360f;
            else if (_currentAngle < 0f) _currentAngle += 360f;

            float rad = _currentAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * radius;
            _controller.transform.position = _controller.Owner.transform.position + offset;

            // Xoay lá bùa mềm mại nảy theo tiếp tuyến đường tròn xoay
            _controller.transform.rotation = Quaternion.Euler(0f, 0f, _currentAngle - 90f);
        }

        public BehaviorHitResult OnHit(ProjectileEventContext context)
        {
            // Đẩy lùi nhẹ yêu ma ra xa tâm người chơi khi lá Bùa Trấn Yêu chạm trúng
            if (context.TargetCollider != null && _controller.Owner != null)
            {
                if (context.TargetCollider.TryGetComponent<Enemies.Enemy>(out var enemy) ||
                    (enemy = context.TargetCollider.GetComponentInParent<Enemies.Enemy>()) != null)
                {
                    Vector2 pushDir = ((Vector2)enemy.transform.position - (Vector2)_controller.Owner.transform.position).normalized;
                    if (pushDir == Vector2.zero) pushDir = Vector2.up;
                    enemy.ApplyKnockback(pushDir, 3.5f, 0.15f);
                }
            }

            // Đạn xoay tròn giữ nguyên trạng thái khi chạm kẻ địch (Pierce/Orbit)
            return BehaviorHitResult.KeepAlive;
        }

        public void OnDespawn() { }
    }
}
