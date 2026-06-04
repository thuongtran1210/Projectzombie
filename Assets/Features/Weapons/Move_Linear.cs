using UnityEngine;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Component hành vi di chuyển: Bay theo một đường thẳng.
    /// </summary>
    [RequireComponent(typeof(ProjectileCore))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Move_Linear : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float speed = 10f;

        private ProjectileCore _core;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _core = GetComponent<ProjectileCore>();
            _rb = GetComponent<Rigidbody2D>();
            _rb.isKinematic = true;
        }

        private void FixedUpdate()
        {
            // Di chuyển thẳng theo hướng được cung cấp từ Core
            _rb.MovePosition(_rb.position + _core.Direction * speed * Time.fixedDeltaTime);
        }
    }
}
