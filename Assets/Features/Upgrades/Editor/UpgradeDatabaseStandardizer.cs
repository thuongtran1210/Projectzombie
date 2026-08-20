#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Upgrades;
using ProjectZombie.Features.Weapons;

namespace ProjectZombie.Features.Upgrades.Editor
{
    public static class UpgradeDatabaseStandardizer
    {
        [MenuItem("ProjectZombie/Upgrades/Standardize & Generate All Upgrades (Full Roguelite Matrix)")]
        public static void StandardizeAndGenerateAll()
        {
            StandardizePassives();
            GenerateWeaponUpgrades();
            StandardizeEvolutions();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=#00FF88>[UpgradeDatabaseStandardizer]</color> Đã chuẩn hóa và sinh thành công toàn bộ Ma Trận Nâng Cấp (Passives, Weapon Lv1-5, Evolutions)!");
        }

        private static void StandardizePassives()
        {
            string passivesFolder = "Assets/_Data/Upgrades/Passives";
            if (!Directory.Exists(passivesFolder)) Directory.CreateDirectory(passivesFolder);

            string[] passiveIds = new string[]
            {
                "P001", "P002", "P003", "P004", "P005", "P006",
                "P007", "P008", "P009", "P010", "P011", "P012"
            };

            string[] passiveNames = new string[]
            {
                "Bùa Sát Thương", "Ấn Chí Mạng", "Chuông Hồi Máu", "Hỏa Chủng", "Tháp Uy Áp", "Thuốc Nổ Thần Tiên",
                "Mộc Giáp", "Hạt Tốc Đánh", "Ngọc Hồi Chiêu", "Túi Hút Hồn", "Bánh Xe Tốc Độ", "Bùa May Mắn"
            };

            string[] descriptions = new string[]
            {
                "+10% Sát thương toàn thể cho nhân vật",
                "+5% Tỉ lệ đòn đánh chí mạng",
                "+1 Máu hồi phục mỗi giây",
                "+15% Sát thương hệ Hỏa và thiêu đốt",
                "+15% Phạm vi ảnh hưởng của kỹ năng",
                "+20% Sát thương nổ diện rộng",
                "+20 Máu tối đa và giảm 5% sát thương nhận vào",
                "+10% Tốc độ ra đòn cho mọi vũ khí",
                "-8% Thời gian hồi chiêu kỹ năng & vũ khí",
                "+25% Tầm hút vật phẩm & ngọc kinh nghiệm",
                "+12% Tốc độ di chuyển",
                "+15% Tỉ lệ nhận vàng và rơi vật phẩm hiếm"
            };

            for (int i = 0; i < passiveIds.Length; i++)
            {
                string id = passiveIds[i];
                string name = passiveNames[i];
                string desc = descriptions[i];

                string[] guids = AssetDatabase.FindAssets($"{id}_ t:CommonUpgradeData", new string[] { passivesFolder });
                CommonUpgradeData asset = null;
                string assetPath = $"{passivesFolder}/{id}_{name.Replace(" ", "")}.asset";

                if (guids.Length > 0)
                {
                    assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    asset = AssetDatabase.LoadAssetAtPath<CommonUpgradeData>(assetPath);
                }

                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<CommonUpgradeData>();
                    AssetDatabase.CreateAsset(asset, assetPath);
                }

                asset.id = id;
                asset.upgradeName = name;
                asset.description = desc;
                asset.upgradeType = UpgradeType.CommonUpgrade;
                asset.spawnWeight = 10f;
                asset.maxLevel = 5;

                // Configure stat modifiers based on index
                var mod = new PlayerStatModifier();
                switch (i)
                {
                    case 0: mod.baseDamageBonus = 5f; break;      // P001 Sát thương
                    case 1: mod.critChanceBonus = 0.05f; break;   // P002 Chí mạng
                    case 2: mod.maxHealthBonus = 15f; break;      // P003 Máu
                    case 3: mod.baseDamageBonus = 4f; break;      // P004 Hỏa
                    case 4: mod.pickupRangeBonus = 1.0f; break;   // P005 Phạm vi
                    case 5: mod.baseDamageBonus = 6f; break;      // P006 Nổ
                    case 6: mod.maxHealthBonus = 25f; break;      // P007 Giáp Mộc
                    case 7: mod.baseDamageBonus = 3f; break;      // P008 Tốc Đánh
                    case 8: mod.expMultiplierBonus = 0.1f; break; // P009 Hồi chiêu / Exp
                    case 9: mod.pickupRangeBonus = 1.5f; break;   // P010 Hút hồn
                    case 10: mod.moveSpeedBonus = 0.6f; break;    // P011 Tốc độ chạy
                    case 11: mod.expMultiplierBonus = 0.15f; break;// P012 May mắn
                }
                asset.playerStatModifier = mod;

                EditorUtility.SetDirty(asset);
            }
        }

