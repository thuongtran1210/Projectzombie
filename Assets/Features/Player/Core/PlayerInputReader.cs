using System;
using UnityEngine;
using UnityEngine.InputSystem;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Player.Core
{
    /// <summary>
    /// Service quản lý và tập trung toàn bộ nguồn Input (New Input System, Legacy Input, Mobile Joystick, UI Buttons).
    /// Đóng vai trò Single Source of Truth cho việc tương tác của người chơi, ngăn chặn việc phân tán input check.
    /// </summary>
    public class PlayerInputReader : MonoBehaviour
    {
        [Header("Input Action References")]
        [Tooltip("Input Action cho di chuyển (Vector2)")]
        [SerializeField] private InputActionReference moveAction;

        [Tooltip("Input Action cho lướt (Button)")]
        [SerializeField] private InputActionReference dashAction;

        [Header("State")]
        [SerializeField] private bool isInputBlocked = false;

        public bool IsInputBlocked
        {
            get => isInputBlocked;
            set => isInputBlocked = value;
        }

        public Vector2 MovementInput { get; private set; }

        public event Action OnDashTriggered;
        public event Action OnAttackTriggered;
        public event Action OnSignatureSkillTriggered;
        public event Action OnRelicSkillTriggered;

        public void SetMoveAction(InputActionReference action) => moveAction = action;
        public void SetDashAction(InputActionReference action) => dashAction = action;

        private void OnEnable()
        {
            if (moveAction != null && moveAction.action != null)
            {
                moveAction.action.Enable();
            }

            if (dashAction != null && dashAction.action != null)
            {
                dashAction.action.Enable();
                dashAction.action.performed += HandleDashPerformed;
            }
        }

        private void OnDisable()
        {
            if (moveAction != null && moveAction.action != null)
            {
                moveAction.action.Disable();
            }

            if (dashAction != null && dashAction.action != null)
            {
                dashAction.action.Disable();
                dashAction.action.performed -= HandleDashPerformed;
            }
        }

        private void HandleDashPerformed(InputAction.CallbackContext context)
        {
            if (isInputBlocked || !GameStateManager.IsPlaying) return;
            OnDashTriggered?.Invoke();
        }

        private void Update()
        {
            if (isInputBlocked || !GameStateManager.IsPlaying)
            {
                MovementInput = Vector2.zero;
                return;
            }

            ReadMovementInput();
            ReadHotkeyInputs();
        }

        private void ReadMovementInput()
        {
            Vector2 rawInput = Vector2.zero;

            // 1. New Input System Action
            if (moveAction != null && moveAction.action != null && moveAction.action.enabled)
            {
                rawInput = moveAction.action.ReadValue<Vector2>();
            }

            // 2. Fallback: Mobile Dynamic Virtual Joystick
            if (rawInput == Vector2.zero && UI.DynamicVirtualJoystick.Instance != null)
            {
                rawInput = UI.DynamicVirtualJoystick.Instance.InputVector;
            }

            // 3. Fallback: PC Keyboard
#if ENABLE_INPUT_SYSTEM
            if (rawInput == Vector2.zero && Keyboard.current != null)
            {
                var kb = Keyboard.current;
                float h = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
                float v = (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1f : 0f);
                if (h != 0 || v != 0)
                {
                    rawInput = new Vector2(h, v);
                }
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (rawInput == Vector2.zero)
            {
                float h = Input.GetAxisRaw("Horizontal");
                float v = Input.GetAxisRaw("Vertical");
                if (h != 0 || v != 0)
                {
                    rawInput = new Vector2(h, v);
                }
            }
#endif

            MovementInput = rawInput.magnitude > 1f ? rawInput.normalized : rawInput;
        }

        private void ReadHotkeyInputs()
        {
#if ENABLE_INPUT_SYSTEM
            // Đòn đánh thường: Chuột trái hoặc J / Z / LeftCtrl
            bool attackPressed = false;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (UnityEngine.EventSystems.EventSystem.current == null || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    attackPressed = true;
                }
            }
            else if (Keyboard.current != null)
            {
                var kb = Keyboard.current;
                if (kb.jKey.wasPressedThisFrame || kb.zKey.wasPressedThisFrame || kb.leftCtrlKey.wasPressedThisFrame)
                {
                    attackPressed = true;
                }
            }

            if (attackPressed)
            {
                TriggerAttack();
            }

            // Lướt: Phím Space
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                TriggerDash();
            }

            // Tuyệt kỹ: Q / U
            if (Keyboard.current != null && (Keyboard.current.qKey.wasPressedThisFrame || Keyboard.current.uKey.wasPressedThisFrame))
            {
                TriggerSignatureSkill();
            }

            // Pháp bảo: E / R / I
            if (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.rKey.wasPressedThisFrame || Keyboard.current.iKey.wasPressedThisFrame))
            {
                TriggerRelicSkill();
            }

#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetMouseButtonDown(0) && (UnityEngine.EventSystems.EventSystem.current == null || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()))
            {
                TriggerAttack();
            }
            else if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.LeftControl))
            {
                TriggerAttack();
            }

            if (Input.GetKeyDown(KeyCode.Space)) TriggerDash();
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.U)) TriggerSignatureSkill();
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.I)) TriggerRelicSkill();
#endif
        }

        public void TriggerDash()
        {
            if (isInputBlocked || !GameStateManager.IsPlaying) return;
            OnDashTriggered?.Invoke();
        }

        public void TriggerAttack()
        {
            if (isInputBlocked || !GameStateManager.IsPlaying) return;
            OnAttackTriggered?.Invoke();
        }

        public void TriggerSignatureSkill()
        {
            if (isInputBlocked || !GameStateManager.IsPlaying) return;
            OnSignatureSkillTriggered?.Invoke();
        }

        public void TriggerRelicSkill()
        {
            if (isInputBlocked || !GameStateManager.IsPlaying) return;
            OnRelicSkillTriggered?.Invoke();
        }
    }
}
