using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectZombie.Features.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(PlayerMagnetTrigger))]
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

        private Rigidbody2D _rb;
        private PlayerStats _playerStats;
        
        private Vector2 _movementInput;
        private float _lastDashTime;
        private bool _isDashing;
        private float _dashEndTime;
        private Vector2 _dashDirection;

        private PlayerAnimator _playerAnimator;
        private Skills.SignatureSkillManager _signatureSkillManager;

        public static PlayerController Instance { get; private set; }
        public float LastDashTime => _lastDashTime;
        public Vector2 MovementInput => _movementInput;
        public bool IsDashing => _isDashing;

        /// <summary>
        /// Sự kiện phát ra khi nhân vật thực hiện kỹ năng Dash.
        /// </summary>
        public event System.Action OnDashed;

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
            _signatureSkillManager = GetComponent<Skills.SignatureSkillManager>();
            
            _rb.freezeRotation = true;
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
            if (_signatureSkillManager != null)
            {
                _signatureSkillManager.OnSkillExecuted += OnSkillExecuted;
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
            if (_signatureSkillManager != null)
            {
                _signatureSkillManager.OnSkillExecuted -= OnSkillExecuted;
            }
        }

        private void OnSkillExecuted()
        {
            if (_playerAnimator != null)
            {
                _playerAnimator.ChangeAnimationState(PlayerAnimationState.Attack);
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

            if (moveAction != null)
            {
                _movementInput = moveAction.action.ReadValue<Vector2>().normalized;
            }
            
            // Xử lý Lật mặt hình ảnh
            if (_playerAnimator != null && _movementInput.x != 0)
            {
                _playerAnimator.FlipToDirection(_movementInput.x);
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
                
                // Phát event để các module quan tâm (như TaoistYinYangTracker) tự lắng nghe
                OnDashed?.Invoke();

                // Gọi hoạt ảnh Lướt
                if (_playerAnimator != null)
                {
                    _playerAnimator.ChangeAnimationState(PlayerAnimationState.Dash);
                }
            }
        }

        private float _slowMultiplier = 1f;
        private Coroutine _slowCoroutine;

        /// <summary>
        /// Áp dụng hiệu ứng làm chậm (% slowPercent) trong khoảng thời gian duration (giây).
        /// </summary>
        public void ApplySlow(float slowPercent, float duration)
        {
            if (_slowCoroutine != null) StopCoroutine(_slowCoroutine);
            _slowCoroutine = StartCoroutine(SlowRoutine(slowPercent, duration));
        }

        private System.Collections.IEnumerator SlowRoutine(float slowPercent, float duration)
        {
            _slowMultiplier = Mathf.Clamp01(1f - slowPercent);
            yield return new WaitForSeconds(duration);
            _slowMultiplier = 1f;
        }

        private void FixedUpdate()
        {
            float currentSpeed = _playerStats.MoveSpeed * _slowMultiplier;

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

