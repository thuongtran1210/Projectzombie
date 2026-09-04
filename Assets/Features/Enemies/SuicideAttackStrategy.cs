using System.Collections;
using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Enemies
{
    /// <summary>
    /// Chiến lược tấn công Tự Sát Nổ AoE (Suicide Explosive Attack).
    /// Dành riêng cho Hồ Ly Tinh Nhỏ (E_HOALYTINH) theo GDD 5.1.
    /// Khi tiếp cận Player, kích hoạt đếm ngược ngắn và nổ diện rộng gây 50 Sát thương hệ Hỏa.
    /// Tối ưu 0 GC Allocation cho Physics2D.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class SuicideAttackStrategy : AttackStrategy
    {
        [Header("Explosion Settings")]
        [Tooltip("Bán kính vùng nổ gây sát thương AoE")]
        [SerializeField] private float explodeRadius = 2.5f;

        [Tooltip("Khoảng cách tiếp cận để bắt đầu đếm ngược nổ")]
        [SerializeField] private float triggerDistance = 1.5f;

        [Tooltip("Thời gian nén đòn/đếm ngược trước khi bùng nổ (giây)")]
        [SerializeField] private float explodeDelay = 0.4f;

        [Tooltip("Sát thương nổ diện rộng")]
        [SerializeField] private float explosionDamage = 50f;

        [Tooltip("Layer đối tượng nhận sát thương (mặc định Player)")]
        [SerializeField] private LayerMask targetLayer;

        private bool _isExploding = false;
        private Coroutine _explodeRoutine;
        private static readonly Collider2D[] _hitBuffer = new Collider2D[15];

        public float TriggerDistance => triggerDistance;
        public float ExplodeRadius => explodeRadius;
        public override bool IsAttacking => _isExploding;

        public override void InterruptAttack()
        {
            _isExploding = false;
            if (_explodeRoutine != null)
            {
                StopCoroutine(_explodeRoutine);
                _explodeRoutine = null;
            }
        }

        public override void Attack()
        {
            if (_enemy != null && _enemy.StatusController != null && !_enemy.StatusController.CanAttack)
            {
                return;
            }

            if (!_isExploding)
            {
                _explodeRoutine = StartCoroutine(ExplodeRoutine());
            }
        }

        private IEnumerator ExplodeRoutine()
        {
            _isExploding = true;

            // Đổi animation sang trạng thái nổ / dừng di chuyển nếu có
            if (_enemy != null && _enemy.Rb != null)
            {
                _enemy.Rb.velocity = Vector2.zero;
            }

            _enemy?.Animator?.TriggerAttack();

            yield return new WaitForSeconds(explodeDelay);

            // Gây sát thương nổ AoE với 0 GC Allocation
            Vector2 center = transform.position;
            int filterMask = targetLayer != 0 ? targetLayer.value : LayerMask.GetMask("Player");
            if (filterMask == 0) filterMask = ~0;

            int hitCount = Physics2D.OverlapCircleNonAlloc(center, explodeRadius, _hitBuffer, filterMask);
            for (int i = 0; i < hitCount; i++)
            {
                var col = _hitBuffer[i];
                if (col != null && col.CompareTag("Player"))
                {
                    if (col.TryGetComponent<HealthSystem>(out var playerHealth))
                    {
                        DamageData damageData = new DamageData(explosionDamage, false, ElementType.Hoa);
                        playerHealth.TakeDamage(damageData);
                    }
                }
            }

            Debug.Log($"[SuicideAttackStrategy] {_enemy?.gameObject.name} phát nổ AoE {explosionDamage} DMG (Hệ Hỏa) tại {center}");

            // Tự sát
            if (_enemy != null && _enemy.HealthSystem != null)
            {
                _enemy.HealthSystem.TakeDamage(new DamageData(99999f));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            _isExploding = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.7f);
            Gizmos.DrawWireSphere(transform.position, triggerDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explodeRadius);
        }
    }
}
