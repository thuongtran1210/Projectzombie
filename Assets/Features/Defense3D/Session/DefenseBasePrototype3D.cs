using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// [CĂN CỨ BỜ TÂY - TRÁI TIM PHÒNG THỦ]
    /// Là mục tiêu bảo vệ tối thượng của người chơi trong màn Thung Lũng Sông Quê.
    /// Quái sau khi vượt cầu sẽ lao vào tấn công Căn Cứ.
    /// Khi máu Căn Cứ về 0 -> Kích hoạt Thất Thủ (GameOver).
    /// </summary>
    public sealed class DefenseBasePrototype3D : DefenseGameplayBehaviour, IDefenseDamageable3D
    {
        [Header("=== THÔNG SỐ CĂN CỨ PHÒNG THỦ BỜ TÂY ===")]
        [Tooltip("Lượng máu tối đa của Căn Cứ. Khi máu giảm về 0, trận đấu lập tức Thất Thủ.")]
        [SerializeField, Min(1f)] private float maxHP = 1000f;

        [Header("── EVENT CHANNELS (Kênh Sự Kiện Decoupling) ──")]
        [Tooltip("Kênh sự kiện phát ra lượng máu hiện tại của Căn cứ để UI lắng nghe")]
        [SerializeField] private SO_FloatEvent _onBaseHealthChangedEvent;

        [Tooltip("Kênh sự kiện phát tín hiệu khi Căn cứ bị phá hủy")]
        [SerializeField] private SO_VoidEvent _onBaseDestroyedEvent;

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
                _health.HealthChanged += (current, maximum) =>
                {
                    HealthChanged?.Invoke(current, maximum);
                    _onBaseHealthChangedEvent?.Raise(current);
                };
                _health.Damaged += context => Damaged?.Invoke(context);
                _health.Defeated += () =>
                {
                    BaseDestroyed?.Invoke();
                    _onBaseDestroyedEvent?.Raise();
                };
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
        public bool Heal(float amount) => isActiveAndEnabled && CanAdvanceGameplay && !IsDestroyed && Health.Heal(amount);
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


