using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades.Runtimes.Augments
{
    /// <summary>
    /// Runtime cho Lõi Kim Cương [Băng Hà Tuyệt Kỹ] (AUG_PRIS_THUYBA_FREEZE_BURST):
    /// Định kỳ mỗi 7s tạo một đợt sóng Băng Tuyết diện rộng 8m gây sát thương Thủy và làm chậm 80% quái vật.
    /// </summary>
    public class ThuyBaFreezeBurstAugmentRuntime : AugmentRuntimeBase
    {
        [Header("Settings")]
        [SerializeField] private float _burstInterval = 7.0f;
        [SerializeField] private float _freezeDamage = 110f;
        [SerializeField] private float _freezeRadius = 8.0f;
        [Header("VFX Settings")]
        [SerializeField] private GameObject _freezeBurstVfxPrefab;

        private float _nextBurstTime = 0f;
        private static readonly Collider2D[] _hitBuffer = new Collider2D[32];

        protected override void OnAugmentInitialized()
        {
            _nextBurstTime = Time.time + _burstInterval;
        }

        private void Update()
        {
            if (Time.time >= _nextBurstTime)
            {
                _nextBurstTime = Time.time + _burstInterval;
                TriggerFreezeBurst();
            }
        }

        private void TriggerFreezeBurst()
        {
            Vector2 origin = transform.position;

            if (_freezeBurstVfxPrefab != null && ProjectZombie.Features.Shared.VFX.GlobalVFXPoolManager.Instance != null)
            {
                ProjectZombie.Features.Shared.VFX.GlobalVFXPoolManager.Instance.PlayEffect(_freezeBurstVfxPrefab, origin, Quaternion.identity, 1.0f);
            }

            int hitCount = Physics2D.OverlapCircleNonAlloc(origin, _freezeRadius, _hitBuffer);

            for (int i = 0; i < hitCount; i++)
            {
                var col = _hitBuffer[i];
                if (col == null || col.gameObject == (Context != null ? Context.GameObject : gameObject)) continue;

                if (col.CompareTag("Enemy") && col.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(new DamageData(_freezeDamage, isCritical: false, element: ElementType.Thuy));
                }
            }

            Debug.Log("<color=#00E5FF>[Băng Hà Tuyệt Kỹ] Hàn Băng Bộc Phát càn quét trận địa!</color>");
        }
    }
}
