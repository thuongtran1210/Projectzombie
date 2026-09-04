using UnityEngine;

namespace ProjectZombie.Features.Enemies
{
    public class EnemyDeadState : EnemyState
    {
        public EnemyDeadState(Enemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
        }

        public override void Enter()
        {
            _enemy.Rb.velocity = Vector2.zero;
            
            if (_enemy.EnemyAnimator != null)
            {
                _enemy.EnemyAnimator.TriggerDeath();
                _enemy.EnemyAnimator.SetRunning(false);
            }
            else if (_enemy.BossAnimator != null)
            {
                _enemy.BossAnimator.TriggerDeath();
            }
        }
    }
}
