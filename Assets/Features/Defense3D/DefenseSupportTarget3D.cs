using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Optional authored friendly health endpoint. No support spawner, AI, weapon or reward is implied.</summary>
    public sealed class DefenseSupportTarget3D : DefenseGameplayBehaviour, IDefenseDamageable3D
    {
        [SerializeField, Min(1f)] private float _maxHp = 100f;
        private DefenseHealthCore _health;
        private int _generation;
        private bool _live;
        public float CurrentHP => _health != null ? _health.CurrentHp : 0f;
        public float MaxHP => _health != null ? _health.MaxHp : _maxHp;
        public bool IsDead => !_live || _health == null || _health.IsDead;
        public event Action<float, float> HealthChanged;
        public event Action<DefenseDamageContext> Damaged;
        public event Action Defeated;
        protected override void OnGameplayBound()
        {
            _health = new DefenseHealthCore(_maxHp);
            _health.HealthChanged += (current, max) => HealthChanged?.Invoke(current, max);
            _health.Damaged += context => Damaged?.Invoke(context);
            _health.Defeated += () => Defeated?.Invoke();
            ResetHealth();
        }
        public bool TakeDamage(DefenseDamageContext context) => isActiveAndEnabled && _live && Gameplay != null && _generation == Gameplay.RunGeneration &&
            DefenseDamageRules.CanApply(context, Gameplay, IsDead) && _health.ApplyDamage(context);
        private void ResetHealth() { _generation = Gameplay.RunGeneration; _health.Reset(_maxHp); _live = true; }
        protected override void OnGameplayReset() => ResetHealth();
        protected override void OnGameplayCleanup() => _live = false;
    }
}
