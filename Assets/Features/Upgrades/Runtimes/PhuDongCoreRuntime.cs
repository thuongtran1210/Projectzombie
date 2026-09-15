using UnityEngine;
using ProjectZombie.Features.Combat.Events;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades.Runtimes
{
    /// <summary>
    /// Runtime thực thi logic chiến đấu của Đại Lõi [PHÙ ĐỔNG THIÊN UY] (Hệ Hỏa - Thể Tu Khổng Lồ, Càn Quét).
    /// Tuân thủ 0 GC Allocations trong toàn bộ gameplay loop.
    /// </summary>
    public class PhuDongCoreRuntime : MythicCoreRuntime
    {
        public override MythicArchetype Archetype => MythicArchetype.PhuDongThienUy;

        [Header("Settings - Phóng To Kích Thước")]
        [SerializeField] private int _killsPerGrowth = 50;
        [SerializeField] private float _growthScaleMultiplier = 1.10f; // +10% kích thước mỗi mốc
        [SerializeField] private float _maxScaleLimit = 2.0f;
        [SerializeField] private float _bonusHealthPerGrowth = 50f;

        [Header("Settings - Ngựa Sắt Dash Lửa")]
        [SerializeField] private float _dashFireDamage = 45f;
        [SerializeField] private float _dashKnockbackForce = 8f;
        [SerializeField] private float _dashHitRadius = 2.5f;

        private int _currentKillCount = 0;
        private float _originalScale = 1f;
        private float _currentScaleMultiplier = 1f;

        // Static buffer tái sử dụng để triệt tiêu GC Allocation
        private static readonly Collider2D[] _hitBuffer = new Collider2D[32];

        protected override void OnInitialize()
        {
            if (Context?.Transform != null)
            {
                _originalScale = Context.Transform.localScale.x;
                _currentScaleMultiplier = 1f;
                _currentKillCount = 0;
            }
        }

        protected override void OnTeardown()
        {
            // Hoàn trả lại kích thước ban đầu cho Player khi Lõi bị thay thế hoặc kết thúc ván
            if (Context?.Transform != null)
            {
                Context.Transform.localScale = Vector3.one * _originalScale;
            }
        }

        protected override void SubscribeCombatEvents()
        {
            if (Context?.CombatEvents != null)
            {
                Context.CombatEvents.OnEnemyKilled += HandleEnemyKilled;
                Context.CombatEvents.OnDashPerformed += HandleDashNguaSat;
            }
        }

        protected override void UnsubscribeCombatEvents()
        {
            if (Context?.CombatEvents != null)
            {
                Context.CombatEvents.OnEnemyKilled -= HandleEnemyKilled;
                Context.CombatEvents.OnDashPerformed -= HandleDashNguaSat;
            }
        }

        private void HandleEnemyKilled(in KillEvent e)
        {
            _currentKillCount++;
            if (_currentKillCount % _killsPerGrowth == 0 && _currentScaleMultiplier < _maxScaleLimit)
            {
                _currentScaleMultiplier = Mathf.Min(_currentScaleMultiplier * _growthScaleMultiplier, _maxScaleLimit);
                
                if (Context?.Transform != null)
                {
                    Context.Transform.localScale = Vector3.one * (_originalScale * _currentScaleMultiplier);
                }

                if (Context?.Stats != null)
                {
                    Context.Stats.AddMaxHealth(_bonusHealthPerGrowth);
                }

                Debug.Log($"<color=#FF7700>[Phù Đổng]</color> Thần Tướng Trưởng Thành! Diệt: {_currentKillCount}, Scale: {_currentScaleMultiplier:F2}x, +{_bonusHealthPerGrowth} HP");
            }
        }

        private void HandleDashNguaSat(in DashEvent e)
        {
            // Quét quái vật trên đường lướt từ startPos đến endPos
            Vector2 midPoint = (e.StartPosition + e.EndPosition) * 0.5f;
            float scanRadius = Mathf.Max(_dashHitRadius, e.DashDistance * 0.5f);

            int hitCount = Physics2D.OverlapCircleNonAlloc(midPoint, scanRadius, _hitBuffer);

            for (int i = 0; i < hitCount; i++)
            {
                var col = _hitBuffer[i];
                if (col == null || col.gameObject == Context?.GameObject) continue;

                if (col.CompareTag("Enemy") && col.TryGetComponent<IDamageable>(out var damageable))
                {
                    // Gây sát thương thiêu đốt Lửa
                    damageable.TakeDamage(new DamageData(_dashFireDamage, isCritical: false, element: ElementType.Hoa));

                    // Đạp lùi quái vật theo hướng lướt
                    if (col.TryGetComponent<Rigidbody2D>(out var rb))
                    {
                        rb.AddForce(e.DashDirection * _dashKnockbackForce, ForceMode2D.Impulse);
                    }
                }
            }
        }

        protected override void OnRevived(in ReviveEvent e)
        {
            // Thiên Uy Tái Sinh: Sấm sét quét sạch quái vật xung quanh trong bán kính 10m
            if (Context?.Transform == null) return;

            Vector2 center = Context.Transform.position;
            int hitCount = Physics2D.OverlapCircleNonAlloc(center, 10f, _hitBuffer);

            for (int i = 0; i < hitCount; i++)
            {
                var col = _hitBuffer[i];
                if (col != null && col.CompareTag("Enemy") && col.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(new DamageData(150f, isCritical: true, element: ElementType.Hoa));
                }
            }

            Debug.Log("<color=#FFD700>[Phù Đổng] Thiên Uy Tái Sinh: Sấm sét quét sạch vạn yêu ma quanh người!</color>");
        }
    }
}
