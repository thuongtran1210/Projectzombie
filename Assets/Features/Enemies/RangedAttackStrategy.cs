using UnityEngine;
using UnityEngine.Pool;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Weapons;

namespace ProjectZombie.Features.Enemies
{
    [RequireComponent(typeof(Enemy))]
    public class RangedAttackStrategy : AttackStrategy
    {
        [Header("Ranged Settings")]
        [Tooltip("Data của viên đạn bắn ra")]
        public Projectiles.Data.ProjectileData projectileData;
        
        [Tooltip("Vị trí đạn sinh ra")]
        public Transform firePoint;

        protected override void Awake()
        {
            base.Awake();
            if (firePoint == null)
            {
                firePoint = transform; // Fallback
            }
        }

        private void Start()
        {
            if (_enemy.EnemyAnimator != null)
            {
                _enemy.EnemyAnimator.OnAttackEvent += Shoot;
            }
        }

        private void OnDestroy()
        {
            if (_enemy != null && _enemy.EnemyAnimator != null)
            {
                _enemy.EnemyAnimator.OnAttackEvent -= Shoot;
            }
        }

        public override void Attack()
        {
            // Chỉ trigger animation. Việc bắn đạn (Shoot) sẽ được gọi bởi Animation Event.
            if (_enemy.EnemyAnimator != null)
            {
                _enemy.EnemyAnimator.TriggerAttack();
            }
        }

        private void Shoot()
        {
            if (projectileData == null || _enemy.PlayerTransform == null) return;

            Vector2 direction = (_enemy.PlayerTransform.position - firePoint.position).normalized;
            DamageData damageData = new DamageData(_enemy.GetTotalDamage(), false);

            Projectiles.Core.ProjectileSystem.Instance.Spawn(
                projectileData,
                firePoint.position,
                direction,
                _enemy.gameObject,
                damageData
            );
        }
    }
}
