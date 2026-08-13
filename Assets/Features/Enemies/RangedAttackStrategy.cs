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

            _hasShotThisAttack = false; // Reset trạng thái cho đợt bắn mới

            if (_enemy.EnemyAnimator != null)
            {
                _enemy.EnemyAnimator.TriggerAttack();
            }

            // Fallback tự động bắn sau 0.25s nếu Animation Clip thiếu Animation Event
            CancelInvoke(nameof(Shoot));
            Invoke(nameof(Shoot), 0.25f);
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
            if (_enemy.StatusController != null && !_enemy.StatusController.CanMove) return;

            _hasShotThisAttack = true;
            CancelInvoke(nameof(Shoot)); // Hủy timer fallback nếu Animation Event đã kích hoạt trước

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
