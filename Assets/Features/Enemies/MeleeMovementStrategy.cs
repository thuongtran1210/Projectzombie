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
                // Chỉ dừng vận tốc nếu không đang trong pha vật lý Knockback/Ragdoll
                if (_enemy.StatusController.CanMove) 
                {
                    _enemy.Rb.velocity = Vector2.zero;
                }
                else if (!_enemy.StatusController.IsRagdollActive && (_enemy.GetComponent<EnemyKinematicPhysics>() == null || !_enemy.GetComponent<EnemyKinematicPhysics>().IsKnockbackActive))
                {
                    _enemy.Rb.velocity = Vector2.zero;
                }
                _enemy.Animator?.SetRunning(false);
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

                _enemy.Animator?.SetRunning(true);
                _enemy.Animator?.FlipToDirection(tangentDir.x);
                return;
            }

            // Xử lý hành vi Ma Đòi Nợ bỏ chạy sau khi thó tiền thành công
            if (_enemy.TryGetComponent<Special.EnemyDebtCollector>(out var debtCollector) && debtCollector.IsFleeing)
            {
                Vector2 awayDir = ((Vector2)_enemy.transform.position - (Vector2)_enemy.PlayerTransform.position).normalized;
                if (awayDir.sqrMagnitude < 0.001f) awayDir = Random.insideUnitCircle.normalized;
                
                Vector2 steeredAwayDir = CalculateSteeringDirection(awayDir);
                _enemy.Rb.velocity = steeredAwayDir * currentSpeed;

                _enemy.Animator?.SetRunning(true);
                _enemy.Animator?.FlipToDirection(steeredAwayDir.x);
                return;
            }

            float distance = Vector2.Distance(_enemy.transform.position, _enemy.PlayerTransform.position);
            
            // Di chuyển về phía người chơi nếu ngoài tầm đánh (kết hợp né vật cản và trượt tường)
            if (distance > _enemy.Config.AttackRange)
            {
                Vector2 rawDirection = ((Vector2)_enemy.PlayerTransform.position - (Vector2)_enemy.transform.position).normalized;
                Vector2 steeredDirection = CalculateSteeringDirection(rawDirection);
                _enemy.Rb.velocity = steeredDirection * currentSpeed;

                _enemy.Animator?.SetRunning(true);
                _enemy.Animator?.FlipToDirection(steeredDirection.x);
            }
            else
            {
                _enemy.Rb.velocity = Vector2.zero;
                _enemy.Animator?.SetRunning(false);
            }
        }

        public override bool IsInAttackRange(float distanceToPlayer)
        {
            if (_enemy == null || _enemy.Config == null) return false;
            if (_enemy.TryGetComponent<Special.EnemyDebtCollector>(out var debtCollector) && debtCollector.IsFleeing)
            {
                return false;
            }
            return distanceToPlayer <= _enemy.Config.AttackRange;
        }

        public override bool ShouldReposition(float distanceToPlayer) => false;
    }
}
