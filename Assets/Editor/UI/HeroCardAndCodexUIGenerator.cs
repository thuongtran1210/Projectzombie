#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectZombie.Editor.UI;
using ProjectZombie.Features.MetaProgression.Gacha.Editor;

namespace ProjectZombie.Editor.Tools
{
    /// <summary>
    /// Master UI Generator Tool cho Hệ Thống Thẻ Tướng (Hero Card Progression) & Bách Bảo Các (Codex) & Gacha Chiêu Mộ.
    /// Đảm bảo chạy độc lập, tự động cô lập (Sandbox), không ảnh hưởng đến bất kỳ hệ thống hay UI nào khác.
    /// </summary>
    public static class HeroCardAndCodexUIGenerator
    {
        [MenuItem("Tools/ProjectZombie/UI/⚡ Tạo Mới UI Thẻ Tướng & Bách Bảo Các (Hero Cards & Codex)", priority = 10)]
        public static void GenerateAllHeroCardAndCodexUIs()
        {
            Debug.Log("<color=#00FFFF>[HeroCardAndCodexUIGenerator]</color> Bắt đầu quy trình tạo mới & đồng bộ UI Thẻ Tướng...");

            // 1. Tự động đồng bộ Data ScriptableObjects của 4 vị tướng & Banner Gacha Anh Hùng
            CharacterDataAssetGenerator.GenerateCharacterAssets();
            GachaDataGenerator.GenerateDefaultGachaBanner();
            Debug.Log("<color=#00FF88>[HeroCardAndCodexUIGenerator]</color> 1/3: Đã cập nhật & nạp ScriptableObject 4 Tướng và Banner Gacha Anh Hùng.");

            // 2. Tạo Prefab Thư Viện Thần Thẻ & Thần Tướng (Card Codex UI)
            GameObject codexPrefab = CardCodexUIGenerator.GenerateCardCodexPrefab();
            Debug.Log("<color=#00FF88>[HeroCardAndCodexUIGenerator]</color> 2/3: Đã tạo hoàn tất Prefab Thư Viện Thần Thẻ & Thần Tướng (Panel_CardCodex).");

            // 3. Rebuild Gacha UI Prefabs (Đã bao gồm Gương Chiêu Mộ Anh Hùng)
            GachaUIPrefabBuilder.BuildGachaUIPrefabs();
            Debug.Log("<color=#00FF88>[HeroCardAndCodexUIGenerator]</color> 3/3: Đã đồng bộ Bảo Rương Chiêu Mộ Anh Hùng vào Gacha Shop.");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Hoàn Tất Tạo Mới UI",
                "Đã khởi tạo thành công UI Thẻ Tướng (Hero Cards), Bách Bảo Các (Codex) và Bảo Rương Chiêu Mộ Tướng (Gacha)!\n\n" +
                "• Prefab Codex: Assets/_Prefabs/UI/CardCodexUI.prefab\n" +
                "• Prefab Gacha: Assets/_Prefabs/UI/Gacha/GachaShopPanel.prefab\n" +
                "• Hoàn toàn độc lập, an toàn với các hệ thống khác.",
                "Đồng Ý"
            );
        }

        [MenuItem("Tools/ProjectZombie/UI/⚡ Tạo Riêng Màn Hình Codex Thẻ Tướng (Codex Only)", priority = 11)]
        public static void GenerateCodexOnly()
        {
            CardCodexUIGenerator.GenerateCardCodexPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Tạo UI Codex Thành Công",
                "Đã tạo mới Prefab Bách Bảo Các (Panel_CardCodex) với Tab THẦN TƯỚNG ANH HÙNG.",
                "Đóng"
            );
        }
    }
}
#endif
