using UnityEditor;
using UnityEngine;

namespace Projectzombie.Editor.Rigging
{
    /// <summary>
    /// Tự động bắt sự kiện import file Sprite từ thư mục Assets/Sprites/Rigging/
    /// để thiết lập Pivot khớp xoay, PPU và Point Filter chuẩn Pixel Art Rigging.
    /// </summary>
    public class RiggingAssetPostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.Contains("Assets/Sprites/Rigging/")) return;

            var textureImporter = (TextureImporter)assetImporter;
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.spritePixelsPerUnit = 64;
            textureImporter.filterMode = FilterMode.Point;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            textureImporter.alphaIsTransparency = true;

            var settings = new TextureImporterSettings();
            textureImporter.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteGenerateFallbackPhysicsShape = false;

            // Thiết lập Pivot chuẩn xác cho từng khớp xoay
            string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath).ToLower();

            if (fileName.Contains("head"))
            {
                // Cổ (đáy đầu)
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = new Vector2(0.5f, 0.15f);
            }
            else if (fileName.Contains("arm_upper"))
            {
                // Khớp vai (đỉnh bắp tay)
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = new Vector2(0.5f, 0.85f);
            }
            else if (fileName.Contains("arm_lower"))
            {
                // Khớp khuỷu tay (đỉnh cẳng tay)
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = new Vector2(0.5f, 0.85f);
            }
            else if (fileName.Contains("leg_thigh"))
            {
                // Khớp hông (đỉnh đùi)
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = new Vector2(0.5f, 0.85f);
            }
            else if (fileName.Contains("leg_shin"))
            {
                // Khớp đầu gối (đỉnh cẳng chân)
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = new Vector2(0.5f, 0.85f);
            }
            else if (fileName.Contains("torso"))
            {
                // Trung tâm hông/bụng
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = new Vector2(0.5f, 0.35f);
            }
            else if (fileName.Contains("staff") || fileName.Contains("prop"))
            {
                // Điểm cầm tay ở giữa thân gậy
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = new Vector2(0.5f, 0.45f);
            }

            textureImporter.SetTextureSettings(settings);
        }
    }
}
