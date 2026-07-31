using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectZombie.Features.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Input Actions")]
        [Tooltip("Kéo thả Input Action cho di chuyển (Vector2)")]
        [SerializeField] private InputActionReference moveAction;
        [Tooltip("Kéo thả Input Action cho lướt (Button)")]
        [SerializeField] private InputActionReference dashAction;

        [Header("Dash Settings")]
        [SerializeField] private float dashSpeedMultiplier = 3f;
        [SerializeField] private float dashDuration = 0.2f;

        [Header("Mobile Joystick Reference (Optional Override)")]
        [SerializeField] private UI.DynamicVirtualJoystick virtualJoystick;

        private Rigidbody2D _rb;
        private PlayerStats _playerStats;
        
        private Vector2 _movementInput;
        private float _lastDashTime;
        private bool _isDashing;
        private float _dashEndTime;
        private Vector2 _dashDirection;

        private PlayerAnimator _playerAnimator;

        public static PlayerController Instance { get; private set; }
        public float LastDashTime => _lastDashTime;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            _rb = GetComponent<Rigidbody2D>();
            _playerStats = GetComponent<PlayerStats>();
            _playerAnimator = GetComponent<PlayerAnimator>();
            
            _rb.freezeRotation = true;
        }

        private void Start()
        {
            if (virtualJoystick == null)
            {
                virtualJoystick = FindObjectOfType<UI.DynamicVirtualJoystick>();
                if (virtualJoystick != null)
                {
                    Debug.Log($"[{nameof(PlayerController)}] Tự động phát hiện và liên kết DynamicVirtualJoystick trong Scene.");
                }
            }
        }

        private void OnEnable()
        {
            if (moveAction != null)
            {
                moveAction.action.Enable();
            }
            if (dashAction != null)
            {
                dashAction.action.Enable();
                dashAction.action.performed += OnDashPerformed;
            }
        }

        private void OnDisable()
        {
            if (moveAction != null)
            {
                moveAction.action.Disable();
            }
            if (dashAction != null)
            {
                dashAction.action.Disable();
                dashAction.action.performed -= OnDashPerformed;
            }
        }

        private void Update()
        {
            if (_isDashing)
            {
                if (Time.time >= _dashEndTime)
                {
                    _isDashing = false;
                }
                return; // Khi đang lướt thì không nhận input di chuyển mới
            }

            if (virtualJoystick != null && virtualJoystick.InputVector != Vector2.zero)
            {
                _movementInput = virtualJoystick.InputVector;
            }
            else if (moveAction != null)
            {
                _movementInput = moveAction.action.ReadValue<Vector2>().normalized;
            }
            
            // Xử lý Lật mặt hình ảnh
            if (_playerAnimator != null && _movementInput.x != 0)
            {
                _playerAnimator.FlipToDirection(_movementInput.x);
            }

            // Tích hợp Cán Cân Âm Dương (v4.0):
            // Di chuyển liên tục / Lướt -> Nghiêng Dương (+delta)
            // Đứng yên né tránh -> Nghiêng Âm (-delta)
            if (YinYang.YinYangManager.Instance != null)
            {
                if (_movementInput.sqrMagnitude > 0.01f || _isDashing)
                {
                    // Di chuyển/Lướt tăng Dương nhẹ (1.5 điểm mỗi giây)
                    YinYang.YinYangManager.Instance.AdjustValue(1.5f * Time.deltaTime);
                }
                else
                {
                    // Đứng yên nghiêng Âm nhẹ (2.0 điểm mỗi giây)
                    YinYang.YinYangManager.Instance.AdjustValue(-2.0f * Time.deltaTime);
                }
            }
        }

        private void OnDashPerformed(InputAction.CallbackContext context)
        {
            PerformDash();
        }

        /// <summary>
        /// Kích hoạt kỹ năng Lướt (Dash). Có thể gọi từ Input Action hoặc UI Dash Button trên mobile.
        /// </summary>
        public void PerformDash()
        {
            if (_playerStats == null) return;

            if (Time.time >= _lastDashTime + _playerStats.DashCooldown && !_isDashing && _movementInput != Vector2.zero)
            {
                _isDashing = true;
                _dashEndTime = Time.time + dashDuration;
                _lastDashTime = Time.time;
                _dashDirection = _movementInput; // Lướt theo hướng đang đi
                
                // Dash cộng thưởng Dương tức thì (+3 điểm Dương)
                if (YinYang.YinYangManager.Instance != null)
                {
                    YinYang.YinYangManager.Instance.AdjustValue(3.0f);
                }

                // Gọi hoạt ảnh Lướt
                if (_playerAnimator != null)
                {
                    _playerAnimator.ChangeAnimationState(PlayerAnimationState.Dash);
                }
            }
        }

        private void FixedUpdate()
        {
            float currentSpeed = _playerStats.MoveSpeed;

            if (_isDashing)
            {
                _rb.velocity = _dashDirection * (currentSpeed * dashSpeedMultiplier);
            }
            else
            {
                _rb.velocity = _movementInput * currentSpeed;

                // Xử lý hoạt ảnh Chạy/Đứng im khi không lướt
                if (_playerAnimator != null)
                {
                    if (_movementInput.sqrMagnitude > 0.01f)
                        _playerAnimator.ChangeAnimationState(PlayerAnimationState.Run);
                    else
                        _playerAnimator.ChangeAnimationState(PlayerAnimationState.Idle);
                }
            }
        }
    }
}

