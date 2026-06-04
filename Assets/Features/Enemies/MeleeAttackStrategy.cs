using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Enemies
{
    [RequireComponent(typeof(Enemy))]
    public class MeleeAttackStrategy : AttackStrategy
    {
        public override void Attack()
        {
            if (_enemy.PlayerHealthSystem != null)
            {
                _enemy.PlayerHealthSystem.TakeDamage(_enemy.GetTotalDamage());
                
                if (_enemy.EnemyAnimator != null)
                {
                    _enemy.EnemyAnimator.TriggerAttack();
                }
            }
        }
    }
}
