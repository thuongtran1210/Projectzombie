using UnityEngine;
using UnityEditor;
using ProjectZombie.Features.Upgrades;
using ProjectZombie.Features.Shared;
using System.IO;

namespace ProjectZombie.Features.Upgrades.Editor
{
    /// <summary>
    /// Generator tự động sinh các Thẻ Nâng Cấp Action RPG (Combo, Relics, Dash, Breakthrough) theo chuẩn GDD v5.0.
    /// </summary>
    public static class ActionRPGUpgradeGenerator
    {
        [MenuItem("Tools/ProjectZombie/Generate Action RPG Upgrades (GDD v5.0)", priority = 10)]
        public static void GenerateActionRPGUpgrades()
        {
            string folderPath = "Assets/_Data/Upgrades/ActionRPG";

            if (!AssetDatabase.IsValidFolder("Assets/_Data"))
                AssetDatabase.CreateFolder("Assets", "_Data");
            if (!AssetDatabase.IsValidFolder("Assets/_Data/Upgrades"))
                AssetDatabase.CreateFolder("Assets/_Data", "Upgrades");
            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder("Assets/_Data/Upgrades", "ActionRPG");

            // ==========================================
            // 1. NHÓM BÍ KÍP ĐÒN CHÉM (Combo Augments)
            // ==========================================
            CreateComboAugment(folderPath, "ARPG_Combo_01_KiemKhiTram", 
                "Kiếm Khí Trảm", 
                "Đòn chém thứ 3 giải phóng <color=#FFD700>sóng kiếm khí</color> bay thẳng, tăng +25% Sát thương và +30% Tầm quét.",
                ElementType.Kim, 5f, 0.25f, 0.15f, 0.30f, 2.5f);

            CreateComboAugment(folderPath, "ARPG_Combo_02_TramPhongLienHoan", 
                "Trảm Phong Liên Hoàn", 
                "Giảm 30% thời gian trễ giữa các nhát chém, tăng <color=#00FF88>+35% Tốc độ vung đòn</color> liên hoàn.",
                ElementType.Moc, 5f, 0.15f, 0.35f, 0.10f, 1.5f);

            CreateComboAugment(folderPath, "ARPG_Combo_03_TrongTramDienRong", 
                "Trọng Trảm Diện Rộng", 
                "Tăng <color=#FF8800>+50% Góc quét hình quạt</color> của nhát chém, tăng mạnh lực hất văng quái vật.",
                ElementType.Tho, 4f, 0.30f, 0.05f, 0.50f, 4.0f);

            // ==========================================
            // 2. NHÓM CƯỜNG HÓA LƯỚT (Dash Traits)
            // ==========================================
            CreateDashTrait(folderPath, "ARPG_Dash_01_TanAnhKiem",
                "Tàn Ảnh Kiếm",
                "Giảm <color=#4DEEEA>30% Thời gian hồi Lướt</color> và tăng 35% Tốc độ lướt né đòn.",
                ElementType.Moc, 6f, 0.30f, 0.35f, 0.15f);

            CreateDashTrait(folderPath, "ARPG_Dash_02_LuotPhanDon",
                "Lướt Phản Đòn (Parry Dash)",
                "Sau khi Lướt né đòn thành công, đòn chém tiếp theo nhận thêm <color=#FFD700>+40% Tỷ lệ Chí mạng</color>.",
                ElementType.Kim, 5f, 0.20f, 0.20f, 0.40f);

            // ==========================================
            // 3. NHÓM ĐỘT PHÁ TUYỆT KỸ (Breakthrough - Level 5 & 10)
            // ==========================================
            CreateBreakthrough(folderPath, "ARPG_Breakthrough_01_BatQuaiKiemTran",
                "Bí Tịch: Bát Quái Kiếm Trận",
                "ĐỘT PHÁ CẤP 5: Tăng <color=#FF00FF>+50% Sát thương toàn diện</color> và tăng 50% Quy mô bảo vệ của tất cả Pháp bảo!",
                5, 10f, 0.50f, 0.50f, 0.15f);

            CreateBreakthrough(folderPath, "ARPG_Breakthrough_02_HoaThanNhapMa",
                "Bí Tịch: Hóa Thần Nhập Ma",
                "ĐỘT PHÁ CẤP 10: Tăng <color=#FF3300>+100% Sát thương Vũ Khí</color>, tự động kết liễu ngay quái thường dưới 20% HP!",
                10, 10f, 1.00f, 0.30f, 0.20f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Tự động đồng bộ nạp toàn bộ thẻ vào UpgradeManager trong Scene (nếu có)
            var upgradeManager = Object.FindAnyObjectByType<UpgradeManager>();
            if (upgradeManager != null)
            {
                upgradeManager.PopulateAllAvailableUpgrades();
                EditorUtility.SetDirty(upgradeManager);
            }

            Debug.Log($"<color=#00FF88>[ActionRPGUpgradeGenerator]</color> Đã sinh và ĐỒNG BỘ thành công trọn bộ Thẻ Nâng Cấp Action RPG vào UpgradeManager!");
        }

        private static void CreateComboAugment(string path, string id, string name, string desc, ElementType elem, float weight, float dmg, float spd, float scale, float knockback)
        {
            string fullPath = $"{path}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<ComboAugmentUpgradeData>(fullPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<ComboAugmentUpgradeData>();
                AssetDatabase.CreateAsset(asset, fullPath);
            }

            asset.id = id;
            asset.upgradeName = name;
            asset.description = desc;
            asset.element = elem;
            asset.upgradeType = UpgradeType.ComboAugment;
            asset.spawnWeight = weight;
            asset.comboDamageMultiplierBonus = dmg;
            asset.attackSpeedBonus = spd;
            asset.slashAreaScaleBonus = scale;
            asset.finisherKnockbackBonus = knockback;
            EditorUtility.SetDirty(asset);
        }

        private static void CreateDashTrait(string path, string id, string name, string desc, ElementType elem, float weight, float cdReduc, float spd, float crit)
        {
            string fullPath = $"{path}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<DashTraitUpgradeData>(fullPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<DashTraitUpgradeData>();
                AssetDatabase.CreateAsset(asset, fullPath);
            }

            asset.id = id;
            asset.upgradeName = name;
            asset.description = desc;
            asset.element = elem;
            asset.upgradeType = UpgradeType.DashTrait;
            asset.spawnWeight = weight;
            asset.dashCooldownReduction = cdReduc;
            asset.dashSpeedBonus = spd;
            asset.postDashCritBonus = crit;
            EditorUtility.SetDirty(asset);
        }

        private static void CreateBreakthrough(string path, string id, string name, string desc, int reqLevel, float weight, float dmgMult, float relicScale, float execute)
        {
            string fullPath = $"{path}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<BreakthroughUpgradeData>(fullPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<BreakthroughUpgradeData>();
                AssetDatabase.CreateAsset(asset, fullPath);
            }

            asset.id = id;
            asset.upgradeName = name;
            asset.description = desc;
            asset.upgradeType = UpgradeType.BreakthroughUltimate;
            asset.spawnWeight = weight;
            asset.requiredPlayerLevel = reqLevel;
            asset.allDamageMultiplier = dmgMult;
            asset.relicScaleMultiplier = relicScale;
            asset.executeHealthThreshold = execute;
            EditorUtility.SetDirty(asset);
        }
    }
}
