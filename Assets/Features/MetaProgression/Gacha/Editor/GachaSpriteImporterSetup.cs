#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.Features.MetaProgression.Gacha.Editor
{
    /// <summary>
    /// Tự động thiết lập TextureImporter cho toàn bộ ảnh trong Assets/Art/UI/Gacha/ sang chuẩn Sprite 2D & UI.
    /// </summary>
    public static class GachaSpriteImporterSetup
    {
        [MenuItem("ProjectZombie/Gacha/Configure Gacha Sprites Importer", priority = 201)]
        public static void ConfigureSprites()
        {
            string folder = "Assets/Art/UI/Gacha";
            if (!Directory.Exists(folder)) return;

            string[] files = Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories);
            string[] jpgFiles = Directory.GetFiles(folder, "*.jpg", SearchOption.AllDirectories);

            var allFiles = new System.Collections.Generic.List<string>(files);
            allFiles.AddRange(jpgFiles);

            int count = 0;
            foreach (var file in allFiles)
            {
                string assetPath = file.Replace("\\", "/");
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = false;
                    importer.filterMode = FilterMode.Bilinear;

                    // Thiết lập 9-slice border cho các khung và nút bấm
                    if (assetPath.Contains("Border_Rarity"))
                    {
                        importer.spriteBorder = new Vector4(36, 36, 36, 36);
                    }
                    else if (assetPath.Contains("Btn_Gacha"))
                    {
                        importer.spriteBorder = new Vector4(40, 20, 40, 20);
                    }

                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    count++;
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"[GachaSpriteImporterSetup] Đã cấu hình chuẩn Sprite 2D cho {count} tệp ảnh trong {folder}.");
        }
    }
}
#endif
