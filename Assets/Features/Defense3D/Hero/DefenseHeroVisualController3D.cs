using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Bộ điều khiển hoạt ảnh (Visual Controller) đa năng cho mọi Tướng trong Defense 3D.
    /// Tuân thủ nguyên tắc Open/Closed (OCP) và Dependency Inversion (DIP):
    /// - Không phụ thuộc vào Tướng cụ thể nào.
    /// - Giao tiếp với tuyệt kỹ qua interface IDefenseSignatureEffect.
    /// - Điều khiển các trạng thái Idle, Run, Dash, Attack, Skill, Die qua Pose Hashes chuẩn.
    /// </summary>
    public class DefenseHeroVisualController3D : DefenseGameplayBehaviour
    {
        public enum Pose { Idle, Run, Dash, Attack, Skill, Die }

        protected static readonly int[] POSE_HASHES =
        {
            Animator.StringToHash("Idle"), Animator.StringToHash("Run"),
            Animator.StringToHash("Dash"), Animator.StringToHash("Attack"),
            Animator.StringToHash("Skill"), Animator.StringToHash("Die")
        };

        [Header("Visual & Physics References")]
        [SerializeField] protected Animator _animator;
        [SerializeField] protected Rigidbody _body;
        [SerializeField] protected DefenseHeroHealth3D _health;
        [SerializeField] protected DefenseHeroMotor3D _motor;
        [SerializeField] protected DefenseHeroAutoAttack3D _attack;

        [Header("Signature & Kit (Abstraction)")]
        [SerializeField] protected MonoBehaviour _signatureEffectComponent;
        [SerializeField] protected DefenseHeroKitSO _kit;
        [SerializeField, Min(0f)] protected float _runThreshold = 0.1f;

        protected IDefenseSignatureEffect _signatureEffect;
        protected readonly float[] _clipLengths = new float[6];
        protected Pose _pose;
        protected bool _hasPose;
        protected bool _wasAttacking;
        protected bool _wasCasting;
        protected float _actionUntil;
        protected Pose _action;
        protected float _dashDuration = 0.25f;

        public Animator Animator => _animator;
        public Pose CurrentPose => _pose;

        public void BindComponents(
            Animator animator,
            Rigidbody body,
            DefenseHeroHealth3D health,
            DefenseHeroMotor3D motor,
            DefenseHeroAutoAttack3D attack,
            IDefenseSignatureEffect signatureEffect,
            DefenseHeroKitSO kit)
        {
            _animator = animator;
            _body = body;
            _health = health;
            _motor = motor;
            _attack = attack;
            _signatureEffect = signatureEffect;
            if (signatureEffect is MonoBehaviour mb) _signatureEffectComponent = mb;
            _kit = kit;
        }

        public void SetKit(DefenseHeroKitSO kit)
        {
            _kit = kit;
            if (_kit != null)
            {
                var dashDef = _kit.DefinitionFor(DefenseSkillKind.Dash);
                if (dashDef != null)
                {
                    _dashDuration = dashDef.Capture().Duration;
                }
            }
        }

        public void SetSignatureEffect(IDefenseSignatureEffect signatureEffect)
        {
            _signatureEffect = signatureEffect;
            if (signatureEffect is MonoBehaviour mb) _signatureEffectComponent = mb;
        }

        protected virtual void Awake()
        {
            if (_body == null) _body = GetComponent<Rigidbody>();
            if (_health == null) _health = GetComponent<DefenseHeroHealth3D>();
            if (_motor == null) _motor = GetComponent<DefenseHeroMotor3D>();
            if (_attack == null) _attack = GetComponent<DefenseHeroAutoAttack3D>();
            if (_animator == null) _animator = GetComponentInChildren<Animator>(true);
            if (_signatureEffect == null && _signatureEffectComponent is IDefenseSignatureEffect sig)
            {
                _signatureEffect = sig;
            }
            else if (_signatureEffect == null)
            {
                _signatureEffect = GetComponent<IDefenseSignatureEffect>();
            }
        }

        protected override void OnGameplayBound()
        {
            if (_signatureEffect == null && _signatureEffectComponent is IDefenseSignatureEffect sig)
            {
                _signatureEffect = sig;
            }

            if (_attack != null)
            {
                _attack.AttackStarted -= OnBasicAttackStarted;
                _attack.AttackStarted += OnBasicAttackStarted;
            }

            if (_kit != null)
            {
                var dashDef = _kit.DefinitionFor(DefenseSkillKind.Dash);
                if (dashDef != null)
                {
                    _dashDuration = dashDef.Capture().Duration;
                }
            }

            CacheClipLengths();
            ResetPose();
        }

        protected virtual void CacheClipLengths()
        {
            if (_animator == null || _animator.runtimeAnimatorController == null) return;

            foreach (var clip in _animator.runtimeAnimatorController.animationClips)
            {
                for (int i = 0; i < POSE_HASHES.Length; i++)
                {
                    if (Animator.StringToHash(clip.name) == POSE_HASHES[i])
                    {
                        _clipLengths[i] = clip.length;
                    }
                }
            }
        }

        protected virtual void OnBasicAttackStarted()
        {
            _action = Pose.Attack;
            _actionUntil = GameplayTime + DefenseBasicAttackRules.Recovery;
            _wasAttacking = true;
            _hasPose = false;
            Play(Pose.Attack);
        }

        protected virtual void Update()
        {
            if (!CanAdvanceGameplay || _animator == null) return;

            if (_health != null && _health.IsDown)
            {
                Play(Pose.Die);
                return;
            }

            bool attacking = _attack != null && _attack.HasPendingAttack;
            bool casting = _signatureEffect != null && _signatureEffect.IsCasting;

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

            if (_motor != null && _motor.IsDashing)
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
                Vector3 velocity = _body != null ? _body.velocity : Vector3.zero;
                bool isMoving = velocity.x * velocity.x + velocity.z * velocity.z > _runThreshold * _runThreshold;
                Play(isMoving ? Pose.Run : Pose.Idle);
            }
        }

        public virtual void Play(Pose pose)
        {
            if (_hasPose && pose == _pose) return;
            _pose = pose;
            _hasPose = true;
            if (_animator != null)
            {
                _animator.speed = 1f;
                _animator.Play(POSE_HASHES[(int)pose], 0, 0f);
            }
        }

        protected override void OnGameplayGateChanged(bool active)
        {
            if (_animator != null) _animator.speed = active ? 1f : 0f;
        }

        protected virtual void ResetPose()
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

        protected virtual void OnDisable()
        {
            if (_animator != null) _animator.speed = 0f;
        }

        protected override void OnDestroy()
        {
            if (_attack != null) _attack.AttackStarted -= OnBasicAttackStarted;
            base.OnDestroy();
        }
    }
}
