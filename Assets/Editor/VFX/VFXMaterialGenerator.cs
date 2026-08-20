using UnityEngine;
using UnityEditor;
using System.IO;

namespace ProjectZombie.Editor.VFX
{
    /// <summary>
    /// Utility Editor script tự động tạo các Material (.mat) URP chuẩn cho Particle System Renderer, TrailRenderer và Decals.
    /// Tích hợp đầy đủ Texture RGBA 100% trong suốt cho toàn bộ 12 Pháp Bảo.
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

            // 1. Slash Materials (Bút Phán Quan W002, Đao Cửu Vĩ W008, Hồ Trảo W004)
            // Lớp Mực Tàu Đen (AlphaBlend) + Lớp Dạ Quang Neon (Additive)
            CreateAlphaBlendInkMaterial("MAT_Ink_Black_Stroke", "Ink_Black_Brush_Arc.png", new Color(0.04f, 0.04f, 0.06f, 1.0f));
            CreateSlashMaterial("MAT_Ink_Neon_Glow", "Neon_Blade_Glow_Arc.png", null, new Color(2.5f, 2.5f, 2.5f, 1f), new Color(0.1f, 0.9f, 0.8f, 1f), false);
            CreateSlashMaterial("MAT_InkSlash_Arc", "Pro_InkSlash_Arc.png", null, new Color(2.5f, 2.2f, 1.2f, 1f), new Color(0.9f, 0.65f, 0.15f, 1f), false);
            CreateSlashMaterial("MAT_FireSlash_Arc", "FoxFlame_Stream.png", null, new Color(2f, 2f, 2f, 1f), new Color(1f, 0.35f, 0.05f, 1f), false);
            CreateSlashMaterial("MAT_Fox_Claws_Slash", "Fox_Claws_Slash.png", null, new Color(2.5f, 0.4f, 0.2f, 1f), new Color(1f, 0.1f, 0.05f, 1f), false);
            CreateParticleMaterial("MAT_Soul_Drain_Orb", "Soul_Drain_Orb.png", true);

            // 2. Shockwave & AoE Materials (Trống Đồng W005, Lựu Đạn W006, Nước Thánh W011, Linh Phù Ma Da W010)
            CreateSonicWaveMaterial("MAT_Shockwave_DongSon", "DongSon_Shockwave_Pattern.png", new Color(1.8f, 1.4f, 0.5f, 1.0f), 1.5f);
            CreateParticleMaterial("MAT_Fire_Pillar", "Fire_Pillar_Tornado.png", true);
            CreateGroundDecalMaterial("MAT_Decal_CrackedEarth", "Decal_Cracked_Circle.png", null, new Color(0.9f, 0.7f, 0.3f, 0.9f), new Color(2f, 1.2f, 0.3f, 1f));
            CreateGroundDecalMaterial("MAT_Decal_HolyWaterPuddle", "Holy_Puddle_Mist.png", null, new Color(0.3f, 0.85f, 1f, 0.8f), new Color(1.5f, 2.2f, 2.5f, 1f));
            CreateGroundDecalMaterial("MAT_Decal_PoisonSwamp", "Poison_Swamp_Mist.png", null, new Color(0.6f, 0.2f, 0.9f, 0.8f), new Color(0.2f, 1.5f, 0.8f, 1f));
            CreateParticleMaterial("MAT_Holy_Bubble", "Holy_Bubble_Particle.png", true);

            // W006 Lựu Đạn Thần Sa Materials (Cinnabar Explosion Suite)
            CreateSlashMaterial("MAT_Cinnabar_Fireball", "Tex_VFX_Cinnabar_Fireball_Burst.png", null, new Color(2.5f, 1.2f, 0.3f, 1f), new Color(1.0f, 0.2f, 0.05f, 1f), false);
            CreateSlashMaterial("MAT_Cinnabar_Shockwave", "Tex_VFX_Cinnabar_Shockwave_Ring.png", null, new Color(2.2f, 0.8f, 0.2f, 1f), new Color(1.0f, 0.15f, 0.05f, 1f), false);
            CreateGroundDecalMaterial("MAT_Cinnabar_MagicArray", "Tex_VFX_Cinnabar_Magic_Array.png", null, new Color(1.0f, 0.25f, 0.1f, 0.9f), new Color(2.5f, 0.8f, 0.2f, 1f));
            CreateAlphaBlendInkMaterial("MAT_Cinnabar_Smoke", "Tex_VFX_Cinnabar_Smoke_Puff.png", new Color(0.45f, 0.08f, 0.05f, 0.75f));
            CreateParticleMaterial("MAT_Cinnabar_Sparks", "Spark_Streak.png", true);
            CreateParticleMaterial("MAT_Cinnabar_Flash", "FireSlash_Impact.png", true);

