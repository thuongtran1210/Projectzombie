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
            if (player == null) return false;
            var exp = player.GetComponent<PlayerExperience>();
            if (exp == null || exp.CurrentLevel < requiredPlayerLevel) return false;

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
                if (allDamageMultiplier > 0f) stats.AddDamageMultiplier(allDamageMultiplier);
                if (relicScaleMultiplier > 0f) stats.AddAreaScale(relicScaleMultiplier);
                if (executeHealthThreshold > 0f) stats.SetExecuteThreshold(executeHealthThreshold);
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

            var passives = player.GetComponent<PlayerPassives>();
            string key = !string.IsNullOrEmpty(id) ? id : upgradeName;
            if (passives != null)
            {
                passives.AddPassive(key, this);
            }

            Debug.Log($"<color=#FF00FF>[Breakthrough]</color> ĐÃ ĐỘT PHÁ TUYỆT KỸ {upgradeName}: +{allDamageMultiplier * 100}% Damage, +{relicScaleMultiplier * 100}% Pháp Bảo Hộ Thân, Kết liễu quái dưới {executeHealthThreshold * 100}% HP!");
        }

        public override string GetCategoryDisplayName()
        {
            return "<color=#6B2D82><b>[ĐỘT PHÁ TUYỆT KỸ]</b></color>";
        }

        public override string GetLevelDisplayName(GameObject player)
        {
            return "ĐỘT PHÁ";
        }

        public override float GetDynamicWeightMultiplier(GameObject player)
        {
            return 6.0f; // Cực kỳ ưu tiên khi đủ điều kiện level
        }
    }
}
