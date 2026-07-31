using System.Collections;
using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Enemies.Boss.Skills
{
    /// <summary>
    /// Skill Địa Chấn Âm Ty (Ground Slam AoE Slow 40%) của Boss Ngưu Đầu Mã Diện theo GDD 5.2.
    /// Dậm đất tạo sóng xung kích bán kính 5m gây 30 Sát thương và Slow 40% người chơi trong 3 giây.
    /// </summary>
    public class GroundSlamSkill : MonoBehaviour
    {
        [Header("Slam Settings")]
        [SerializeField] private float slamRadius = 5.0f;
        [SerializeField] private float slamDamage = 30f;
        [SerializeField] private float slowPercentage = 0.40f;
        [SerializeField] private float slowDuration = 3.0f;
        [SerializeField] private LayerMask targetLayer;

        public void PerformGroundSlam()
        {
            StartCoroutine(SlamRoutine());
        }

        private IEnumerator SlamRoutine()
        {
            Debug.Log("[GroundSlamSkill] Boss kích hoạt chiêu 'Địa Chấn Âm Ty'!");
            yield return new WaitForSeconds(0.3f); // Delay gồng chiêu

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, slamRadius, targetLayer);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    var health = hit.GetComponent<HealthSystem>();
                    if (health != null)
                    {
                        health.TakeDamage(new DamageData(slamDamage, false, ElementType.Tho));
                    }

                    var playerController = hit.GetComponent<ProjectZombie.Features.Player.PlayerController>();
                    if (playerController != null)
                    {
                        playerController.ApplySlow(slowPercentage, slowDuration);
                    }
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, slamRadius);
        }
#endif
    }
}
