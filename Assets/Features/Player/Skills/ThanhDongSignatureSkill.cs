using System.Collections;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Collectibles;
using ProjectZombie.Core.Juice;

namespace ProjectZombie.Features.Player.Skills
{
    /// <summary>
    /// Kỹ năng Tuyệt Kỹ Thanh Đồng: "Giá Đồng Tứ Phủ" (Chuẩn hóa Action RPG v5.1).
    /// Thi triển trực tiếp khi nhấn nút HUD (Cooldown 30s).
    /// Hiệu ứng:
    /// 1. PHÁN TRUYỀN: Tạo sóng xung kích Tứ Phủ (Oracle Shockwave), gây 180% Sát thương và Choáng (Stun) toàn bộ quái trong 8.0m (2.5s).
    /// 2. BAN LỘC: Thu hút tức thời và liên tục toàn bộ ExpGem trên toàn màn hình.
    /// 3. HÀO QUANG THÁNH GIÁNG (5s): +35% Tốc độ chạy (kèm tàn ảnh ngọc lục), +35% Sát thương toàn thể, hồi phục 10% HP.
    /// 4. Camera Shake và Sound FX.
    /// </summary>
    public class ThanhDongSignatureSkill : SignatureSkillBase
    {
        public override float Cooldown => 30.0f;

        private readonly GameObject _auraPrefab;
        private readonly GameObject _shockwavePrefab;
        private const float DURATION = 5.0f;
        private const float STUN_RADIUS = 8.0f;
        private const float STUN_DURATION = 2.5f;
        private const float SPEED_BUFF_RATIO = 0.35f;
        private const float DAMAGE_BUFF_RATIO = 0.35f;
        private const float DAMAGE_RATIO = 1.8f; // 180% Base Damage

        private static readonly Collider2D[] _hitBuffer = new Collider2D[80];

        public ThanhDongSignatureSkill(GameObject auraPrefab = null, GameObject shockwavePrefab = null)
        {
            _auraPrefab = auraPrefab;
            _shockwavePrefab = shockwavePrefab;
        }

        public override void Execute(GameObject playerObj, System.Action<ElementType> onElementSelectedCallback = null)
        {
            if (playerObj == null) return;

            Vector3 spawnPos = playerObj.transform.position;

            // 1. Phán Truyền: Sóng xung kích làm choáng quái diện rộng & gây sát thương
            ExecuteOracleStunAndDamage(playerObj, spawnPos);

            // 2. Ban Lộc: Hút toàn bộ ExpGem trên sân đấu
            if (ExpGemPoolManager.Instance != null)
            {
                ExpGemPoolManager.Instance.CollectAllActiveGems(playerObj.transform);
            }

            // 3. Callback hệ Mộc
            onElementSelectedCallback?.Invoke(ElementType.Moc);

            // 4. Rung Camera
            GameJuiceEvents.RequestCameraShake(0.22f, 0.45f);

            // 5. Kích hoạt Coroutine Buff Hào Quang & Tốc chạy & Sát thương (5s)
            var playerMono = playerObj.GetComponent<MonoBehaviour>();
            if (playerMono != null)
            {
                playerMono.StartCoroutine(PossessionBuffRoutine(playerObj));
            }
        }

        private void ExecuteOracleStunAndDamage(GameObject playerObj, Vector3 center)
        {
            if (_shockwavePrefab != null)
            {
                Object.Instantiate(_shockwavePrefab, center, Quaternion.identity);
            }

            var stats = playerObj.GetComponent<PlayerStats>();
            float baseDmg = stats != null ? stats.GetTotalDamage() : 20f;
            float totalDamage = baseDmg * DAMAGE_RATIO;

            int mask = TargetingUtility.EnemyLayerMask;
            int count = Physics2D.OverlapCircleNonAlloc(center, STUN_RADIUS, _hitBuffer, mask);

            for (int i = 0; i < count; i++)
            {
                Collider2D col = _hitBuffer[i];
                if (col == null || col.gameObject == playerObj) continue;

                if (col.TryGetComponent<HealthSystem>(out var enemyHealth))
                {
                    DamageData dmg = new DamageData(totalDamage, false, ElementType.Moc, false);
                    enemyHealth.TakeDamage(dmg);
                }

                if (col.TryGetComponent<EnemyStatusController>(out var status))
                {
                    status.ApplyStatusEffect(StatusEffectType.Stun, STUN_DURATION);
                }

                if (col.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    Vector2 pushDir = ((Vector2)col.transform.position - (Vector2)center).normalized;
                    if (pushDir == Vector2.zero) pushDir = Vector2.up;
                    rb.AddForce(pushDir * 5.0f, ForceMode2D.Impulse);
                }
            }
        }

        private IEnumerator PossessionBuffRoutine(GameObject playerObj)
        {
            if (playerObj == null) yield break;

            var playerStats = playerObj.GetComponent<PlayerStats>();
            var health = playerObj.GetComponent<HealthSystem>();
            float speedBonus = 0f;
            float dmgBonus = DAMAGE_BUFF_RATIO;

            if (playerStats != null)
            {
                speedBonus = playerStats.MoveSpeed * SPEED_BUFF_RATIO;
                playerStats.AddMoveSpeed(speedBonus);
                playerStats.AddDamageMultiplier(dmgBonus);
            }

            // Hồi phục 10% Max HP khi thỉnh Thánh
            if (health != null)
            {
                health.Heal(health.MaxHealth * 0.10f);
            }

            // Kích hoạt Tàn Ảnh Tốc Độ Xanh Lục Tứ Phủ
            var dashVisuals = playerObj.GetComponent<Visuals.PlayerDashVisuals>();
            if (dashVisuals != null)
            {
                dashVisuals.StartSpeedBuffVisual(DURATION, new Color(0.2f, 0.95f, 0.45f, 0.65f));
            }

            GameObject auraObj = null;
            if (_auraPrefab != null)
            {
                auraObj = Object.Instantiate(_auraPrefab, playerObj.transform.position, Quaternion.identity, playerObj.transform);
            }

            float timer = DURATION;
            while (timer > 0f)
            {
                timer -= Time.deltaTime;

                // Liên tục hút gem trong thời gian giáng ngự
                if (ExpGemPoolManager.Instance != null && playerObj != null)
                {
                    ExpGemPoolManager.Instance.CollectAllActiveGems(playerObj.transform);
                }

                yield return null;
            }

            // Kết thúc hiệu ứng
            if (playerStats != null)
            {
                if (speedBonus > 0f) playerStats.AddMoveSpeed(-speedBonus);
                playerStats.AddDamageMultiplier(-dmgBonus);
            }

            if (auraObj != null)
            {
                Object.Destroy(auraObj);
            }
        }
    }
}
