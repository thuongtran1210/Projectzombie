#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Upgrades;
using ProjectZombie.Features.YinYang;
using ProjectZombie.Features.Weapons;

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
                new PassiveDef("P001", "Bùa Sát Thương", "+10% Sát thương toàn thể cho nhân vật", new PlayerStatModifier { baseDamageBonus = 3f }),
                new PassiveDef("P002", "Ấn Chí Mạng", "+5% Tỷ lệ đòn đánh chí mạng", new PlayerStatModifier { critChanceBonus = 0.05f }),
                new PassiveDef("P003", "Chuông Hồi Máu", "Hồi phục máu tối đa cho nhân vật", new PlayerStatModifier { maxHealthBonus = 20f }),
                new PassiveDef("P004", "Hỏa Chủng", "+15% Sát thương hệ Hỏa và thiêu đốt", new PlayerStatModifier { fireDamageBonus = 0.15f }),
                new PassiveDef("P005", "Tháp Uy Áp", "+15% Phạm vi ảnh hưởng của kỹ năng", new PlayerStatModifier { areaScaleBonus = 0.15f }),
                new PassiveDef("P006", "Thuốc Nổ Thần Tiên", "+20% Sát thương nổ diện rộng", new PlayerStatModifier { baseDamageBonus = 4f, areaScaleBonus = 0.10f }),
                new PassiveDef("P007", "Mộc Giáp", "+30 Máu tối đa và giáp bảo hộ", new PlayerStatModifier { maxHealthBonus = 30f }),
                new PassiveDef("P008", "Hạt Tốc Đánh", "+12% Tốc độ tấn công", new PlayerStatModifier { attackSpeedBonus = 0.12f }),
                new PassiveDef("P009", "Ngọc Hồi Chiêu", "-8% Thời gian hồi chiêu Lướt & Kỹ năng", new PlayerStatModifier { dashCooldownReduction = 0.08f }),
                new PassiveDef("P010", "Túi Hút Hồn", "+30% Bán kính nhặt Hạt Kinh Nghiệm", new PlayerStatModifier { pickupRangeBonus = 1.5f }),
                new PassiveDef("P011", "Bánh Xe Tốc Độ", "+10% Tốc độ di chuyển nhân vật", new PlayerStatModifier { moveSpeedBonus = 0.6f }),
                new PassiveDef("P012", "Bùa May Mắn", "+15% Tỷ lệ nhận Exp và may mắn", new PlayerStatModifier { expMultiplierBonus = 0.15f })
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
                so.FindProperty("id").stringValue = p.id;
                so.FindProperty("upgradeName").stringValue = p.name;
                so.FindProperty("description").stringValue = p.desc;
                so.FindProperty("upgradeType").enumValueIndex = (int)UpgradeType.CommonUpgrade;
                so.FindProperty("spawnWeight").floatValue = 10f;
                so.FindProperty("maxLevel").intValue = 5;

                var maxHp = so.FindProperty("playerStatModifier.maxHealthBonus");
                if (maxHp != null) maxHp.floatValue = p.modifier.maxHealthBonus;

                var dmg = so.FindProperty("playerStatModifier.baseDamageBonus");
                if (dmg != null) dmg.floatValue = p.modifier.baseDamageBonus;

                var spd = so.FindProperty("playerStatModifier.moveSpeedBonus");
                if (spd != null) spd.floatValue = p.modifier.moveSpeedBonus;

                var crit = so.FindProperty("playerStatModifier.critChanceBonus");
                if (crit != null) crit.floatValue = p.modifier.critChanceBonus;

                var range = so.FindProperty("playerStatModifier.pickupRangeBonus");
                if (range != null) range.floatValue = p.modifier.pickupRangeBonus;

                var exp = so.FindProperty("playerStatModifier.expMultiplierBonus");
                if (exp != null) exp.floatValue = p.modifier.expMultiplierBonus;

                var atkSpd = so.FindProperty("playerStatModifier.attackSpeedBonus");
                if (atkSpd != null) atkSpd.floatValue = p.modifier.attackSpeedBonus;

                var cd = so.FindProperty("playerStatModifier.dashCooldownReduction");
                if (cd != null) cd.floatValue = p.modifier.dashCooldownReduction;

                var area = so.FindProperty("playerStatModifier.areaScaleBonus");
                if (area != null) area.floatValue = p.modifier.areaScaleBonus;

                var fire = so.FindProperty("playerStatModifier.fireDamageBonus");
                if (fire != null) fire.floatValue = p.modifier.fireDamageBonus;

                so.ApplyModifiedProperties();
            }
        }

        private static void GenerateEvolutions(string baseFolder)
        {
            EvolutionDef[] evolutions = new EvolutionDef[]
            {
                new EvolutionDef("E001", "Nỏ Thần Vạn Tiễn", "W001", "P001", "Bắn ra mưa tên ánh sáng xoay tròn 360 độ liên tục."),
                new EvolutionDef("E002", "Bút Phán Quan Âm Dương", "W002", "P002", "Nhát chém thư họa biến thành vòng xoáy Âm Dương càn quét chiến trường."),
                new EvolutionDef("E003", "Thiên Cương Trận", "W003", "P007", "Triệu hồi 12 lá bùa hộ mệnh bao quanh bảo vệ tuyệt đối."),
                new EvolutionDef("E004", "Cửu Vĩ Huyết Trảo", "W004", "P003", "Cào xé diện rộng và hút sinh mệnh cực đại từ kẻ địch."),
                new EvolutionDef("E005", "Trống Đồng Khải Hoàn", "W005", "P005", "Sóng âm chấn động toàn màn hình làm choáng váng mọi yêu ma."),
                new EvolutionDef("E006", "Thiên Hỏa Thần Sa", "W006", "P006", "Bão lửa địa ngục thiêu rụi toàn bộ vùng đất."),
                new EvolutionDef("E007", "Cung Thần Diệt Quỷ", "W007", "P004", "Mũi tên thần khí phân tách thành 8 mũi tên bay luân hồi."),
                new EvolutionDef("E008", "Cửu Thiên Long Hỏa", "W008", "P008", "Rồng lửa 9 đầu phun trào biển lửa vô tận."),
                new EvolutionDef("E009", "Long Vương Lôi Kiếp", "W009", "P009", "Thiên lôi giáng xuống liên hoàn không ngừng nghỉ."),
                new EvolutionDef("E010", "Ma Da Vạn Độc Trận", "W010", "P010", "Đầm lầy hắc ám lan rộng làm tan chảy quái vật chạm phải."),
                new EvolutionDef("E011", "Thánh Thủy Cứu Khổ", "W011", "P011", "Mưa nước thánh gột rửa và ban phước lành bất tử."),
                new EvolutionDef("E012", "Bát Quái Tru Tiên Tiêu", "W012", "P012", "Ma trận phi tiêu bát quái chém nát mọi rào cản.")
            };

            foreach (var e in evolutions)
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
                so.FindProperty("spawnWeight").floatValue = 100f;
                so.FindProperty("weaponId").stringValue = e.baseWeaponId;
                so.FindProperty("requiredCurrentLevel").intValue = 5;
                so.FindProperty("requiredPassiveId").stringValue = e.requiredPassiveId;
                so.ApplyModifiedProperties();
            }
        }
    }
}
#endif
