using UnityEngine;
using ProjectZombie.Features.Shared;

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

            // Tâm X luôn được đẩy ra đúng 1/2 attackRange để tầm chém phủ trọn từ 0m -> attackRange
            float offsetX = (BaseRange * 0.5f) * facingSign;
            return (Vector2)transform.position + new Vector2(offsetX, offsetY);
        }

        public float GetHitboxRadius()
        {
            // Bán kính = 1/2 attackRange (mặt quái 0m -> tầm chém R)
            return BaseRange * 0.5f;
        }

        public Vector2 GetHitboxBoxSize()
        {
            // Chiều rộng = attackRange, Chiều cao = attackRange * boxHeightRatio
            return new Vector2(BaseRange, BaseRange * boxHeightRatio);
        }

        private void Start()
        {
            if (_enemy.EnemyAnimator != null)
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

            for (int i = 0; i < hitCount; i++)
            {
                var col = _hitBuffer[i];
                if (col != null && col.CompareTag("Player"))
                {
                    if (col.TryGetComponent<HealthSystem>(out var playerHealth))
                    {
                        playerHealth.TakeDamage(_enemy.GetTotalDamage());
                    }
                }
            }
        }

        public override void Attack()
        {
            if (_enemy.EnemyAnimator != null)
            {
                _enemy.EnemyAnimator.TriggerAttack();
            }
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





