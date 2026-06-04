using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Projectiles.Components
{
    public class ProjectileCollision : MonoBehaviour
    {
        private ProjectileController _controller;
        private bool _isInitialized;

        private Vector2 _lastPosition;

        public void Initialize(ProjectileController controller)
        {
            _controller = controller;
            _isInitialized = true;
            _lastPosition = transform.position;
        }

        private void FixedUpdate()
        {
            if (_isInitialized)
            {
                _lastPosition = transform.position;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!_isInitialized) return;

            // Check if collision is in HitLayer
            if (((1 << collision.gameObject.layer) & _controller.Data.HitLayer) != 0)
            {
                // Verify we are not hitting the owner
                if (collision.gameObject == _controller.Owner) return;

                // Calculate HitPoint and HitNormal using a short Raycast
                Vector2 currentPos = transform.position;
                Vector2 dir = (currentPos - _lastPosition).normalized;
                float dist = Vector2.Distance(_lastPosition, currentPos) + 0.1f; // Add a small buffer

                // Fallback values
                Vector2 hitPoint = currentPos;
                Vector2 hitNormal = -dir;

                RaycastHit2D hit = Physics2D.Raycast(_lastPosition, dir, dist, _controller.Data.HitLayer);
                if (hit.collider != null && hit.collider == collision)
                {
                    hitPoint = hit.point;
                    hitNormal = hit.normal;
                }

                Core.ProjectileEventContext context = new Core.ProjectileEventContext(_controller, collision, hitPoint, hitNormal);

                if (collision.TryGetComponent(out HealthSystem targetHealth))
                {
                    targetHealth.TakeDamage(_controller.Damage);
                }

                _controller.HandleHit(context);
            }
        }
    }
}
