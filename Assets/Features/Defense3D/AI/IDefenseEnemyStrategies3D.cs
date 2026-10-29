using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Strategy contract for enemy locomotion (Ground, Air, Water, etc.).
    /// </summary>
    public interface IDefenseLocomotionStrategy3D
    {
        bool IsMoving { get; }
        bool HasFailed { get; }
        void Stop();
        bool TryDisplace(Vector3 delta, bool apply);
        void Configure(int lane, DefenseBasePrototype3D target);
    }

    /// <summary>
    /// Segregated contract for Boss-specific arena lifecycle and transition to base assault.
    /// </summary>
    public interface IDefenseBossCombatAdapter3D
    {
        void PrepareArenaBoss(float hp);
        void BeginBossAssault(DefenseBasePrototype3D target);
        void StopBossCombat();
    }

    /// <summary>
    /// Strategy contract for enemy attack behavior (Melee, Ranged, Cone, Telegraph).
    /// </summary>
    public interface IDefenseAttackStrategy3D
    {
        bool IsAttacking { get; }
        bool IsWindingUp { get; }
        bool IsTargetInRange { get; }
        void Begin(IDefenseDamageable3D target, Collider targetCollider = null);
        void StopAttack();
        void CancelPendingAttack();
    }

    /// <summary>
    /// Strategy contract for enemy target acquisition and aggro locking.
    /// </summary>
    public interface IDefenseTargetingStrategy3D
    {
        bool OwnsCombat { get; }
        void Stop();
    }
}
