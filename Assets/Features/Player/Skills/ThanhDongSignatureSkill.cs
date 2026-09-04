using System.Collections;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Collectibles;

namespace ProjectZombie.Features.Player.Skills
{
    /// <summary>
    /// Kỹ năng Tuyệt Kỹ Thanh Đồng: "Giá Đồng Tứ Phủ" (Chuẩn hóa Action RPG v5.1).
    /// Thi triển trực tiếp khi nhấn nút HUD (Cooldown 30s).
    /// Hiệu ứng trong 5s:
    /// 1. PHÁN TRUYỀN: Tạo sóng xung kích Choáng (Stun) toàn bộ quái xung quanh trong 2.0s.
    /// 2. BAN LỘC: Thu hút toàn bộ ExpGem trên toàn màn hình.
    /// 3. HÀO QUANG THÁNH GIÁNG: +30% Tốc độ chạy & +30% Sát thương toàn thể trong 5 giây.
    /// </summary>
    public class ThanhDongSignatureSkill : SignatureSkillBase
    {
        public override float Cooldown => 30.0f;

        private GameObject _auraPrefab;
        private GameObject _shockwavePrefab;
        private const float DURATION = 5.0f;
        private const float STUN_RADIUS = 10.0f;
        private const float STUN_DURATION = 2.0f;
        private const float SPEED_BUFF_RATIO = 0.30f;

        public ThanhDongSignatureSkill(GameObject auraPrefab = null, GameObject shockwavePrefab = null)
        {
            _auraPrefab = auraPrefab;
            _shockwavePrefab = shockwavePrefab;
        }

        public override void Execute(GameObject playerObj, System.Action<ElementType> onElementSelectedCallback = null)
        {
            if (playerObj == null) return;

            Vector3 spawnPos = playerObj.transform.position;

            // 1. Phán Truyền: Sóng xung kích làm choáng quái diện rộng
            ExecuteOracleStun(spawnPos);

            // 2. Ban Lộc: Hút toàn bộ ExpGem trên sân đấu
            if (ExpGemPoolManager.Instance != null)
            {
                ExpGemPoolManager.Instance.CollectAllActiveGems(playerObj.transform);
            }

            // 3. Callback hệ Mộc
            onElementSelectedCallback?.Invoke(ElementType.Moc);

            // 4. Kích hoạt Coroutine Buff Hào Quang & Tốc chạy
            var playerMono = playerObj.GetComponent<MonoBehaviour>();
            if (playerMono != null)
            {
                playerMono.StartCoroutine(PossessionBuffRoutine(playerObj));
            }
        }

        private void ExecuteOracleStun(Vector3 center)
        {
            if (_shockwavePrefab != null)
            {
                Object.Instantiate(_shockwavePrefab, center, Quaternion.identity);
            }

            Collider2D[] hits = Physics2D.OverlapCircleAll(center, STUN_RADIUS);
            foreach (var col in hits)
            {
                if (col.CompareTag("Enemy"))
                {
                    var status = col.GetComponent<EnemyStatusController>();
                    if (status != null)
                    {
                        status.ApplyStatusEffect(StatusEffectType.Stun, STUN_DURATION);
                    }
                }
            }
        }

        private IEnumerator PossessionBuffRoutine(GameObject playerObj)
        {
            if (playerObj == null) yield break;

            var playerStats = playerObj.GetComponent<PlayerStats>();
            float speedBonus = 0f;

            if (playerStats != null)
            {
                speedBonus = playerStats.MoveSpeed * SPEED_BUFF_RATIO;
                playerStats.AddMoveSpeed(speedBonus);
            }

            // Kích hoạt Tàn Ảnh Tốc Độ Xanh Lục Tứ Phủ
            var dashVisuals = playerObj.GetComponent<Visuals.PlayerDashVisuals>();
            if (dashVisuals != null)
            {
                dashVisuals.StartSpeedBuffVisual(DURATION, new Color(0.2f, 0.9f, 0.4f, 0.55f));
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
            if (playerStats != null && speedBonus > 0f)
            {
                playerStats.AddMoveSpeed(-speedBonus);
            }

            if (auraObj != null)
            {
                Object.Destroy(auraObj);
            }
        }
    }
}
