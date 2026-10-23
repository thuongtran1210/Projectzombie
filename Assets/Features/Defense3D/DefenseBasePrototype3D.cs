using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Defense vertical slice base. BaseHP == 0 is the only loss trigger.</summary>
    public sealed class DefenseBasePrototype3D : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maxHP = 1000f;
        public float MaxHP => maxHP;
        public float CurrentHP { get; private set; }
        public bool IsDestroyed => CurrentHP <= 0f;
        public bool DamageLockedAfterWin { get; private set; }
        public void LockDamageAfterWin() { DamageLockedAfterWin = true; }
        public event Action<float, float> HealthChanged;
        public event Action BaseDestroyed;

        private void Awake() { CurrentHP = maxHP; }

        public void ReceiveDamage(float amount)
        {
            if (IsDestroyed || DamageLockedAfterWin || amount <= 0f) return;
            CurrentHP = Mathf.Max(0f, CurrentHP - amount);
            HealthChanged?.Invoke(CurrentHP, maxHP);
            Debug.Log("[TREBRAVE Defense3D] Base HP: " + CurrentHP + "/" + maxHP);
            if (!IsDestroyed) return;
            Debug.Log("[TREBRAVE Defense3D] LOSS: BaseHP=0.");
            BaseDestroyed?.Invoke();
        }
    }
}
