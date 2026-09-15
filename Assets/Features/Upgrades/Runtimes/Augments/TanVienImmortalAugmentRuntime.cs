using UnityEngine;
using ProjectZombie.Features.Combat;
using ProjectZombie.Features.Combat.Events;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades.Runtimes.Augments
{
    /// <summary>
    /// Runtime cho Lõi Kim Cương [Tản Viên Bất Hoại] (AUG_PRIS_TANVIEN_IMMORTAL):
    /// Thạch Trụ Kim Cang: Khi máu dưới 25%, kích hoạt miễn nhiễm sát thương 3s và phát nổ địa chấn (Hồi chiêu 60s).
    /// </summary>
    public class TanVienImmortalAugmentRuntime : AugmentRuntimeBase
    {
        [Header("Settings")]
        [SerializeField] private float _emergencyHpThreshold = 0.25f;
        [SerializeField] private float _invulnerableDuration = 3.0f;
        [SerializeField] private float _burstDamage = 150f;
        [SerializeField] private float _burstRadius = 8.0f;
        [SerializeField] private float _cooldown = 60f;
        [Header("VFX Settings")]
        [SerializeField] private GameObject _pillarBurstVfxPrefab;

        private float _nextAvailableTime = 0f;
        private static readonly Collider2D[] _hitBuffer = new Collider2D[32];

        private void Update()
        {
            if (Context?.Health == null) return;

            if (Time.time >= _nextAvailableTime)
            {
                if (Context.Health.CurrentHealth > 0 && (Context.Health.CurrentHealth / Context.Health.MaxHealth) <= _emergencyHpThreshold)
                {
                    _nextAvailableTime = Time.time + _cooldown;
                    TriggerImmortalBurst();
                }
            }
        }

        private void TriggerImmortalBurst()
        {
            Vector2 center = transform.position;

            if (_pillarBurstVfxPrefab != null && ProjectZombie.Features.Shared.VFX.GlobalVFXPoolManager.Instance != null)
            {
                ProjectZombie.Features.Shared.VFX.GlobalVFXPoolManager.Instance.PlayEffect(_pillarBurstVfxPrefab, center, Quaternion.identity, 1.2f);
            }

            int hitCount = Physics2D.OverlapCircleNonAlloc(center, _burstRadius, _hitBuffer);

            for (int i = 0; i < hitCount; i++)
            {
                var col = _hitBuffer[i];
                if (col == null || col.gameObject == (Context != null ? Context.GameObject : gameObject)) continue;

                if (col.CompareTag("Enemy") && col.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(new DamageData(_burstDamage, isCritical: true, element: ElementType.Tho));
                }
            }

            if (Context?.Health != null)
            {
                Context.Health.TriggerInvulnerability(_invulnerableDuration);
            }

            Debug.Log($"<color=#00E5FF>[Tản Viên Bất Hoại] Kích hoạt Kim Cang Hộ Thể (Bất tử {_invulnerableDuration}s)!</color>");
        }
    }
}
