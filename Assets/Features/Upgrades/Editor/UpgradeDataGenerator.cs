#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Upgrades;
using ProjectZombie.Features.YinYang;

namespace ProjectZombie.Features.Upgrades.Editor
{
    /// <summary>
    /// Editor Tool giúp tự động khởi tạo hệ thống Upgrade Data SOs (Common Passives P001-P012, Evolution E001-E012, Weapon Unlocks W001-W012).
    /// Lưu vào: Assets/_Data/Upgrades/
    /// Menu: ProjectZombie > Upgrades > Generate Upgrade Data SOs
    /// </summary>
    public static class UpgradeDataGenerator
    {
        private struct PassiveDef
        {
            public string id;
            public string name;
            public string desc;
            public PlayerStatModifier modifier;

            public PassiveDef(string id, string name, string desc, PlayerStatModifier modifier)
            {
                this.id = id;
                this.name = name;
                this.desc = desc;
                this.modifier = modifier;
            }
        }

        private struct EvolutionDef
        {
            public string evoId;
            public string evoName;
            public string baseWeaponId;
            public string requiredPassiveId;
            public string desc;

            public EvolutionDef(string evoId, string evoName, string baseWeaponId, string requiredPassiveId, string desc)
            {
                this.evoId = evoId;
                this.evoName = evoName;
                this.baseWeaponId = baseWeaponId;
                this.requiredPassiveId = requiredPassiveId;
                this.desc = desc;
            }
        }

        [MenuItem("ProjectZombie/Upgrades/Generate Upgrade Data SOs")]
        public static void GenerateAllUpgrades()
        {
            string baseFolder = "Assets/_Data/Upgrades";
            EnsureFoldersExist(baseFolder);

            GenerateCommonPassives(baseFolder);
            GenerateEvolutions(baseFolder);
            GenerateWeaponUnlockUpgrades(baseFolder);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[UpgradeDataGenerator] Đã tạo thành công toàn bộ Upgrade Data SOs trong {baseFolder}");
        }

        private static void EnsureFoldersExist(string baseFolder)
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Data")) AssetDatabase.CreateFolder("Assets", "_Data");
            if (!AssetDatabase.IsValidFolder(baseFolder)) AssetDatabase.CreateFolder("Assets/_Data", "Upgrades");
            if (!AssetDatabase.IsValidFolder($"{baseFolder}/Passives")) AssetDatabase.CreateFolder(baseFolder, "Passives");
            if (!AssetDatabase.IsValidFolder($"{baseFolder}/Evolutions")) AssetDatabase.CreateFolder(baseFolder, "Evolutions");
            if (!AssetDatabase.IsValidFolder($"{baseFolder}/Weapons")) AssetDatabase.CreateFolder(baseFolder, "Weapons");
        }

