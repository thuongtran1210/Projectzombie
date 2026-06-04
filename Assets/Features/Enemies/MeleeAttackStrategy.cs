using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Enemies
{
    [RequireComponent(typeof(Enemy))]
    public class MeleeAttackStrategy : AttackStrategy
    {
        private void Start()
        {
            if (_enemy.EnemyAnimator != null)
            {
                _enemy.EnemyAnimator.OnAttackEvent += DealMeleeDamage;
            }
        }

        private void OnDestroy()
        {
            if (_enemy != null && _enemy.EnemyAnimator != null)
            {
                _enemy.EnemyAnimator.OnAttackEvent -= DealMeleeDamage;
            }
        }

        private void DealMeleeDamage()
        {
            if (_enemy.PlayerHealthSystem != null)
            {
                _enemy.PlayerHealthSystem.TakeDamage(_enemy.GetTotalDamage());
            }
        }

        public override void Attack()
        {
            // Chỉ trigger animation. Việc gây sát thương sẽ được gọi bởi Animation Event.
            if (_enemy.EnemyAnimator != null)
            {
                _enemy.EnemyAnimator.TriggerAttack();
            }
        }
    }
}
