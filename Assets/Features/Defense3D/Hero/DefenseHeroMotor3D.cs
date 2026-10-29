using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Di chuyển Defense3D và trạng thái dash; phát kết quả để phần trình diễn xử lý độc lập.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class DefenseHeroMotor3D : DefenseGameplayBehaviour
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private float turnRate = 14f;

        public float MoveSpeed => speed;
        public float TurnRate => turnRate;

        public void SetMovementParameters(float newSpeed, float newTurnRate)
        {
            if (newSpeed > 0f) speed = newSpeed;
            if (newTurnRate > 0f) turnRate = newTurnRate;
        }

        private Rigidbody _body;
        private DefenseHeroAutoAttack3D _basicAttack;
        public void FaceBasicAttack(object owner, DefensePoint3 direction)
        {
            SetAimFacing(owner, direction);
            if (_body != null && ReferenceEquals(_aimFacingOwner, owner)) _body.rotation = Quaternion.LookRotation(_aimFacing);
        }
        private IDefenseHeroCommands _commands;
        private object _aimFacingOwner;
        private Vector3 _aimFacing;

        public bool HasAimFacing => _aimFacingOwner != null;
        public bool CanAim(object owner) => _aimFacingOwner == null || ReferenceEquals(_aimFacingOwner, owner);
        public bool TryClaimAim(object owner)
        {
            if (owner == null || !CanAdvanceGameplay || !CanAim(owner)) return false;
            _aimFacingOwner = owner;
            _aimFacing = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            if (_aimFacing.sqrMagnitude < 0.000001f) _aimFacing = Vector3.forward;
            return true;
        }

        public void SetAimFacing(object owner, DefensePoint3 direction)
        {
            if (owner == null || !CanAdvanceGameplay || !direction.IsFinite || !CanAim(owner)) return;
            Vector3 horizontal = new Vector3(direction.X,0f,direction.Z);
            if (horizontal.sqrMagnitude < 0.000001f) return;
            _aimFacingOwner = owner;
            _aimFacing = horizontal.normalized;
        }

        public void ClearAimFacing(object owner)
        {
            if (ReferenceEquals(_aimFacingOwner,owner)) _aimFacingOwner = null;
        }
        private DefenseDashTraversal3D _dashTraversal;
        private Vector3 _dashStart, _dashTarget;
        private float _dashStartedAt, _dashDuration;
        public bool IsDashing { get; private set; }
        private long _nextDashId, _activeDashId;
        private object _dashIdentity;
        private int _dashGeneration;
        private DefensePoint3 _dashDirection;
        private DefenseDashFeedbackEvents _dashFeedback;
        public DefenseDashFeedbackEvents DashFeedback => _dashFeedback ?? (_dashFeedback = new DefenseDashFeedbackEvents(error => Debug.LogException(error, this)));

        public void BeginDash(Vector3 target, float duration, DefenseDashTraversal3D traversal)
        {
            if (!CanAdvanceGameplay || !isActiveAndEnabled || _body == null || IsDashing ||
                !DefenseDamageRules.IsFinite(duration) || duration <= 0f || traversal == null ||
                !DefenseDamageRules.IsFinite(target.x) || !DefenseDamageRules.IsFinite(target.y) || !DefenseDamageRules.IsFinite(target.z)) return;
            _dashStart = _body.position;
            _dashTarget = target;
            _dashDuration = duration;
            _dashStartedAt = GameplayTime;
            _dashTraversal = traversal;
            _activeDashId = checked(++_nextDashId);
            _dashIdentity = Gameplay.SessionIdentity;
            _dashGeneration = Gameplay.RunGeneration;
            Vector3 direction = target - _dashStart; direction.y = 0f;
            if (direction.sqrMagnitude < .000001f) direction = transform.forward;
            direction.Normalize();
            _dashDirection = Point(direction);
            IsDashing = true;
            StopHorizontalMovement();
            DashFeedback.PublishStarted(new DefenseDashFeedback(_activeDashId, Point(_dashStart), _dashDirection, _dashIdentity, _dashGeneration));
        }

        public void CancelDash() => FinishDash(false, _body != null ? _body.position : transform.position);

        private void FinishDash(bool completed, Vector3 position)
        {
            if (!IsDashing) return;
            var feedback = new DefenseDashFeedback(_activeDashId, Point(position), _dashDirection, _dashIdentity, _dashGeneration, completed);
            // Đóng trạng thái trước callback để cancel tái nhập không phát Ended lần thứ hai.
            IsDashing = false; _dashTraversal = null; _activeDashId = 0;
            _dashIdentity = null; _dashGeneration = -1; _dashDirection = default;
            DashFeedback.PublishEnded(feedback);
        }
        protected override void OnGameplayBound() => CancelDash();
        protected override void OnGameplayCleanup() => CancelDash();
        private static DefensePoint3 Point(Vector3 point) => new DefensePoint3(point.x, point.y, point.z);
        public void BindCommands(IDefenseHeroCommands commands) => _commands = commands;
        public float EffectiveSpeed => Gameplay != null ? Gameplay.Stats.Evaluate(DefenseUpgradeType.HeroMoveSpeed, speed) : speed;

        protected override void OnGameplayGateChanged(bool active)
        {
            if (!active) { _aimFacingOwner = null; CancelDash(); StopHorizontalMovement(); }
        }

        protected override void OnGameplayReset() { _aimFacingOwner = null; CancelDash(); StopHorizontalMovement(); }

        private void OnDisable() { _aimFacingOwner = null; CancelDash(); StopHorizontalMovement(); }

        private void StopHorizontalMovement()
        {
            if (_body == null) return;
            _body.velocity = new Vector3(0f, _body.velocity.y, 0f);
            _body.angularVelocity = Vector3.zero;
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _basicAttack = GetComponent<DefenseHeroAutoAttack3D>();
            _body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _body.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void FixedUpdate()
        {
            if (!CanAdvanceGameplay)
            {
                StopHorizontalMovement();
                return;
            }
            DefensePoint3 command = _commands != null ? _commands.Movement : default;
            if (_aimFacingOwner != null) _body.MoveRotation(Quaternion.LookRotation(_aimFacing));
            if (IsDashing)
            {
                float progress = Mathf.Clamp01((GameplayTime - _dashStartedAt) / _dashDuration);
                Vector3 desired = Vector3.Lerp(_dashStart, _dashTarget, progress);
                Vector3 reached = _body.position;
                Vector3 delta = desired - _body.position;
                delta.y = 0f;
                if (delta.sqrMagnitude > 0.001f)
                {
                    if (_dashTraversal == null || !_dashTraversal.TryResolve(_body.position, desired, out var safe))
                    { CancelDash(); StopHorizontalMovement(); return; }
                    _body.MovePosition(safe);
                    reached = safe;
                }
                StopHorizontalMovement();
                // MovePosition có thể chưa cập nhật body ngay trong callback vật lý;
                // dùng điểm đã được traversal chấp nhận cho burst kết thúc.
                if (progress >= 1f) FinishDash(true, reached);
                return;
            }
            Vector2 input = new Vector2(command.X, command.Z);
            Vector3 move = new Vector3(input.x, 0f, input.y) * EffectiveSpeed *
                (_basicAttack != null && _basicAttack.IsRecovering ? DefenseBasicAttackRules.MoveMultiplier : 1f);
            _body.velocity = new Vector3(move.x, _body.velocity.y, move.z);
            if (_aimFacingOwner == null && input.sqrMagnitude > 0.001f)
            {
                Quaternion face = Quaternion.LookRotation(new Vector3(input.x, 0f, input.y));
                _body.MoveRotation(Quaternion.Slerp(_body.rotation, face, turnRate * Time.fixedDeltaTime));
            }
        }
    }
}
