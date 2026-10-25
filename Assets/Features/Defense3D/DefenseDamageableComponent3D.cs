using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// U3D-007 MonoBehaviour wrapper implementing IDefenseDamageable3D.
    /// Can be attached to any 3D entity (Enemy, Tower, Base, Hero) without tight coupling.
    /// </summary>
    public sealed class DefenseDamageableComponent3D : MonoBehaviour, IDefenseDamageable3D
    {
        [SerializeField, Min(1f)] private float maxHP = 100f;

        private DefenseHealthCore healthCore;

        public float CurrentHP => HealthCore.CurrentHp;
        public float MaxHP => HealthCore.MaxHp;
        public bool IsDead => HealthCore.IsDead;

        public event Action<float, float> HealthChanged;
        public event Action<DefenseDamageContext> Damaged;
        public event Action Defeated;

        private DefenseHealthCore HealthCore
        {
            get
            {
                if (healthCore == null)
                {
                    healthCore = new DefenseHealthCore(maxHP);
                    healthCore.HealthChanged += (cur, max) => HealthChanged?.Invoke(cur, max);
                    healthCore.Damaged += ctx => Damaged?.Invoke(ctx);
                    healthCore.Defeated += () => Defeated?.Invoke();
                }
                return healthCore;
            }
        }

        private void Awake()
        {
            _ = HealthCore;
        }

        public void SetMaxHealth(float newMaxHp)
        {
            maxHP = Mathf.Max(1f, newMaxHp);
            HealthCore.Reset(maxHP);
        }

        public bool TakeDamage(DefenseDamageContext context)
        {
            return HealthCore.ApplyDamage(context);
        }

        public bool TakeDamage(float amount, object source = null)
        {
            return TakeDamage(new DefenseDamageContext(amount, source));
        }

        public void ResetHealth()
        {
            HealthCore.Reset();
        }
    }
}
