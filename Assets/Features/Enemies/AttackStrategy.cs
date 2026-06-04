using UnityEngine;

namespace ProjectZombie.Features.Enemies
{
    public abstract class AttackStrategy : MonoBehaviour
    {
        protected Enemy _enemy;

        protected virtual void Awake()
        {
            _enemy = GetComponent<Enemy>();
        }

        public abstract void Attack();
    }
}