        private static void GenerateCommonPassives(string baseFolder)
        {
            PassiveDef[] passives = new PassiveDef[]
            {
                new PassiveDef("P001", "Bùa Sát Thương", "+10% Sát thương cơ bản cho nhân vật", new PlayerStatModifier { baseDamageBonus = 10f }),
                new PassiveDef("P002", "Ấn Chí Mạng", "+5% Tỷ lệ đòn đánh chí mạng", new PlayerStatModifier { critChanceBonus = 0.05f }),
                new PassiveDef("P003", "Chuông Hồi Máu", "Hồi phục 1% máu tối đa mỗi khoảng thời gian", new PlayerStatModifier { maxHealthBonus = 20f }),
                new PassiveDef("P004", "Hỏa Chủng", "+10% Tốc độ đánh cho toàn bộ vũ khí", new PlayerStatModifier { baseDamageBonus = 5f }),
                new PassiveDef("P005", "Tháp Uy Áp", "+15% Phạm vi ảnh hưởng đòn đánh (AoE)", new PlayerStatModifier { baseDamageBonus = 5f }),
                new PassiveDef("P006", "Thuốc Nổ Thần Tiên", "+20% Bán kính vụ nổ và đẩy lùi", new PlayerStatModifier { baseDamageBonus = 8f }),
                new PassiveDef("P007", "Mộc Giáp", "+5 Giáp phòng thủ cho nhân vật", new PlayerStatModifier { maxHealthBonus = 30f }),
                new PassiveDef("P008", "Hạt Tốc Đánh", "+12% Tốc độ tấn công", new PlayerStatModifier { baseDamageBonus = 5f }),
                new PassiveDef("P009", "Ngọc Hồi Chiêu", "-10% Thời gian hồi chiêu tất cả chiêu thức", new PlayerStatModifier { baseDamageBonus = 5f }),
                new PassiveDef("P010", "Túi Hút Hồn", "+30% Bán kính nhặt Hạt Kinh Nghiệm", new PlayerStatModifier { pickupRangeBonus = 1.5f }),
                new PassiveDef("P011", "Bánh Xe Tốc Độ", "+10% Tốc độ di chuyển nhân vật", new PlayerStatModifier { moveSpeedBonus = 0.5f }),
                new PassiveDef("P012", "Bùa May Mắn", "+15% Tỷ lệ rơi Cổ Tiền và may mắn Gacha", new PlayerStatModifier { expMultiplierBonus = 0.15f })
            };

            foreach (var p in passives)
            {
                string assetPath = $"{baseFolder}/Passives/{p.id}_{p.name.Replace(" ", "")}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<CommonUpgradeData>(assetPath);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<CommonUpgradeData>();
                    AssetDatabase.CreateAsset(asset, assetPath);
                }

                SerializedObject so = new SerializedObject(asset);
                so.FindProperty("upgradeName").stringValue = p.name;
                so.FindProperty("description").stringValue = p.desc;
                so.FindProperty("upgradeType").enumValueIndex = (int)UpgradeType.CommonUpgrade;
                so.FindProperty("spawnWeight").floatValue = 10f;
                so.FindProperty("maxLevel").intValue = 5;
                so.FindProperty("playerStatModifier.baseDamageBonus").floatValue = p.modifier.baseDamageBonus;
                so.FindProperty("playerStatModifier.maxHealthBonus").floatValue = p.modifier.maxHealthBonus;
                so.FindProperty("playerStatModifier.moveSpeedBonus").floatValue = p.modifier.moveSpeedBonus;
                so.FindProperty("playerStatModifier.critChanceBonus").floatValue = p.modifier.critChanceBonus;
                so.FindProperty("playerStatModifier.pickupRangeBonus").floatValue = p.modifier.pickupRangeBonus;
                so.FindProperty("playerStatModifier.expMultiplierBonus").floatValue = p.modifier.expMultiplierBonus;
                so.ApplyModifiedProperties();
            }
        }

        private static void GenerateEvolutions(string baseFolder)
        {
            EvolutionDef[] evos = new EvolutionDef[]
            {
                new EvolutionDef("E001", "Nỏ Liên Châu", "W001", "Bùa Sát Thương", "Bắn liên hoàn 3 mũi tên thần xuyên qua tất cả kẻ địch trên đường bay."),
                new EvolutionDef("E002", "Bút Sinh Tử", "W002", "Ấn Chí Mạng", "Nhát chém 360 độ chí mạng 100%, tự động kết liễu ngay quái dưới 15% HP."),
                new EvolutionDef("E003", "Bùa Cửu Huyền", "W003", "Chuông Hồi Máu", "Mở rộng bán kính vòng bùa xoay + Hồi 1% HP tối đa mỗi 50 đòn trúng."),
                new EvolutionDef("E004", "Hồ Ly Cửu Vĩ", "W004", "Hỏa Chủng", "Triệu hồi 9 móng vuốt lửa tự tìm diệt quái và để lại vệt lửa thiêu đốt."),
                new EvolutionDef("E005", "Trống Trấn Quốc", "W005", "Tháp Uy Áp", "Sóng âm trảm linh nổ 8 hướng diện rộng, gây choáng 1.5s cho toàn bộ quái."),
                new EvolutionDef("E006", "Bão Hỏa Diệm", "W006", "Thuốc Nổ Thần Tiên", "Nổ bán kính 5.0m để lại vùng lửa thiêu rụi 3s + Knockback cực mạnh."),
                new EvolutionDef("E007", "Cung Thần Tiễn", "W007", "Mộc Giáp", "Bắn 3 mũi tên thần lực xuyên vô hạn kèm hiệu ứng đẩy lùi dồn quái vào góc."),
                new EvolutionDef("E008", "Hỏa Long Đao", "W008", "Hạt Tốc Đánh", "Phun luồng rồng lửa 360 độ xoay quanh nhân vật liên tục không ngừng."),
                new EvolutionDef("E009", "Long Vương Trượng", "W009", "Ngọc Hồi Chiêu", "Sét nước nảy qua 12 quái liên tiếp + Đóng băng quái 1.0s."),
                new EvolutionDef("E010", "Thủy Cung Linh", "W010", "Túi Hút Hồn", "Linh thú Ma Da kích thước gấp đôi, phun độc 4m + Làm chậm quái 30%."),
                new EvolutionDef("E011", "Giếng Thiêng", "W011", "Bánh Xe Tốc Độ", "Tạo vũng giếng thiêng 5m làm chậm quái 50% + Hồi HP cho Player."),
                new EvolutionDef("E012", "Phi Tiêu Cửu Cung", "W012", "Bùa May Mắn", "Triệu hồi 9 phi tiêu xoay theo quỹ đạo hoa sen mở rộng rồi thu về player.")
            };

            foreach (var e in evos)
            {
                string assetPath = $"{baseFolder}/Evolutions/{e.evoId}_{e.evoName.Replace(" ", "")}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<EvolutionUpgradeData>(assetPath);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<EvolutionUpgradeData>();
                    AssetDatabase.CreateAsset(asset, assetPath);
                }

                SerializedObject so = new SerializedObject(asset);
                so.FindProperty("upgradeName").stringValue = e.evoName;
                so.FindProperty("description").stringValue = e.desc;
                so.FindProperty("upgradeType").enumValueIndex = (int)UpgradeType.EvolutionUpgrade;
                so.FindProperty("spawnWeight").floatValue = 100f; // Độ ưu tiên xuất hiện cao khi đủ điều kiện
                so.FindProperty("weaponId").stringValue = e.baseWeaponId;
                so.FindProperty("requiredCurrentLevel").intValue = 5;
                so.FindProperty("requiredPassiveId").stringValue = e.requiredPassiveId;
                so.ApplyModifiedProperties();
            }
        }

        private static void GenerateWeaponUnlockUpgrades(string baseFolder)
        {
            string[] weaponIds = { "W001", "W002", "W003", "W004", "W005", "W006", "W007", "W008", "W009", "W010", "W011", "W012" };
            string[] weaponNames = { "Nỏ Thần", "Bút Phán Quan", "Bùa Trấn Yêu", "Cửu Vĩ Hồ Trảo", "Trống Đồng Đông Sơn", "Lựu Đạn Thần Sa", "Cung Thạch Sanh", "Đao Cửu Vĩ", "Trượng Long Vương", "Linh Phù Ma Da", "Nước Thánh Chùa Hương", "Phi Tiêu Bát Quái" };

            for (int i = 0; i < weaponIds.Length; i++)
            {
                string wId = weaponIds[i];
                string wName = weaponNames[i];
                string assetPath = $"{baseFolder}/Weapons/Unlock_{wId}_{wName.Replace(" ", "")}.asset";

                var asset = AssetDatabase.LoadAssetAtPath<WeaponUpgradeData>(assetPath);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<WeaponUpgradeData>();
                    AssetDatabase.CreateAsset(asset, assetPath);
                }

                SerializedObject so = new SerializedObject(asset);
                so.FindProperty("upgradeName").stringValue = $"Mở khóa: {wName}";
                so.FindProperty("description").stringValue = $"Nhận Pháp Bảo {wName} vào trang bị active.";
                so.FindProperty("upgradeType").enumValueIndex = (int)UpgradeType.WeaponUpgrade;
                so.FindProperty("spawnWeight").floatValue = 5f;
                so.FindProperty("weaponId").stringValue = wId;
                so.FindProperty("requiredCurrentLevel").intValue = 0; // 0 = Unlock new weapon
                so.ApplyModifiedProperties();
            }
        }
    }
}
#endif
