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
        private int _revision;
        private int _lifeVersion;

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
            if (IsDead || !DefenseDamageRules.IsValid(context))
                return false;

            CurrentHp = Math.Max(0f, CurrentHp - context.Amount);
            int revision = ++_revision;
            int lifeVersion = _lifeVersion;
            bool lethal = IsDead && !defeatedTriggered;
            if (lethal) defeatedTriggered = true;
            Damaged?.Invoke(context);
            if (_lifeVersion != lifeVersion) return true;
            if (_revision == revision) HealthChanged?.Invoke(CurrentHp, MaxHp);
            if (_lifeVersion != lifeVersion) return true;

            if (lethal)
            {
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
            _revision++;
            _lifeVersion++;
            HealthChanged?.Invoke(CurrentHp, MaxHp);
        }

        /// <summary>Changes capacity without reviving dead actors; positive grants may fill only the added capacity.</summary>
        public void SetCapacity(float maxHp, bool fillIncrease)
        {
            if (maxHp <= 0f || !DefenseDamageRules.IsFinite(maxHp)) throw new ArgumentOutOfRangeException(nameof(maxHp));
            if (maxHp == MaxHp) return;
            float delta = Math.Max(0f, maxHp - MaxHp);
            CurrentHp = IsDead ? 0f : Math.Min(maxHp, CurrentHp + (fillIncrease ? delta : 0f));
            MaxHp = maxHp;
            _revision++;
            HealthChanged?.Invoke(CurrentHp, MaxHp);
        }
    }
}
