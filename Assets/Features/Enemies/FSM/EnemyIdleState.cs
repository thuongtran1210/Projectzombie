using UnityEngine;

namespace ProjectZombie.Features.Enemies
{
    public class EnemyIdleState : EnemyState
    {
        public EnemyIdleState(Enemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
        }

        public override void Enter()
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

        public override void Update()
        {
            if (_enemy.PlayerTransform == null)
            {
                _enemy.FindPlayer();
            }

            // Nếu phát hiện người chơi, chuyển sang Chase
            if (_enemy.PlayerTransform != null)
            {
                _stateMachine.ChangeState(_enemy.ChaseState);
            }
        }
    }
}
