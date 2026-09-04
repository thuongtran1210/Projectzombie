using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.VFX.Indicators;
using ProjectZombie.Features.Boss;

namespace ProjectZombie.Features.Enemies.Boss.Skills
{
    /// <summary>
    /// Chiến lược tấn công Cận chiến độc quyền cho Boss (Ngưu Đầu Mã Diện / Diêm Vương).
    /// Tách biệt hoàn toàn với MeleeAttackStrategy của Quái thường.
    /// Tích hợp Animation Event (OnHitFrame) và Vệt đỏ báo hiệu (Indicator).
    /// </summary>
    public class BossMeleeAttackStrategy : AttackStrategy
    {
        [Header("Targeting & Damage Settings")]
        [Tooltip("Layer nhận sát thương (mặc định Player)")]
        [SerializeField] private LayerMask targetLayer;

        [Tooltip("Sát thương cơ bản của đòn vung vũ khí")]
        [SerializeField] private float attackDamage = 35f;

        [Tooltip("Tầm chém của Boss (bán kính / chiều dài)")]
        [SerializeField] private float attackRange = 2.5f;

        [Tooltip("Thời gian phát vệt đỏ báo hiệu trước khi nện đòn")]
        [SerializeField] private float telegraphDuration = 0.3f;

        [Header("Hitbox Shape & Position")]
        [SerializeField] private HitboxShape hitboxShape = HitboxShape.Circle;
        [SerializeField] private float offsetY = 0f;
        [SerializeField] private float boxHeightRatio = 1.0f;

        private BossAnimationEventHandler _eventHandler;
        private BossAnimator _bossAnimator;
        private static readonly Collider2D[] _hitBuffer = new Collider2D[15];

        public float AttackRange => attackRange;

        protected override void Awake()
        {
            base.Awake();
            _eventHandler = GetComponentInChildren<BossAnimationEventHandler>();
            _bossAnimator = GetComponentInChildren<BossAnimator>();
        }

        private void Start()
        {
            if (_eventHandler != null)
            {
                _eventHandler.OnHitFrame += DealDamageOnHitFrame;
            }
        }

        private void OnDestroy()
        {
            if (_eventHandler != null)
            {
                _eventHandler.OnHitFrame -= DealDamageOnHitFrame;
            }
        }

        public Vector2 GetHitboxCenter()
        {
            float facingSign = transform.localScale.x < 0 ? -1f : 1f;
            if (_enemy != null && _enemy.PlayerTransform != null)
            {
                facingSign = (_enemy.PlayerTransform.position.x < transform.position.x) ? -1f : 1f;
            }

            float offsetX = (attackRange * 0.5f) * facingSign;
            return (Vector2)transform.position + new Vector2(offsetX, offsetY);
        }

        private bool _isAttacking = false;
        private bool _hasDealtDamageThisAttack = false;
        private Coroutine _attackRoutine;

        public override bool IsAttacking => _isAttacking;

        public override void InterruptAttack()
        {
            _isAttacking = false;
            _hasDealtDamageThisAttack = false;
            if (_attackRoutine != null)
            {
                StopCoroutine(_attackRoutine);
                _attackRoutine = null;
            }
            if (_bossAnimator != null)
            {
                _bossAnimator.PlayAnimation("Idle");
            }
        }

        public override void Attack()
        {
            PerformAttack();
        }

        public void PerformAttack()
        {
            if (_isAttacking) return;
            if (_attackRoutine != null) StopCoroutine(_attackRoutine);
            _attackRoutine = StartCoroutine(ExecuteAttackRoutine());
        }