            // 3. Projectile & Beam Materials (Nỏ Thần W001, Cung Thạch Sanh W007, Trượng Long Vương W009)
            CreateSonicWaveMaterial("MAT_ThachSanh_SonicArrow", "VFX_ThachSanh_SonicArrow.png", new Color(1.8f, 1.5f, 0.4f, 1.0f), 1.4f);
            CreateParticleMaterial("MAT_Arrow_Golden_Beam", "Arrow_Golden_Beam.png", true);
            CreateParticleMaterial("MAT_Wind_Pierce_Ring", "Wind_Pierce_Ring.png", true);
            CreateParticleMaterial("MAT_Lightning_Bolt", "Lightning_Bolt_Segment.png", true);

            // 4. Trail & Vortex Materials (Bùa Trấn Yêu W003 & Phi Tiêu Bát Quái W012)
            CreateParticleMaterial("MAT_Talisman_Ribbon_Trail", "Talisman_Ribbon_Trail.png", true);
            CreateParticleMaterial("MAT_BatQuai_Wind_Vortex", "BatQuai_Wind_Vortex.png", true);
            CreateParticleMaterial("MAT_Repulsion_Pulse", "Repulsion_Pulse_Ring.png", true);

            // 5. General Sparks & Hit Flash
            CreateParticleMaterial("MAT_FireSlash_Sparks", "Spark_Streak.png", true);
            CreateEnemyHitFlashMaterial("MAT_Enemy_HitFlash_Default");

