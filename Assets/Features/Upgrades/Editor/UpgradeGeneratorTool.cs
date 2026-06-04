using UnityEngine;
using UnityEditor;
using System.IO;
using ProjectZombie.Features.Upgrades;

namespace ProjectZombie.Features.Upgrades.Editor
{
    public class UpgradeGeneratorTool
    {
        private const string FOLDER_PATH = "Assets/_Data/Upgrades/Samples";

        [MenuItem("Tools/ProjectZombie/Generate Sample Upgrades")]
        public static void GenerateSamples()
        {
            // Đảm bảo thư mục tồn tại
            if (!Directory.Exists(FOLDER_PATH))
            {
                Directory.CreateDirectory(FOLDER_PATH);
                AssetDatabase.Refresh();
            }

            // 1. Thẻ Tăng Tốc Độ Chạy (Common)
            CreateUpgrade("Giày Siêu Tốc", "Tăng 20% tốc độ chạy cơ bản của nhân vật.", UpgradeType.CommonUpgrade, 
                playerModifier: new PlayerStatModifier { moveSpeedBonus = 2f });

            // 2. Thẻ Tăng Máu & Hồi Phục (Common)
            CreateUpgrade("Trái Tim Thép", "Tăng 50 máu tối đa cho nhân vật.", UpgradeType.CommonUpgrade, 
                playerModifier: new PlayerStatModifier { maxHealthBonus = 50f });

            // 3. Thẻ Kinh Nghiệm (Common)
            CreateUpgrade("Sách Trí Tuệ", "Tăng 25% lượng kinh nghiệm nhận được.", UpgradeType.CommonUpgrade, 
                playerModifier: new PlayerStatModifier { expMultiplierBonus = 0.25f });

            // 4. Thẻ Chí Mạng Vũ Khí (Weapon)
            CreateUpgrade("Mắt Diều Hâu", "Tăng 30% tỉ lệ chí mạng và +1.0 hệ số sát thương bạo kích cho vũ khí này.", UpgradeType.WeaponUpgrade, 
                weaponModifier: new WeaponStatModifier { critChanceBonus = 0.3f, critDamageBonus = 1.0f });

            // 5. Thẻ Đạn Khổng Lồ (Weapon)
            CreateUpgrade("Đạn Khổng Lồ", "Viên đạn to gấp rưỡi (Scale +0.5) và tăng 5 sát thương.", UpgradeType.WeaponUpgrade, 
                weaponModifier: new WeaponStatModifier { scaleBonus = 0.5f, damageBonus = 5f });

            // 6. Thẻ Mưa Đạn (Weapon) - Đa mục tiêu
            CreateUpgrade("Đạn Trùm (Shotgun)", "Bắn thêm 3 tia đạn mỗi lần xuất chiêu, đạn bay nhanh hơn 30%.", UpgradeType.WeaponUpgrade, 
                weaponModifier: new WeaponStatModifier { projectileCountBonus = 3, projectileSpeedBonus = 3f });

            // 7. Thẻ Xuyên Thấu (Weapon)
            CreateUpgrade("Đạn Xuyên Thấu", "Đạn có khả năng xuyên qua thêm 2 kẻ địch.", UpgradeType.WeaponUpgrade, 
                weaponModifier: new WeaponStatModifier { pierceBonus = 2, damageBonus = 2f });

            // 8. Thẻ Tiến Hóa Mẫu (Evolution)
            var evoUpgrade = CreateUpgrade("Tiến Hóa: Stream Blade", "Tiến hóa vũ khí của bạn thành phiên bản tối thượng.", UpgradeType.EvolutionUpgrade) as EvolutionUpgradeData;
            if (evoUpgrade != null)
            {
                evoUpgrade.requiredCurrentLevel = 6;
                evoUpgrade.requiredPassiveId = "Giày Siêu Tốc"; // Yêu cầu nhặt giày trước
                evoUpgrade.weaponId = "StreamBlade"; // Tên hoặc ID vũ khí
                EditorUtility.SetDirty(evoUpgrade);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[UpgradeGeneratorTool] Đã tạo thành công các thẻ Sample tại đường dẫn: {FOLDER_PATH}");
        }

        private static UpgradeData CreateUpgrade(
            string name, 
            string description, 
            UpgradeType type, 
            PlayerStatModifier playerModifier = default, 
            WeaponStatModifier weaponModifier = default)
        {
            UpgradeData upgrade = null;

            if (type == UpgradeType.CommonUpgrade)
            {
                var common = ScriptableObject.CreateInstance<CommonUpgradeData>();
                common.playerStatModifier = playerModifier;
                upgrade = common;
            }
            else if (type == UpgradeType.EvolutionUpgrade)
            {
                upgrade = ScriptableObject.CreateInstance<EvolutionUpgradeData>();
            }
            else // WeaponUpgrade or others
            {
                var weapon = ScriptableObject.CreateInstance<WeaponUpgradeData>();
                weapon.statModifier = weaponModifier;
                upgrade = weapon;
            }
            
            upgrade.upgradeName = name;
            upgrade.description = description;
            upgrade.upgradeType = type;
            upgrade.spawnWeight = 1f;

            // Xóa ký tự đặc biệt để làm tên file
            string safeName = name.Replace(":", "").Replace(" ", "_").Replace("(", "").Replace(")", "");
            string assetPath = $"{FOLDER_PATH}/UPG_{safeName}.asset";

            AssetDatabase.CreateAsset(upgrade, assetPath);
            return upgrade;
        }
    }
}
