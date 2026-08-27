using UnityEngine;

namespace ProjectZombie.Features.Projectiles.Components
{
    public class ProjectileLifetime : MonoBehaviour
    {
        private ProjectileController _controller;
        private float _spawnTime;
        private Vector2 _spawnPosition;

        public void Initialize(ProjectileController controller)
        {
            _controller = controller;
            _spawnTime = Time.time;
            _spawnPosition = transform.position;
        }

        public void OnUpdate()
        {
            if (_controller == null || _controller.Data == null) return;

            // Check Lifetime (Lifetime <= 0 hoặc Infinity = vô hạn)
            float lifetime = _controller.Data.Lifetime;
            if (lifetime > 0f && !float.IsInfinity(lifetime))
            {
                if (Time.time >= _spawnTime + lifetime)
                {
                    _controller.HandleExpiration();
                    return;
                }
            }

            // Check Max Range (Chỉ áp dụng cho Transient Projectiles. Orbit / PersistentAura không bị despawn theo khoảng cách spawn)
            if (_controller.Data.Category == Data.ProjectileCategory.Transient)
            {
                float maxRange = _controller.Data.MaxRange;
                if (maxRange > 0f && !float.IsInfinity(maxRange))
                {
                    float traveledDistance = Vector2.Distance(_spawnPosition, transform.position);
                    if (traveledDistance >= maxRange)
                    {
                        _controller.HandleExpiration();
                    }
                }
            }
        }
    }
}
