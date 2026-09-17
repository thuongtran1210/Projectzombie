#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace ProjectZombie.Editor.UI
{
    /// <summary>
    /// Editor Tool tự động kiểm tra và chuyển toàn bộ Source Textures của UI trong `Assets/Art/UI/`
    /// về định dạng Không Nén (Uncompressed / Automatic) trước khi đóng gói vào SpriteAtlas.
    /// Giúp sửa triệt để cảnh báo "Source Texture is using compressed format" và đảm bảo chất lượng hình ảnh sắc nét 100%.
    /// </summary>
    public static class FixUITextureFormatsForAtlasTool
    {
        [MenuItem("ProjectZombie/2. 📱 Mobile UI/3. 🛠️ Sửa Lỗi Nén SpriteAtlas UI (Uncompressed Source Textures)", priority = 105)]
        public static void FixAllUITextureFormats()
        {
            string[] searchFolders = new string[]
            {
                "Assets/Art/UI",
                "Assets/Art/Tilemaps/UI"
            };

            int fixedCount = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var folder in searchFolders)
                {
                    if (!Directory.Exists(folder)) continue;

                    string[] files = Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories);
                    foreach (string filePath in files)
                    {
                        string unityPath = filePath.Replace('\\', '/');
                        var importer = AssetImporter.GetAtPath(unityPath) as TextureImporter;

                        if (importer != null)
                        {
                            bool modified = false;

                            // 1. Kiểm tra Android Platform Override
                            var androidSettings = importer.GetPlatformTextureSettings("Android");
                            if (androidSettings != null && androidSettings.overridden)
                            {
                                androidSettings.overridden = false;
                                importer.SetPlatformTextureSettings(androidSettings);
                                modified = true;
                            }

                            // 2. Kiểm tra Default Platform Settings (đảm bảo textureCompression là Uncompressed)
                            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                            {
                                importer.textureCompression = TextureImporterCompression.Uncompressed;
                                modified = true;
                            }

                            if (modified)
                            {
                                importer.SaveAndReimport();
                                fixedCount++;
                            }
                        }
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();
            Debug.Log($"<color=#00FF88>[FixUITextureFormatsForAtlasTool]</color> Đã hoàn tất sửa lỗi nén cho {fixedCount} UI Source Textures! Toàn bộ SpriteAtlas bây giờ đóng gói mượt mà và không còn cảnh báo.");
        }
    }
}
#endif
