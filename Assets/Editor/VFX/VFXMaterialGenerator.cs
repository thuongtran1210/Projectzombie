using UnityEngine;
using UnityEditor;
using System.IO;

namespace ProjectZombie.Editor.VFX
{
    /// <summary>
    /// Utility Editor script tự động tạo các Material (.mat) URP chuẩn cho Particle System Renderer.
    /// Hỗ trợ Vũ Khí & Đạn Projectile (FireSlash, IceBlade, DarkOrb, IceBullet).
    /// </summary>
    public static class VFXMaterialGenerator
    {
        private const string MATERIAL_FOLDER = "Assets/VFX/SkillLibrary/Materials";
        private const string TEXTURE_FOLDER = "Assets/VFX/SkillLibrary/Textures/Skills";

        [MenuItem("Tools/VFX Generator/Generate All VFX Materials", false, 20)]
        public static void GenerateAllVFXMaterials()
        {
            if (!Directory.Exists(MATERIAL_FOLDER))
            {
                Directory.CreateDirectory(MATERIAL_FOLDER);
                AssetDatabase.Refresh();
            }

            // FireSlash Materials
            CreateParticleMaterial("MAT_FireSlash_Flash", "FireSlash_Flash.png", true);
            CreateParticleMaterial("MAT_FireSlash_Arc", "FireSlash_Arc.png", true);
            CreateParticleMaterial("MAT_FireSlash_Glow", "FireSlash_Arc.png", true);
            CreateParticleMaterial("MAT_FireSlash_Sparks", "FireSlash_Sparks.png", true);
            CreateParticleMaterial("MAT_FireSlash_Impact", "FireSlash_Impact.png", true);
            CreateParticleMaterial("MAT_FireSlash_Smoke", "FireSlash_Smoke.png", false);

            // IceBlade Materials
            CreateParticleMaterial("MAT_IceBlade_Flash", "IceBlade_Flash.png", true);
            CreateParticleMaterial("MAT_IceBlade_Arc", "IceBlade_Arc.png", true);
            CreateParticleMaterial("MAT_IceBlade_Glow", "IceBlade_Arc.png", true);
            CreateParticleMaterial("MAT_IceBlade_Sparks", "IceBlade_Sparks.png", true);
            CreateParticleMaterial("MAT_IceBlade_Impact", "IceBlade_Impact.png", true);
            CreateParticleMaterial("MAT_IceBlade_Smoke", "IceBlade_Smoke.png", false);

            // IceBullet Materials (Đạn Băng Projectile)
            CreateParticleMaterial("MAT_IceBullet_Muzzle", "IceBullet_Core.png", true);
            CreateParticleMaterial("MAT_IceBullet_Core", "IceBullet_Core.png", true);
            CreateParticleMaterial("MAT_IceBullet_Trail", "IceBullet_Trail.png", true);
            CreateParticleMaterial("MAT_IceBullet_Sparks", "IceBlade_Sparks.png", true);
            CreateParticleMaterial("MAT_IceBullet_Impact", "IceBlade_Impact.png", true);
            CreateParticleMaterial("MAT_IceBullet_Smoke", "IceBlade_Smoke.png", false);

            // DarkOrb Materials
            CreateParticleMaterial("MAT_DarkOrb_Flash", "DarkOrb_Flash.png", true);
            CreateParticleMaterial("MAT_DarkOrb_Arc", "DarkOrb_Core.png", true);
            CreateParticleMaterial("MAT_DarkOrb_Core", "DarkOrb_Core.png", true);
            CreateParticleMaterial("MAT_DarkOrb_Glow", "DarkOrb_Core.png", true);
            CreateParticleMaterial("MAT_DarkOrb_Sparks", "DarkOrb_Sparks.png", true);
            CreateParticleMaterial("MAT_DarkOrb_Impact", "DarkOrb_Impact.png", true);
            CreateParticleMaterial("MAT_DarkOrb_Smoke", "DarkOrb_Smoke.png", false);

            CreateParticleMaterial("MAT_Additive_Default", null, true);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=#00FF00>[VFX Material Generator]</color> Đã tạo thành công trọn bộ Material (.mat) trong Assets/VFX/SkillLibrary/Materials!");
        }

        public static Material CreateParticleMaterial(string materialName, string textureFileName, bool isAdditive)
        {
            string matPath = $"{MATERIAL_FOLDER}/{materialName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            if (mat == null)
            {
                Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                if (particleShader == null) particleShader = Shader.Find("Particles/Additive");
                if (particleShader == null) particleShader = Shader.Find("Sprites/Default");

                mat = new Material(particleShader);
                AssetDatabase.CreateAsset(mat, matPath);
            }

            if (isAdditive)
            {
                mat.SetFloat("_Surface", 1.0f); // Transparent
                mat.SetFloat("_Blend", 1.0f);   // Additive
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_ZWrite", 0);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.EnableKeyword("_ALPHABLEND_ON");
            }

            if (!string.IsNullOrEmpty(textureFileName))
            {
                string texPath = $"{TEXTURE_FOLDER}/{textureFileName}";
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                if (tex != null)
                {
                    mat.mainTexture = tex;
                    if (mat.HasProperty("_BaseMap"))
                    {
                        mat.SetTexture("_BaseMap", tex);
                    }
                }
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }
    }
}
