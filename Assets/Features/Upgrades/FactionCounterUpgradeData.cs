using UnityEngine;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Upgrades
{
    /// <summary>
    /// Thẻ nâng cấp Khắc Chế Hệ (Faction Counter).
    /// Tăng sát thương khi đối đầu với phe địch cụ thể (VD: Zombie Nhiễm Độc, Zombie Giáp...).
    /// Bonus được lưu vào PlayerPassives dưới dạng key "faction_counter_{factionId}".
    /// Các FactionPassive_* trên Enemy sẽ đọc bonus này từ PlayerStats thông qua PlayerPassives.
    /// </summary>
    [CreateAssetMenu(fileName = "NewFactionCounterUpgrade", menuName = "ProjectZombie/Upgrades/Faction Counter Upgrade")]
    public class FactionCounterUpgradeData : UpgradeData
    {
        [Header("Faction Counter Settings")]
        [Tooltip("ID của phe địch bị khắc chế. Phải khớp với factionId trong Enemy (vd: 'Infected', 'Undead', 'Void').")]
        public string factionId;

        [Tooltip("% tăng sát thương thêm khi đánh vào phe này (0.2 = +20%).")]
        [Range(0f, 2f)]
        public float damageMultiplierBonus = 0.2f;

        [Tooltip("Cho phép stack nhiều lần (mỗi lần chọn lại cộng thêm bonus). Cần tạo thẻ riêng cho mỗi cấp stack nếu false.")]
        public bool isStackable = false;

        public override bool IsAvailable(GameObject player)
        {
            if (string.IsNullOrEmpty(factionId))
            {
                Debug.LogWarning("[FactionCounterUpgradeData] factionId chưa được thiết lập!");
                return false;
            }

            if (!isStackable)
            {
                // Nếu không stack, kiểm tra xem đã có passive này chưa
                var playerPassives = player.GetComponent<PlayerPassives>();
                if (playerPassives != null)
                {
                    return !playerPassives.HasPassive(GetPassiveKey());
                }
            }

            return true;
        }

        public override void ApplyUpgrade(GameObject player)
        {
            var playerPassives = player.GetComponent<PlayerPassives>();
            if (playerPassives == null) return;

            string key = GetPassiveKey();

            if (isStackable)
            {
                // Stack: cộng dồn bonus bằng counter
                playerPassives.IncrementUpgradeCount(key);
                float totalBonus = damageMultiplierBonus * playerPassives.GetUpgradeCount(key);
                Debug.Log($"[FactionCounter] Faction '{factionId}' — Tổng bonus sát thương: +{totalBonus * 100f:F0}%");
            }
            else
            {
                // Non-stack: chỉ thêm passive một lần
                playerPassives.AddPassive(key);
                Debug.Log($"[FactionCounter] Đã mở khóa khắc chế phe '{factionId}': +{damageMultiplierBonus * 100f:F0}% sát thương.");
            }
        }

        private string GetPassiveKey() => $"faction_counter_{factionId.ToLower()}";

        /// <summary>
        /// Hàm tiện ích để Enemy hoặc hệ thống sát thương tra cứu bonus của một faction.
        /// </summary>
        public static float GetFactionBonus(PlayerPassives playerPassives, string factionId, float bonusPerStack = 0.2f)
        {
            if (playerPassives == null || string.IsNullOrEmpty(factionId)) return 0f;

            string key = $"faction_counter_{factionId.ToLower()}";

            // Kiểm tra cả dạng non-stack (HasPassive) và stack (GetUpgradeCount)
            int stackCount = playerPassives.GetUpgradeCount(key);
            if (stackCount > 0)
            {
                return bonusPerStack * stackCount;
            }

            return playerPassives.HasPassive(key) ? bonusPerStack : 0f;
        }
    }
}
