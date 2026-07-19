using UnityEngine;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Upgrades
{
    /// <summary>
    /// Thẻ nâng cấp Hiếm (Rare Upgrade).
    /// Mang lại chỉ số đột biến lớn hơn nhiều so với CommonUpgrade.
    /// Mặc định chỉ xuất hiện 1 lần (isOneTimeOnly = true).
    /// spawnWeight nên được đặt thấp (0.1 – 0.4) trong Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "NewRareUpgrade", menuName = "ProjectZombie/Upgrades/Rare Upgrade")]
    public class RareUpgradeData : UpgradeData
    {
        [Header("Rare Upgrade Settings")]
        [Tooltip("Bộ chỉ số nâng cấp (thường cao hơn CommonUpgrade nhiều lần).")]
        public PlayerStatModifier statModifier;

        [Tooltip("Nếu true, thẻ sẽ biến mất khỏi pool sau khi được chọn một lần.")]
        public bool isOneTimeOnly = true;

        private void OnEnable()
        {
            // Đảm bảo Rare Upgrade có trọng số thấp theo mặc định
            // (Chỉ áp dụng khi tạo SO mới, không override giá trị đã set)
            if (spawnWeight > 0.5f)
            {
                spawnWeight = 0.3f;
            }
            upgradeType = UpgradeType.RareUpgrade;
        }

        public override bool IsAvailable(GameObject player)
        {
            if (!isOneTimeOnly) return true;

            // Kiểm tra xem thẻ này đã được chọn chưa (dùng upgradeName làm key)
            var playerPassives = player.GetComponent<PlayerPassives>();
            if (playerPassives != null)
            {
                return !playerPassives.HasPassive(GetOneTimeKey());
            }

            return true;
        }

        public override void ApplyUpgrade(GameObject player)
        {
            var playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                if (statModifier.maxHealthBonus != 0f)
                    playerStats.AddMaxHealth(statModifier.maxHealthBonus);
                if (statModifier.moveSpeedBonus != 0f)
                    playerStats.AddMoveSpeed(statModifier.moveSpeedBonus);
                if (statModifier.critChanceBonus != 0f)
                    playerStats.AddCritChance(statModifier.critChanceBonus);
                if (statModifier.baseDamageBonus != 0f)
                    playerStats.AddBaseDamage(statModifier.baseDamageBonus);
                if (statModifier.pickupRangeBonus != 0f)
                    playerStats.AddPickupRange(statModifier.pickupRangeBonus);
                if (statModifier.expMultiplierBonus != 0f)
                    playerStats.AddExpMultiplier(statModifier.expMultiplierBonus);
            }

            // Đánh dấu đã được chọn (ngăn xuất hiện lại nếu isOneTimeOnly)
            if (isOneTimeOnly)
            {
                var playerPassives = player.GetComponent<PlayerPassives>();
                playerPassives?.AddPassive(GetOneTimeKey());
            }

            Debug.Log($"[RareUpgrade] Áp dụng nâng cấp hiếm: '{upgradeName}'");
        }

        private string GetOneTimeKey() => $"rare_upgrade_obtained_{upgradeName}";
    }
}
