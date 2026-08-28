using System.Collections;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.VFX.Indicators;
using ProjectZombie.Features.Boss;

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
        [SerializeField] private float telegraphDuration = 1.0f; // Thời gian báo vệt đỏ phình to
        [SerializeField] private LayerMask targetLayer;

        public void PerformGroundSlam()
        {
            StartCoroutine(SlamRoutine());
        }

        private IEnumerator SlamRoutine()
        {
            // BƯỚC 1: BÁO HỆU VỆT ĐỎ TRÒN PHÌNH DẦN
            bool telegraphFinished = false;
            if (SkillIndicatorManager.Instance != null)
            {
                SkillIndicatorManager.Instance.ShowIndicator(new IndicatorRequest(
                    IndicatorShape.Circle,
                    transform.position,
                    Vector3.zero,
                    new Vector2(slamRadius, slamRadius),
                    telegraphDuration,
                    new Color(1f, 0.2f, 0.2f, 0.4f)
                ), () => telegraphFinished = true);
            }
            else
            {
                yield return new WaitForSeconds(telegraphDuration);
                telegraphFinished = true;
            }

            while (!telegraphFinished)
            {
                yield return null;
            }

            // BƯỚC 2: TUNG ĐÒN GIẬM ĐẤT AOE
            Debug.Log("[GroundSlamSkill] Boss kích hoạt chiêu 'Địa Chấn Âm Ty'!");

            var bossAnimator = GetComponentInChildren<BossAnimator>();
            if (bossAnimator != null)
            {
                bossAnimator.PlayAnimation("GroundSlam");
            }

            StartCoroutine(SpawnShockwaveVFX(transform.position, slamRadius));

            global::Core.Audio.AudioManager.Instance?.PlayBossSmash(transform.position);

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

        private IEnumerator SpawnShockwaveVFX(Vector3 pos, float maxRadius)
        {
            GameObject vfxObj = new GameObject("GroundSlam_ShockwaveVFX");
            vfxObj.transform.position = pos;

            var sr = vfxObj.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.9f, 0.6f, 0.2f, 0.8f);
            sr.sortingOrder = 5;

            // Tạo Sprite vòng tròn mặc định
            Texture2D tex = new Texture2D(64, 64);
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f));
                    if (dist >= 24f && dist <= 31.5f)
                        tex.SetPixel(x, y, Color.white);
                    else
                        tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f);

            float elapsed = 0f;
            float duration = 0.45f;
            float maxDiameter = maxRadius * 2f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float currentScale = Mathf.Lerp(0.5f, maxDiameter, t);
                vfxObj.transform.localScale = new Vector3(currentScale, currentScale, 1f);

                Color c = sr.color;
                c.a = Mathf.Lerp(0.8f, 0f, t);
                sr.color = c;

                yield return null;
            }

            Destroy(vfxObj);
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

