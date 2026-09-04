using UnityEngine;
using ProjectZombie.Features.Boss;

namespace ProjectZombie.Features.Enemies.Boss.Skills
{
    /// <summary>
    /// Chiến lược di chuyển độc quyền cho Boss (Ngưu Đầu Mã Diện / Diêm Vương).
    /// Tách biệt hoàn toàn với MeleeMovementStrategy của Quái thường.
    /// Tương thích 100% với BossAnimator và bỏ qua di chuyển khi đang thi triển Skill (BullDash).
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class BossMovementStrategy : CombatMovementStrategy
    {
        private BossAnimator _bossAnimator;
        private BullDashSkill _bullDashSkill;

        protected override void Awake()
        {
            base.Awake();
            _bossAnimator = GetComponentInChildren<BossAnimator>();
            _bullDashSkill = GetComponent<BullDashSkill>();
        }

        public override void Move()
        {
            if (_enemy == null || _enemy.PlayerTransform == null || _enemy.Config == null) return;

            // Nếu đang lướt chiêu Ngưu Xung Thiên (Bull Dash) thì không can thiệp velocity di chuyển thường
            if (_bullDashSkill != null && _bullDashSkill.IsDashing)
            {
                return;
            }

            // Nếu đang bị Stun/Freeze
            if (_enemy.StatusController != null && !_enemy.StatusController.CanMove)
            {
                if (_enemy.Rb != null) _enemy.Rb.velocity = Vector2.zero;
                if (_bossAnimator != null) _bossAnimator.PlayAnimation("Idle");
                return;
            }

            float currentSpeed = _enemy.Config.moveSpeed * _enemy.MoveSpeedMultiplier;
            if (_enemy.StatusController != null)
            {
                currentSpeed = _enemy.StatusController.GetModifiedMoveSpeed(currentSpeed);
            }

            float distance = Vector2.Distance(_enemy.transform.position, _enemy.PlayerTransform.position);
            Vector2 direction = (_enemy.PlayerTransform.position - _enemy.transform.position).normalized;

            // Ngoài tầm đánh: Đuổi theo Player
            if (distance > _enemy.Config.AttackRange)
            {
                _enemy.Rb.velocity = direction * currentSpeed;

                if (_bossAnimator != null)
                {
                    _bossAnimator.PlayAnimation("Run");
                    _bossAnimator.FlipToDirection(direction.x);
                }
            }
            else
            {
                // Trong tầm đánh: Dừng di chuyển
                _enemy.Rb.velocity = Vector2.zero;
                if (_bossAnimator != null)
                {
                    _bossAnimator.PlayAnimation("Idle");
                    _bossAnimator.FlipToDirection(direction.x);
                }
            }
        }

        public override bool IsInAttackRange(float distanceToPlayer)
        {
            if (_enemy == null || _enemy.Config == null) return false;
            return distanceToPlayer <= _enemy.Config.AttackRange;
        }

        public override bool ShouldReposition(float distanceToPlayer) => false;
    }
}
