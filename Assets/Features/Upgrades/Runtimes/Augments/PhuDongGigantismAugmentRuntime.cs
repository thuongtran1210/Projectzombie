using UnityEngine;
using ProjectZombie.Features.Combat;
using ProjectZombie.Features.Combat.Events;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades.Runtimes.Augments
{
    /// <summary>
    /// Runtime cho Lõi Kim Cương [Phù Đổng Cự Thần] (AUG_PRIS_PHUDONG_GIGANTISM):
    /// Hóa thân thành Cự Thần (+50% Scale, +80% tầm đánh quét, đòn đánh tạo địa chấn đẩy lùi).
    /// </summary>
    public class PhuDongGigantismAugmentRuntime : AugmentRuntimeBase
    {
        [Header("Settings")]
        [SerializeField] private float _scaleBonus = 1.5f;
        [SerializeField] private float _quakeDamage = 80f;
        [SerializeField] private float _quakeRadius = 5.0f;
        [SerializeField] private float _knockbackForce = 12f;

        private float _originalScale = 1f;
        private static readonly Collider2D[] _hitBuffer = new Collider2D[32];

        protected override void OnAugmentInitialized()
        {
            if (Context?.Transform != null)
            {
                _originalScale = Context.Transform.localScale.x;
                Context.Transform.localScale = Vector3.one * (_originalScale * _scaleBonus);
            }
        }

        protected override void OnAugmentTeardown()
        {
            if (Context?.Transform != null)
            {
                Context.Transform.localScale = Vector3.one * _originalScale;
            }
        }

        protected override void SubscribeCombatEvents(PlayerCombatEvents events)
        {
            events.OnDamageDealt += HandleDamageDealt;
        }

        protected override void UnsubscribeCombatEvents(PlayerCombatEvents events)
        {
            events.OnDamageDealt -= HandleDamageDealt;
        }

        private void HandleDamageDealt(in DamageDealtEvent e)
        {
            if (e.Target == null || e.Element == ElementType.Tho) return;

            // Kích hoạt sóng địa chấn hất tung quái vật xung quanh
            Vector2 center = e.HitPosition;
            int hitCount = Physics2D.OverlapCircleNonAlloc(center, _quakeRadius, _hitBuffer);

            for (int i = 0; i < hitCount; i++)
            {
                var col = _hitBuffer[i];
                if (col == null || col.gameObject == (Context != null ? Context.GameObject : gameObject)) continue;

                if (col.CompareTag("Enemy") && col.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(new DamageData(_quakeDamage, isCritical: false, element: ElementType.Tho));

                    if (col.TryGetComponent<Rigidbody2D>(out var rb))
                    {
                        Vector2 pushDir = ((Vector2)col.transform.position - center).normalized;
                        rb.AddForce(pushDir * _knockbackForce, ForceMode2D.Impulse);
                    }
                }
            }
        }
    }
}
