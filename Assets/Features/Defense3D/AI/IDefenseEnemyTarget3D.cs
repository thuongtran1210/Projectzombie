using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    public interface IDefenseEnemyTarget3D : IDefenseDamageable3D
    {
        Transform TargetTransform { get; }
        bool IsTargetable { get; }
    }
}
