using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Enemies
{
    /// <summary>
    /// Chiến lược tấn công Va Chạm (Touch / Collision Damage).
    /// Dành riêng cho Yêu ma dạng linh hồn, đốm lửa (như Ma Trơi E_MATROI).
    /// Gây sát thương ngay lập tức khi va chạm/chạm vào Player Collider mà không cần Animation Event vết chém.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class TouchAttackStrategy : AttackStrategy
    {
        [Header("Touch Damage Settings")]
        [Tooltip("Layer đối tượng nhận sát thương (mặc định Player)")]
        [SerializeField] private LayerMask targetLayer;

        [Tooltip("Bán kính vùng va chạm quanh tâm quái (nếu = 0 sẽ tự lấy theo Config.attackRange)")]
        [SerializeField] private float customTouchRadius = 0f;

        private float _lastDamageTime;
        private static readonly Collider2D[] _hitBuffer = new Collider2D[10];

        public float TouchRadius => (customTouchRadius > 0f) 
            ? customTouchRadius 
            : ((_enemy != null && _enemy.Config != null) ? _enemy.Config.attackRange : 0.8f);

        public override void Attack()
        {
            // Trong FSM AttackState, hàm Attack() được gọi mỗi cooldown.
            TryDealTouchDamage();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") || (targetLayer != 0 && ((1 << other.gameObject.layer) & targetLayer.value) != 0))
            {
                CheckAndApplyDamage(other.gameObject);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player") || (targetLayer != 0 && ((1 << collision.gameObject.layer) & targetLayer.value) != 0))
            {
                CheckAndApplyDamage(collision.gameObject);
            }
        }

        private void TryDealTouchDamage()
        {
            if (_enemy == null || _enemy.Config == null) return;
            if (Time.time < _lastDamageTime + _enemy.Config.attackCooldown) return;

            // Tâm quét được nâng nhẹ 0.5m để khớp vùng bụng/ngực nhân vật
            Vector2 center = (Vector2)transform.position + new Vector2(0f, 0.5f);
            float radius = TouchRadius;
            int filterMask = targetLayer != 0 ? targetLayer.value : LayerMask.GetMask("Player");
            if (filterMask == 0) filterMask = ~0;

            int hitCount = Physics2D.OverlapCircleNonAlloc(center, radius, _hitBuffer, filterMask);
            for (int i = 0; i < hitCount; i++)
            {
                var col = _hitBuffer[i];
                if (col != null && (col.CompareTag("Player") || (targetLayer != 0 && ((1 << col.gameObject.layer) & targetLayer.value) != 0)))
                {
                    if (CheckAndApplyDamage(col.gameObject))
                    {
                        break;
                    }
                }
            }
        }

        private bool CheckAndApplyDamage(GameObject targetObj)
        {
            if (_enemy == null || _enemy.Config == null || targetObj == null) return false;
            if (Time.time < _lastDamageTime + _enemy.Config.attackCooldown) return false;

            if (targetObj.TryGetComponent<IDamageable>(out var damageable) ||
                (damageable = targetObj.GetComponentInParent<IDamageable>()) != null)
            {
                damageable.TakeDamage(_enemy.GetTotalDamage());
                _lastDamageTime = Time.time;

                // Kích hoạt animation Attack nếu có
                _enemy.Animator?.TriggerAttack();
                return true;
            }
            return false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.4f, 0f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, TouchRadius);
        }
    }
}
