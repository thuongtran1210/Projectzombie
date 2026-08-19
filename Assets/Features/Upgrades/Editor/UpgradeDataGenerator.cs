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

                // Tự động tìm Prefab vũ khí tương ứng trong Assets/_Prefabs/Weapons/
                GameObject weaponPrefab = null;
                string[] guids = AssetDatabase.FindAssets($"Weapon_{wId}_ t:Prefab", new string[] { "Assets/_Prefabs/Weapons" });
                if (guids.Length > 0)
                {
                    string prefabPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    weaponPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                }

                SerializedObject so = new SerializedObject(asset);
                so.FindProperty("upgradeName").stringValue = $"Mở khóa: {wName}";
                so.FindProperty("description").stringValue = $"Nhận Pháp Bảo {wName} vào trang bị active.";
                so.FindProperty("upgradeType").enumValueIndex = (int)UpgradeType.WeaponUpgrade;
                so.FindProperty("spawnWeight").floatValue = 5f;
                so.FindProperty("weaponId").stringValue = wId;
                so.FindProperty("requiredCurrentLevel").intValue = 0; // 0 = Unlock new weapon
                if (weaponPrefab != null)
                {
                    so.FindProperty("weaponPrefab").objectReferenceValue = weaponPrefab;
                    
                    // Tìm icon tương ứng từ WeaponBase hoặc icon file
                    var weaponComp = weaponPrefab.GetComponent<WeaponBase>();
                    if (weaponComp != null && weaponComp.icon != null)
                    {
                        so.FindProperty("icon").objectReferenceValue = weaponComp.icon;
                    }
                    else
                    {
                        // Thử tìm từ file icon độc lập trong Assets/Art/Weapons/
                        string[] iconGuids = AssetDatabase.FindAssets($"Icon_{wId}_ t:Sprite", new string[] { "Assets/Art/Weapons" });
                        if (iconGuids.Length > 0)
                        {
                            string iconPath = AssetDatabase.GUIDToAssetPath(iconGuids[0]);
                            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                            if (iconSprite != null)
                            {
                                so.FindProperty("icon").objectReferenceValue = iconSprite;
                            }
                        }
                    }
                }
                so.ApplyModifiedProperties();
            }
        }
    }
}
#endif
