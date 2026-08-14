using System.Text;
using ProjectZombie.Features.Upgrades;

namespace ProjectZombie.Features.UI.Formatters
{
    /// <summary>
    /// Triển khai chuyển đổi các chỉ số trong WeaponStatModifier và PlayerStatModifier sang định dạng TMP Rich Text tiếng Việt trực quan.
    /// </summary>
    public class UpgradeStatFormatter : IUpgradeStatFormatter
    {
        private readonly StringBuilder _sb = new StringBuilder(128);

        private const string POSITIVE_COLOR = "#4DEEEA"; // Xanh Ngọc Bích cho chỉ số tăng
        private const string CRIT_COLOR = "#FFD700";     // Hoàng Kim cho Chí Mạng / Đột Biến
        private const string PIERCE_COLOR = "#FFA726";   // Cam Sáng cho Xuyên Thấu / Đạn

        public string FormatStatDiff(UpgradeData upgradeData)
        {
            if (upgradeData == null) return string.Empty;

            _sb.Clear();

            if (upgradeData is WeaponUpgradeData weaponData)
            {
                FormatWeaponStatDiff(weaponData.statModifier);
            }
            else if (upgradeData is CommonUpgradeData commonData)
            {
                FormatPlayerStatDiff(commonData.playerStatModifier);
            }

            return _sb.ToString().TrimEnd();
        }

        private void FormatWeaponStatDiff(WeaponStatModifier modifier)
        {
            if (modifier.damageBonus != 0)
            {
                AppendStatLine("Sát Thương", modifier.damageBonus * 100f, "%", POSITIVE_COLOR);
            }

            if (modifier.attackSpeedBonus != 0)
            {
                AppendStatLine("Tốc Đánh", modifier.attackSpeedBonus * 100f, "%", POSITIVE_COLOR);
            }

            if (modifier.projectileCountBonus != 0)
            {
                AppendStatLine("Số Tia Đạn", modifier.projectileCountBonus, "", PIERCE_COLOR);
            }

            if (modifier.pierceBonus != 0)
            {
                AppendStatLine("Xuyên Thấu", modifier.pierceBonus, " mục tiêu", PIERCE_COLOR);
            }

            if (modifier.scaleBonus != 0)
            {
                AppendStatLine("Phạm Vi/Kích Thước", modifier.scaleBonus * 100f, "%", POSITIVE_COLOR);
            }

            if (modifier.critChanceBonus != 0)
            {
                AppendStatLine("Tỉ Lệ Chí Mạng", modifier.critChanceBonus * 100f, "%", CRIT_COLOR);
            }

            if (modifier.critDamageBonus != 0)
            {
                AppendStatLine("Sát Thương Chí Mạng", modifier.critDamageBonus * 100f, "%", CRIT_COLOR);
            }

            if (modifier.projectileSpeedBonus != 0)
            {
                AppendStatLine("Tốc Độ Đạn", modifier.projectileSpeedBonus * 100f, "%", POSITIVE_COLOR);
            }
        }

        private void FormatPlayerStatDiff(PlayerStatModifier modifier)
        {
            if (modifier.maxHealthBonus != 0)
            {
                AppendStatLine("Máu Tối Đa", modifier.maxHealthBonus, "", POSITIVE_COLOR);
            }

            if (modifier.moveSpeedBonus != 0)
            {
                AppendStatLine("Tốc Độ Di Chuyển", modifier.moveSpeedBonus * 100f, "%", POSITIVE_COLOR);
            }

            if (modifier.baseDamageBonus != 0)
            {
                AppendStatLine("Sát Thương Gốc", modifier.baseDamageBonus * 100f, "%", POSITIVE_COLOR);
            }

            if (modifier.critChanceBonus != 0)
            {
                AppendStatLine("Tỉ Lệ Chí Mạng", modifier.critChanceBonus * 100f, "%", CRIT_COLOR);
            }

            if (modifier.pickupRangeBonus != 0)
            {
                AppendStatLine("Bán Kính Nhặt", modifier.pickupRangeBonus * 100f, "%", POSITIVE_COLOR);
            }

            if (modifier.expMultiplierBonus != 0)
            {
                AppendStatLine("Hấp Thu EXP", modifier.expMultiplierBonus * 100f, "%", CRIT_COLOR);
            }
        }

        private void AppendStatLine(string label, float value, string unit, string hexColor)
        {
            string sign = value > 0 ? "+" : "";
            _sb.Append($"<color={hexColor}>{sign}{value:0.#}{unit}</color> {label}\n");
        }

        private void AppendStatLine(string label, int value, string unit, string hexColor)
        {
            string sign = value > 0 ? "+" : "";
            _sb.Append($"<color={hexColor}>{sign}{value}{unit}</color> {label}\n");
        }
    }
}
