using UnityEngine;
using ProjectZombie.Features.Combat.Events;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades.Runtimes
{
    /// <summary>
    /// Runtime thực thi logic chiến đấu của Đại Lõi [TẢN VIÊN SƠN THÁNH] (Hệ Thổ - Bất Tử Giáp Đá, Thạch Trụ, Phản Đòn).
    /// Tuân thủ 0 GC Allocations trong toàn bộ gameplay loop.
    /// </summary>
    public class SonThanhCoreRuntime : MythicCoreRuntime
    {
        public override MythicArchetype Archetype => MythicArchetype.TanVienSonThanh;

        [Header("Settings - Giáp Đá Bất Hoại")]
        [SerializeField] private float _shieldRechargeDelay = 8.0f; // 8s không nhận dame -> Hồi đầy Giáp Đá
        [SerializeField] private float _stoneShieldMaxHpPercent = 1.0f; // 100% Max HP

        [Header("Settings - Thạch Trụ Mọc Đè Quái (Stand Still)")]
        [SerializeField] private float _standStillTimeToPillars = 2.5f;
        [SerializeField] private float _pillarRadius = 5.0f;
        [SerializeField] private float _pillarDamage = 80f;

        private float _currentStoneShield = 0f;
        private float _maxStoneShield = 100f;
        private float _timeSinceLastDamage = 0f;
        private float _standStillTimer = 0f;
        private Vector2 _lastPosition;

        // Static buffer tái sử dụng để triệt tiêu GC Allocation
        private static readonly Collider2D[] _stoneBuffer = new Collider2D[32];

        public float CurrentStoneShield => _currentStoneShield;
        public float MaxStoneShield => _maxStoneShield;

        protected override void OnInitialize()
        {
            if (Context?.Health != null)
            {
                _maxStoneShield = Context.Health.MaxHealth * _stoneShieldMaxHpPercent;
                _currentStoneShield = _maxStoneShield;
            }

            if (Context?.Transform != null)
            {
                _lastPosition = Context.Transform.position;
            }

            _timeSinceLastDamage = _shieldRechargeDelay;
            _standStillTimer = 0f;

            Debug.Log($"<color=#D4AF37>[Sơn Thánh]</color> Giáp Đá Kích Hoạt: {_currentStoneShield:F0}/{_maxStoneShield:F0} HP!");
        }

        protected override void OnTeardown()
        {
            _currentStoneShield = 0f;
        }

        private void Update()
        {
            if (!IsActive || Context?.Transform == null) return;

            float dt = Time.deltaTime;

            // 1. Tự động hồi phục Giáp Đá sau 8s không nhận sát thương
            _timeSinceLastDamage += dt;
            if (_timeSinceLastDamage >= _shieldRechargeDelay && _currentStoneShield < _maxStoneShield)
            {
                _currentStoneShield = _maxStoneShield;
                Debug.Log("<color=#D4AF37>[Sơn Thánh]</color> Giáp Đá Bất Hoại đã hồi phục đầy đủ!");
            }

            // 2. Kiểm tra đứng yên để mọc Thạch Trụ
            Vector2 currentPos = Context.Transform.position;
            float movedDist = Vector2.Distance(currentPos, _lastPosition);
            _lastPosition = currentPos;

            if (movedDist < 0.01f)
            {
                _standStillTimer += dt;
                if (_standStillTimer >= _standStillTimeToPillars)
                {
                    _standStillTimer = 0f;
                    SpawnStonePillars();
                }
            }
            else
            {
                _standStillTimer = 0f;
            }
        }

        private void SpawnStonePillars()
        {
            // Triệu hồi 4 Thạch Trụ mọc từ đất đè bẹp quái vật xung quanh
            if (Context?.Transform == null) return;

            Vector2 center = Context.Transform.position;
            int hitCount = Physics2D.OverlapCircleNonAlloc(center, _pillarRadius, _stoneBuffer);

            for (int i = 0; i < hitCount; i++)
            {
                var col = _stoneBuffer[i];
                if (col == null || col.gameObject == Context?.GameObject) continue;

                if (col.CompareTag("Enemy") && col.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(new DamageData(_pillarDamage, isCritical: false, element: ElementType.Tho));
                }
            }

            Debug.Log($"<color=#D4AF37>[Sơn Thánh]</color> Bạt Sơn Địa Trận: 4 Thạch Trụ trồi lên đè bẹp quái vật!");
        }

        protected override void SubscribeCombatEvents()
        {
            if (Context?.CombatEvents != null)
            {
                Context.CombatEvents.OnDashPerformed += HandleDashQuake;
            }
        }

        protected override void UnsubscribeCombatEvents()
        {
            if (Context?.CombatEvents != null)
            {
                Context.CombatEvents.OnDashPerformed -= HandleDashQuake;
            }
        }

        private void HandleDashQuake(in DashEvent e)
        {
            // Khi Dash kích hoạt đợt chấn địa làm choáng quái vật tại điểm đáp
            int hitCount = Physics2D.OverlapCircleNonAlloc(e.EndPosition, 3.5f, _stoneBuffer);
            for (int i = 0; i < hitCount; i++)
            {
                var col = _stoneBuffer[i];
                if (col != null && col.CompareTag("Enemy") && col.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(new DamageData(40f, isCritical: false, element: ElementType.Tho));
                }
            }
        }

        protected override void OnRevived(in ReviveEvent e)
        {
            // Hồi phục 100% Giáp Đá và tạo cơn địa chấn hất tung quái xung quanh
            _currentStoneShield = _maxStoneShield;
            _timeSinceLastDamage = _shieldRechargeDelay;

            if (Context?.Transform != null)
            {
                Vector2 center = Context.Transform.position;
                int hitCount = Physics2D.OverlapCircleNonAlloc(center, 8.0f, _stoneBuffer);
                for (int i = 0; i < hitCount; i++)
                {
                    var col = _stoneBuffer[i];
                    if (col != null && col.CompareTag("Enemy") && col.TryGetComponent<IDamageable>(out var damageable))
                    {
                        damageable.TakeDamage(new DamageData(100f, isCritical: true, element: ElementType.Tho));
                    }
                }
            }

            Debug.Log("<color=#D4AF37>[Sơn Thánh] Bạt Sơn Tái Sinh: Giáp Đá hồi 100% & Địa Chấn hất văng quái!</color>");
        }
    }
}
