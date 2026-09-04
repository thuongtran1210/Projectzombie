using UnityEngine;

namespace ProjectZombie.Features.Enemies
{
    [RequireComponent(typeof(Enemy))]
    public class MeleeMovementStrategy : CombatMovementStrategy
    {
        public override void Move()
        {
            if (_enemy.PlayerTransform == null || _enemy.Config == null) return;

            // Nếu đang bị Stun/Freeze hoặc Knockback thì không di chuyển logic
            if (_enemy.StatusController != null && !_enemy.StatusController.CanMove)
            {
                if (_enemy.EnemyAnimator != null) _enemy.EnemyAnimator.SetRunning(false);
                else if (_enemy.BossAnimator != null) _enemy.BossAnimator.PlayAnimation("Idle");
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
                Vector2 tangentDir = new Vector2(-fromCenter.y, fromCenter.x).normalized; // Hướng di chuyển vòng quanh tâm
                _enemy.Rb.velocity = tangentDir * (currentSpeed * 0.8f);

                if (_enemy.EnemyAnimator != null)
                {
                    _enemy.EnemyAnimator.SetRunning(true);
                    _enemy.EnemyAnimator.FlipToDirection(tangentDir.x);
                }
                else if (_enemy.BossAnimator != null)
                {
                    _enemy.BossAnimator.PlayAnimation("Run");
                    _enemy.BossAnimator.FlipToDirection(tangentDir.x);
                }
                return;
            }

            float distance = Vector2.Distance(_enemy.transform.position, _enemy.PlayerTransform.position);
            
            // Di chuyển thẳng về phía người chơi nếu ngoài tầm đánh
            if (distance > _enemy.Config.AttackRange)
            {
                Vector2 direction = (_enemy.PlayerTransform.position - _enemy.transform.position).normalized;
                _enemy.Rb.velocity = direction * currentSpeed;

                if (_enemy.EnemyAnimator != null)
                {
                    _enemy.EnemyAnimator.SetRunning(true);
                    _enemy.EnemyAnimator.FlipToDirection(direction.x);
                }
                else if (_enemy.BossAnimator != null)
                {
                    _enemy.BossAnimator.PlayAnimation("Run");
                    _enemy.BossAnimator.FlipToDirection(direction.x);
                }
            }
            else
            {
                _enemy.Rb.velocity = Vector2.zero;
                if (_enemy.EnemyAnimator != null)
                {
                    _enemy.EnemyAnimator.SetRunning(false);
                }
                else if (_enemy.BossAnimator != null)
                {
                    _enemy.BossAnimator.PlayAnimation("Idle");
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
