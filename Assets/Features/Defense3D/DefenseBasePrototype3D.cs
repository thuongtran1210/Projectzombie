using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Minimal base health for the first Defense3D scene smoke-test.
    /// Full IDamageable and run end arbitration belong to the next vertical-slice PR.
    /// </summary>
    public sealed class DefenseBasePrototype3D : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maxHP = 1000f;
        public float CurrentHP { get; private set; }
        public bool IsDestroyed => CurrentHP <= 0f;

        private void Awake() { CurrentHP = maxHP; }

        public void ReceiveDamage(float amount)
        {
            if (IsDestroyed || amount <= 0f) return;
            CurrentHP = Mathf.Max(0f, CurrentHP - amount);
            if (IsDestroyed)
                Debug.Log("[TREBRAVE Defense3D] BaseHP=0. Prototype LOSS; run result UI is not implemented.");
        }
    }
}
