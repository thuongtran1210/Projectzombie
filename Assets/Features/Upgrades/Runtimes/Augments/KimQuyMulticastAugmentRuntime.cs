using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades.Runtimes.Augments
{
    /// <summary>
    /// Runtime cho Lõi Kim Cương [Nỏ Thần Vạn Tiễn Đa Xạ] (AUG_PRIS_KIMQUY_MULTICAST):
    /// Mỗi 2.5s tự động phát xạ loạt tên năng lượng Kim Quy quét sạch quái vật trên màn hình.
    /// </summary>
    public class KimQuyMulticastAugmentRuntime : AugmentRuntimeBase
    {
        [Header("Settings")]
        [SerializeField] private float _volleyInterval = 2.5f;
        [SerializeField] private float _arrowDamage = 75f;
        [SerializeField] private float _scanRadius = 12f;
        [SerializeField] private int _targetCount = 6;

        private float _nextVolleyTime = 0f;
        private static readonly Collider2D[] _scanBuffer = new Collider2D[32];

        protected override void OnAugmentInitialized()
        {
            _nextVolleyTime = Time.time + _volleyInterval;
        }

        private void Update()
        {
            if (Time.time >= _nextVolleyTime)
            {
                _nextVolleyTime = Time.time + _volleyInterval;
                FireVolley();
            }
        }

        private void FireVolley()
        {
            Vector2 origin = transform.position;
            int hitCount = Physics2D.OverlapCircleNonAlloc(origin, _scanRadius, _scanBuffer);
            int fired = 0;

            for (int i = 0; i < hitCount && fired < _targetCount; i++)
            {
                var col = _scanBuffer[i];
                if (col == null || col.gameObject == (Context != null ? Context.GameObject : gameObject)) continue;

                if (col.CompareTag("Enemy") && col.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(new DamageData(_arrowDamage, isCritical: true, element: ElementType.Kim));
                    fired++;
                }
            }
        }
    }
}
