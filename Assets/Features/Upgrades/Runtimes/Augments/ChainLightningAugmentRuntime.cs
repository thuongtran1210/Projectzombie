using UnityEngine;
using ProjectZombie.Features.Combat;
using ProjectZombie.Features.Combat.Events;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades.Runtimes.Augments
{
    /// <summary>
    /// Runtime cho Lõi [Lôi Đình Liên Hoàn] (AUG_GOLD_CHAIN_LIGHTNING):
    /// Khi gây sát thương có 35% tỷ lệ giật sét lan sang tối đa 3 quái vật lân cận.
    /// </summary>
    public class ChainLightningAugmentRuntime : AugmentRuntimeBase
    {
        [Header("Settings")]
        [SerializeField] private float _triggerChance = 0.35f;
        [SerializeField] private float _lightningDamage = 40f;
        [SerializeField] private float _chainRadius = 5f;
        [SerializeField] private int _maxBounces = 3;

        private static readonly Collider2D[] _bounceBuffer = new Collider2D[16];

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
            if (e.Target == null || e.Element == ElementType.Kim) return; // Tránh loop nếu chính tia sét gây damage
            if (Random.value > _triggerChance) return;

            Vector2 origin = e.HitPosition;
            int bounces = 0;
            int hitCount = Physics2D.OverlapCircleNonAlloc(origin, _chainRadius, _bounceBuffer);

            for (int i = 0; i < hitCount && bounces < _maxBounces; i++)
            {
                var col = _bounceBuffer[i];
                if (col == null || col.gameObject == e.Target) continue;

                if (col.CompareTag("Enemy") && col.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(new DamageData(_lightningDamage, isCritical: false, element: ElementType.Kim));
                    bounces++;
                }
            }
        }
    }
}
