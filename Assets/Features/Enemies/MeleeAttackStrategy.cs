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
        private bool _isAttacking = false;
        private Coroutine _attackRoutine;

        public override bool IsAttacking => _isAttacking;

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

        public override void InterruptAttack()
        {
            _isAttacking = false;
            _hasDealtDamageThisAttack = false;
            if (_attackRoutine != null)
            {
                StopCoroutine(_attackRoutine);
                _attackRoutine = null;
            }
            CancelInvoke(nameof(DealMeleeDamage));
        }

        private void DealMeleeDamage()
        {
            if (_enemy == null || _enemy.Config == null) return;
            if (_enemy.StatusController != null && !_enemy.StatusController.CanAttack) return;
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
            if (_enemy != null && _enemy.StatusController != null && !_enemy.StatusController.CanAttack)
            {
                return;
            }

            _hasDealtDamageThisAttack = false;

            if (_attackRoutine != null) StopCoroutine(_attackRoutine);
            _attackRoutine = StartCoroutine(ExecuteAttackRoutine());
        }

        private System.Collections.IEnumerator ExecuteAttackRoutine()
        {
            if (_enemy == null || _enemy.Config == null) yield break;

            _isAttacking = true;

            // 1. Lấy độ dài thực tế của Clip Animation Attack
            float clipLength = 0.5f;
            if (_enemy.EnemyAnimator != null)
            {
                clipLength = _enemy.EnemyAnimator.GetCurrentAttackClipLength(0.5f);
            }

            float cooldown = Mathf.Max(0.2f, _enemy.Config.attackCooldown);
            float animSpeed = Mathf.Clamp(clipLength / Mathf.Min(clipLength, cooldown * 0.8f), 0.8f, 2.2f);

            var bossAnimator = GetComponentInChildren<BossAnimator>();
            if (bossAnimator != null)
            {
                bossAnimator.PlayAnimation("Attack");
            }
            else if (_enemy.EnemyAnimator != null)
            {
                _enemy.EnemyAnimator.SetAttackAnimationSpeed(animSpeed);
                _enemy.EnemyAnimator.TriggerAttack();
            }

            // 2. Chờ đúng Frame vung tay chạm đòn (45% thời lượng thực tế của Clip sau khi áp dụng Speed)
            float totalAnimDuration = clipLength / animSpeed;
            float impactDelay = totalAnimDuration * 0.45f;
            yield return new WaitForSeconds(impactDelay);

            // Kiểm tra nếu quái bị dính khống chế trong lúc vung tay thì dập tắt đòn
            if (_enemy.StatusController != null && !_enemy.StatusController.CanAttack)
            {
                InterruptAttack();
                yield break;
            }

            // 3. Đúng lúc chạm tay -> Sinh VFX Vệt Cào Quái & Quét Hitbox gây Damage lên Player
            if (_enemy.Config.attackVfxPrefab != null)
            {
                Vector2 center = GetHitboxCenter();
                float facingSign = transform.localScale.x < 0 ? -1f : 1f;
                if (_enemy.PlayerTransform != null)
                {
                    facingSign = (_enemy.PlayerTransform.position.x < transform.position.x) ? -1f : 1f;
                }
                float angle = facingSign < 0 ? 180f : 0f;

                GameObject vfx = Instantiate(_enemy.Config.attackVfxPrefab, center, Quaternion.Euler(0, 0, angle));
                float life = _enemy.Config.vfxDuration > 0 ? _enemy.Config.vfxDuration : 0.35f;
                Destroy(vfx, life);
            }

            DealMeleeDamage();

            // 4. Chờ hoàn tất nốt giai đoạn thu tay về (Recovery Phase 55% còn lại)
            float recoveryDelay = totalAnimDuration * 0.55f;
            yield return new WaitForSeconds(recoveryDelay);

            _isAttacking = false;
            _attackRoutine = null;
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





