using UnityEngine;
using ProjectZombie.Features.Combat;
using ProjectZombie.Features.Combat.Events;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades.Runtimes.Augments
{
    /// <summary>
    /// Runtime cho Lõi [Bạo Kích Bộc Phá] (AUG_GOLD_CRIT_EXPLODE):
    /// Khi đòn đánh nổ Chí Mạng, tạo ra vụ nổ xung kích gây sát thương diện rộng.
    /// </summary>
    public class CritExplodeAugmentRuntime : AugmentRuntimeBase
    {
        [Header("Settings")]
        [SerializeField] private float _explosionDamage = 50f;
        [SerializeField] private float _explosionRadius = 3.5f;
        [Header("VFX Settings")]
        [SerializeField] private GameObject _critVfxPrefab;

        private static readonly Collider2D[] _hitBuffer = new Collider2D[24];

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
            if (!e.IsCrit || e.Target == null) return;

            Vector2 center = e.HitPosition;

            if (_critVfxPrefab != null && ProjectZombie.Features.Shared.VFX.GlobalVFXPoolManager.Instance != null)
            {
                ProjectZombie.Features.Shared.VFX.GlobalVFXPoolManager.Instance.PlayEffect(_critVfxPrefab, center, Quaternion.identity, 0.5f);
            }

            int hitCount = Physics2D.OverlapCircleNonAlloc(center, _explosionRadius, _hitBuffer);

            for (int i = 0; i < hitCount; i++)
            {
                var col = _hitBuffer[i];
                if (col == null || col.gameObject == e.Target) continue;

                if (col.CompareTag("Enemy") && col.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(new DamageData(_explosionDamage, isCritical: false, element: ElementType.Hoa));
                }
            }
        }
    }
}
