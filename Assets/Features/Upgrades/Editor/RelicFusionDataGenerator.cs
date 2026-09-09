#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Weapons;

namespace ProjectZombie.Features.Upgrades.Editor
{
    /// <summary>
    /// Công cụ Editor tự động sinh dữ liệu ScriptableObject cho hệ thống Luyện Hóa & Gộp Thẻ Pháp Bảo.
    /// Menu: ProjectZombie -> Data Generators -> Generate Relic Fusion Upgrades
    /// </summary>
    public static class RelicFusionDataGenerator
    {
        private const string SAVE_FOLDER = "Assets/_Data/Upgrades/RelicFusions";

        [MenuItem("ProjectZombie/Data Generators/Generate Relic Fusion Upgrades")]
        public static void GenerateFusionData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Data"))
            {
                AssetDatabase.CreateFolder("Assets", "_Data");
            }
            if (!AssetDatabase.IsValidFolder("Assets/_Data/Upgrades"))
            {
                AssetDatabase.CreateFolder("Assets/_Data", "Upgrades");
            }
            if (!AssetDatabase.IsValidFolder(SAVE_FOLDER))
            {
                AssetDatabase.CreateFolder("Assets/_Data/Upgrades", "RelicFusions");
            }

            var fusions = new List<RelicFusionRecipe>
            {
                new RelicFusionRecipe(
                    "FUSION_SLIPPER_FIRE",
                    "Vạn Dép Bát Quái Thần Hỏa",
                    "W_SLIPPER",
                    5,
                    new List<string> { "P001", "P006" }, // Dép + Giảm Hồi Chiêu + Sát Thương Hỏa
                    null,
                    "Nâng cấp Dép Tổ Ong thành Thần Binh: Phóng 8 đôi dép hỏa diệm xoay tròn, Phase 2 lướt đạp Shockwave gây sát thương thiêu đốt toàn bản đồ."
                ),
                new RelicFusionRecipe(
                    "FUSION_POT_IMMORTAL",
                    "Nồi Thần Càn Khôn Bất Tử",
                    "W_POT",
                    5,
                    new List<string> { "P005", "P003" }, // Nồi Cơm + Máu Tối Đa + Giáp Bùa
                    null,
                    "Nâng cấp Nồi Cơm Thạch Sanh: Lực hút hố đen tăng gấp đôi, sau khi nổ giải phóng Cơm Tiên hồi 30% Máu và tạo khiên vô địch 3 giây."
                ),
                new RelicFusionRecipe(
                    "FUSION_PIPE_DRAGON",
                    "Cửu U Hỏa Long Khói",
                    "W_PIPE",
                    5,
                    new List<string> { "P004", "P008" }, // Điếu Cày + Hút Máu + Sát Thương Chém
                    null,
                    "Nâng cấp Điếu Cày Cửu U: Phun Hỏa Long khói thiêu rụi phạm vi cực đại, quái trúng phải bị câm lặng, mù lòa và rút máu liên tục."
                ),
                new RelicFusionRecipe(
                    "FUSION_BROOM_CELESTIAL",
                    "Thiên Binh Thần Chổi Quét",
                    "R008",
                    5,
                    new List<string> { "P001", "P007" }, // Chổi Lông Gà + Tốc Đánh + Xuyên Thấu
                    null,
                    "Nâng cấp Chổi Lông Gà: Mỗi đòn Finisher Hit 3 triệu hồi 3 Chổi Thần khổng lồ quét sập trận địa, đẩy văng toàn bộ Boss và Quái tinh anh."
                )
            };

            int count = 0;
            foreach (var f in fusions)
            {
                string assetPath = $"{SAVE_FOLDER}/Fusion_{f.fusionId}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<FusionUpgradeData>(assetPath);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<FusionUpgradeData>();
                    AssetDatabase.CreateAsset(asset, assetPath);
                }

                asset.id = f.fusionId;
                asset.upgradeName = f.displayName;
                asset.description = f.description;
                asset.upgradeType = UpgradeType.RelicFusion;
                asset.spawnWeight = 1.0f;
                asset.maxLevel = 1;
                asset.recipe = f;
                asset.element = ElementType.Kim;

                EditorUtility.SetDirty(asset);
                count++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=#00FF88><b>[RelicFusionDataGenerator]</b></color> Đã sinh thành công {count} Thẻ Nâng Cấp Luyện Hóa Pháp Bảo tại {SAVE_FOLDER}.");
        }
    }
}
#endif
