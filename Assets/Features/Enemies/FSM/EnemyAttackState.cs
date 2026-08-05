using UnityEngine;

namespace ProjectZombie.Features.Enemies
{
    public class EnemyAttackState : EnemyState
    {
        private float _lastAttackTime;

        public EnemyAttackState(Enemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
        }

        public override void Enter()
        {
            _enemy.Rb.velocity = Vector2.zero;
            if (_enemy.EnemyAnimator != null)
            {
                _enemy.EnemyAnimator.SetRunning(false);
            }
        }

        public override void Update()
        {
            if (_enemy.PlayerTransform == null)
            {
                _stateMachine.ChangeState(_enemy.IdleState);
                return;
            }

            float distance = Vector2.Distance(_enemy.transform.position, _enemy.PlayerTransform.position);
            
            // Xoay mặt về phía người chơi
            if (_enemy.EnemyAnimator != null)
            {
                float dirX = _enemy.PlayerTransform.position.x - _enemy.transform.position.x;
                _enemy.EnemyAnimator.FlipToDirection(dirX);
            }

            // Kiểm tra xem quái có cần Reposition hay đổi lại trạng thái Chase dựa trên Strategy
            if (_enemy.Movement != null)
            {
                if (_enemy.Movement.ShouldReposition(distance))
                {
                    _stateMachine.ChangeState(_enemy.RepositionState);
                    return;
                }
                if (!_enemy.Movement.IsInAttackRange(distance))
                {
                    _stateMachine.ChangeState(_enemy.ChaseState);
                    return;
                }
            }
            else
            {
                // Fallback nếu không có Movement component
                if (distance > _enemy.Config.AttackRange)
                {
                    _stateMachine.ChangeState(_enemy.ChaseState);
                    return;
                }
            }

            // Logic tấn công
            if (Time.time >= _lastAttackTime + _enemy.Config.attackCooldown)
            {
                if (_enemy.Attacker != null)
                {
                    _enemy.Attacker.Attack();
                }
                _lastAttackTime = Time.time;
            }
        }

        public override void FixedUpdate()
        {
            _enemy.Rb.velocity = Vector2.zero;
        }
    }
}
