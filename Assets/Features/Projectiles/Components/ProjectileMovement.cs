using UnityEngine;

namespace ProjectZombie.Features.Projectiles.Components
{
    public class ProjectileMovement : MonoBehaviour
    {
        private ProjectileController _controller;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void Initialize(ProjectileController controller)
        {
            _controller = controller;
            
            // Align rotation with direction
            float angle = Mathf.Atan2(controller.CurrentDirection.y, controller.CurrentDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void FixedUpdate()
        {
            if (_controller == null) return;

            // Align rotation with current direction in case it changed (e.g. Homing)
            float angle = Mathf.Atan2(_controller.CurrentDirection.y, _controller.CurrentDirection.x) * Mathf.Rad2Deg;
            _rb.MoveRotation(angle);

            // Move
            Vector2 movement = _controller.CurrentDirection * (_controller.Data.Speed * Time.fixedDeltaTime);
            _rb.MovePosition(_rb.position + movement);
        }
    }
}
