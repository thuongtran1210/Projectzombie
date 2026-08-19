using UnityEngine;
using UnityEditor;
using System.IO;

namespace ProjectZombie.Editor.VFX
{
    /// <summary>
    /// Utility Editor script tự động tạo các Material (.mat) URP chuẩn cho Particle System Renderer và Decals.
    /// Tích hợp trực tiếp bộ 4 Shader HLSL URP mới:
    /// - ProjectZombie/VFX/Slash_Additive
    /// - ProjectZombie/VFX/Distortion_Shockwave
    /// - ProjectZombie/VFX/GroundDecal_Dissolve
    /// - ProjectZombie/Sprite_HitFlash
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

            // 1. Slash Materials (Bút Phán Quan W002 & Đao Cửu Vĩ W008)
            CreateSlashMaterial("MAT_FireSlash_Arc", "FireSlash_Arc.png", null, new Color(2f, 2f, 2f, 1f), new Color(1f, 0.35f, 0.05f, 1f), false);
            CreateSlashMaterial("MAT_FireSlash_PolarGlow", "FireSlash_Arc.png", null, new Color(2f, 1.8f, 1.2f, 1f), new Color(1f, 0.2f, 0.0f, 1f), true);
            CreateSlashMaterial("MAT_IceBlade_Arc", "IceBlade_Arc.png", null, new Color(1.8f, 2f, 2f, 1f), new Color(0.2f, 0.7f, 1f, 1f), false);
            CreateSlashMaterial("MAT_InkSlash_Arc", "FireSlash_Arc.png", null, new Color(1.5f, 1.5f, 1.5f, 1f), new Color(0.1f, 0.1f, 0.15f, 1f), false); // Mực Bút Phán Quan

            // 2. Shockwave Materials (Trống Đồng W005 & Lựu Đạn W006)
            CreateShockwaveMaterial("MAT_Shockwave_Explosion", null, new Color(1f, 0.6f, 0.2f, 0.6f), 0.06f);
            CreateShockwaveMaterial("MAT_Shockwave_DongSon", null, new Color(1f, 0.85f, 0.4f, 0.8f), 0.08f); // Sóng âm Trống Đồng

            // 3. Ground Decal Materials (Vết Nứt Đất & Bãi Nước Thánh W011)
            CreateGroundDecalMaterial("MAT_Decal_CrackedEarth", null, null, new Color(0.4f, 0.3f, 0.2f, 1f), new Color(2f, 0.8f, 0.2f, 1f));
            CreateGroundDecalMaterial("MAT_Decal_HolyWaterPuddle", null, null, new Color(0.3f, 0.8f, 1f, 0.7f), new Color(1f, 1.5f, 2f, 1f));

            // 4. Particle Additive / Alpha Fallback Materials
            CreateParticleMaterial("MAT_FireSlash_Flash", "FireSlash_Flash.png", true);
            CreateParticleMaterial("MAT_FireSlash_Sparks", "FireSlash_Sparks.png", true);
            CreateParticleMaterial("MAT_FireSlash_Impact", "FireSlash_Impact.png", true);
            CreateParticleMaterial("MAT_FireSlash_Smoke", "FireSlash_Smoke.png", false);

            CreateParticleMaterial("MAT_IceBlade_Flash", "IceBlade_Flash.png", true);
            CreateParticleMaterial("MAT_IceBlade_Sparks", "IceBlade_Sparks.png", true);
            CreateParticleMaterial("MAT_IceBlade_Impact", "IceBlade_Impact.png", true);
            CreateParticleMaterial("MAT_IceBlade_Smoke", "IceBlade_Smoke.png", false);

            CreateParticleMaterial("MAT_Additive_Default", null, true);

            // 5. Enemy Sprite Hit Flash Material Mẫu
            CreateEnemyHitFlashMaterial("MAT_Enemy_HitFlash_Default");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=#00FF00>[VFX Material Generator]</color> Đã tạo thành công trọn bộ Material URP (.mat) với Shaders mới trong " + MATERIAL_FOLDER);
        }

        public static Material CreateSlashMaterial(string materialName, string mainTexName, string noiseTexName, Color coreColor, Color edgeColor, bool usePolar)
        {
            string matPath = $"{MATERIAL_FOLDER}/{materialName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            Shader shader = Shader.Find("ProjectZombie/VFX/Slash_Additive");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = shader;
            }

            mat.SetColor("_CoreColor", coreColor);
            mat.SetColor("_EdgeColor", edgeColor);
            mat.SetFloat("_UsePolar", usePolar ? 1.0f : 0.0f);
            if (usePolar) mat.EnableKeyword("_USE_POLAR_COORDS");
            else mat.DisableKeyword("_USE_POLAR_COORDS");

            if (!string.IsNullOrEmpty(mainTexName))
            {
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TEXTURE_FOLDER}/{mainTexName}");
                if (tex != null) mat.SetTexture("_MainTex", tex);
            }

            if (!string.IsNullOrEmpty(noiseTexName))
            {
                Texture2D noise = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TEXTURE_FOLDER}/{noiseTexName}");
                if (noise != null) mat.SetTexture("_NoiseTex", noise);
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        public static Material CreateShockwaveMaterial(string materialName, string maskTexName, Color tintColor, float distortionStrength)
        {
            string matPath = $"{MATERIAL_FOLDER}/{materialName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            Shader shader = Shader.Find("ProjectZombie/VFX/Distortion_Shockwave");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = shader;
            }

            mat.SetColor("_TintColor", tintColor);
            mat.SetFloat("_DistortionStrength", distortionStrength);

            if (!string.IsNullOrEmpty(maskTexName))
            {
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TEXTURE_FOLDER}/{maskTexName}");
                if (tex != null) mat.SetTexture("_MainTex", tex);
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        public static Material CreateGroundDecalMaterial(string materialName, string decalTexName, string noiseTexName, Color baseColor, Color burnColor)
        {
            string matPath = $"{MATERIAL_FOLDER}/{materialName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            Shader shader = Shader.Find("ProjectZombie/VFX/GroundDecal_Dissolve");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = shader;
            }

            mat.SetColor("_Color", baseColor);
            mat.SetColor("_BurnColor", burnColor);

            if (!string.IsNullOrEmpty(decalTexName))
            {
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TEXTURE_FOLDER}/{decalTexName}");
                if (tex != null) mat.SetTexture("_MainTex", tex);
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        public static Material CreateEnemyHitFlashMaterial(string materialName)
        {
            string matPath = $"{MATERIAL_FOLDER}/{materialName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            Shader shader = Shader.Find("ProjectZombie/Sprite_HitFlash");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = shader;
            }

            mat.EnableKeyword("UNITY_INSTANCING_ENABLED");
            EditorUtility.SetDirty(mat);
            return mat;
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
