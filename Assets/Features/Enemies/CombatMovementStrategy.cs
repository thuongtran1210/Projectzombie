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
    }
}
