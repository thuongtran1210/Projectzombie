using UnityEngine;
using ProjectZombie.Features.Combat;
using ProjectZombie.Features.Combat.Events;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades.Runtimes.Augments
{
    /// <summary>
    /// Runtime cho Lõi [Tuyệt Diệt] (AUG_GOLD_EXECUTE):
    /// Khi quái vật trúng đòn còn dưới 20% máu tối đa, lập tức hành quyết trực tiếp.
    /// </summary>
    public class ExecuteAugmentRuntime : AugmentRuntimeBase
    {
        [Header("Settings")]
        [SerializeField] private float _executeThreshold = 0.20f; // Dưới 20% máu
        [SerializeField] private float _executeDamage = 9999f;
        [Header("VFX Settings")]
        [SerializeField] private GameObject _executeVfxPrefab;

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
            if (e.Target == null) return;

            // Kiểm tra máu mục tiêu nếu có HealthSystem hoặc tương đương
            if (e.Target.TryGetComponent<HealthSystem>(out var enemyHealth))
            {
                if (enemyHealth.CurrentHealth > 0 && (enemyHealth.CurrentHealth / enemyHealth.MaxHealth) <= _executeThreshold)
                {
                    if (e.Target.TryGetComponent<IDamageable>(out var damageable))
                    {
                        damageable.TakeDamage(new DamageData(_executeDamage, isCritical: true, element: ElementType.Kim));

                        if (_executeVfxPrefab != null && ProjectZombie.Features.Shared.VFX.GlobalVFXPoolManager.Instance != null)
                        {
                            ProjectZombie.Features.Shared.VFX.GlobalVFXPoolManager.Instance.PlayEffect(_executeVfxPrefab, e.Target.transform.position, Quaternion.identity, 0.4f);
                        }
                    }
                }
            }
        }
    }
}
