using System.Collections;
using UnityEngine;
using ProjectZombie.Features.VFX.Indicators;
using ProjectZombie.Features.Boss;

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
            float dashDistance = baseMoveSpeed * dashSpeedMultiplier * dashDuration;

            // BƯỚC 1: BÁO HỆU VỆT ĐỎ CHỈ DẤU
            bool telegraphFinished = false;
            if (SkillIndicatorManager.Instance != null)
            {
                SkillIndicatorManager.Instance.ShowIndicator(new IndicatorRequest(
                    IndicatorShape.Box,
                    transform.position,
                    dashDirection,
                    new Vector2(1.5f, dashDistance), // Rộng 1.5m, Dài bằng tầm đòn lao
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
            while (elapsed < dashDuration)
            {
                elapsed += Time.deltaTime;
                transform.position += dashDirection * (baseMoveSpeed * dashSpeedMultiplier) * Time.deltaTime;
                yield return null;
            }

            if (bossAnimator != null)
            {
                bossAnimator.PlayAnimation("Idle");
            }

            _isDashing = false;
        }
    }
}

