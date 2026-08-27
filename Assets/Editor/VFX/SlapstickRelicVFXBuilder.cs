#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace ProjectZombie.Editor.VFX
{
    /// <summary>
    /// Editor Tool tự động dựng 5 Prefab Particle VFX cho 5 Pháp Bảo Slapstick Dân Gian
    /// Chuẩn URP Mobile, Particle System đa tầng (Core, Glow, Sparks, Debris).
    /// </summary>
    public static class SlapstickRelicVFXBuilder
    {
        private const string PREFAB_DIR = "Assets/VFX/SkillLibrary/Prefabs";
        private const string MAT_DIR = "Assets/VFX/SkillLibrary/Materials";

        [MenuItem("Tools/VFX Generator/🔥 Build 5 Slapstick Relic VFX (1-Click)", false, 10)]
        public static void BuildAllSlapstickVFX()
        {
            if (!Directory.Exists(PREFAB_DIR)) Directory.CreateDirectory(PREFAB_DIR);
            if (!Directory.Exists(MAT_DIR)) Directory.CreateDirectory(MAT_DIR);

            BuildSlipperVFX();
            BuildPotVFX();
            BuildPipeVFX();
            BuildSleepingMatVFX();
            BuildChickenBroomVFX();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=#00FF88>[SlapstickRelicVFXBuilder]</color> 🚀 ĐÃ DỰNG THÀNH CÔNG 5 PREFAB VFX CHO PHÁP BẢO SLAPSTICK!");
        }

        // 1. W_SLIPPER: Lốc Dép Vạn Năng (Kim)
        public static void BuildSlipperVFX()
        {
            string prefabPath = $"{PREFAB_DIR}/VFX_Relic_Slipper_Whirlwind.prefab";
            GameObject root = new GameObject("VFX_Relic_Slipper_Whirlwind");

            var ps = root.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = 0.45f;
            main.startSpeed = 3.5f;
            main.startSize = 1.2f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 6) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1.5f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.85f, 0.2f), 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            var rot = ps.rotationOverLifetime;
            rot.enabled = true;
            rot.z = new ParticleSystem.MinMaxCurve(720f);

            var rend = root.GetComponent<ParticleSystemRenderer>();
            rend.material = GetOrCreateVFXMaterial("MAT_VFX_Slipper_Whirlwind", new Color(1.8f, 1.5f, 0.4f, 1f), true);
            rend.sortingLayerName = "Skill";
            rend.sortingOrder = 10;

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[SlapstickRelicVFXBuilder] Dựng thành công: {prefabPath}");
        }

        // 2. W_POT: Nồi Cơm Hút Quái & Đại Bác (Thổ)
        public static void BuildPotVFX()
        {
            string prefabPath = $"{PREFAB_DIR}/VFX_Relic_Pot_Suction.prefab";
            GameObject root = new GameObject("VFX_Relic_Pot_Suction");

            var ps = root.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.6f;
            main.loop = false;
            main.startLifetime = 0.4f;
            main.startSpeed = -4.0f; // Hút vào tâm
            main.startSize = 0.6f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            var emission = ps.emission;
            emission.rateOverTime = 25;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 2.5f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.85f, 0.65f, 0.35f), 0f), new GradientColorKey(new Color(0.4f, 0.9f, 0.4f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            var rend = root.GetComponent<ParticleSystemRenderer>();
            rend.material = GetOrCreateVFXMaterial("MAT_VFX_Pot_Suction", new Color(1.2f, 0.9f, 0.5f, 1f), true);
            rend.sortingLayerName = "Skill";
            rend.sortingOrder = 10;

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[SlapstickRelicVFXBuilder] Dựng thành công: {prefabPath}");
        }

        // 3. W_PIPE: Mây Khói Thuốc Lào Dân Gian (Hỏa - Chuẩn Phun Cụm Khói Đặc Quánh -> Nở To & Mờ Dần)
        public static void BuildPipeVFX()
        {
            string prefabPath = $"{PREFAB_DIR}/VFX_Relic_Pipe_DragonSmoke.prefab";
            string texPuffPath = "Assets/VFX/SkillLibrary/Textures/Tex_Smoke_Puff_Clean.png";
            string texTendrilPath = "Assets/VFX/SkillLibrary/Textures/Tex_Smoke_Tendril_Clean.png";
            string texEmberPath = "Assets/VFX/SkillLibrary/Textures/Tex_Fire_Ember_Clean.png";

            Texture2D texPuff = AssetDatabase.LoadAssetAtPath<Texture2D>(texPuffPath);
            Texture2D texTendril = AssetDatabase.LoadAssetAtPath<Texture2D>(texTendrilPath);
            Texture2D texEmber = AssetDatabase.LoadAssetAtPath<Texture2D>(texEmberPath);

            // Material AlphaBlend giữ độ đục tự nhiên của khói trắng
            Material matPuff = GetOrCreateTextureMaterial("MAT_VFX_Pipe_SmokeCloud", texPuff, new Color(0.98f, 0.98f, 0.98f, 1.0f), false);
            Material matTendril = GetOrCreateTextureMaterial("MAT_VFX_Pipe_SmokeTendril", texTendril, new Color(0.92f, 0.92f, 0.95f, 0.8f), false);
            Material matEmber = GetOrCreateTextureMaterial("MAT_VFX_Fire_Embers", texEmber, new Color(2.5f, 1.0f, 0.2f, 1.0f), true);

            GameObject root = new GameObject("VFX_Relic_Pipe_Smoke");

            // --- LAYER 1: Cụm Khói Tròn Đậm Đặc Trung Tâm (Dense Puff Cluster) ---
            var psCore = root.AddComponent<ParticleSystem>();
            var main1 = psCore.main;
            main1.duration = 3.5f;
            main1.loop = false;
            main1.startLifetime = 3.0f;
            main1.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.35f);
            main1.startSize = new ParticleSystem.MinMaxCurve(0.7f, 1.1f); // Thu nhỏ vừa vặn (bằng 45% lúc trước)
            main1.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main1.simulationSpace = ParticleSystemSimulationSpace.World;

            var emiss1 = psCore.emission;
            emiss1.rateOverTime = 0;
            // Burst 4 cụm khói nhỏ đan khít nhau
            emiss1.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 4) });

            var shape1 = psCore.shape;
            shape1.shapeType = ParticleSystemShapeType.Circle;
            shape1.radius = 0.25f; // Bán kính tâm nhỏ gọn

            // ĐỘ MỜ THEO THỜI GIAN: Ban đầu 80% Alpha -> Mờ dần về 0%
            var col1 = psCore.colorOverLifetime;
            col1.enabled = true;
            Gradient grad1 = new Gradient();
            grad1.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(new Color(0.98f, 0.98f, 0.98f), 0f), 
                    new GradientColorKey(new Color(0.85f, 0.85f, 0.88f), 0.6f),
                    new GradientColorKey(new Color(0.65f, 0.65f, 0.70f), 1f) 
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(0f, 0f), 
                    new GradientAlphaKey(0.80f, 0.08f), // 80% Alpha vừa đủ đậm rõ nét
                    new GradientAlphaKey(0.70f, 0.45f), 
                    new GradientAlphaKey(0.25f, 0.75f), 
                    new GradientAlphaKey(0f, 1.0f)       
                }
            );
            col1.color = grad1;

            // KÍCH THƯỚC NỞ NHẸ: 1.0 -> 1.35 lần
            var size1 = psCore.sizeOverLifetime;
            size1.enabled = true;
            AnimationCurve curve1 = new AnimationCurve();
            curve1.AddKey(0f, 0.7f);
            curve1.AddKey(0.3f, 1.0f);
            curve1.AddKey(0.7f, 1.2f);
            curve1.AddKey(1f, 1.35f);
            size1.size = new ParticleSystem.MinMaxCurve(1f, curve1);

            var rot1 = psCore.rotationOverLifetime;
            rot1.enabled = true;
            rot1.z = new ParticleSystem.MinMaxCurve(-10f * Mathf.Deg2Rad, 10f * Mathf.Deg2Rad);

            var rend1 = root.GetComponent<ParticleSystemRenderer>();
            rend1.material = matPuff;
            rend1.sortingLayerName = "Skill";
            rend1.sortingOrder = 8; // Nằm DƯỚI nhân vật để không che mặt Tướng

            // --- LAYER 2: Vệt Mây Khói Cuộn Tản Rìa (Realistic Swirling Plumes) ---
            GameObject tendrilObj = new GameObject("Wispy_Smoke_Tendril");
            tendrilObj.transform.SetParent(root.transform, false);

            var psTendril = tendrilObj.AddComponent<ParticleSystem>();
            var main2 = psTendril.main;
            main2.duration = 3.5f;
            main2.loop = false;
            main2.startLifetime = 2.8f;
            main2.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.4f);
            main2.startSize = new ParticleSystem.MinMaxCurve(0.8f, 1.2f); // Kích thước gọn gàng
            main2.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main2.simulationSpace = ParticleSystemSimulationSpace.World;

            var emiss2 = psTendril.emission;
            emiss2.rateOverTime = 0;
            // Burst 2-3 cụm khói xoắn lượn
            emiss2.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.02f, 3) });

            var shape2 = psTendril.shape;
            shape2.shapeType = ParticleSystemShapeType.Circle;
            shape2.radius = 0.35f;

            var col2 = psTendril.colorOverLifetime;
            col2.enabled = true;
            col2.color = grad1;

            var size2 = psTendril.sizeOverLifetime;
            size2.enabled = true;
            size2.size = new ParticleSystem.MinMaxCurve(1f, curve1);

            var rot2 = psTendril.rotationOverLifetime;
            rot2.enabled = true;
            rot2.z = new ParticleSystem.MinMaxCurve(-12f * Mathf.Deg2Rad, 12f * Mathf.Deg2Rad);

            var rend2 = tendrilObj.GetComponent<ParticleSystemRenderer>();
            rend2.material = matTendril;
            rend2.sortingLayerName = "Skill";
            rend2.sortingOrder = 8; // Dưới nhân vật

            // --- LAYER 3: Tàn Than Hồng Bắn Ra Lúc Đầu (Red Fire Embers) ---
            GameObject embersObj = new GameObject("Red_Fire_Embers");
            embersObj.transform.SetParent(root.transform, false);

            var psEmbers = embersObj.AddComponent<ParticleSystem>();
            var main3 = psEmbers.main;
            main3.duration = 3.5f;
            main3.loop = false;
            main3.startLifetime = 1.0f;
            main3.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.0f);
            main3.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.12f); // Siêu nhỏ li ti
            main3.simulationSpace = ParticleSystemSimulationSpace.World;

            var emiss3 = psEmbers.emission;
            emiss3.rateOverTime = 0;
            emiss3.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 4) });

            var shape3 = psEmbers.shape;
            shape3.shapeType = ParticleSystemShapeType.Circle;
            shape3.radius = 0.2f;

            var col3 = psEmbers.colorOverLifetime;
            col3.enabled = true;
            Gradient grad3 = new Gradient();
            grad3.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.4f, 0.1f), 0f), new GradientColorKey(new Color(1f, 0.8f, 0.2f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            col3.color = grad3;

            var rend3 = embersObj.GetComponent<ParticleSystemRenderer>();
            rend3.material = matEmber;
            rend3.sortingLayerName = "Skill";
            rend3.sortingOrder = 9;

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[SlapstickRelicVFXBuilder] 💨 Dựng thành công CỤM KHÓI ĐẶC QUÁNH ĐỨNG YÊN tại: {prefabPath}");
        }

        private static Material GetOrCreateTextureMaterial(string matName, Texture2D tex, Color tintColor, bool isAdditive)
        {
            string path = $"{MAT_DIR}/{matName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            
            Shader shader = Shader.Find("ProjectZombie/VFX/Slash_Additive");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.shader = shader;
            }

            if (mat != null)
            {
                if (tex != null)
                {
                    if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                    if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                }
                if (mat.HasProperty("_CoreColor")) mat.SetColor("_CoreColor", tintColor);
                if (mat.HasProperty("_EdgeColor")) mat.SetColor("_EdgeColor", tintColor);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tintColor);
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", tintColor);
                
                // Cấu hình Blend: Nếu không phải Additive thì dùng AlphaBlend tiêu chuẩn (SrcAlpha, OneMinusSrcAlpha = 5, 10)
                if (mat.HasProperty("_SrcBlend")) mat.SetFloat("_SrcBlend", 5f); // SrcAlpha
                if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", isAdditive ? 1f : 10f); // 1: Additive, 10: OneMinusSrcAlpha (AlphaBlend thuần)
                EditorUtility.SetDirty(mat);
            }
            return mat;
        }

        // 4. R007: Chiếu Trải Hoàng Tuyền (Mộc)
        public static void BuildSleepingMatVFX()
        {
            string prefabPath = $"{PREFAB_DIR}/VFX_Relic_SleepingMat_Decal.prefab";
            GameObject root = new GameObject("VFX_Relic_SleepingMat_Decal");

            var ps = root.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = 3.0f;
            main.startSpeed = 0f;
            main.startSize = 3.5f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1) });

            var shape = ps.shape;
            shape.enabled = false;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.4f, 0.9f, 0.5f), 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0.9f, 0.7f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            var rend = root.GetComponent<ParticleSystemRenderer>();
            rend.material = GetOrCreateVFXMaterial("MAT_VFX_SleepingMat", new Color(0.5f, 1.4f, 0.7f, 1f), true);
            rend.sortingLayerName = "Skill";
            rend.sortingOrder = 2; // Decal sát mặt đất

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[SlapstickRelicVFXBuilder] Dựng thành công: {prefabPath}");
        }

        // 5. R008: Chổi Lông Gà Giáng Trời (Kim)
        public static void BuildChickenBroomVFX()
        {
            string prefabPath = $"{PREFAB_DIR}/VFX_Relic_ChickenBroom_Smash.prefab";
            GameObject root = new GameObject("VFX_Relic_ChickenBroom_Smash");

            var ps = root.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.4f;
            main.loop = false;
            main.startLifetime = 0.35f;
            main.startSpeed = 7.0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 18) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.5f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.8f, 0.2f), 0f), new GradientColorKey(new Color(0.9f, 0.3f, 0.2f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            var rend = root.GetComponent<ParticleSystemRenderer>();
            rend.material = GetOrCreateVFXMaterial("MAT_VFX_ChickenBroom_Feathers", new Color(1.8f, 1.2f, 0.4f, 1f), true);
            rend.sortingLayerName = "Skill";
            rend.sortingOrder = 12;

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[SlapstickRelicVFXBuilder] Dựng thành công: {prefabPath}");
        }

        private static Material GetOrCreateVFXMaterial(string matName, Color coreColor, bool isAdditive)
        {
            string path = $"{MAT_DIR}/{matName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = isAdditive 
                    ? (Shader.Find("ProjectZombie/VFX/Slash_Additive") ?? Shader.Find("Universal Render Pipeline/Particles/Unlit"))
                    : Shader.Find("Universal Render Pipeline/Particles/Unlit");

                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }

            if (mat != null)
            {
                if (mat.HasProperty("_CoreColor")) mat.SetColor("_CoreColor", coreColor);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", coreColor);
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", coreColor);
                EditorUtility.SetDirty(mat);
            }
            return mat;
        }
    }
}
#endif
