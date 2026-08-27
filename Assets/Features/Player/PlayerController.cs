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
        public float FacingDirection => _playerAnimator != null ? _playerAnimator.FacingDirection : (transform.localScale.x >= 0 ? 1f : -1f);
        public Vector2 FacingVector => new Vector2(FacingDirection, 0f);

        /// <summary>
        /// Sự kiện phát ra khi nhân vật thực hiện kỹ năng Dash.
        /// </summary>
        public event System.Action OnDashed;

        private void Awake()
        {
            Instance = this;

            _rb = GetComponent<Rigidbody2D>();
            _playerStats = GetComponent<PlayerStats>();
            _playerAnimator = GetComponent<PlayerAnimator>();
            _signatureSkillManager = GetComponent<Skills.SignatureSkillManager>();

            if (GetComponent<Visuals.PlayerStatusVisuals>() == null)
            {
                gameObject.AddComponent<Visuals.PlayerStatusVisuals>();
            }
            
            _rb.freezeRotation = true;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
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

        private bool _isAttacking;
        private float _attackSlowdownEndTime;

        public bool IsAttacking => _isAttacking;

        /// <summary>
        /// Thông báo từ Vũ khí chính khi bắt đầu tung 1 đòn chém.
        /// </summary>
        public void NotifyAttackStarted(int comboStep)
        {
            _isAttacking = true;
            _attackSlowdownEndTime = Time.time + 0.12f; // Giảm tốc trong 0.12s để tạo lực đầm

            // Kích hoạt Smart Soft-Lock nếu người chơi không chủ động kéo cần di chuyển
            if (_movementInput == Vector2.zero)
            {
                TryAutoAimAtNearestEnemy(5.0f);
            }

            if (_playerAnimator != null)
            {
                _playerAnimator.ChangeAnimationState(PlayerAnimationState.Attack);
            }
        }

        /// <summary>
        /// Tự động quay mặt về phía kẻ địch gần nhất trong bán kính cho phép.
        /// </summary>
        public void TryAutoAimAtNearestEnemy(float radius = 5.0f)
        {
            Collider2D[] buffer = new Collider2D[10];
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, radius, buffer, Shared.TargetingUtility.EnemyLayerMask);
            if (count == 0) return;

            Collider2D closest = null;
            float closestDistSqr = float.MaxValue;
            Vector2 currentPos = transform.position;

            for (int i = 0; i < count; i++)
            {
                var col = buffer[i];
                if (col == null) continue;
                float dSqr = ((Vector2)col.transform.position - currentPos).sqrMagnitude;
                if (dSqr < closestDistSqr)
                {
                    closestDistSqr = dSqr;
                    closest = col;
                }
            }

            if (closest != null && _playerAnimator != null)
            {
                float dirX = closest.transform.position.x - transform.position.x;
                if (Mathf.Abs(dirX) > 0.05f)
                {
                    _playerAnimator.FlipToDirection(dirX);
                }
            }
        }

        private void OnDashPerformed(InputAction.CallbackContext context)
        {
            PerformDash();
        }

        /// <summary>
        /// Kích hoạt kỹ năng Lướt (Dash). Có thể gọi từ Input Action hoặc UI Dash Button trên mobile.
        /// Hỗ trợ Dash Cancel (hủy ngay đòn chém đang dở để né đòn).
        /// </summary>
        public void PerformDash()
        {
            if (_playerStats == null) return;

            if (Time.time >= _lastDashTime + _playerStats.DashCooldown && !_isDashing)
            {
                // Dash Cancel: Hủy trạng thái chém đang dở
                _isAttacking = false;

                _isDashing = true;
                _dashEndTime = Time.time + dashDuration;
                _lastDashTime = Time.time;
                
                // Nếu đang đứng yên, lướt theo hướng mặt hiện tại
                if (_movementInput != Vector2.zero)
                {
                    _dashDirection = _movementInput;
                }
                else
                {
                    _dashDirection = transform.localScale.x >= 0 ? Vector2.right : Vector2.left;
                }
                
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

        public float CurrentSlowMultiplier => _slowMultiplier;
        public event System.Action<bool, float> OnSlowStatusChanged;

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
            OnSlowStatusChanged?.Invoke(true, _slowMultiplier);

            yield return new WaitForSeconds(duration);

            _slowMultiplier = 1f;
            OnSlowStatusChanged?.Invoke(false, 1f);
            _slowCoroutine = null;
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
                // Giảm tốc nhẹ 40% trong lúc vung đòn (Movement Slowdown) để tạo lực đầm
                if (_isAttacking)
                {
                    if (Time.time < _attackSlowdownEndTime)
                    {
                        currentSpeed *= 0.4f;
                    }
                    else
                    {
                        _isAttacking = false;
                    }
                }

                _rb.velocity = _movementInput * currentSpeed;

                // Xử lý hoạt ảnh Chạy/Đứng im khi không lướt và không chém
                if (_playerAnimator != null && !_isAttacking)
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

