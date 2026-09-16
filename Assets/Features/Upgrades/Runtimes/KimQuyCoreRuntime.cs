using UnityEngine;
using ProjectZombie.Features.Combat.Events;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades.Runtimes
{
    /// <summary>
    /// Runtime thực thi logic chiến đấu của Đại Lõi [KIM QUY THẦN CƠ] (Hệ KIM - Xạ Kích Mũi Tên Nảy / Cận Chiến Bắn Kiếm Khí Nảy, Mai Rùa Thủ).
    /// Hỗ trợ hoàn hảo cho CẢ Tay Ngắn (Melee) và Tay Dài (Ranged).
    /// Tuân thủ 0 GC Allocations trong toàn bộ gameplay loop.
    /// </summary>
    public class KimQuyCoreRuntime : MythicCoreRuntime
    {
        public override MythicArchetype Archetype => MythicArchetype.KimQuyThanCo;

        [Header("Settings - Đạn Nảy / Kiếm Khí Nảy (Kim Ricochet)")]
        [SerializeField] private float _ricochetRadius = 6.0f;
        [SerializeField] private float _ricochetDamagePercent = 0.60f; // 60% sát thương gốc
        [SerializeField] private int _maxRicochetTargets = 2;

        [Header("Settings - Mai Rùa Hoàng Kim (Stand Still Shield)")]
        [SerializeField] private float _standStillTimeToShield = 1.0f;
        [SerializeField] private float _shieldDamageReduction = 0.50f; // Giảm 50% sát thương khi có Mai Rùa

        private float _currentStandStillTimer = 0f;
        private bool _isShieldActive = false;
        private Vector2 _lastPosition;

        // Static buffer tái sử dụng để triệt tiêu GC Allocation
        private static readonly Collider2D[] _ricochetBuffer = new Collider2D[32];

        protected override void OnInitialize()
        {
            if (Context?.Transform != null)
            {
                _lastPosition = Context.Transform.position;
                _currentStandStillTimer = 0f;
                _isShieldActive = false;
            }

            // Tự động tăng 10% Tỷ lệ bạo kích khởi đầu cho Hệ Kim
            if (Context?.Stats != null)
            {
                Context.Stats.AddCritChance(10f);
            }
        }

        protected override void OnTeardown()
        {
            _isShieldActive = false;
        }

        private void Update()
        {
            if (!IsActive || Context?.Transform == null) return;

            Vector2 currentPos = Context.Transform.position;
            float movedDist = Vector2.Distance(currentPos, _lastPosition);
            _lastPosition = currentPos;

            // Kiểm tra đứng yên
            if (movedDist < 0.01f)
            {
                _currentStandStillTimer += Time.deltaTime;
                if (_currentStandStillTimer >= _standStillTimeToShield && !_isShieldActive)
                {
                    _isShieldActive = true;
                    Debug.Log("<color=#FFD700>[Kim Quy]</color> Mai Rùa Hoàng Kim Kích Hoạt! Giảm 50% Sát Thương & Phản Xạ Đạn!");
                }
            }
            else
            {
                _currentStandStillTimer = 0f;
                _isShieldActive = false;
            }
        }

        protected override void SubscribeCombatEvents()
        {
            if (Context?.CombatEvents != null)
            {
                Context.CombatEvents.OnDamageDealt += HandleDamageDealt;
            }
        }

        protected override void UnsubscribeCombatEvents()
        {
            if (Context?.CombatEvents != null)
            {
                Context.CombatEvents.OnDamageDealt -= HandleDamageDealt;
            }
        }

        public bool IsShieldActive => _isShieldActive;
        public float ShieldDamageReduction => _shieldDamageReduction;

        private void HandleDamageDealt(in DamageDealtEvent e)
        {
            // Cơ chế Kim Nảy (Ricochet): 
            // - Với Tay Dài (Ranged): Mũi tên trúng quái phân tách nảy sang quái lân cận.
            // - Với Tay Ngắn (Melee): Vệt chém trúng quái phóng ra 2 Tia Kiếm Khí Nảy sang quái lân cận.
            if (e.Target == null) return;

            Vector2 hitPos = e.HitPosition;
            int hitCount = Physics2D.OverlapCircleNonAlloc(hitPos, _ricochetRadius, _ricochetBuffer);

            int ricocheted = 0;
            float ricochetDmg = e.Damage * _ricochetDamagePercent;

            for (int i = 0; i < hitCount; i++)
            {
                var col = _ricochetBuffer[i];
                if (col == null || col.gameObject == e.Target || col.gameObject == Context?.GameObject) continue;

                if (col.CompareTag("Enemy") && col.TryGetComponent<IDamageable>(out var damageable))
                {
                    // Gây sát thương nảy hệ Kim
                    damageable.TakeDamage(new DamageData(ricochetDmg, isCritical: e.IsCrit, element: ElementType.Kim));
                    ricocheted++;
                    if (ricocheted >= _maxRicochetTargets) break;
                }
            }
        }

        protected override void OnRevived(in ReviveEvent e)
        {
            // Khi hồi sinh, nhận Mai Rùa Bất Tử ngay lập tức trong 4s
            _isShieldActive = true;
            _currentStandStillTimer = _standStillTimeToShield;
            if (Context?.Health != null)
            {
                Context.Health.TriggerInvulnerability(4.0f);
            }
            Debug.Log("<color=#FFD700>[Kim Quy] Linh Quy Hộ Thể Tái Sinh: Bất tử 4s!</color>");
        }
    }
}
