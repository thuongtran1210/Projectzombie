using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Single-player Defense 3D prototype only. Does not bind Legacy PlayerController,
    /// Physics2D, survival loadouts or the old game-state bootstrapper.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class DefenseHeroMotor3D : DefenseGameplayBehaviour
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private float turnRate = 14f;

        private Rigidbody _body;
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
            IsDashing = true;
            StopHorizontalMovement();
        }

        public void CancelDash() { IsDashing = false; _dashTraversal = null; }
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
                Vector3 delta = desired - _body.position;
                delta.y = 0f;
                if (delta.sqrMagnitude > 0.001f)
                {
                    if (_dashTraversal == null || !_dashTraversal.TryResolve(_body.position, desired, out var safe))
                    { CancelDash(); StopHorizontalMovement(); return; }
                    _body.MovePosition(safe);
                }
                StopHorizontalMovement();
                if (progress >= 1f) CancelDash();
                return;
            }
            Vector2 input = new Vector2(command.X, command.Z);
            Vector3 move = new Vector3(input.x, 0f, input.y) * EffectiveSpeed;
            _body.velocity = new Vector3(move.x, _body.velocity.y, move.z);
            if (_aimFacingOwner == null && input.sqrMagnitude > 0.001f)
            {
                Quaternion face = Quaternion.LookRotation(new Vector3(input.x, 0f, input.y));
                _body.MoveRotation(Quaternion.Slerp(_body.rotation, face, turnRate * Time.fixedDeltaTime));
            }
        }
    }
}
