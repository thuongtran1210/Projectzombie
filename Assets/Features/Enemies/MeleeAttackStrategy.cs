using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Boss;

namespace ProjectZombie.Features.Enemies
{
    public enum HitboxShape { Circle, Box }

    /// <summary>
    /// Chiến lược tấn công Cận chiến (Melee Slash).
    /// Tự động đồng bộ 100% tầm đánh với EnemyConfig.attackRange (không gây mâu thuẫn dữ liệu).
    /// Cho phép căn chỉnh vị trí Y (lệch trên/dưới) và tỉ lệ hình dáng để khớp hoàn toàn với Sprite Animation.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class MeleeAttackStrategy : AttackStrategy
    {
        [Header("Targeting & Layer Settings")]
        [Tooltip("Layer đối tượng nhận sát thương (mặc định Player)")]
        [SerializeField] private LayerMask targetLayer;

        [Header("Visual Alignment (Giao diện & Hoạt ảnh)")]
        [Tooltip("Kéo thả GameObject con đại diện cho tâm vết chém (nếu muốn định vị thủ công tuyệt đối).")]
        [SerializeField] private Transform attackPoint;

        [Tooltip("Hình dạng Hitbox (Hình tròn hoặc Hình hộp)")]
        [SerializeField] private HitboxShape hitboxShape = HitboxShape.Circle;

        [Tooltip("Độ lệch trục Y (trên/dưới) để khớp với vạch chém của Sprite.")]
        [SerializeField] private float offsetY = 0f;

        [Tooltip("Tỉ lệ chiều cao/rộng của Hitbox hình hộp (Default: 1.0). Chiều rộng luôn tự động chuẩn bằng attackRange.")]
        [SerializeField] private float boxHeightRatio = 1.0f;

        private static readonly Collider2D[] _hitBuffer = new Collider2D[10];

        /// <summary>
        /// Bán kính/Tầm với luôn lấy chuẩn theo Config.attackRange (Single Source of Truth).
        /// </summary>
        public float BaseRange => (_enemy != null && _enemy.Config != null) ? _enemy.Config.attackRange : 1.5f;

        public Vector2 GetHitboxCenter()
        {
            if (attackPoint != null) return attackPoint.position;

            float facingSign = transform.localScale.x < 0 ? -1f : 1f;
            if (_enemy != null && _enemy.PlayerTransform != null)
            {
                facingSign = (_enemy.PlayerTransform.position.x < transform.position.x) ? -1f : 1f;
            }

            // Tâm X được đẩy ra 1/2 attackRange, Tâm Y tự động bù 0.6m để khớp thân người chơi 2.5D
            float effectiveOffsetY = (offsetY != 0f) ? offsetY : 0.6f;
            float offsetX = (BaseRange * 0.5f) * facingSign;
            return (Vector2)transform.position + new Vector2(offsetX, effectiveOffsetY);
        }

        public float GetHitboxRadius()
        {
            // Bán kính tối thiểu 0.8m để đảm bảo bao trọn thân người chơi từ chân tới đầu
            return Mathf.Max(BaseRange * 0.5f, 0.8f);
        }

        public Vector2 GetHitboxBoxSize()
        {
            // Chiều rộng = attackRange, Chiều cao tối thiểu 1.6m để bao trọn mọi chiều cao tướng
            float effectiveHeight = Mathf.Max(BaseRange * boxHeightRatio, 1.6f);
            return new Vector2(BaseRange, effectiveHeight);
        }

        private bool _hasDealtDamageThisAttack = false;

        private void Start()
        {
            if (_enemy != null && _enemy.EnemyAnimator != null)
            {
                _enemy.EnemyAnimator.OnAttackEvent += DealMeleeDamage;
            }
        }

        private void OnDestroy()
        {
            if (_enemy != null && _enemy.EnemyAnimator != null)
            {
                _enemy.EnemyAnimator.OnAttackEvent -= DealMeleeDamage;
            }
        }

        private void DealMeleeDamage()
        {
            if (_enemy == null || _enemy.Config == null) return;
            if (_hasDealtDamageThisAttack) return; // Khóa chống lặp sát thương trong 1 nhịp chém

            _hasDealtDamageThisAttack = true;
            CancelInvoke(nameof(DealMeleeDamage)); // Hủy timer fallback nếu Animation Event đã kích hoạt trước

            Vector2 center = GetHitboxCenter();
            int filterMask = targetLayer != 0 ? targetLayer.value : LayerMask.GetMask("Player");
            if (filterMask == 0) filterMask = ~0;

            int hitCount = 0;
            if (hitboxShape == HitboxShape.Circle)
            {
                float radius = GetHitboxRadius();
                hitCount = Physics2D.OverlapCircleNonAlloc(center, radius, _hitBuffer, filterMask);
            }
            else
            {
                Vector2 boxSize = GetHitboxBoxSize();
                hitCount = Physics2D.OverlapBoxNonAlloc(center, boxSize, 0f, _hitBuffer, filterMask);
            }

            IDamageable targetDamageable = null;
            for (int i = 0; i < hitCount; i++)
            {
                var col = _hitBuffer[i];
                if (col != null && (col.CompareTag("Player") || (targetLayer != 0 && ((1 << col.gameObject.layer) & targetLayer.value) != 0)))
                {
                    if (col.TryGetComponent<IDamageable>(out var damageable) ||
                        (damageable = col.GetComponentInParent<IDamageable>()) != null)
                    {
                        targetDamageable = damageable;
                        break; // Đã xác định được Player, ngắt vòng lặp để tránh tính lại khi Player có nhiều Collider2D
                    }
                }
            }

            if (targetDamageable != null)
            {
                targetDamageable.TakeDamage(_enemy.GetTotalDamage());
            }
        }

        public override void Attack()
        {
            _hasDealtDamageThisAttack = false; // Reset trạng thái cho lượt đánh mới

            var bossAnimator = GetComponentInChildren<BossAnimator>();
            if (bossAnimator != null)
            {
                bossAnimator.PlayAnimation("Attack");
            }
            else if (_enemy != null && _enemy.EnemyAnimator != null)
            {
                _enemy.EnemyAnimator.TriggerAttack();
            }

            // Fallback tự động gây sát thương sau 0.15s nếu Animation Clip thiếu Animation Event (giảm độ trễ tránh người chơi lùi né)
            CancelInvoke(nameof(DealMeleeDamage));
            Invoke(nameof(DealMeleeDamage), 0.15f);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_enemy == null) _enemy = GetComponent<Enemy>();
            Vector2 center = GetHitboxCenter();

            Gizmos.color = new Color(1f, 0.3f, 0f, 0.9f);

            if (hitboxShape == HitboxShape.Circle)
            {
                float radius = GetHitboxRadius();
                Gizmos.DrawWireSphere(center, radius);
                Gizmos.color = new Color(1f, 0.3f, 0f, 0.2f);
                Gizmos.DrawSphere(center, radius);
            }
            else
            {
                Vector2 boxSize = GetHitboxBoxSize();
                Gizmos.DrawWireCube(center, boxSize);
                Gizmos.color = new Color(1f, 0.3f, 0f, 0.2f);
                Gizmos.DrawCube(center, boxSize);
            }
        }
#endif
    }
}





