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

        private void Update()
        {
            if (_controller == null) return;

            // Check Lifetime
            if (Time.time >= _spawnTime + _controller.Data.Lifetime)
            {
                _controller.HandleExpiration();
                return;
            }

            // Check Max Range
            float traveledDistance = Vector2.Distance(_spawnPosition, transform.position);
            if (traveledDistance >= _controller.Data.MaxRange)
            {
                _controller.HandleExpiration();
            }
        }
    }
}
