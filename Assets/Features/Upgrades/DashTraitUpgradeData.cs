using UnityEngine;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Upgrades
{
    /// <summary>
    /// Thẻ nâng cấp Kỹ Năng Lướt (Dash Traits) trong Action RPG.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDashTraitUpgrade", menuName = "ProjectZombie/Upgrades/Action RPG/Dash Trait")]
    public class DashTraitUpgradeData : UpgradeData
    {
        [Header("Dash Trait Modifiers")]
        [Tooltip("% Giảm thời gian hồi lướt (0.25 = -25% Dash Cooldown)")]
        public float dashCooldownReduction = 0.25f;

        [Tooltip("% Tăng tốc độ lướt")]
        public float dashSpeedBonus = 0.3f;

        [Tooltip("Hiệu ứng phản đòn (Parry): Tăng % Crit Chance cho đòn đánh ngay sau khi lướt")]
        public float postDashCritBonus = 0.3f;

        public override bool IsAvailable(GameObject player)
        {
            var stats = player.GetComponent<PlayerStats>();
            return stats != null;
        }

        public override void ApplyUpgrade(GameObject player)
        {
            var stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                // Giảm cooldown dash và tăng crit
                stats.ReduceDashCooldown(dashCooldownReduction);
                stats.AddCritChance(postDashCritBonus);

                Debug.Log($"<color=#00FF88>[DashTrait]</color> Đã cường hóa Lướt (Dash): Cooldown còn {stats.DashCooldown:F2}s, +{postDashCritBonus*100}% Crit.");
            }
        }
    }
}
