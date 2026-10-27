using System;

namespace ProjectZombie.Features.Defense3D
{
    public interface IDefenseStatModifiers
    {
        float GetModifier(DefenseUpgradeType type);
        float Evaluate(DefenseUpgradeType type, float baseline);
        float EvaluateHeroAttackInterval(float baseline);
        event Action Changed;
    }

    /// <summary>One session modifier ledger; authored actor baselines are never overwritten by upgrades.</summary>
    public sealed class DefenseRunStats : IDefenseStatModifiers
    {
        private readonly float[] _modifiers = new float[Enum.GetValues(typeof(DefenseUpgradeType)).Length];
        public event Action Changed;

        public bool TryApplyCard(DefenseUpgradeCard card)
        {
            int index = (int)card.UpgradeType;
            if (index < 0 || index >= _modifiers.Length || !DefenseDamageRules.IsFinite(card.Value) || card.Value < 0f) return false;
            float next = _modifiers[index] + card.Value;
            if (!DefenseDamageRules.IsFinite(next)) return false;
            if (next == _modifiers[index]) return true;
            _modifiers[index] = next;
            Changed?.Invoke();
            return true;
        }

        public float GetModifier(DefenseUpgradeType type)
        {
            int index = (int)type;
            return index >= 0 && index < _modifiers.Length ? _modifiers[index] : 0f;
        }

        public float Evaluate(DefenseUpgradeType type, float baseline)
        {
            if (!DefenseDamageRules.IsFinite(baseline) || baseline < 0f ||
                (type == DefenseUpgradeType.BaseMaxHp && baseline == 0f)) throw new ArgumentOutOfRangeException(nameof(baseline));
            double value = type == DefenseUpgradeType.BaseMaxHp ? (double)baseline + GetModifier(type)
                : (double)baseline * (1d + GetModifier(type));
            if (value > float.MaxValue) throw new OverflowException("Evaluated Defense stat exceeds finite float range.");
            return (float)value;
        }

        public float EvaluateHeroAttackInterval(float baseline)
        {
            if (!DefenseDamageRules.IsFinite(baseline) || baseline <= 0f) throw new ArgumentOutOfRangeException(nameof(baseline));
            return (float)(baseline / (1d + GetModifier(DefenseUpgradeType.HeroAttackSpeed)));
        }

        public void Reset()
        {
            bool changed = false;
            for (int i = 0; i < _modifiers.Length; i++) { changed |= _modifiers[i] != 0f; _modifiers[i] = 0f; }
            if (changed) Changed?.Invoke();
        }
    }
}
