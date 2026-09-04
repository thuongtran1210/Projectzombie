using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Enemies
{
    /// <summary>
    /// Chiến thuật tấn công cho Quái Bắn Xa (Ranged).
    /// Hỗ trợ bắn đạn qua ProjectileSystem, hỗ trợ Aim Prediction (đón đầu hướng di chuyển của Player)
    /// và kiểm tra trạng thái bị khống chế trước khi bắn.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class RangedAttackStrategy : AttackStrategy
    {
        [Header("Ranged Settings")]
        [Tooltip("Data của viên đạn bắn ra")]
        public Projectiles.Data.ProjectileData projectileData;
        
        [Tooltip("Vị trí đạn sinh ra")]
        public Transform firePoint;

        [Header("Aim Prediction")]
        [Tooltip("Có tính toán góc đón đầu hướng di chuyển của Player hay không")]
        [SerializeField] private bool leadTarget = true;
        [Tooltip("Tốc độ đạn dự kiến (dùng để tính toán lead time)")]
        [SerializeField] private float estimatedProjectileSpeed = 8f;

        protected override void Awake()
        { base.Awake();
            if (firePoint == null)
            {
                firePoint = transform; // Fallback
            }
        }

        private bool _hasShotThisAttack = false;
        private bool _isAttacking = false;
        private Coroutine _rangedAttackRoutine;

        public override bool IsAttacking => _isAttacking;

        private void Start()
        {
            if (_enemy != null && _enemy.EnemyAnimator != null)
            {
                _enemy.EnemyAnimator.OnAttackEvent += Shoot;
            }
        }

        private void OnDestroy()
        {
            if (_enemy != null && _enemy.EnemyAnimator != null)
            {
                _enemy.EnemyAnimator.OnAttackEvent -= Shoot;
            }
        }

        public override void InterruptAttack()
        {
            _isAttacking = false;
            _hasShotThisAttack = false;
            if (_rangedAttackRoutine != null)
            {
                StopCoroutine(_rangedAttackRoutine);
                _rangedAttackRoutine = null;
            }
            CancelInvoke(nameof(Shoot));
        }

        public override void Attack()
        {
            if (_enemy.StatusController != null && !_enemy.StatusController.CanAttack)
            {
                return;
            }

            _hasShotThisAttack = false;
            if (_rangedAttackRoutine != null) StopCoroutine(_rangedAttackRoutine);
            _rangedAttackRoutine = StartCoroutine(ExecuteRangedAttackRoutine());
        }

        private System.Collections.IEnumerator ExecuteRangedAttackRoutine()
        {
            _isAttacking = true;

            float clipLength = _enemy.Animator != null 
                ? _enemy.Animator.GetCurrentClipLength(ProjectZombie.Features.Shared.AnimationConstants.ATTACK, 0.5f) 
                : 0.5f;

            _enemy.Animator?.TriggerAttack();

            // Chờ đúng thời điểm bắn đạn (45% clip)
            float shootDelay = clipLength * 0.45f;
            yield return new WaitForSeconds(shootDelay);

            if (_enemy.StatusController != null && !_enemy.StatusController.CanAttack)
            {
                InterruptAttack();
                yield break;
            }

            Shoot();

            // Chờ hoàn tất nốt animation (55% clip còn lại)
            float recoveryDelay = clipLength * 0.55f;
            yield return new WaitForSeconds(recoveryDelay);

            // Trả Animator về tư thế Idle trong thời gian chờ cooldown tiếp theo
            _enemy.Animator?.SetRunning(false);

            _isAttacking = false;
            _rangedAttackRoutine = null;
        }

        private void Shoot()
        {
            if (_hasShotThisAttack) return; // Khóa chống bắn đạn 2 lần trong 1 đợt tấn công

            if (projectileData == null)
            {
                Debug.LogWarning($"[{gameObject.name}] ⚠️ RangedAttackStrategy thiếu `projectileData` trong Inspector!");
                return;
            }

            if (_enemy.PlayerTransform == null) return;
            if (_enemy.StatusController != null && !_enemy.StatusController.CanAttack) return;

            _hasShotThisAttack = true;
            CancelInvoke(nameof(Shoot));

            Vector2 targetPosition = _enemy.PlayerTransform.position;

            // Tính toán Predict Position (Đón đầu hướng di chuyển của Player)
            if (leadTarget)
            {
                var playerRb = _enemy.PlayerTransform.GetComponent<Rigidbody2D>();
                if (playerRb != null && playerRb.velocity.sqrMagnitude > 0.1f)
                {
                    float distance = Vector2.Distance(firePoint.position, targetPosition);
                    float timeToTarget = distance / Mathf.Max(estimatedProjectileSpeed, 1f);
                    targetPosition += playerRb.velocity * timeToTarget;
                }
            }

            Vector2 direction = (targetPosition - (Vector2)firePoint.position).normalized;
            DamageData damageData = new DamageData(_enemy.GetTotalDamage(), false);

            if (Projectiles.Core.ProjectileSystem.Instance != null)
            {
                Projectiles.Core.ProjectileSystem.Instance.Spawn(
                    projectileData,
                    firePoint.position,
                    direction,
                    _enemy.gameObject,
                    damageData
                );
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] ⚠️ ProjectileSystem.Instance chưa được khởi tạo trong Scene!");
            }
        }
    }
}
