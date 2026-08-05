using UnityEngine;

namespace ProjectZombie.Features.Enemies
{
    public abstract class CombatMovementStrategy : MonoBehaviour
    {
        protected Enemy _enemy;

        protected virtual void Awake()
        {
            _enemy = GetComponent<Enemy>();
        }

        /// <summary>
        /// Thực thi logic di chuyển trong trạng thái giao tranh/đuổi theo.
        /// </summary>
        public abstract void Move();

        /// <summary>
        /// Kiểm tra khoảng cách hiện tại có nằm trong cự ly tấn công hiệu quả hay không.
        /// </summary>
        public abstract bool IsInAttackRange(float distanceToPlayer);

        /// <summary>
        /// Kiểm tra quái có cần điều chỉnh lại vị trí (Reposition/Lùi lại) hay không (mặc định false cho Melee).
        /// </summary>
        public virtual bool ShouldReposition(float distanceToPlayer) => false;
    }
}
