using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades
{
    public enum FallbackRewardType
    {
        HealHealth,
        GrantGold
    }

    /// <summary>
    /// Thẻ nâng cấp dự phòng vĩnh cửu (Fallback Upgrade):
    /// Luôn luôn khả dụng, tự động xuất hiện khi người chơi đã Max Level toàn bộ hoặc pool thẻ cạn kiệt.
    /// </summary>
    [CreateAssetMenu(fileName = "NewFallbackRewardUpgradeData", menuName = "ProjectZombie/Upgrades/Fallback Reward Upgrade Data")]
    public class FallbackRewardUpgradeData : UpgradeData
    {
        [Header("Fallback Reward Settings")]
        public FallbackRewardType rewardType = FallbackRewardType.HealHealth;
        public float healPercentage = 0.40f; // 40% Max Health
        public int goldAmount = 150;

        public override bool IsAvailable(GameObject player)
        {
            // Thẻ Fallback luôn luôn khả dụng
            return true;
        }

        public override void ApplyUpgrade(GameObject player)
        {
            if (player == null) return;

            if (rewardType == FallbackRewardType.HealHealth)
            {
                var healthSystem = player.GetComponent<HealthSystem>() ?? player.GetComponentInParent<HealthSystem>();
                if (healthSystem != null)
                {
                    float amount = healthSystem.MaxHealth * healPercentage;
                    healthSystem.Heal(amount);
                    Debug.Log($"<color=#00FF88>[FallbackReward]</color> Nhận Tiên Đan Trị Thương, hồi phục {amount:F0} Máu!");
                }
            }
            else if (rewardType == FallbackRewardType.GrantGold)
            {
                Debug.Log($"<color=#FFD700>[FallbackReward]</color> Nhận Túi Vàng Phong Thủy: +{goldAmount} Vàng!");
            }
        }

        public override string GetCategoryDisplayName()
        {
            return "<color=#A33418><b>[THƯỞNG CỨU MỆNH]</b></color>";
        }

        public override string GetLevelDisplayName(GameObject player)
        {
            return "THƯỞNG";
        }

        public override float GetDynamicWeightMultiplier(GameObject player)
        {
            return 1.0f;
        }
    }
}
