using UnityEngine;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Upgrades
{
    /// <summary>
    /// Thẻ Bí Tịch Đột Phá Tuyệt Kỹ (Breakthrough Ultimates) xuất hiện tại các cột mốc Level 5 & Level 10.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBreakthroughUpgrade", menuName = "ProjectZombie/Upgrades/Action RPG/Breakthrough Ultimate")]
    public class BreakthroughUpgradeData : UpgradeData
    {
        [Header("Breakthrough Milestones")]
        [Tooltip("Cấp độ Player tối thiểu yêu cầu (VD: Level 5 hoặc Level 10)")]
        public int requiredPlayerLevel = 5;

        [Header("Ultimate Combat Modifiers")]
        [Tooltip("% Tăng sát thương toàn diện")]
        public float allDamageMultiplier = 0.5f;

        [Tooltip("% Tăng quy mô và bán kính bảo vệ của tất cả Pháp bảo")]
        public float relicScaleMultiplier = 0.5f;

        [Tooltip("Tỷ lệ kết liễu quái thường máu yếu")]
        public float executeHealthThreshold = 0.15f;

        public override bool IsAvailable(GameObject player)
        {
            var exp = player.GetComponent<PlayerExperience>();
            if (exp == null) return false;

            return exp.CurrentLevel >= requiredPlayerLevel;
        }

        public override void ApplyUpgrade(GameObject player)
        {
            var stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.AddDamageMultiplier(allDamageMultiplier);
            }

            var weaponManager = player.GetComponent<WeaponManager>();
            if (weaponManager != null)
            {
                foreach (var relic in weaponManager.RelicWeapons)
                {
                    WeaponStatModifier mod = new WeaponStatModifier
                    {
                        scaleBonus = relicScaleMultiplier,
                        damageBonus = relic.GetDamage() * 0.3f
                    };
                    relic.ApplyStatModifier(mod);
                }
                weaponManager.NotifyWeaponsChanged();
            }

            Debug.Log($"<color=#FF00FF>[Breakthrough]</color> ĐÃ ĐỘT PHÁ TUYỆT KỸ: +{allDamageMultiplier*100}% Damage, +{relicScaleMultiplier*100}% Pháp Bảo Hộ Thân!");
        }
    }
}
