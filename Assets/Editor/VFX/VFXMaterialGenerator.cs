using UnityEngine;
using UnityEditor;
using System.IO;

namespace ProjectZombie.Editor.VFX
{
    /// <summary>
    /// Utility Editor script tự động tạo các Material (.mat) URP chuẩn cho Particle System Renderer.
    /// Giải quyết triệt để việc người dùng không thể kéo trực tiếp file ảnh (.png) vào ô Material của Particle System.
    /// </summary>
    public static class VFXMaterialGenerator
    {
        private const string MATERIAL_FOLDER = "Assets/VFX/SkillLibrary/Materials";
        private const string TEXTURE_FOLDER = "Assets/VFX/SkillLibrary/Textures/Skills";

        [MenuItem("Tools/VFX Skill Generator/Generate All VFX Materials", false, 20)]
        public static void GenerateAllVFXMaterials()
        {
            if (!Directory.Exists(MATERIAL_FOLDER))
            {
                Directory.CreateDirectory(MATERIAL_FOLDER);
                AssetDatabase.Refresh();
            }

            // Tạo các Material chuẩn
            CreateParticleMaterial("MAT_FireSlash_Arc", "FireSlash_Arc.png", true);
            CreateParticleMaterial("MAT_FireSlash_Impact", "FireSlash_Impact.png", true);
            CreateParticleMaterial("MAT_IceBlade_Arc", "IceBlade_Arc.png", true);
            CreateParticleMaterial("MAT_IceBlade_Impact", "IceBlade_Impact.png", true);
            CreateParticleMaterial("MAT_Additive_Default", null, true);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=#00FF00>[VFX Material Generator]</color> Đã tạo thành công toàn bộ file Material (.mat) trong Assets/VFX/SkillLibrary/Materials!");
        }

        public static Material CreateParticleMaterial(string materialName, string textureFileName, bool isAdditive)
        {
            string matPath = $"{MATERIAL_FOLDER}/{materialName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            if (mat == null)
            {
                // Tìm shader URP Particles/Unlit hoặc Fallback Particles/Additive
                Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                if (particleShader == null)
                {
                    particleShader = Shader.Find("Particles/Additive");
                }
                if (particleShader == null)
                {
                    particleShader = Shader.Find("Sprites/Default");
                }

                mat = new Material(particleShader);
                AssetDatabase.CreateAsset(mat, matPath);
            }

            // Cấu hình Blend Mode Additive / Transparent cho URP Particles Unlit
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

            // Gán Texture nếu có
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
