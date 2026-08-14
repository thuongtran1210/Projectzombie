using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades
{
    /// <summary>
    /// Thẻ nâng cấp Tăng Sát Thương Ngũ Hành (Element Counter / Bonus).
    /// Tăng sát thương thuộc tính Ngũ Hành cụ thể (Kim, Mộc, Thủy, Hỏa, Thổ).
    /// Bonus được lưu vào PlayerPassives dưới dạng key "element_bonus_{element}".
    /// </summary>
    [CreateAssetMenu(fileName = "NewElementCounterUpgrade", menuName = "ProjectZombie/Upgrades/Element Counter Upgrade")]
    public class FactionCounterUpgradeData : UpgradeData
    {
        [Header("Element Bonus Settings")]
        [Tooltip("Hệ Ngũ Hành được tăng sát thương.")]
        public ElementType targetElement = ElementType.None;

        [Tooltip("% tăng sát thương thêm cho hệ này (0.2 = +20%).")]
        [Range(0f, 2f)]
        public float damageMultiplierBonus = 0.2f;

        [Tooltip("Cho phép stack nhiều lần (mỗi lần chọn lại cộng thêm bonus).")]
        public bool isStackable = false;

        public override bool IsAvailable(GameObject player)
        {
            if (targetElement == ElementType.None)
            {
                // Fallback nếu thẻ vẫn dùng thuộc tính element từ UpgradeData gốc
                if (element != ElementType.None)
                {
                    targetElement = element;
                }
                else
                {
                    Debug.LogWarning("[FactionCounterUpgradeData] targetElement chưa được thiết lập!");
                    return false;
                }
            }

            if (!isStackable)
            {
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

            if (targetElement == ElementType.None && element != ElementType.None)
            {
                targetElement = element;
            }

            string key = GetPassiveKey();

            if (isStackable)
            {
                playerPassives.AddPassive(key, this);
                playerPassives.IncrementUpgradeCount(key);
                float totalBonus = damageMultiplierBonus * playerPassives.GetUpgradeCount(key);
                Debug.Log($"[ElementBonus] Ngũ Hành '{targetElement}' — Tổng bonus sát thương: +{totalBonus * 100f:F0}%");
            }
            else
            {
                playerPassives.AddPassive(key, this);
                Debug.Log($"[ElementBonus] Đã mở khóa tăng sát thương hệ '{targetElement}': +{damageMultiplierBonus * 100f:F0}% sát thương.");
            }
        }

        private string GetPassiveKey() => $"element_bonus_{targetElement.ToString().ToLower()}";

        /// <summary>
        /// Tra cứu bonus sát thương của một hệ Ngũ Hành từ PlayerPassives.
        /// </summary>
        public static float GetElementBonus(PlayerPassives playerPassives, ElementType elementType, float bonusPerStack = 0.2f)
        {
            if (playerPassives == null || elementType == ElementType.None) return 0f;

            string key = $"element_bonus_{elementType.ToString().ToLower()}";

            int stackCount = playerPassives.GetUpgradeCount(key);
            if (stackCount > 0)
            {
                return bonusPerStack * stackCount;
            }

            return playerPassives.HasPassive(key) ? bonusPerStack : 0f;
        }
    }
}

