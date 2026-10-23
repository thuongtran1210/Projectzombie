using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Single-player Defense 3D prototype only. Does not bind Legacy PlayerController,
    /// Physics2D, survival loadouts or the old game-state bootstrapper.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class DefenseHeroMotor3D : MonoBehaviour
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private float turnRate = 14f;

        private Rigidbody _body;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _body.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void FixedUpdate()
        {
            Vector2 input = Vector2.zero;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1f;
            }

            if (Gamepad.current != null)
            {
                Vector2 stick = Gamepad.current.leftStick.ReadValue();
                if (stick.sqrMagnitude > input.sqrMagnitude) input = stick;
            }

            input = Vector2.ClampMagnitude(input, 1f);
            Vector3 move = new Vector3(input.x, 0f, input.y) * speed;
            _body.velocity = new Vector3(move.x, _body.velocity.y, move.z);
            if (input.sqrMagnitude > 0.001f)
            {
                Quaternion face = Quaternion.LookRotation(new Vector3(input.x, 0f, input.y));
                _body.MoveRotation(Quaternion.Slerp(_body.rotation, face, turnRate * Time.fixedDeltaTime));
            }
        }
    }
}
