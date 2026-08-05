using UnityEngine;

namespace ProjectZombie.Features.Enemies
{
    [RequireComponent(typeof(Enemy))]
    public class RangedMovementStrategy : CombatMovementStrategy
    {
        public override void Move()
        {
            if (_enemy.PlayerTransform == null || _enemy.Config == null) return;

            float distance = Vector2.Distance(_enemy.transform.position, _enemy.PlayerTransform.position);
            Vector2 directionToPlayer = (_enemy.PlayerTransform.position - _enemy.transform.position).normalized;

            if (distance < _enemy.Config.minDistance)
            {
                // Người chơi quá gần -> Lùi lại
                Vector2 retreatDir = -directionToPlayer;
                _enemy.Rb.velocity = retreatDir * (_enemy.Config.moveSpeed * _enemy.MoveSpeedMultiplier);

                if (_enemy.EnemyAnimator != null)
                {
                    _enemy.EnemyAnimator.SetRunning(true);
                    // Vẫn quay mặt về phía người chơi khi lùi
                    _enemy.EnemyAnimator.FlipToDirection(directionToPlayer.x); 
                }
            }
            else if (distance > _enemy.Config.preferredDistance)
            {
                // Người chơi ở quá xa -> Tiến lại gần
                _enemy.Rb.velocity = directionToPlayer * (_enemy.Config.moveSpeed * _enemy.MoveSpeedMultiplier);

                if (_enemy.EnemyAnimator != null)
                {
                    _enemy.EnemyAnimator.SetRunning(true);
                    _enemy.EnemyAnimator.FlipToDirection(directionToPlayer.x);
                }
            }
            else
            {
                // Trong tầm an toàn -> Đứng yên
                _enemy.Rb.velocity = Vector2.zero;
                if (_enemy.EnemyAnimator != null)
                {
                    _enemy.EnemyAnimator.SetRunning(false);
                    _enemy.EnemyAnimator.FlipToDirection(directionToPlayer.x);
                }
            }
        }

        public override bool IsInAttackRange(float distanceToPlayer)
        {
            if (_enemy == null || _enemy.Config == null) return false;
            // Nằm trong khoảng an toàn (tối thiểu -> tối đa/preferred)
            return distanceToPlayer >= _enemy.Config.minDistance && distanceToPlayer <= _enemy.Config.preferredDistance;
        }

        public override bool ShouldReposition(float distanceToPlayer)
        {
            if (_enemy == null || _enemy.Config == null) return false;
            // Nếu người chơi quá gần (cần lùi lại) hoặc quá xa (cần đuổi theo)
            return distanceToPlayer < _enemy.Config.minDistance || distanceToPlayer > _enemy.Config.preferredDistance;
        }
    }
}
