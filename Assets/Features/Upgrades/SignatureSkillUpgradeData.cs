using UnityEngine;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Upgrades
{
    /// <summary>
    /// Thẻ nâng cấp Signature Skill của nhân vật.
    /// Mỗi cấp nâng cấp sẽ giảm cooldown hoặc tăng sát thương Skill đặc trưng.
    /// Mỗi nhân vật chỉ có 1 Signature Skill, thẻ này có thể xuất hiện nhiều lần (1 lần/cấp).
    /// </summary>
    [CreateAssetMenu(fileName = "NewSignatureSkillUpgrade", menuName = "ProjectZombie/Upgrades/Signature Skill Upgrade")]
    public class SignatureSkillUpgradeData : UpgradeData
    {
        [Header("Signature Skill Settings")]
        [Tooltip("Lượng giảm cooldown (giây) mỗi lần chọn thẻ này. Giá trị âm = giảm cooldown.")]
        public float cooldownReduction = 10f;

        [Tooltip("Lượng tăng sát thương của Skill.")]
        public float damageBonus = 50f;

        [Tooltip("Số lần thẻ này có thể được chọn tối đa (0 = không giới hạn).")]
        public int maxUpgradeCount = 3;

        [Header("Required Component")]
        [Tooltip("Tên class MonoBehaviour của Signature Skill cần được nâng cấp. Phải khớp chính xác.")]
        public string signatureSkillTypeName = "ViralBurstUltimate";

        public override bool IsAvailable(GameObject player)
        {
            // Kiểm tra player có component Skill tương ứng không
            var skill = player.GetComponent(signatureSkillTypeName);
            if (skill == null) return false;

            // Kiểm tra giới hạn số lần nâng cấp
            if (maxUpgradeCount > 0)
            {
                var playerPassives = player.GetComponent<PlayerPassives>();
                if (playerPassives != null)
                {
                    string trackingKey = $"sig_skill_upgrade_{upgradeName}";
                    int count = playerPassives.GetUpgradeCount(trackingKey);
                    return count < maxUpgradeCount;
                }
            }

            return true;
        }

        public override void ApplyUpgrade(GameObject player)
        {
            // Tìm và áp dụng buff cho ViralBurstUltimate (hoặc bất kỳ ISignatureSkill nào)
            var ultimate = player.GetComponent<ViralBurstUltimate>();
            if (ultimate != null)
            {
                ultimate.ReduceCooldown(cooldownReduction);
                ultimate.AddDamage(damageBonus);
            }

            // Track số lần đã nâng cấp qua PlayerPassives
            var playerPassives = player.GetComponent<PlayerPassives>();
            if (playerPassives != null)
            {
                string trackingKey = $"sig_skill_upgrade_{upgradeName}";
                playerPassives.IncrementUpgradeCount(trackingKey);
            }

            Debug.Log($"[SignatureSkillUpgrade] Áp dụng nâng cấp Skill: -{cooldownReduction}s cooldown, +{damageBonus} damage.");
        }
    }
}
