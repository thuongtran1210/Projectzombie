using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Authored Base adapter sharing health rules with Enemy and Hero.</summary>
    public sealed class DefenseBasePrototype3D : DefenseGameplayBehaviour, IDefenseDamageable3D
    {
        [SerializeField, Min(1f)] private float maxHP = 1000f;
        private DefenseHealthCore _health;
        private IDefenseStatModifiers _stats;
        public float MaxHP => Health.MaxHp;
        public float CurrentHP => Health.CurrentHp;
        public bool IsDestroyed => Health.IsDead;
        public bool IsDead => IsDestroyed;
        public bool DamageLockedAfterWin { get; private set; }
        public event Action<float, float> HealthChanged;
        public event Action<DefenseDamageContext> Damaged;
        public event Action BaseDestroyed;
        public event Action Defeated { add => BaseDestroyed += value; remove => BaseDestroyed -= value; }

        private DefenseHealthCore Health
        {
            get
            {
                if (_health != null) return _health;
                _health = new DefenseHealthCore(maxHP);
                _health.HealthChanged += (current, maximum) => HealthChanged?.Invoke(current, maximum);
                _health.Damaged += context => Damaged?.Invoke(context);
                _health.Defeated += () => BaseDestroyed?.Invoke();
                return _health;
            }
        }

        private void Awake() { _ = Health; }

        protected override void OnGameplayBound()
        {
            if (_stats != null) _stats.Changed -= ApplyCapacity;
            _stats = Gameplay.Stats;
            _stats.Changed += ApplyCapacity;
            ApplyCapacity();
        }

        private void ApplyCapacity()
        {
            Health.SetCapacity(_stats.Evaluate(DefenseUpgradeType.BaseMaxHp, maxHP), true);
        }

        public bool TakeDamage(DefenseDamageContext context)
        {
            return isActiveAndEnabled && !DamageLockedAfterWin &&
                DefenseDamageRules.CanApply(context, Gameplay, IsDead) && Health.ApplyDamage(context);
        }

        public void ReceiveDamage(float amount) => TakeDamage(new DefenseDamageContext(amount));
        public void LockDamageAfterWin() => DamageLockedAfterWin = true;

        public void ResetBase()
        {
            DamageLockedAfterWin = false;
            Health.Reset(_stats != null ? _stats.Evaluate(DefenseUpgradeType.BaseMaxHp, maxHP) : maxHP);
        }

        protected override void OnDestroy()
        {
            if (_stats != null) _stats.Changed -= ApplyCapacity;
            base.OnDestroy();
        }
    }
}


