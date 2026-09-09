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
            if (player == null) return false;
            var stats = player.GetComponent<PlayerStats>();
            if (stats == null) return false;

            var passives = player.GetComponent<PlayerPassives>();
            string key = !string.IsNullOrEmpty(id) ? id : upgradeName;
            if (passives != null && passives.HasPassive(key))
            {
                return false;
            }

            return true;
        }

        public override void ApplyUpgrade(GameObject player)
        {
            if (player == null) return;
            var stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                // Giảm cooldown dash, tăng tốc lướt và tăng crit
                if (dashCooldownReduction > 0f) stats.ReduceDashCooldown(dashCooldownReduction);
                if (dashSpeedBonus > 0f) stats.AddDashSpeedMultiplier(dashSpeedBonus);
                if (postDashCritBonus > 0f) stats.AddCritChance(postDashCritBonus);

                var passives = player.GetComponent<PlayerPassives>();
                string key = !string.IsNullOrEmpty(id) ? id : upgradeName;
                if (passives != null)
                {
                    passives.AddPassive(key, this);
                }

                Debug.Log($"<color=#00FF88>[DashTrait]</color> Đã cường hóa Lướt (Dash): Cooldown còn {stats.DashCooldown:F2}s, Tốc lướt x{stats.DashSpeedMultiplier:F2}, +{postDashCritBonus * 100}% Crit.");
            }
        }

        public override string GetCategoryDisplayName()
        {
            return "<color=#0E6073><b>[CƯỜNG HÓA LƯỚT]</b></color>";
        }

        public override string GetLevelDisplayName(GameObject player)
        {
            return "LƯỚT";
        }

        public override float GetDynamicWeightMultiplier(GameObject player)
        {
            return 2.5f; // Ưu tiên cao
        }
    }
}
