using System;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// U3D-007 Pure C# health and damage tracker.
    /// Invariants:
    /// 1. Damage <= 0 is rejected (returns false).
    /// 2. CurrentHP clamped to [0, MaxHp].
    /// 3. Defeated event fires EXACTLY ONCE, even when multiple damage sources land on the same frame.
    /// 4. Cannot take damage while dead.
    /// </summary>
    public sealed class DefenseHealthCore
    {
        public float MaxHp { get; private set; }
        public float CurrentHp { get; private set; }
        public bool IsDead => CurrentHp <= 0f;

        public event Action<float, float> HealthChanged;
        public event Action<DefenseDamageContext> Damaged;
        public event Action Defeated;

        private bool defeatedTriggered;

        public DefenseHealthCore(float maxHp)
        {
            if (maxHp <= 0f || float.IsNaN(maxHp) || float.IsInfinity(maxHp))
                throw new ArgumentOutOfRangeException(nameof(maxHp), "MaxHP must be a positive finite number.");

            MaxHp = maxHp;
            CurrentHp = maxHp;
            defeatedTriggered = false;
        }

        public bool ApplyDamage(DefenseDamageContext context)
        {
            if (IsDead || context.Amount <= 0f || float.IsNaN(context.Amount) || float.IsInfinity(context.Amount))
                return false;

            CurrentHp = Math.Max(0f, CurrentHp - context.Amount);
            Damaged?.Invoke(context);
            HealthChanged?.Invoke(CurrentHp, MaxHp);

            if (CurrentHp <= 0f && !defeatedTriggered)
            {
                defeatedTriggered = true;
                Defeated?.Invoke();
            }

            return true;
        }

        public void Reset(float? newMaxHp = null)
        {
            if (newMaxHp.HasValue)
            {
                if (newMaxHp.Value <= 0f || float.IsNaN(newMaxHp.Value) || float.IsInfinity(newMaxHp.Value))
                    throw new ArgumentOutOfRangeException(nameof(newMaxHp));
                MaxHp = newMaxHp.Value;
            }
            CurrentHp = MaxHp;
            defeatedTriggered = false;
            HealthChanged?.Invoke(CurrentHp, MaxHp);
        }
    }
}
