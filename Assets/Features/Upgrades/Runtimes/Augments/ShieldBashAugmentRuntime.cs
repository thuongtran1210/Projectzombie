using UnityEngine;
using ProjectZombie.Features.Combat;
using ProjectZombie.Features.Combat.Events;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades.Runtimes.Augments
{
    /// <summary>
    /// Runtime cho Lõi [Kim Giáp Phản Kích] (AUG_GOLD_SHIELD_BASH):
    /// Định kỳ mỗi 6s phóng ra luồng sóng chấn động đẩy lùi quái vật và gây choáng.
    /// </summary>
    public class ShieldBashAugmentRuntime : AugmentRuntimeBase
    {
        [Header("Settings")]
        [SerializeField] private float _pulseInterval = 6f;
        [SerializeField] private float _pulseDamage = 60f;
        [SerializeField] private float _pulseRadius = 4.5f;
        [SerializeField] private float _knockbackForce = 10f;

        private float _nextPulseTime = 0f;
        private static readonly Collider2D[] _hitBuffer = new Collider2D[24];

        protected override void OnAugmentInitialized()
        {
            _nextPulseTime = Time.time + _pulseInterval;
        }

        private void Update()
        {
            if (Time.time >= _nextPulseTime)
            {
                _nextPulseTime = Time.time + _pulseInterval;
                TriggerShieldPulse();
            }
        }

        private void TriggerShieldPulse()
        {
            Vector2 origin = transform.position;
            int hitCount = Physics2D.OverlapCircleNonAlloc(origin, _pulseRadius, _hitBuffer);

            for (int i = 0; i < hitCount; i++)
            {
                var col = _hitBuffer[i];
                if (col == null || col.gameObject == (Context != null ? Context.GameObject : gameObject)) continue;

                if (col.CompareTag("Enemy") && col.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(new DamageData(_pulseDamage, isCritical: false, element: ElementType.Kim));

                    if (col.TryGetComponent<Rigidbody2D>(out var rb))
                    {
                        Vector2 pushDir = ((Vector2)col.transform.position - origin).normalized;
                        rb.AddForce(pushDir * _knockbackForce, ForceMode2D.Impulse);
                    }
                }
            }
        }
    }
}
