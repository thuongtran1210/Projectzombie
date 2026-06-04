using UnityEngine;
using System.Collections.Generic;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Component hành vi va chạm: Gây sát thương liên tục lên các kẻ địch nằm trong vùng (có cooldown).
    /// </summary>
    [RequireComponent(typeof(ProjectileCore))]
    [RequireComponent(typeof(Collider2D))]
    public class Hit_Periodic : MonoBehaviour
    {
        [Header("Periodic Settings")]
        [Tooltip("Khoảng thời gian (giây) giữa 2 lần giật máu trên cùng 1 mục tiêu")]
        [SerializeField] private float hitCooldown = 0.5f;

        private ProjectileCore _core;
        private Dictionary<Collider2D, float> _lastHitTimes = new Dictionary<Collider2D, float>();

        private void Awake()
        {
            _core = GetComponent<ProjectileCore>();
        }

        private void OnEnable()
        {
            _lastHitTimes.Clear();
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.CompareTag("Enemy"))
            {
                // Kiểm tra xem đã hết thời gian hồi để giật máu tiếp chưa
                if (_lastHitTimes.TryGetValue(collision, out float lastHitTime))
                {
                    if (Time.time < lastHitTime + hitCooldown)
                    {
                        return; // Chưa hết cooldown, không gây sát thương
                    }
                }

                if (collision.TryGetComponent(out Shared.HealthSystem enemyHealth))
                {
                    enemyHealth.TakeDamage(_core.DamageData);
                    _lastHitTimes[collision] = Time.time;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            // Quên mục tiêu khi nó đi ra khỏi vùng, để lần sau chạm vào giật máu luôn
            if (collision.CompareTag("Enemy"))
            {
                _lastHitTimes.Remove(collision);
            }
        }
    }
}
