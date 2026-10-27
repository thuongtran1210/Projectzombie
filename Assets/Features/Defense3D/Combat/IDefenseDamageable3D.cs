using System;

namespace ProjectZombie.Features.Defense3D
{
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
