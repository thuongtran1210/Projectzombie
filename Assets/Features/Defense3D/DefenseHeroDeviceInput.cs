using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Device adapter only. Gameplay consumers depend on commands, not input devices.</summary>
    public sealed class DefenseHeroDeviceInput
    {
        public void ReadInto(DefenseHeroCommands commands, bool available)
        {
            if (!available)
            {
                commands.Clear();
                return;
            }
            Vector2 movement = Vector2.zero;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) movement.x -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) movement.x += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) movement.y -= 1f;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) movement.y += 1f;
            }
            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                Vector2 stick = gamepad.leftStick.ReadValue();
                if (stick.sqrMagnitude > movement.sqrMagnitude) movement = stick;
            }
            commands.Submit(movement.x, movement.y,
                keyboard != null && keyboard.bKey.wasPressedThisFrame, true);
        }
    }
}
