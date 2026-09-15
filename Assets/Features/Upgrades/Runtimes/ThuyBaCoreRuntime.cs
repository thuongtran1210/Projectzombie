using UnityEngine;
using ProjectZombie.Features.Combat.Events;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades.Runtimes
{
    /// <summary>
    /// Runtime thực thi logic chiến đấu của Đại Lõi [THỦY BÁ CUỒNG NỘ] (Hệ Thủy - Mưa Bão, Sóng Thần, Đóng Băng).
    /// Tuân thủ 0 GC Allocations trong toàn bộ gameplay loop.
    /// </summary>
    public class ThuyBaCoreRuntime : MythicCoreRuntime
    {
        public override MythicArchetype Archetype => MythicArchetype.ThuyBaCuongNo;

        [Header("Settings - Sóng Thần Cuộn Trào (Periodic Tsunami)")]
        [SerializeField] private float _tsunamiInterval = 6.0f; // Mỗi 6s tạo 1 đợt Sóng Thần
        [SerializeField] private float _tsunamiRadius = 8.0f;
        [SerializeField] private float _tsunamiDamage = 60f;
        [SerializeField] private float _pullForce = 12f;

        [Header("Settings - Đóng Băng Băng Phong (Freeze & Shatter)")]
        [SerializeField] private float _freezeChance = 0.30f; // 30% cơ hội Đóng Băng quái
        [SerializeField] private float _shatterDamage = 50f;

        private float _tsunamiTimer = 0f;

        // Static buffer tái sử dụng để triệt tiêu GC Allocation
        private static readonly Collider2D[] _waterBuffer = new Collider2D[32];

        protected override void OnInitialize()
        {
            _tsunamiTimer = 0f;
            Debug.Log("<color=#00BFFF>[Thủy Bá]</color> Hô Phong Hoán Vũ Kích Hoạt: Mưa bão bao phủ toàn chiến trường!");
        }

        protected override void OnTeardown()
        {
            _tsunamiTimer = 0f;
        }

        private void Update()
        {
            if (!IsActive || Context?.Transform == null) return;

            _tsunamiTimer += Time.deltaTime;
            if (_tsunamiTimer >= _tsunamiInterval)
            {
                _tsunamiTimer = 0f;
                TriggerTsunamiWave();
            }
        }

        private void TriggerTsunamiWave()
        {
            if (Context?.Transform == null) return;

            Vector2 playerPos = Context.Transform.position;
            int hitCount = Physics2D.OverlapCircleNonAlloc(playerPos, _tsunamiRadius, _waterBuffer);

            for (int i = 0; i < hitCount; i++)
            {
                var col = _waterBuffer[i];
                if (col == null || col.gameObject == Context?.GameObject) continue;

                if (col.CompareTag("Enemy") && col.TryGetComponent<IDamageable>(out var damageable))
                {
                    // Gây sát thương Thủy và cuốn quái vật gom về hướng trước mặt Player
                    damageable.TakeDamage(new DamageData(_tsunamiDamage, isCritical: false, element: ElementType.Thuy));

                    if (col.TryGetComponent<Rigidbody2D>(out var rb))
                    {
                        Vector2 pullDir = (playerPos - (Vector2)col.transform.position).normalized;
                        rb.AddForce(pullDir * _pullForce, ForceMode2D.Impulse);
                    }
                }
            }

            Debug.Log("<color=#00BFFF>[Thủy Bá]</color> Sóng Thần Cuộn Trào quét sạch quái vật!");
        }

        protected override void SubscribeCombatEvents()
        {
            if (Context?.CombatEvents != null)
            {
                Context.CombatEvents.OnDamageDealt += HandleDamageDealt;
                Context.CombatEvents.OnDashPerformed += HandleDashThuyLong;
            }
        }

        protected override void UnsubscribeCombatEvents()
        {
            if (Context?.CombatEvents != null)
            {
                Context.CombatEvents.OnDamageDealt -= HandleDamageDealt;
                Context.CombatEvents.OnDashPerformed -= HandleDashThuyLong;
            }
        }

        private void HandleDamageDealt(in DamageDealtEvent e)
        {
            if (e.Target == null) return;

            // Đóng băng quái vật ngẫu nhiên
            if (Random.value <= _freezeChance)
            {
                // Khi vỡ băng gây thêm sát thương mảnh vụn
                if (e.Target.TryGetComponent<IDamageable>(out var dmg))
                {
                    dmg.TakeDamage(new DamageData(_shatterDamage, isCritical: true, element: ElementType.Thuy));
                }
            }
        }

        private void HandleDashThuyLong(in DashEvent e)
        {
            // Dash biến thành Rồng Nước gom quái dọc đường
            int hitCount = Physics2D.OverlapCircleNonAlloc(e.EndPosition, 3.0f, _waterBuffer);
            for (int i = 0; i < hitCount; i++)
            {
                var col = _waterBuffer[i];
                if (col != null && col.CompareTag("Enemy") && col.TryGetComponent<IDamageable>(out var dmg))
                {
                    dmg.TakeDamage(new DamageData(35f, isCritical: false, element: ElementType.Thuy));
                }
            }
        }

        protected override void OnRevived(in ReviveEvent e)
        {
            // Đại Thủy Triều Tái Sinh: Đẩy lùi quái vật 12m và đóng băng toàn bộ trong 3s
            if (Context?.Transform == null) return;

            Vector2 center = Context.Transform.position;
            int hitCount = Physics2D.OverlapCircleNonAlloc(center, 12.0f, _waterBuffer);

            for (int i = 0; i < hitCount; i++)
            {
                var col = _waterBuffer[i];
                if (col != null && col.CompareTag("Enemy"))
                {
                    if (col.TryGetComponent<IDamageable>(out var dmg))
                    {
                        dmg.TakeDamage(new DamageData(120f, isCritical: true, element: ElementType.Thuy));
                    }
                    if (col.TryGetComponent<Rigidbody2D>(out var rb))
                    {
                        Vector2 pushDir = ((Vector2)col.transform.position - center).normalized;
                        rb.AddForce(pushDir * 20f, ForceMode2D.Impulse);
                    }
                }
            }

            Debug.Log("<color=#00BFFF>[Thủy Bá] Đại Thủy Triều Tái Sinh: Sóng thần dạt quái ra 12m!</color>");
        }
    }
}
