using UnityEngine;
using UnityEditor;

namespace ProjectZombie.EditorTools
{
    public static class SpriteBorderConfigurator
    {
        [MenuItem("Tools/ProjectZombie/UI/Tối Ưu & Tiện Ích (Optimization)/3. Cấu Hình 9-Slice Border (Border Editor)", priority = 43)]
        public static void ConfigureBorders()
        {
            ConfigureSingleSprite("Assets/Art/UI/VongXuyen/Frame_Card_Wood_9Slice.png", new Vector4(45, 45, 45, 45));
            ConfigureSingleSprite("Assets/Art/UI/VongXuyen/Frame_Card_Jade_9Slice.png", new Vector4(80, 80, 80, 80));
            ConfigureSingleSprite("Assets/Art/UI/VongXuyen/Frame_Card_Synergy_9Slice.png", new Vector4(45, 45, 45, 45));
            ConfigureSingleSprite("Assets/Art/UI/VongXuyen/Frame_Card_Evolution_Gold_9Slice.png", new Vector4(45, 45, 45, 45));

            AssetDatabase.Refresh();
            UpgradeUIHierarchyOptimizer.OptimizeUpgradeUI();
            Debug.Log("<color=#00FF88>[SpriteBorderConfigurator] Đã cấu hình lại Border 9-Slice và đồng bộ toàn bộ UI!</color>");
        }

        private static void ConfigureSingleSprite(string path, Vector4 border)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spriteBorder = border;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
        }
    }
}
