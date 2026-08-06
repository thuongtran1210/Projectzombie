using UnityEngine;

namespace ProjectZombie.Features.Enemies
{
    /// <summary>
    /// Chiến thuật di chuyển cho Quái Bắn Xa (Ranged).
    /// Hỗ trợ Kiting (lùi lại khi Player áp sát), giữ khoảng cách preferredDistance,
    /// và Strafe (di chuyển ngang nhẹ) để né tránh đạn của Player.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class RangedMovementStrategy : CombatMovementStrategy
    {
        [Header("Tactical Settings")]
        [SerializeField] private bool allowStrafe = true;
        [SerializeField] private float strafeChangeInterval = 2f;

        private float _nextStrafeTime;
        private float _strafeDirection = 1f; // 1: Phải, -1: Trái

        public override void Move()
        {
            if (_enemy.PlayerTransform == null || _enemy.Config == null) return;

            // 1. Kiểm tra trạng thái bị khống chế (Stun/Freeze/Knockback)
            if (_enemy.StatusController != null && !_enemy.StatusController.CanMove)
            {
                if (_enemy.EnemyAnimator != null) _enemy.EnemyAnimator.SetRunning(false);
                return;
            }

            float currentSpeed = _enemy.Config.moveSpeed * _enemy.MoveSpeedMultiplier;
            if (_enemy.StatusController != null)
            {
                currentSpeed = _enemy.StatusController.GetModifiedMoveSpeed(currentSpeed);
            }

            // Xử lý trạng thái bị nhốt trong Bát Quái Trận (TrapCircling)
            if (_enemy.IsTrapCircling)
            {
                Vector2 fromCenter = (Vector2)_enemy.transform.position - (Vector2)_enemy.TrapCenter;
                Vector2 tangentDir = new Vector2(-fromCenter.y, fromCenter.x).normalized;
                _enemy.Rb.velocity = tangentDir * (currentSpeed * 0.8f);

                if (_enemy.EnemyAnimator != null)
                {
                    _enemy.EnemyAnimator.SetRunning(true);
                    _enemy.EnemyAnimator.FlipToDirection(tangentDir.x);
                }
                return;
            }

            float distance = Vector2.Distance(_enemy.transform.position, _enemy.PlayerTransform.position);
            Vector2 directionToPlayer = (_enemy.PlayerTransform.position - _enemy.transform.position).normalized;

            // Đổi hướng Strafe định kỳ
            if (allowStrafe && Time.time >= _nextStrafeTime)
            {
                _strafeDirection = Random.value > 0.5f ? 1f : -1f;
                _nextStrafeTime = Time.time + Random.Range(strafeChangeInterval * 0.8f, strafeChangeInterval * 1.2f);
            }

            Vector2 strafeVector = new Vector2(-directionToPlayer.y, directionToPlayer.x) * _strafeDirection;
            Vector2 moveVelocity = Vector2.zero;

            if (distance < _enemy.Config.minDistance)
            {
                // Player quá gần (< minDistance) -> Lùi lại khẩn cấp + Strafe nhẹ
                Vector2 retreatDir = (-directionToPlayer + strafeVector * 0.4f).normalized;
                moveVelocity = retreatDir * (currentSpeed * 1.1f); // Tăng 10% tốc độ khi tháo chạy
            }
            else if (distance > _enemy.Config.preferredDistance)
            {
                // Player quá xa (> preferredDistance) -> Tiến lại gần + Strafe nhẹ
                Vector2 advanceDir = (directionToPlayer + strafeVector * 0.3f).normalized;
                moveVelocity = advanceDir * currentSpeed;
            }
            else
            {
                // Trong tầm an toàn (minDistance <= distance <= preferredDistance) -> Strafe di chuyển ngang nhẹ
                if (allowStrafe)
                {
                    moveVelocity = strafeVector * (currentSpeed * 0.5f);
                }
                else
                {
                    moveVelocity = Vector2.zero;
                }
            }

            _enemy.Rb.velocity = moveVelocity;

            if (_enemy.EnemyAnimator != null)
            {
                bool isMoving = moveVelocity.sqrMagnitude > 0.05f;
                _enemy.EnemyAnimator.SetRunning(isMoving);
                // Luôn luôn hướng mặt về phía Player khi bắn/di chuyển
                _enemy.EnemyAnimator.FlipToDirection(directionToPlayer.x);
            }
        }

        public override bool IsInAttackRange(float distanceToPlayer)
        {
            if (_enemy == null || _enemy.Config == null) return false;
            // Cho phép bắn khi xa hơn minDistance và nằm trong attackRange tổng thể
            return distanceToPlayer >= _enemy.Config.minDistance && distanceToPlayer <= _enemy.Config.AttackRange;
        }

        public override bool ShouldReposition(float distanceToPlayer)
        {
            if (_enemy == null || _enemy.Config == null) return false;
            // Chỉ bắt buộc Reposition khi Player áp sát quá gần (< minDistance) hoặc chạy khỏi tầm bắn
            return distanceToPlayer < _enemy.Config.minDistance || distanceToPlayer > _enemy.Config.AttackRange;
        }
    }
}
