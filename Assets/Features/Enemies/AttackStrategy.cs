using UnityEngine;

namespace ProjectZombie.Features.Enemies
{
    public abstract class AttackStrategy : MonoBehaviour
    {
        protected Enemy _enemy;

        public virtual bool IsAttacking => false;

        protected virtual void Awake()
        {
            _enemy = GetComponent<Enemy>();
        }

        public abstract void Attack();

        /// <summary>
        /// Hủy đòn tấn công đang vung dở khi quái bị khống chế (Choáng, Đóng Băng, Ngủ).
        /// </summary>
        public virtual void InterruptAttack()
        {
        }
    }
}
