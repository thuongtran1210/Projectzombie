using System.Collections;
using UnityEngine;

namespace ProjectZombie.Features.Enemies.Boss.Skills
{
    /// <summary>
    /// Skill Ngưu Xung Thiên (Bull Dash x3 speed) của Boss Ngưu Đầu Mã Diện theo GDD 5.2.
    /// Khóa vị trí Player và lao thẳng với tốc độ gấp 3 lần trong 1.5 giây.
    /// </summary>
    public class BullDashSkill : MonoBehaviour
    {
        [Header("Dash Settings")]
        [SerializeField] private float dashSpeedMultiplier = 3f;
        [SerializeField] private float dashDuration = 1.5f;
        [SerializeField] private float baseMoveSpeed = 2.2f;

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
            Debug.Log($"[BullDashSkill] Boss kích hoạt chiêu 'Ngưu Xung Thiên' lao tới {targetPosition}!");

            Vector3 dashDirection = (targetPosition - transform.position).normalized;
            float elapsed = 0f;

            while (elapsed < dashDuration)
            {
                elapsed += Time.deltaTime;
                transform.position += dashDirection * (baseMoveSpeed * dashSpeedMultiplier) * Time.deltaTime;
                yield return null;
            }

            _isDashing = false;
        }
    }
}
