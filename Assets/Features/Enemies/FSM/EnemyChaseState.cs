using UnityEngine;

namespace ProjectZombie.Features.Enemies
{
    public class EnemyChaseState : EnemyState
    {
        public EnemyChaseState(Enemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
        }

        public override void Update()
        {
            if (_enemy.PlayerTransform == null)
            {
                _stateMachine.ChangeState(_enemy.IdleState);
                return;
            }

            float distance = Vector2.Distance(_enemy.transform.position, _enemy.PlayerTransform.position);
            
            if (_enemy.Config == null)
            {
                Debug.LogWarning($"[{_enemy.gameObject.name}] Thiếu EnemyConfig! Hãy gán scriptable object vào ô Config.");
                return;
            }

            if (_enemy.StatusController != null && !_enemy.StatusController.CanMove)
            {
                return;
            }

            if (_enemy.Movement != null && _enemy.Movement.IsInAttackRange(distance))
            {
                _stateMachine.ChangeState(_enemy.AttackState);
            }
            else if (_enemy.Movement == null && distance <= _enemy.Config.AttackRange)
            {
                _stateMachine.ChangeState(_enemy.AttackState);
            }
        }

        public override void FixedUpdate()
        {
            if (_enemy.Movement != null)
            {
                _enemy.Movement.Move();
            }
        }

        public override void Exit()
        {
            if (_enemy.Rb != null)
            {
                _enemy.Rb.velocity = Vector2.zero;
            }
            _enemy.Animator?.SetRunning(false);
        }
    }
}
