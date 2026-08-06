using System.Collections;
using UnityEngine;
using ProjectZombie.Features.VFX.Indicators;
using ProjectZombie.Features.Boss;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Enemies.Boss.Skills
{
    /// <summary>
    /// Skill Ngưu Xung Thiên (Bull Dash x3 speed) của Boss Ngưu Đầu Mã Diện theo GDD 5.2.
    /// Khóa vị trí Player, phát vệt đỏ chỉ dấu trong 1.5s và lao thẳng với tốc độ gấp 3 lần.
    /// </summary>
    public class BullDashSkill : MonoBehaviour
    {
        [Header("Dash Settings")]
        [SerializeField] private float dashSpeedMultiplier = 3f;
        [SerializeField] private float dashDuration = 1.5f;
        [SerializeField] private float baseMoveSpeed = 2.2f;
        [SerializeField] private float telegraphDuration = 1.5f; // Thời gian phát vệt đỏ báo hiệu
        [SerializeField] private float dashWidth = 1.5f; // Độ rộng vệt báo & bán kính va chạm (mét)

        private bool _isDashing = false;
        public bool IsDashing => _isDashing;

        public void PerformDash(Vector3 targetPosition)
        {
            if (_isDashing) return;
            StartCoroutine(DashRoutine(targetPosition));
        }

        private IEnumerator DashRoutine(Vector3 targetPosition)
        {
            _isDashing = true;
            Vector3 dashDirection = (targetPosition - transform.position).normalized;
            Vector2 dashSize = IndicatorUtility.CalculateDashSize(baseMoveSpeed, dashSpeedMultiplier, dashDuration, dashWidth);

            // BƯỚC 1: BÁO HỆU VỆT ĐỎ CHỈ DẤU
            bool telegraphFinished = false;
            if (SkillIndicatorManager.Instance != null)
            {
                SkillIndicatorManager.Instance.ShowIndicator(new IndicatorRequest(
                    IndicatorShape.Box,
                    transform.position,
                    dashDirection,
                    dashSize, // Rộng dashWidth (m), Dài chuẩn quãng đường lao (9.9m)
                    telegraphDuration,
                    new Color(1f, 0.1f, 0.1f, 0.4f)
                ), () => telegraphFinished = true);
            }
            else
            {
                telegraphFinished = true;
            }

            while (!telegraphFinished)
            {
                yield return null;
            }

            // BƯỚC 2: TUNG ĐÒN LAO TÔNG
            Debug.Log($"[BullDashSkill] Boss kích hoạt chiêu 'Ngưu Xung Thiên' lao tới {targetPosition}!");
            
            var bossAnimator = GetComponentInChildren<BossAnimator>();
            if (bossAnimator != null)
            {
                bossAnimator.PlayAnimation("Dash");
                bossAnimator.FlipToDirection(dashDirection.x);
            }

            float elapsed = 0f;
            float ghostTimer = 0f;
            bool hitPlayerDuringDash = false;

            var sr = GetComponentInChildren<SpriteRenderer>();

            while (elapsed < dashDuration)
            {
                elapsed += Time.deltaTime;
                ghostTimer += Time.deltaTime;
                transform.position += dashDirection * (baseMoveSpeed * dashSpeedMultiplier) * Time.deltaTime;

                if (ghostTimer >= 0.08f)
                {
                    ghostTimer = 0f;
                    if (sr != null && sr.sprite != null)
                    {
                        StartCoroutine(SpawnGhostTrail(transform.position, sr.sprite, sr.transform.localScale));
                    }
                }

                if (!hitPlayerDuringDash)
                {
                    Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, dashWidth);
                    foreach (var col in hits)
                    {
                        if (col.CompareTag("Player"))
                        {
                            var health = col.GetComponent<HealthSystem>();
                            if (health != null)
                            {
                                health.TakeDamage(new DamageData(35f, false, ElementType.Tho));
                                hitPlayerDuringDash = true;
                                Debug.Log("[BullDashSkill] Boss lao tông trúng người chơi! Gây 35 Sát thương.");
                            }
                        }
                    }
                }

                yield return null;
            }

            if (bossAnimator != null)
            {
                bossAnimator.PlayAnimation("Idle");
            }

            _isDashing = false;
        }

        private IEnumerator SpawnGhostTrail(Vector3 pos, Sprite sprite, Vector3 scale)
        {
            GameObject ghost = new GameObject("BullDash_GhostTrail");
            ghost.transform.position = pos;
            ghost.transform.localScale = scale * 2.2f;

            var sr = ghost.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(1f, 0.2f, 0.1f, 0.5f);
            sr.sortingOrder = 2;

            float elapsed = 0f;
            float duration = 0.3f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0.5f, 0f, elapsed / duration);
                sr.color = new Color(1f, 0.2f, 0.1f, alpha);
                yield return null;
            }

            Destroy(ghost);
        }
    }
}

