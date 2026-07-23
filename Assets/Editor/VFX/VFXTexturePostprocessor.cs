using UnityEditor;
using UnityEngine;

namespace ProjectZombie.Editor.VFX
{
    /// <summary>
    /// Tự động cấu hình thuộc tính Texture Import cho tất cả file ảnh VFX trong thư mục SkillLibrary/Textures.
    /// Chuyển TextureType sang Default (Texture2D) và bật AlphaIsTransparency để gán trực tiếp vào Material Shader.
    /// </summary>
    public class VFXTexturePostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            // Chỉ áp dụng cho các file ảnh nằm trong thư mục VFX SkillLibrary Textures
            if (assetPath.Contains("Assets/VFX/SkillLibrary/Textures"))
            {
                TextureImporter importer = (TextureImporter)assetImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = false;
                    importer.wrapMode = TextureWrapMode.Clamp;
                    importer.filterMode = FilterMode.Bilinear;
                }
            }
        }
    }
}
