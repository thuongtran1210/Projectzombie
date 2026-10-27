using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Stateless Unity adapter for a session-owned target index.</summary>
    public static class DefenseEnemyTargets3D
    {
        private sealed class Target : IDefenseTargetPosition
        {
            private readonly IDefenseEnemyTarget3D _target;
            public Target(IDefenseEnemyTarget3D target, bool airborne) { _target = target; IsAirborne = airborne; }
            public object Identity => _target;
            public bool IsAirborne { get; }
            public bool IsAvailable => !(_target is UnityEngine.Object obj && obj == null) &&
                _target.IsTargetable && _target.TargetTransform != null;
            public DefensePoint3 Position
            {
                get
                {
                    var point = _target.TargetTransform.position;
                    return new DefensePoint3(point.x, point.y, point.z);
                }
            }
        }

        public static void Register(DefenseTargetIndex index, IDefenseEnemyTarget3D target, bool airborne = false)
        {
            if (index == null) throw new ArgumentNullException(nameof(index));
            if (target == null) throw new ArgumentNullException(nameof(target));
            index.Register(new Target(target, airborne));
        }

        public static void Unregister(DefenseTargetIndex index, IDefenseEnemyTarget3D target)
        {
            index?.Unregister(target);
        }

        public static IDefenseEnemyTarget3D FindNearest(IDefenseTargetQuery query, Vector3 origin, float range,
            DefenseTargetMask mask = DefenseTargetMask.All)
        {
            return query?.FindNearest(new DefensePoint3(origin.x, origin.y, origin.z), range, mask)?.Identity as IDefenseEnemyTarget3D;
        }
    }
}
