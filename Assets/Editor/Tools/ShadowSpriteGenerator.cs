#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.Editor.Tools
{
    public static class ShadowSpriteGenerator
    {
        private const string TexturePath = "Assets/_Art/Sprites/Common/spr_blob_shadow.png";

        public static Sprite GetOrCreateShadowSprite()
        {
            Sprite existingSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TexturePath);
            if (existingSprite != null) return existingSprite;

            string dir = Path.GetDirectoryName(TexturePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            int width = 128;
            int height = 64;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Vector2 center = new Vector2((width - 1) / 2f, (height - 1) / 2f);
            float radiusX = width / 2f;
            float radiusY = height / 2f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = (x - center.x) / radiusX;
                    float dy = (y - center.y) / radiusY;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist >= 1f)
                    {
                        tex.SetPixel(x, y, new Color(0f, 0f, 0f, 0f));
                    }
                    else
                    {
                        // Smooth cubic fade out towards border
                        float alpha = Mathf.SmoothStep(1f, 0f, dist);
                        // Multiply by base dark opacity
                        tex.SetPixel(x, y, new Color(0f, 0f, 0f, alpha));
                    }
                }
            }

            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(TexturePath, bytes);
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(TexturePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 100;
                importer.spritePivot = new Vector2(0.5f, 0.5f);
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(TexturePath);
        }
    }
}
#endif
