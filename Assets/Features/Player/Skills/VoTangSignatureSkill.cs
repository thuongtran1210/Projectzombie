using System.Collections;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.YinYang;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Core.Juice;

namespace ProjectZombie.Features.Player.Skills
{
    /// <summary>
    /// Kỹ năng Chủ động Ẩn Sĩ Sơn Lâm: "Thập Phương Chấn Thế" (GDD v5.1).
    /// Thi triển:
    /// 1. Dậm nát mặt đất giải phóng sóng địa chấn đất đá bùng nổ (Shockwave & Earth Impact).
    /// 2. Gây 320% Sát thương Hệ Thổ trong bán kính 7.0m.
    /// 3. Hất văng quái cực mạnh (10m/s) và làm Choáng (Stun) trong 2.0s.
    /// 4. HÓA THÂN BÀN THẠCH (4s): Hồi 15% HP, tăng +30% Sát thương & Miễn khống chế (kèm tàn ảnh nham thạch).
    /// 5. +25 điểm Dương vào bàn cân Âm Dương.
    /// 6. Rung Camera mạnh & Heavy SFX.
    /// </summary>
    public class VoTangSignatureSkill : SignatureSkillBase
    {
        public override float Cooldown => 20.0f;

        private readonly GameObject _shockwavePrefab;
        private readonly GameObject _earthImpactPrefab;

        private const float AOE_RADIUS = 7.0f;
        private const float DAMAGE_RATIO = 3.2f; // 320% Base Damage
        private const float STUN_DURATION = 2.0f;
        private const float KNOCKBACK_FORCE = 10.0f;
        private const float BUFF_DURATION = 4.0f;
        private const float DAMAGE_BUFF_RATIO = 0.30f;

        private static readonly Collider2D[] _hitBuffer = new Collider2D[80];

        public VoTangSignatureSkill(GameObject shockwavePrefab = null, GameObject earthImpactPrefab = null)
        {
            _shockwavePrefab = shockwavePrefab;
            _earthImpactPrefab = earthImpactPrefab;
        }

        public override void Execute(GameObject playerObj, System.Action<ElementType> onElementSelectedCallback = null)
        {
            if (playerObj == null) return;

            Vector3 center = playerObj.transform.position;

            // 1. Sinh Hiệu Ứng VFX Sóng Địa Chấn & Nứt Đất
            SpawnVisualEffects(center);

            // 2. Quét Sát Thương Diện Rộng, Hất Văng và Choáng quái
            ExecuteEarthquakeImpact(playerObj, center);

            // 3. Tác động Cán cân Âm Dương: Cộng thẳng +25 điểm Dương
            if (YinYangManager.Instance != null)
            {
                YinYangManager.Instance.AdjustValue(25f);
            }

            // 4. Callback hệ Thổ
            onElementSelectedCallback?.Invoke(ElementType.Tho);

            // 5. Rung Camera Mạnh
            GameJuiceEvents.RequestCameraShake(0.28f, 0.55f);

            // 6. Kích hoạt Coroutine Bàn Thạch Hộ Thể (4s)
            var playerMono = playerObj.GetComponent<MonoBehaviour>();
            if (playerMono != null)
            {
                playerMono.StartCoroutine(StoneBodyBuffRoutine(playerObj));
            }
        }

        private void SpawnVisualEffects(Vector3 center)
        {
            if (_shockwavePrefab != null)
            {
                Object.Instantiate(_shockwavePrefab, center, Quaternion.identity);
            }

            if (_earthImpactPrefab != null)
            {
                Object.Instantiate(_earthImpactPrefab, center, Quaternion.identity);
            }
        }

        private void ExecuteEarthquakeImpact(GameObject playerObj, Vector3 center)
        {
            var playerStats = playerObj.GetComponent<PlayerStats>();
            float baseDmg = playerStats != null ? playerStats.GetTotalDamage() : 20f;
            float totalDamage = baseDmg * DAMAGE_RATIO;

            int mask = TargetingUtility.EnemyLayerMask;
            int count = Physics2D.OverlapCircleNonAlloc(center, AOE_RADIUS, _hitBuffer, mask);

            for (int i = 0; i < count; i++)
            {
                Collider2D col = _hitBuffer[i];
                if (col == null || col.gameObject == playerObj) continue;

                if (col.TryGetComponent<HealthSystem>(out var enemyHealth))
                {
                    DamageData dmg = new DamageData(
                        totalDamage,
                        isCritical: false,
                        element: ElementType.Tho,
                        isCounter: false
                    );
                    enemyHealth.TakeDamage(dmg);
                }

                if (col.TryGetComponent<EnemyStatusController>(out var status))
                {
                    status.ApplyStatusEffect(StatusEffectType.Stun, STUN_DURATION);
                }

                if (col.TryGetComponent<Rigidbody2D>(out var enemyRb))
                {
                    Vector2 knockbackDir = ((Vector2)col.transform.position - (Vector2)center).normalized;
                    if (knockbackDir == Vector2.zero) knockbackDir = Vector2.up;
                    enemyRb.AddForce(knockbackDir * KNOCKBACK_FORCE, ForceMode2D.Impulse);
                }
            }
        }

        private IEnumerator StoneBodyBuffRoutine(GameObject playerObj)
        {
            if (playerObj == null) yield break;

            var playerStats = playerObj.GetComponent<PlayerStats>();
            var health = playerObj.GetComponent<HealthSystem>();

            // Hồi phục 15% Max HP (Khiên Bàn Thạch)
            if (health != null)
            {
                health.Heal(health.MaxHealth * 0.15f);
            }

            // Tăng sát thương trong 4s
            if (playerStats != null)
            {
                playerStats.AddDamageMultiplier(DAMAGE_BUFF_RATIO);
            }

            // Kích hoạt Tàn Ảnh Hổ Phách Nham Thạch
            var dashVisuals = playerObj.GetComponent<Visuals.PlayerDashVisuals>();
            if (dashVisuals != null)
            {
                dashVisuals.StartSpeedBuffVisual(BUFF_DURATION, new Color(0.95f, 0.55f, 0.1f, 0.7f));
            }

            yield return new WaitForSeconds(BUFF_DURATION);

            if (playerStats != null)
            {
                playerStats.AddDamageMultiplier(-DAMAGE_BUFF_RATIO);
            }
        }
    }
}
