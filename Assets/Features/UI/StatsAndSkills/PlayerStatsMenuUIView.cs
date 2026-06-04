using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.UI.StatsAndSkills
{
    public class PlayerStatsMenuUIView : MonoBehaviour
    {
        [SerializeField] private StatUIEntry statEntryPrefab;
        [SerializeField] private Transform statsContainer;

        // Lưu trữ để dễ cập nhật lại giá trị mà không cần xoá/tạo lại
        private StatUIEntry damageEntry;
        private StatUIEntry speedEntry;
        private StatUIEntry critEntry;
        private StatUIEntry attackSpeedEntry;
        private StatUIEntry maxHealthEntry;
        private StatUIEntry dashCooldownEntry;
        private StatUIEntry pickupRangeEntry;
        private StatUIEntry expMultiplierEntry;

        private void Awake()
        {
            // Tạo sẵn các dòng chỉ số (có thể config bằng tay trên Editor thay vì code sinh ra)
            if (statEntryPrefab != null && statsContainer != null)
            {
                damageEntry = Instantiate(statEntryPrefab, statsContainer);
                speedEntry = Instantiate(statEntryPrefab, statsContainer);
                critEntry = Instantiate(statEntryPrefab, statsContainer);
                attackSpeedEntry = Instantiate(statEntryPrefab, statsContainer);
                maxHealthEntry = Instantiate(statEntryPrefab, statsContainer);
                dashCooldownEntry = Instantiate(statEntryPrefab, statsContainer);
                pickupRangeEntry = Instantiate(statEntryPrefab, statsContainer);
                expMultiplierEntry = Instantiate(statEntryPrefab, statsContainer);
            }
        }

        public void UpdateStats(PlayerStats stats)
        {
            if (damageEntry != null)
            {
                float val = stats.GetTotalDamage();
                string colorVal = RarityColorUtility.FormatText(val.ToString("F1"), GetDamageRarity(val));
                damageEntry.Setup("Damage", colorVal);
            }
            
            if (speedEntry != null)
            {
                float val = stats.MoveSpeed;
                string colorVal = RarityColorUtility.FormatText(val.ToString("F1"), GetSpeedRarity(val));
                speedEntry.Setup("Move Speed", colorVal);
            }
            
            if (critEntry != null)
            {
                float val = stats.CritChance;
                string colorVal = RarityColorUtility.FormatText((val * 100f).ToString("F1") + "%", GetCritRarity(val));
                critEntry.Setup("Crit Chance", colorVal);
            }
                
            if (attackSpeedEntry != null)
            {
                float val = stats.AttackSpeed;
                string colorVal = RarityColorUtility.FormatText(val.ToString("F2"), GetAttackSpeedRarity(val));
                attackSpeedEntry.Setup("Attack Speed", colorVal);
            }
                
            if (maxHealthEntry != null)
            {
                float val = stats.MaxHealth;
                string colorVal = RarityColorUtility.FormatText(val.ToString("F0"), GetHealthRarity(val));
                maxHealthEntry.Setup("Max Health", colorVal);
            }
                
            if (dashCooldownEntry != null)
            {
                float val = stats.DashCooldown;
                // Dash cooldown tốt hơn khi giá trị càng nhỏ
                string colorVal = RarityColorUtility.FormatText(val.ToString("F1") + "s", GetDashCooldownRarity(val));
                dashCooldownEntry.Setup("Dash Cooldown", colorVal);
            }

            if (pickupRangeEntry != null)
            {
                float val = stats.PickupRange;
                string colorVal = RarityColorUtility.FormatText(val.ToString("F1"), GetPickupRangeRarity(val));
                pickupRangeEntry.Setup("Pickup Range", colorVal);
            }

            if (expMultiplierEntry != null)
            {
                float val = stats.ExpMultiplier;
                string colorVal = RarityColorUtility.FormatText((val * 100f).ToString("F0") + "%", GetExpRarity(val));
                expMultiplierEntry.Setup("Exp Bonus", colorVal);
            }
        }

        #region Thresholds - Điều kiện Rarity
        private Rarity GetDamageRarity(float value)
        {
            if (value >= 200f) return Rarity.Mythic;
            if (value >= 100f) return Rarity.Legendary;
            if (value >= 50f) return Rarity.Epic;
            if (value >= 30f) return Rarity.Rare;
            if (value >= 15f) return Rarity.Uncommon;
            return Rarity.Common;
        }

        private Rarity GetSpeedRarity(float value)
        {
            if (value >= 10f) return Rarity.Mythic;
            if (value >= 8f) return Rarity.Legendary;
            if (value >= 6f) return Rarity.Epic;
            if (value >= 5f) return Rarity.Rare;
            if (value >= 4f) return Rarity.Uncommon;
            return Rarity.Common;
        }

        private Rarity GetCritRarity(float value)
        {
            if (value >= 0.5f) return Rarity.Mythic;
            if (value >= 0.3f) return Rarity.Legendary;
            if (value >= 0.2f) return Rarity.Epic;
            if (value >= 0.1f) return Rarity.Rare;
            if (value >= 0.05f) return Rarity.Uncommon;
            return Rarity.Common;
        }

        private Rarity GetAttackSpeedRarity(float value)
        {
            if (value >= 3f) return Rarity.Mythic;
            if (value >= 2f) return Rarity.Legendary;
            if (value >= 1.5f) return Rarity.Epic;
            if (value >= 1.2f) return Rarity.Rare;
            if (value >= 1.1f) return Rarity.Uncommon;
            return Rarity.Common;
        }

        private Rarity GetHealthRarity(float value)
        {
            if (value >= 500f) return Rarity.Mythic;
            if (value >= 300f) return Rarity.Legendary;
            if (value >= 200f) return Rarity.Epic;
            if (value >= 150f) return Rarity.Rare;
            if (value >= 120f) return Rarity.Uncommon;
            return Rarity.Common;
        }

        private Rarity GetDashCooldownRarity(float value)
        {
            // Cooldown nhỏ thì càng xịn
            if (value <= 0.5f) return Rarity.Mythic;
            if (value <= 1.0f) return Rarity.Legendary;
            if (value <= 1.5f) return Rarity.Epic;
            if (value <= 2.0f) return Rarity.Rare;
            if (value <= 2.5f) return Rarity.Uncommon;
            return Rarity.Common;
        }

        private Rarity GetPickupRangeRarity(float value)
        {
            if (value >= 10f) return Rarity.Mythic;
            if (value >= 7f) return Rarity.Legendary;
            if (value >= 5f) return Rarity.Epic;
            if (value >= 3f) return Rarity.Rare;
            if (value >= 2f) return Rarity.Uncommon;
            return Rarity.Common;
        }

        private Rarity GetExpRarity(float value)
        {
            if (value >= 3f) return Rarity.Mythic;
            if (value >= 2f) return Rarity.Legendary;
            if (value >= 1.5f) return Rarity.Epic;
            if (value >= 1.2f) return Rarity.Rare;
            if (value >= 1.1f) return Rarity.Uncommon;
            return Rarity.Common;
        }
        #endregion
    }
}
