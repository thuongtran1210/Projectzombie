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

        private void Start()
        {
            if (_enemy.EnemyAnimator != null)
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

        public override void Attack()
        {
            // Kiểm tra xem có đang bị Stun/Freeze không trước khi trigger attack
            if (_enemy.StatusController != null && !_enemy.StatusController.CanMove)
            {
                return;
            }

            // Chỉ trigger animation. Việc bắn đạn (Shoot) sẽ được gọi bởi Animation Event.
            if (_enemy.EnemyAnimator != null)
            {
                _enemy.EnemyAnimator.TriggerAttack();
            }
            else
            {
                // Fallback nếu không có animator -> Bắn trực tiếp
                Shoot();
            }
        }

        private void Shoot()
        {
            if (projectileData == null || _enemy.PlayerTransform == null) return;
            if (_enemy.StatusController != null && !_enemy.StatusController.CanMove) return;

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
        }
    }
}