        private System.Collections.IEnumerator ExecuteAttackRoutine()
        {
            _isAttacking = true;
            _hasDealtDamageThisAttack = false;

            // BƯỚC 1: BÁO HỆU VỆT ĐỎ CHỈ DẤU (INDICATOR)
            Vector2 center = GetHitboxCenter();
            Vector3 direction = (_enemy != null && _enemy.PlayerTransform != null)
                ? (_enemy.PlayerTransform.position - transform.position).normalized
                : transform.right;

            if (SkillIndicatorManager.Instance != null && telegraphDuration > 0f)
            {
                IndicatorShape shape = (hitboxShape == HitboxShape.Circle) ? IndicatorShape.Circle : IndicatorShape.Box;
                Vector2 size = (hitboxShape == HitboxShape.Circle)
                    ? new Vector2(attackRange, attackRange)
                    : new Vector2(attackRange, attackRange * boxHeightRatio);

                SkillIndicatorManager.Instance.ShowIndicator(new IndicatorRequest(
                    shape,
                    center,
                    direction,
                    size,
                    telegraphDuration,
                    new Color(1f, 0.2f, 0.2f, 0.45f)
                ));

                yield return new WaitForSeconds(telegraphDuration);
            }

            // BƯỚC 2: PHÁT ANIMATION CỦA BOSS
            float clipLength = 0.7f;
            if (_bossAnimator != null)
            {
                clipLength = _bossAnimator.GetCurrentClipLength("Attack", 0.7f);
                _bossAnimator.PlayAnimation("Attack", true);
                _bossAnimator.FlipToDirection(direction.x);
            }

            // BƯỚC 3: ĐỢI FRAME CHẠM ĐÒN (45% clip)
            float hitDelay = clipLength * 0.45f;
            yield return new WaitForSeconds(hitDelay);

            // Gây damage nếu Animation Event chưa kích hoạt
            if (!_hasDealtDamageThisAttack)
            {
                DealDamageOnHitFrame();
            }

            // BƯỚC 4: ĐỢI KẾT THÚC CLIP (55% còn lại)
            float recoveryDelay = clipLength * 0.55f;
            yield return new WaitForSeconds(recoveryDelay);

            if (_bossAnimator != null)
            {
                _bossAnimator.PlayAnimation("Idle");
            }

            _isAttacking = false;
            _attackRoutine = null;
        }

        /// <summary>
        /// Được gọi tự động từ BossAnimationEventHandler.OnHitFrame khi clip hoạt ảnh chạm đến frame ra đòn.
        /// </summary>
        private void DealDamageOnHitFrame()
        {
            if (_hasDealtDamageThisAttack) return;
            _hasDealtDamageThisAttack = true;
            Vector2 center = GetHitboxCenter();
            int filterMask = targetLayer != 0 ? targetLayer.value : LayerMask.GetMask("Player");
            if (filterMask == 0) filterMask = ~0;

            int hitCount = 0;
            if (hitboxShape == HitboxShape.Circle)
            {
                float radius = attackRange * 0.5f;
                hitCount = Physics2D.OverlapCircleNonAlloc(center, radius, _hitBuffer, filterMask);
            }
            else
            {
                Vector2 boxSize = new Vector2(attackRange, attackRange * boxHeightRatio);
                hitCount = Physics2D.OverlapBoxNonAlloc(center, boxSize, 0f, _hitBuffer, filterMask);
            }

            ElementType currentElem = ElementType.Tho;
            var elementController = GetComponent<BossElementController>();
            if (elementController != null)
            {
                currentElem = elementController.CurrentElement;
            }

            for (int i = 0; i < hitCount; i++)
            {
                var col = _hitBuffer[i];
                if (col != null && col.CompareTag("Player"))
                {
                    if (col.TryGetComponent<HealthSystem>(out var playerHealth))
                    {
                        float finalDamage = (_enemy != null && _enemy.Config != null) ? _enemy.GetTotalDamage() : attackDamage;
                        playerHealth.TakeDamage(new DamageData(finalDamage, false, currentElem));
                    }
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector2 center = GetHitboxCenter();
            Gizmos.color = new Color(1f, 0.1f, 0.1f, 0.8f);

            if (hitboxShape == HitboxShape.Circle)
            {
                float radius = attackRange * 0.5f;
                Gizmos.DrawWireSphere(center, radius);
            }
            else
            {
                Vector2 boxSize = new Vector2(attackRange, attackRange * boxHeightRatio);
                Gizmos.DrawWireCube(center, boxSize);
            }
        }
#endif
    }
}
