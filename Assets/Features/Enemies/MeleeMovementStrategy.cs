using UnityEngine;

namespace ProjectZombie.Features.Enemies
{
    [RequireComponent(typeof(Enemy))]
    public class MeleeMovementStrategy : CombatMovementStrategy
    {
        public override void Move()
        {
            if (_enemy.PlayerTransform == null || _enemy.Config == null) return;

            float distance = Vector2.Distance(_enemy.transform.position, _enemy.PlayerTransform.position);
            
            // Di chuyển thẳng về phía người chơi nếu ngoài tầm đánh
            if (distance > _enemy.Config.AttackRange)
            {
                Vector2 direction = (_enemy.PlayerTransform.position - _enemy.transform.position).normalized;
                _enemy.Rb.velocity = direction * (_enemy.Config.moveSpeed * _enemy.MoveSpeedMultiplier);

                if (_enemy.EnemyAnimator != null)
                {
                    _enemy.EnemyAnimator.SetRunning(true);
                    _enemy.EnemyAnimator.FlipToDirection(direction.x);
                }
            }
            else
            {
                _enemy.Rb.velocity = Vector2.zero;
                if (_enemy.EnemyAnimator != null)
                {
                    _enemy.EnemyAnimator.SetRunning(false);
                }
            }
        }
    }
}
