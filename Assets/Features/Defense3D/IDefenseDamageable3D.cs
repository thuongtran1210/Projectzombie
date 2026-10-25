using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    public struct DefenseDamageContext
    {
        public float Amount;
        public object Source;
        public Vector3 HitPoint;
        public bool IsCritical;

        public DefenseDamageContext(float amount, object source = null, Vector3 hitPoint = default, bool isCritical = false)
        {
            Amount = amount;
            Source = source;
            HitPoint = hitPoint;
            IsCritical = isCritical;
        }
    }

    /// <summary>
    /// U3D-007: Unified Damage & Health interface.
    /// Guarantees: damage > 0, HP clamped to [0, MaxHP], death triggered exactly-once.
    /// </summary>
    public interface IDefenseDamageable3D
    {
        float CurrentHP { get; }
        float MaxHP { get; }
        bool IsDead { get; }

        event Action<float, float> HealthChanged;
        event Action<DefenseDamageContext> Damaged;
        event Action Defeated;

        bool TakeDamage(DefenseDamageContext context);
    }
}
