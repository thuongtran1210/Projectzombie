using UnityEngine;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Player;
using System.Linq;

namespace ProjectZombie.Features.Upgrades
{
    /// <summary>
    /// Thẻ nâng cấp Bí Kíp Đòn Chém (Combo Augments) cho Vũ Khí Chính trong Action RPG.
    /// </summary>
    [CreateAssetMenu(fileName = "NewComboAugmentUpgrade", menuName = "ProjectZombie/Upgrades/Action RPG/Combo Augment")]
    public class ComboAugmentUpgradeData : UpgradeData
    {
        [Header("Combo Augment Settings")]
        [Tooltip("ID của vũ khí áp dụng (VD: W002_ButPhanQuan, W_SWORD)")]
        public string targetWeaponId;

        [Header("Combo Combat Modifiers")]
        [Tooltip("% Tăng sát thương chuỗi Combo (0.2 = +20%)")]
        public float comboDamageMultiplierBonus = 0.2f;

        [Tooltip("% Tăng tốc độ vung đòn giữa các nhát chém")]
        public float attackSpeedBonus = 0.15f;

        [Tooltip("% Mở rộng phạm vi quét của nhát chém")]
        public float slashAreaScaleBonus = 0.25f;

        [Tooltip("Lực đẩy lùi tăng thêm cho đòn kết liễu thứ 3")]
        public float finisherKnockbackBonus = 2.0f;

        public override bool IsAvailable(GameObject player)
        {
            if (player == null) return false;
            var combat = player.GetComponent<CharacterCombat>();
            if (combat == null) return false;

            // Kiểm tra nếu đã nhận Bí Kíp này rồi thì không xuất hiện lại
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
            var combat = player.GetComponent<CharacterCombat>();
            if (combat != null)
            {
                combat.AddComboDamageBonus(comboDamageMultiplierBonus);
                combat.AddAttackSpeedBonus(attackSpeedBonus);
                combat.AddSlashAreaScaleBonus(slashAreaScaleBonus);
                combat.AddFinisherKnockbackBonus(finisherKnockbackBonus);

                var passives = player.GetComponent<PlayerPassives>();
                string key = !string.IsNullOrEmpty(id) ? id : upgradeName;
                if (passives != null)
                {
                    passives.AddPassive(key, this);
                }

                Debug.Log($"<color=#FFD700>[ComboAugment]</color> Đã nâng cấp Bí Kíp {upgradeName}: +{comboDamageMultiplierBonus * 100}% Dmg, +{slashAreaScaleBonus * 100}% Range, +{attackSpeedBonus * 100}% Speed.");
            }
        }

        public override string GetCategoryDisplayName()
        {
            return "<color=#B85D00><b>[BÍ KÍP ĐÒN CHÉM]</b></color>";
        }

        public override string GetLevelDisplayName(GameObject player)
        {
            return "BÍ KÍP";
        }

        public override float GetDynamicWeightMultiplier(GameObject player)
        {
            return 3.0f; // Siêu ưu tiên cho vũ khí chính
        }
    }
}
