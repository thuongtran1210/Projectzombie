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
            _enemy.Animator?.TriggerDeath();
        }
    }
}
