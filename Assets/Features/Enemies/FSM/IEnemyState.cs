using UnityEngine;

namespace ProjectZombie.Features.Enemies
{
    public interface IEnemyState
    {
        void Enter();
        void Update();
        void FixedUpdate();
        void Exit();
    }
}