        private static void GenerateWeaponUpgrades()
        {
            string weaponsFolder = "Assets/_Data/Upgrades/Weapons";
            if (!Directory.Exists(weaponsFolder)) Directory.CreateDirectory(weaponsFolder);

            string[] weaponIds = new string[]
            {
                "W001", "W002", "W003", "W004", "W005", "W006",
                "W007", "W008", "W009", "W010", "W011", "W012"
            };

            string[] weaponNames = new string[]
            {
                "Nỏ Thần", "Bút Phán Quan", "Bùa Trấn Yêu", "Cửu Vĩ Hồ Trảo", "Trống Đồng Đông Sơn", "Lựu Đạn Thần Sa",
                "Cung Thạch Sanh", "Đao Cửu Vĩ", "Trượng Long Vương", "Linh Phù Ma Da", "Nước Thánh Chùa Hương", "Phi Tiêu Bát Quái"
            };

            for (int w = 0; w < weaponIds.Length; w++)
            {
                string wId = weaponIds[w];
                string wName = weaponNames[w];

                // Ensure Unlock card (Level 0) has correct id
                string unlockPath = $"{weaponsFolder}/Unlock_{wId}_{wName.Replace(" ", "")}.asset";
                var unlockAsset = AssetDatabase.LoadAssetAtPath<WeaponUpgradeData>(unlockPath);
                if (unlockAsset != null)
                {
                    unlockAsset.id = $"Unlock_{wId}";
                    unlockAsset.weaponId = wId;
                    unlockAsset.requiredCurrentLevel = 0;
                    EditorUtility.SetDirty(unlockAsset);
                }

                // Generate Level 2 to Level 5 upgrade cards
                for (int lvl = 1; lvl <= 4; lvl++)
                {
                    int targetLevel = lvl + 1;
                    string upgradeId = $"{wId}_Lv{targetLevel}";
                    string assetPath = $"{weaponsFolder}/{upgradeId}_{wName.Replace(" ", "")}.asset";

                    var asset = AssetDatabase.LoadAssetAtPath<WeaponUpgradeData>(assetPath);
                    if (asset == null)
                    {
                        asset = ScriptableObject.CreateInstance<WeaponUpgradeData>();
                        AssetDatabase.CreateAsset(asset, assetPath);
                    }

                    asset.id = upgradeId;
                    asset.weaponId = wId;
                    asset.requiredCurrentLevel = lvl; // Cần đạt lvl hiện tại để lên targetLevel
                    asset.upgradeType = UpgradeType.WeaponUpgrade;
                    asset.spawnWeight = 8f;
                    asset.maxLevel = 1;

                    // Copy icon from unlock card if exists
                    if (unlockAsset != null && unlockAsset.icon != null)
                    {
                        asset.icon = unlockAsset.icon;
                    }

                    var mod = new WeaponStatModifier();
                    string desc = "";

                    switch (targetLevel)
                    {
                        case 2:
                            mod.damageBonus = 6f;
                            mod.attackSpeedBonus = 0.15f;
                            desc = $"{wName} [Cấp 2]: <color=#00FF88>+25% Sát thương</color>, <color=#FFD700>+15% Tốc độ xuất chiêu</color>.";
                            break;
                        case 3:
                            mod.damageBonus = 8f;
                            mod.projectileCountBonus = 1;
                            mod.scaleBonus = 0.15f;
                            desc = $"{wName} [Cấp 3]: <color=#FFD700>+1 Tia đạn / Số lượng đòn đánh</color>, <color=#00FF88>+15% Kích thước</color>.";
                            break;
                        case 4:
                            mod.damageBonus = 10f;
                            mod.attackSpeedBonus = 0.2f;
                            mod.projectileSpeedBonus = 2f;
                            desc = $"{wName} [Cấp 4]: <color=#00FF88>+30% Sát thương</color>, <color=#FFD700>+20% Tốc độ đánh & Bay đạn</color>.";
                            break;
                        case 5:
                            mod.damageBonus = 15f;
                            mod.projectileCountBonus = 1;
                            mod.scaleBonus = 0.2f;
                            desc = $"{wName} [Cấp 5 - Max Base]: <color=#FFD700>+1 Tia đạn</color>, <color=#FF4444>+35% Sát thương cực đại</color>. Đạt điều kiện Tiến Hóa!";
                            break;
                    }

                    asset.upgradeName = $"{wName} [Cấp {targetLevel}]";
                    asset.description = desc;
                    asset.statModifier = mod;

                    EditorUtility.SetDirty(asset);
                }
            }
        }

        private static void StandardizeEvolutions()
        {
            string evolutionsFolder = "Assets/_Data/Upgrades/Evolutions";
            if (!Directory.Exists(evolutionsFolder)) return;

            string[] weaponIds = new string[] { "W001", "W002", "W003", "W004", "W005", "W006", "W007", "W008", "W009", "W010", "W011", "W012" };
            string[] passiveIds = new string[] { "P001", "P002", "P003", "P004", "P005", "P006", "P007", "P008", "P009", "P010", "P011", "P012" };
            string[] evoIds = new string[] { "E001", "E002", "E003", "E004", "E005", "E006", "E007", "E008", "E009", "E010", "E011", "E012" };

            for (int i = 0; i < evoIds.Length; i++)
            {
                string evoId = evoIds[i];
                string wId = weaponIds[i];
                string pId = passiveIds[i];

                string[] guids = AssetDatabase.FindAssets($"{evoId}_ t:EvolutionUpgradeData", new string[] { evolutionsFolder });
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<EvolutionUpgradeData>(path);
                    if (asset != null)
                    {
                        asset.id = evoId;
                        asset.weaponId = wId;
                        asset.requiredPassiveId = pId;
                        asset.requiredCurrentLevel = 5; // Cần đạt Max Level 5
                        asset.upgradeType = UpgradeType.EvolutionUpgrade;
                        asset.spawnWeight = 100f; // Trọng số ưu tiên cao khi đã đủ điều kiện tiến hóa

                        // Auto-assign weaponPrefab if missing by searching prefabs
                        if (asset.weaponPrefab == null)
                        {
                            string[] prefabGuids = AssetDatabase.FindAssets($"Weapon_{wId} t:Prefab");
                            if (prefabGuids.Length > 0)
                            {
                                string pPath = AssetDatabase.GUIDToAssetPath(prefabGuids[0]);
                                asset.weaponPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(pPath);
                            }
                        }

                        EditorUtility.SetDirty(asset);
                    }
                }
            }
        }
    }
}
#endif
