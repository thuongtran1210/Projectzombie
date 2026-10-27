using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Plays the imported Scholar poses from the existing Defense gameplay state.</summary>
    public sealed class DefenseScholarVisual3D : DefenseGameplayBehaviour
    {
        private enum Pose { Idle, Run, Dash, Attack, Skill, Die }
        private static readonly int[] POSE_HASHES =
        {
            Animator.StringToHash("Idle"), Animator.StringToHash("Run"),
            Animator.StringToHash("Dash"), Animator.StringToHash("Attack"),
            Animator.StringToHash("Skill"), Animator.StringToHash("Die")
        };

        [SerializeField] private Animator _animator;
        [SerializeField] private Rigidbody _body;
        [SerializeField] private DefenseHeroHealth3D _health;
        [SerializeField] private DefenseHeroMotor3D _motor;
        [SerializeField] private DefenseHeroAutoAttack3D _attack;
        [SerializeField] private DefenseScholarSignatureEffect3D _signature;
        [SerializeField] private DefenseHeroKitSO _kit;
        [SerializeField, Min(0f)] private float _runThreshold = 0.1f;
        private readonly float[] _clipLengths = new float[6];
        private Pose _pose;
        private bool _hasPose;
        private bool _wasAttacking;
        private bool _wasCasting;
        private float _actionUntil;
        private Pose _action;
        private float _dashDuration;

        protected override void OnGameplayBound()
        {
            _attack.AttackStarted -= OnBasicAttackStarted;
            _attack.AttackStarted += OnBasicAttackStarted;
            _dashDuration = _kit.DefinitionFor(DefenseSkillKind.Dash).Capture().Duration;
            foreach (var clip in _animator.runtimeAnimatorController.animationClips)
            {
                for (int i = 0; i < POSE_HASHES.Length; i++)
                {
                    if (Animator.StringToHash(clip.name) == POSE_HASHES[i]) _clipLengths[i] = clip.length;
                }
            }
            ResetPose();
        }

        private void OnBasicAttackStarted()
        {
            _action = Pose.Attack;
            _actionUntil = GameplayTime + DefenseBasicAttackRules.Recovery;
            _wasAttacking = true;
            _hasPose = false;
            Play(Pose.Attack);
        }

        private void Update()
        {
            if (!CanAdvanceGameplay || _animator == null) return;
            if (_health.IsDown)
            {
                Play(Pose.Die);
                return;
            }

            bool attacking = _attack.HasPendingAttack;
            bool casting = _signature.IsCasting;
            if (casting && !_wasCasting)
            {
                _action = Pose.Skill;
                _actionUntil = GameplayTime + _clipLengths[(int)Pose.Skill];
                _hasPose = false;
            }
            else if (attacking && !_wasAttacking)
            {
                _action = Pose.Attack;
                _actionUntil = GameplayTime + _clipLengths[(int)Pose.Attack];
                _hasPose = false;
            }
            _wasAttacking = attacking;
            _wasCasting = casting;

            if (_motor.IsDashing)
            {
                _actionUntil = 0f;
                Play(Pose.Dash);
                _animator.speed = _clipLengths[(int)Pose.Dash] / Mathf.Max(0.01f, _dashDuration);
            }
            else if (GameplayTime < _actionUntil)
            {
                Play(_action);
            }
            else
            {
                Vector3 velocity = _body.velocity;
                Play(velocity.x * velocity.x + velocity.z * velocity.z > _runThreshold * _runThreshold ? Pose.Run : Pose.Idle);
            }
        }

        private void Play(Pose pose)
        {
            if (_hasPose && pose == _pose) return;
            _pose = pose;
            _hasPose = true;
            _animator.speed = 1f;
            _animator.Play(POSE_HASHES[(int)pose], 0, 0f);
        }

        protected override void OnGameplayGateChanged(bool active)
        {
            if (_animator != null) _animator.speed = active ? 1f : 0f;
        }

        private void ResetPose()
        {
            _hasPose = false;
            _wasAttacking = false;
            _wasCasting = false;
            _actionUntil = 0f;
            Play(Pose.Idle);
        }

        protected override void OnGameplayReset() => ResetPose();
        protected override void OnGameplayCleanup()
        {
            _actionUntil = 0f;
            if (_animator != null) _animator.speed = 0f;
        }

        private void OnDisable()
        {
            if (_animator != null) _animator.speed = 0f;
        }
        protected override void OnDestroy() { if (_attack != null) _attack.AttackStarted -= OnBasicAttackStarted; base.OnDestroy(); }
    }
}
