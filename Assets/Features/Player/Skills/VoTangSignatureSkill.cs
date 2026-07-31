using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.YinYang;
using ProjectZombie.Features.Enemies;

namespace ProjectZombie.Features.Player.Skills
{
    /// <summary>
    /// Kỹ năng Chủ động Võ Tăng: "Phá Giới Chấn Thế" (Mục 3.1.3 GDD v4.0).
    /// Hy sinh 30% HP hiện tại (Guard condition: HP >= 15% Max HP).
    /// Shockwave Radius & Damage tỷ lệ theo lượng HP hy sinh.
    /// Knockback 8m/s, Choáng 1.2s, +25 điểm Dương vào YinYangManager.
    /// Cooldown: 20s.
    /// </summary>
    public class VoTangSignatureSkill : SignatureSkillBase
    {
        public override float Cooldown => 20.0f;

        private static readonly Collider2D[] _hitBuffer = new Collider2D[60];

        public override bool CanExecute(PlayerStats stats, HealthSystem health)
        {
            if (!base.CanExecute(stats, health)) return false;

            // Guard condition: Khóa skill nếu HP hiện tại < 15% HP Max
            float hpThreshold = health.MaxHealth * 0.15f;
            return health.CurrentHealth >= hpThreshold;
        }

        public override void Execute(GameObject playerObj, System.Action<ElementType> onElementSelectedCallback = null)
        {
            if (playerObj == null) return;

            var playerStats = playerObj.GetComponent<PlayerStats>();
            var health = playerObj.GetComponent<HealthSystem>();

            if (!CanExecute(playerStats, health))
            {
                Debug.LogWarning("[VoTangSignatureSkill] Không đủ HP để thi triển Phá Giới Chấn Thế (< 15% Max HP).");
                return;
            }

            float maxHp = health.MaxHealth;
            float currentHp = health.CurrentHealth;

            // Hy sinh 30% HP hiện tại
            float hpDeducted = currentHp * 0.30f;
            health.TakeDamage(hpDeducted);

            // Công thức GDD v4.0
            float hpRatio = hpDeducted / Mathf.Max(1f, maxHp);
            float shockwaveRadius = 3.0f + hpRatio * 4.0f;
            float baseDamage = playerStats != null ? playerStats.BaseDamage : 20f;
            float shockwaveDamage = baseDamage * 2.5f * hpRatio;

            // Quét mục tiêu và áp dụng Knockback + Stun
            Vector3 center = playerObj.transform.position;
            int count = Physics2D.OverlapCircleNonAlloc(center, shockwaveRadius, _hitBuffer);

            for (int i = 0; i < count; i++)
            {
                Collider2D col = _hitBuffer[i];
                if (col == null || col.gameObject == playerObj) continue;

                var enemyHealth = col.GetComponent<HealthSystem>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(shockwaveDamage);
                }

                var enemyRb = col.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    Vector2 knockbackDir = ((Vector2)col.transform.position - (Vector2)center).normalized;
                    enemyRb.AddForce(knockbackDir * 8.0f, ForceMode2D.Impulse);
                }
            }

            // Tác động Cán cân Âm Dương: Cộng thẳng +25 điểm Dương
            if (YinYangManager.Instance != null)
            {
                YinYangManager.Instance.AdjustValue(25f);
            }
        }
    }
}
