using UnityEngine;
using ProjectZombie.Features.Weapons;
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
            var weaponManager = player.GetComponent<WeaponManager>();
            if (weaponManager == null) return false;

            // Nếu không chỉ định targetWeaponId thì áp dụng cho Vũ khí chính hiện tại
            var primary = weaponManager.PrimaryWeapon;
            if (primary == null) return false;

            if (!string.IsNullOrEmpty(targetWeaponId))
            {
                return string.Equals(primary.weaponId, targetWeaponId, System.StringComparison.OrdinalIgnoreCase);
            }

            return true;
        }

        public override void ApplyUpgrade(GameObject player)
        {
            var weaponManager = player.GetComponent<WeaponManager>();
            if (weaponManager == null) return;

            var primary = weaponManager.PrimaryWeapon;
            if (primary != null)
            {
                WeaponStatModifier mod = new WeaponStatModifier
                {
                    damageBonus = primary.GetDamage() * comboDamageMultiplierBonus,
                    attackSpeedBonus = attackSpeedBonus,
                    scaleBonus = slashAreaScaleBonus
                };

                primary.ApplyStatModifier(mod);
                weaponManager.NotifyWeaponsChanged();
                Debug.Log($"<color=#FFD700>[ComboAugment]</color> Đã nâng cấp Bí Kíp cho {primary.displayName}: +{comboDamageMultiplierBonus*100}% Dmg, +{slashAreaScaleBonus*100}% Range.");
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
