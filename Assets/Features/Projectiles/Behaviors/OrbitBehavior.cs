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
        }

        public bool OnHit(ProjectileEventContext context)
        {
            // Đạn xoay tròn không tự hủy khi chạm kẻ địch
            return false;
        }

        public void OnDespawn() { }
    }
}
