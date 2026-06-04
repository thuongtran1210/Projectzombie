using UnityEngine;

namespace ProjectZombie.Features.Enemies
{
    public class EnemyRepositionState : EnemyState
    {
        private float _lastAttackTime;

        public EnemyRepositionState(Enemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
        }

        public override void Enter()
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

            // Kiểm tra xem đã đạt đến vị trí an toàn chưa (dành cho Ranged)
            if (_enemy.Movement is RangedMovementStrategy)
            {
                if (distance >= _enemy.Config.minDistance && distance <= _enemy.Config.preferredDistance)
                {
                    _stateMachine.ChangeState(_enemy.AttackState);
                }
            }
            else
            {
                // Nếu là Melee lỡ vào trạng thái này, đưa về Chase
                _stateMachine.ChangeState(_enemy.ChaseState);
            }
            
            // Có thể tấn công kể cả khi đang di chuyển Reposition (vd vừa chạy vừa bắn)
            if (Time.time >= _lastAttackTime + _enemy.Config.attackCooldown)
            {
                if (_enemy.Attacker != null && distance <= _enemy.Config.AttackRange)
                {
                    _enemy.Attacker.Attack();
                    _lastAttackTime = Time.time;
                }
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
        }
    }
}