            // 6. Thanh Đồng Specific Materials (Mồi Lửa Định Hướng, Vòng Trận Tứ Phủ, Sóng Phán Truyền, Khói Xung Kích)
            CreateParticleMaterial("MAT_TorchFlame_Bullet", "Tex_VFX_TorchFlame_Bullet.png", true);
            CreateParticleMaterial("MAT_TuPhu_PossessionCircle", "Tex_VFX_TuPhu_PossessionCircle.png", true);
            CreateParticleMaterial("MAT_Oracle_Shockwave", "Tex_VFX_Oracle_Shockwave.png", true);
            CreateAlphaBlendInkMaterial("MAT_Oracle_ShockwaveSmoke", "Tex_VFX_Shockwave_SmokePuff.png", new Color(0.3f, 0.25f, 0.2f, 0.7f));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=#00FF00>[VFX Material Generator]</color> Đã tạo & nạp toàn bộ Materials URP cho trọn bộ 12 Pháp Bảo!");
        }

        public static Texture2D FindTexture(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;

            string[] candidateDirs = new string[]
            {
                TEXTURE_FOLDER,
                "Assets/Art/VFX",
                "Assets/Art/Skills",
                "Assets/Art/Weapons",
                "Assets/Art/Projectiles",
                "Assets/Art/DaoSi"
            };

            foreach (var dir in candidateDirs)
            {
                string fullPath = $"{dir}/{fileName}";
                if (File.Exists(fullPath))
                {
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
                    if (tex != null) return tex;
                }
            }

            string[] guids = AssetDatabase.FindAssets($"{Path.GetFileNameWithoutExtension(fileName)} t:Texture2D");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }

            return null;
        }

        public static Material CreateSlashMaterial(string materialName, string mainTexName, string noiseTexName, Color coreColor, Color edgeColor, bool dissolve)
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

            if (mat.HasProperty("_CoreColor")) mat.SetColor("_CoreColor", coreColor);
            if (mat.HasProperty("_TintColor")) mat.SetColor("_TintColor", coreColor);
            if (mat.HasProperty("_EdgeColor")) mat.SetColor("_EdgeColor", edgeColor);
            if (mat.HasProperty("_DissolveAmount")) mat.SetFloat("_DissolveAmount", dissolve ? 0.3f : 0.0f);

            Texture2D mainTex = FindTexture(mainTexName);
            if (mainTex != null)
            {
                mat.mainTexture = mainTex;
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", mainTex);
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", mainTex);
            }

            if (!string.IsNullOrEmpty(noiseTexName))
            {
                Texture2D noiseTex = FindTexture(noiseTexName);
                if (noiseTex != null && mat.HasProperty("_NoiseTex")) mat.SetTexture("_NoiseTex", noiseTex);
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        public static Material CreateSonicWaveMaterial(string materialName, string textureFileName, Color tintColor, float brightness = 1.2f)
        {
            string matPath = $"{MATERIAL_FOLDER}/{materialName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            Shader shader = Shader.Find("ProjectZombie/VFX/SonicWave_Additive");
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

            if (mat.HasProperty("_TintColor")) mat.SetColor("_TintColor", tintColor);
            if (mat.HasProperty("_Brightness")) mat.SetFloat("_Brightness", brightness);

            Texture2D tex = FindTexture(textureFileName);
            if (tex != null)
            {
                mat.mainTexture = tex;
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        public static Material CreateAlphaBlendInkMaterial(string materialName, string textureFileName, Color tintColor)
        {
            string matPath = $"{MATERIAL_FOLDER}/{materialName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            Shader inkShader = Shader.Find("ProjectZombie/VFX/URP_VFX_Ink_AlphaBlend");
            if (inkShader == null) inkShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (inkShader == null) inkShader = Shader.Find("Sprites/Default");

            if (mat == null)
            {
                mat = new Material(inkShader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = inkShader;
            }

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tintColor);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", tintColor);

            Texture2D tex = FindTexture(textureFileName);
            if (tex != null)
            {
                mat.mainTexture = tex;
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        public static Material CreateShockwaveMaterial(string materialName, string normalMapName, Color ringColor, float bumpStrength)
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

            if (mat.HasProperty("_TintColor")) mat.SetColor("_TintColor", ringColor);
            if (mat.HasProperty("_RingColor")) mat.SetColor("_RingColor", ringColor);
            if (mat.HasProperty("_DistortionStrength")) mat.SetFloat("_DistortionStrength", bumpStrength);
            if (mat.HasProperty("_BumpStrength")) mat.SetFloat("_BumpStrength", bumpStrength);

            Texture2D normTex = FindTexture(normalMapName);
            if (normTex != null)
            {
                mat.mainTexture = normTex;
                if (mat.HasProperty("_NormalMap")) mat.SetTexture("_NormalMap", normTex);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", normTex);
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

            if (mat.HasProperty("_Color")) mat.SetColor("_Color", baseColor);
            if (mat.HasProperty("_BurnColor")) mat.SetColor("_BurnColor", burnColor);

            Texture2D tex = FindTexture(decalTexName);
            if (tex != null)
            {
                mat.mainTexture = tex;
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
            }

            if (!string.IsNullOrEmpty(noiseTexName))
            {
                Texture2D noiseTex = FindTexture(noiseTexName);
                if (noiseTex != null && mat.HasProperty("_NoiseTex")) mat.SetTexture("_NoiseTex", noiseTex);
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
                mat.SetFloat("_Surface", 1.0f);
                mat.SetFloat("_Blend", 1.0f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_ZWrite", 0);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.EnableKeyword("_ALPHABLEND_ON");
            }
            else
            {
                mat.SetFloat("_Surface", 1.0f);
                mat.SetFloat("_Blend", 0.0f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.EnableKeyword("_ALPHABLEND_ON");
            }

            Texture2D tex = FindTexture(textureFileName);
            if (tex != null)
            {
                mat.mainTexture = tex;
                if (mat.HasProperty("_BaseMap"))
                {
                    mat.SetTexture("_BaseMap", tex);
                }
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }
    }
}
