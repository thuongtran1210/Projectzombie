#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using ProjectZombie.Features.Weapons;

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

        [MenuItem("Tools/VFX Generator/Slapstick Relics/1. 🩴 Build W_SLIPPER (Dép Tổ Ong)", false, 1)]
        public static void BuildSlipperVFX()
        {
            EnsureDirectories();
            string prefabPath = $"{PREFAB_DIR}/VFX_Relic_Slipper_Whirlwind.prefab";
            string texSlipperPath = "Assets/VFX/SkillLibrary/Textures/Tex_Slipper_Projectile.png";
            string texArcPath = "Assets/VFX/SkillLibrary/Textures/Tex_Slipper_VortexArc.png";
            string texDropsPath = "Assets/VFX/SkillLibrary/Textures/Tex_Slipper_ComicDrops.png";
            string texRingPath = "Assets/Art/VFX/Tex_VFX_Cinnabar_Shockwave_Ring.png";

            Texture2D texSlipper = AssetDatabase.LoadAssetAtPath<Texture2D>(texSlipperPath);
            Texture2D texArc = AssetDatabase.LoadAssetAtPath<Texture2D>(texArcPath);
            Texture2D texDrops = AssetDatabase.LoadAssetAtPath<Texture2D>(texDropsPath);
            Texture2D texRing = AssetDatabase.LoadAssetAtPath<Texture2D>(texRingPath);

            Material matSlipper = GetOrCreateTextureMaterial("MAT_VFX_Slipper_Item", texSlipper, Color.white, false);
            Material matArc = GetOrCreateTextureMaterial("MAT_VFX_Slipper_Arc", texArc, new Color(3.2f, 2.6f, 0.9f, 1.0f), true);
            Material matWhirl = GetOrCreateTextureMaterial("MAT_VFX_Slipper_Whirlwind", texArc, new Color(3.5f, 3.0f, 1.2f, 1.0f), true);
            Material matDrops = GetOrCreateTextureMaterial("MAT_VFX_Slipper_Drops", texDrops, new Color(3.2f, 3.0f, 1.5f, 1.0f), true);

            GameObject root = new GameObject("VFX_Relic_Slipper_Whirlwind");

            // --- LAYER 1: Lõi Sáng Bùng Nổ (Golden Core Flash) ---
            var psCore = root.AddComponent<ParticleSystem>();
            var main1 = psCore.main;
            main1.duration = 0.85f;
            main1.loop = false;
            main1.startLifetime = 0.4f;
            main1.startSpeed = 0f;
            main1.startSize = new ParticleSystem.MinMaxCurve(2.0f, 2.6f);
            main1.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main1.simulationSpace = ParticleSystemSimulationSpace.World;

            var emiss1 = psCore.emission;
            emiss1.rateOverTime = 0;
            emiss1.SetBursts(new ParticleSystem.Burst[] { 
                new ParticleSystem.Burst(0.0f, 2),
                new ParticleSystem.Burst(0.15f, 2)
            });

            var col1 = psCore.colorOverLifetime;
            col1.enabled = true;
            Gradient gradCore = new Gradient();
            gradCore.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 1f, 0.8f), 0f), new GradientColorKey(new Color(1f, 0.8f, 0.2f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            col1.color = gradCore;

            var rend1 = root.GetComponent<ParticleSystemRenderer>();
            rend1.material = matWhirl;
            rend1.sortingLayerName = "Skill";
            rend1.sortingOrder = 8;

            // --- LAYER 2: Lưỡi Kiếm Khí Xoáy Chém 3 Tầng (Vortex Blade Arcs) ---
            GameObject vortexObj = new GameObject("Vortex_Blade_Arcs");
            vortexObj.transform.SetParent(root.transform, false);

            var psVortex = vortexObj.AddComponent<ParticleSystem>();
            var main2 = psVortex.main;
            main2.duration = 0.85f;
            main2.loop = false;
            main2.startLifetime = 0.65f;
            main2.startSpeed = 0f;
            main2.startSize = new ParticleSystem.MinMaxCurve(3.2f, 4.2f);
            main2.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main2.simulationSpace = ParticleSystemSimulationSpace.World;

            var emiss2 = psVortex.emission;
            emiss2.rateOverTime = 0;
            emiss2.SetBursts(new ParticleSystem.Burst[] { 
                new ParticleSystem.Burst(0.0f, 2),
                new ParticleSystem.Burst(0.12f, 3),
                new ParticleSystem.Burst(0.25f, 3)
            });

            var sol2 = psVortex.sizeOverLifetime;
            sol2.enabled = true;
            AnimationCurve curveSize = new AnimationCurve();
            curveSize.AddKey(0f, 0.4f);
            curveSize.AddKey(0.25f, 1.0f);
            curveSize.AddKey(1f, 1.25f);
            sol2.size = new ParticleSystem.MinMaxCurve(1f, curveSize);

            var rot2 = psVortex.rotationOverLifetime;
            rot2.enabled = true;
            rot2.z = new ParticleSystem.MinMaxCurve(1080f * Mathf.Deg2Rad, 1440f * Mathf.Deg2Rad);

            var col2 = psVortex.colorOverLifetime;
            col2.enabled = true;
            Gradient gradArc = new Gradient();
            gradArc.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.95f, 0.5f), 0f), new GradientColorKey(new Color(1f, 0.6f, 0.1f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(0f, 1f) }
            );
            col2.color = gradArc;

            var rend2 = vortexObj.GetComponent<ParticleSystemRenderer>();
            rend2.material = matArc;
            rend2.sortingLayerName = "Skill";
            rend2.sortingOrder = 9;

            // --- LAYER 3: Dép Vàng Tổ Ong Lộn Nhào 360 Độ (Flying Chibi Slippers) ---
            GameObject slippersObj = new GameObject("Flying_Slippers");
            slippersObj.transform.SetParent(root.transform, false);

            var psSlippers = slippersObj.AddComponent<ParticleSystem>();
            var main3 = psSlippers.main;
            main3.duration = 0.85f;
            main3.loop = false;
            main3.startLifetime = 0.75f;
            main3.startSpeed = new ParticleSystem.MinMaxCurve(3.0f, 5.5f);
            main3.startSize = new ParticleSystem.MinMaxCurve(0.7f, 1.0f);
            main3.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main3.simulationSpace = ParticleSystemSimulationSpace.World;

            var emiss3 = psSlippers.emission;
            emiss3.rateOverTime = 0;
            emiss3.SetBursts(new ParticleSystem.Burst[] { 
                new ParticleSystem.Burst(0.02f, 5),
                new ParticleSystem.Burst(0.15f, 5)
            });

            var shape3 = psSlippers.shape;
            shape3.shapeType = ParticleSystemShapeType.Circle;
            shape3.radius = 1.1f;

            var rot3 = psSlippers.rotationOverLifetime;
            rot3.enabled = true;
            rot3.z = new ParticleSystem.MinMaxCurve(-1080f * Mathf.Deg2Rad, 1080f * Mathf.Deg2Rad);

            var col3 = psSlippers.colorOverLifetime;
            col3.enabled = true;
            col3.color = gradCore;

            var rend3 = slippersObj.GetComponent<ParticleSystemRenderer>();
            rend3.material = matSlipper;
            rend3.sortingLayerName = "Skill";
            rend3.sortingOrder = 11;

            // --- LAYER 4: Giọt Mồ Hôi & Bụi Vàng Quê Độ (Comic Sweat Drops & Radiant Sparkles) ---
            GameObject dropsObj = new GameObject("Comic_Sweat_Sparkles");
            dropsObj.transform.SetParent(root.transform, false);

            var psDrops = dropsObj.AddComponent<ParticleSystem>();
            var main4 = psDrops.main;
            main4.duration = 0.85f;
            main4.loop = false;
            main4.startLifetime = 0.65f;
            main4.startSpeed = new ParticleSystem.MinMaxCurve(2.0f, 4.5f);
            main4.startSize = new ParticleSystem.MinMaxCurve(0.45f, 0.8f);
            main4.simulationSpace = ParticleSystemSimulationSpace.World;

            var emiss4 = psDrops.emission;
            emiss4.rateOverTime = 0;
            emiss4.SetBursts(new ParticleSystem.Burst[] { 
                new ParticleSystem.Burst(0.05f, 10),
                new ParticleSystem.Burst(0.2f, 12)
            });

            var shape4 = psDrops.shape;
            shape4.shapeType = ParticleSystemShapeType.Circle;
            shape4.radius = 1.4f;

            var col4 = psDrops.colorOverLifetime;
            col4.enabled = true;
            col4.color = gradArc;

            var rend4 = dropsObj.GetComponent<ParticleSystemRenderer>();
            rend4.material = matDrops;
            rend4.sortingLayerName = "Skill";
            rend4.sortingOrder = 12;

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (savedPrefab != null) EditorGUIUtility.PingObject(savedPrefab);
            Debug.Log($"<color=#00FF88>[SlapstickRelicVFXBuilder]</color> 🩴 Dựng thành công Prefab Lốc Dép Vạn Năng (High Polish): {prefabPath}");
        }

        // 2. W_POT: Nồi Cơm Hút Quái & Đại Bác (Thổ - Chuẩn Multi-Layer AAA Anime VFX)
        [MenuItem("Tools/VFX Generator/Slapstick Relics/2. 🍚 Build W_POT (Nồi Cơm Thạch Sanh)", false, 2)]
        public static void BuildPotVFX()
        {
            EnsureDirectories();
            string prefabPath = $"{PREFAB_DIR}/VFX_Relic_Pot_Suction.prefab";
            string texVortexPath = "Assets/Art/Weapons/VFX/Tex_Pot_Suction_Vortex.png";
            string texRicePath = "Assets/Art/Weapons/VFX/Tex_Rice_Collectible.png";
            string texSmokePath = "Assets/VFX/SkillLibrary/Textures/Tex_Smoke_BigCloud.png";
            string texRingPath = "Assets/Art/VFX/Tex_VFX_Cinnabar_Shockwave_Ring.png";

            Texture2D texVortex = AssetDatabase.LoadAssetAtPath<Texture2D>(texVortexPath);
            Texture2D texRice = AssetDatabase.LoadAssetAtPath<Texture2D>(texRicePath);
            Texture2D texSmoke = AssetDatabase.LoadAssetAtPath<Texture2D>(texSmokePath);
            Texture2D texRing = AssetDatabase.LoadAssetAtPath<Texture2D>(texRingPath);

            Material matVortex = GetOrCreateTextureMaterial("MAT_VFX_Pot_Suction", texVortex, new Color(3.2f, 2.4f, 0.8f, 1.0f), true);
            Material matRice = GetOrCreateTextureMaterial("M_Rice_Collectible", texRice, Color.white, false);
            Material matSmoke = GetOrCreateTextureMaterial("MAT_VFX_Pipe_SmokeCloud", texSmoke, new Color(1.8f, 1.5f, 1.1f, 1.0f), true);

            GameObject root = new GameObject("VFX_Relic_Pot_Suction");

            // --- LAYER 1: Vòng Xoáy Hút Chân Không Hoàng Thổ (Golden Earth Suction Spiral) ---
            var psVortex = root.AddComponent<ParticleSystem>();
            var main1 = psVortex.main;
            main1.duration = 0.85f;
            main1.loop = false;
            main1.startLifetime = 0.75f;
            main1.startSpeed = 0f;
            main1.startSize = new ParticleSystem.MinMaxCurve(3.8f, 5.0f);
            main1.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main1.simulationSpace = ParticleSystemSimulationSpace.World;

            var emiss1 = psVortex.emission;
            emiss1.rateOverTime = 0;
            emiss1.SetBursts(new ParticleSystem.Burst[] { 
                new ParticleSystem.Burst(0.0f, 2),
                new ParticleSystem.Burst(0.15f, 2),
                new ParticleSystem.Burst(0.35f, 2)
            });

            var sol1 = psVortex.sizeOverLifetime;
            sol1.enabled = true;
            AnimationCurve curveVortex = new AnimationCurve();
            curveVortex.AddKey(0f, 1.2f);
            curveVortex.AddKey(0.7f, 0.6f);
            curveVortex.AddKey(1f, 0.1f); // Co rút hút thẳng vào tâm
            sol1.size = new ParticleSystem.MinMaxCurve(1f, curveVortex);

            var rot1 = psVortex.rotationOverLifetime;
            rot1.enabled = true;
            rot1.z = new ParticleSystem.MinMaxCurve(720f * Mathf.Deg2Rad, 1440f * Mathf.Deg2Rad);

            var col1 = psVortex.colorOverLifetime;
            col1.enabled = true;
            Gradient gradEarth = new Gradient();
            gradEarth.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.85f, 0.4f), 0f), new GradientColorKey(new Color(0.85f, 0.45f, 0.1f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(0f, 1f) }
            );
            col1.color = gradEarth;

            var rend1 = root.GetComponent<ParticleSystemRenderer>();
            rend1.material = matVortex;
            rend1.sortingLayerName = "Skill";
            rend1.sortingOrder = 9;

            // --- LAYER 2: Hạt Bụi Năng Lượng & Trọng Lực Hút Vào Tâm (Inward Vacuum Particles) ---
            GameObject inwardObj = new GameObject("Inward_Vacuum_Embers");
            inwardObj.transform.SetParent(root.transform, false);

            var psInward = inwardObj.AddComponent<ParticleSystem>();
            var main2 = psInward.main;
            main2.duration = 0.85f;
            main2.loop = false;
            main2.startLifetime = 0.45f;
            main2.startSpeed = -6.5f; // Tốc độ hút cực mạnh vào tâm
            main2.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
            main2.simulationSpace = ParticleSystemSimulationSpace.Local;

            var emiss2 = psInward.emission;
            emiss2.rateOverTime = 40;

            var shape2 = psInward.shape;
            shape2.shapeType = ParticleSystemShapeType.Circle;
            shape2.radius = 3.2f;

            var col2 = psInward.colorOverLifetime;
            col2.enabled = true;
            col2.color = gradEarth;

            var rend2 = inwardObj.GetComponent<ParticleSystemRenderer>();
            rend2.material = matVortex;
            rend2.sortingLayerName = "Skill";
            rend2.sortingOrder = 10;

            // --- LAYER 3: Hạt Cơm Tiên Phát Sáng Bay Xoáy Vào Lòng Nồi (Flying Rice Collectibles) ---
            GameObject riceObj = new GameObject("Flying_Rice_Grains");
            riceObj.transform.SetParent(root.transform, false);

            var psRice = riceObj.AddComponent<ParticleSystem>();
            var main3 = psRice.main;
            main3.duration = 0.85f;
            main3.loop = false;
            main3.startLifetime = 0.55f;
            main3.startSpeed = -4.5f;
            main3.startSize = new ParticleSystem.MinMaxCurve(0.5f, 0.8f);
            main3.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main3.simulationSpace = ParticleSystemSimulationSpace.Local;

            var emiss3 = psRice.emission;
            emiss3.rateOverTime = 0;
            emiss3.SetBursts(new ParticleSystem.Burst[] { 
                new ParticleSystem.Burst(0.05f, 6),
                new ParticleSystem.Burst(0.25f, 8)
            });

            var shape3 = psRice.shape;
            shape3.shapeType = ParticleSystemShapeType.Circle;
            shape3.radius = 2.8f;

            var rot3 = psRice.rotationOverLifetime;
            rot3.enabled = true;
            rot3.z = new ParticleSystem.MinMaxCurve(-720f * Mathf.Deg2Rad, 720f * Mathf.Deg2Rad);

            var rend3 = riceObj.GetComponent<ParticleSystemRenderer>();
            rend3.material = matRice;
            rend3.sortingLayerName = "Skill";
            rend3.sortingOrder = 11;

            // --- LAYER 4: Khói Áp Suất & Sóng Hơi Nóng (Steam Shockwave Puff) ---
            GameObject steamObj = new GameObject("Steam_Shockwave");
            steamObj.transform.SetParent(root.transform, false);

            var psSteam = steamObj.AddComponent<ParticleSystem>();
            var main4 = psSteam.main;
            main4.duration = 0.85f;
            main4.loop = false;
            main4.startLifetime = 0.5f;
            main4.startSpeed = new ParticleSystem.MinMaxCurve(3.0f, 6.0f);
            main4.startSize = new ParticleSystem.MinMaxCurve(1.0f, 1.8f);
            main4.simulationSpace = ParticleSystemSimulationSpace.World;

            var emiss4 = psSteam.emission;
            emiss4.rateOverTime = 0;
            emiss4.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.4f, 10) }); // Phụt khói áp suất khi bắn quái

            var shape4 = psSteam.shape;
            shape4.shapeType = ParticleSystemShapeType.Circle;
            shape4.radius = 0.8f;

            var col4 = psSteam.colorOverLifetime;
            col4.enabled = true;
            Gradient gradSteam = new Gradient();
            gradSteam.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.95f, 0.8f), 0f), new GradientColorKey(new Color(0.8f, 0.7f, 0.5f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            col4.color = gradSteam;

            var rend4 = steamObj.GetComponent<ParticleSystemRenderer>();
            rend4.material = matSmoke;
            rend4.sortingLayerName = "Skill";
            rend4.sortingOrder = 12;

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (savedPrefab != null) EditorGUIUtility.PingObject(savedPrefab);
            Debug.Log($"<color=#00FF88>[SlapstickRelicVFXBuilder]</color> 🍚 Dựng thành công Prefab Nồi Cơm Hút Chân Không (High Polish): {prefabPath}");
        }

        // 3. W_PIPE: Mây Khói Thuốc Lào Dân Gian (Hỏa - Chuẩn Phun Cụm Khói Đặc Quánh -> Nở To & Mờ Dần)
        [MenuItem("Tools/VFX Generator/Slapstick Relics/3. 💨 Build W_PIPE (Điếu Cày Thuốc Lào)", false, 3)]
        public static void BuildPipeVFX()
        {
            EnsureDirectories();
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
            main1.startSize = new ParticleSystem.MinMaxCurve(0.7f, 1.1f);
            main1.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main1.simulationSpace = ParticleSystemSimulationSpace.World;

            var emiss1 = psCore.emission;
            emiss1.rateOverTime = 0;
            emiss1.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 4) });

            var shape1 = psCore.shape;
            shape1.shapeType = ParticleSystemShapeType.Circle;
            shape1.radius = 0.25f;

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
                    new GradientAlphaKey(0.80f, 0.08f),
                    new GradientAlphaKey(0.70f, 0.45f), 
                    new GradientAlphaKey(0.25f, 0.75f), 
                    new GradientAlphaKey(0f, 1.0f)       
                }
            );
            col1.color = grad1;

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
            rend1.sortingOrder = 8;

            // --- LAYER 2: Vệt Mây Khói Cuộn Tản Rìa (Realistic Swirling Plumes) ---
            GameObject tendrilObj = new GameObject("Wispy_Smoke_Tendril");
            tendrilObj.transform.SetParent(root.transform, false);

            var psTendril = tendrilObj.AddComponent<ParticleSystem>();
            var main2 = psTendril.main;
            main2.duration = 3.5f;
            main2.loop = false;
            main2.startLifetime = 2.8f;
            main2.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.4f);
            main2.startSize = new ParticleSystem.MinMaxCurve(0.8f, 1.2f);
            main2.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main2.simulationSpace = ParticleSystemSimulationSpace.World;

            var emiss2 = psTendril.emission;
            emiss2.rateOverTime = 0;
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
            rend2.sortingOrder = 8;

            // --- LAYER 3: Tàn Than Hồng Bắn Ra Lúc Đầu (Red Fire Embers) ---
            GameObject embersObj = new GameObject("Red_Fire_Embers");
            embersObj.transform.SetParent(root.transform, false);

            var psEmbers = embersObj.AddComponent<ParticleSystem>();
            var main3 = psEmbers.main;
            main3.duration = 3.5f;
            main3.loop = false;
            main3.startLifetime = 1.0f;
            main3.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.0f);
            main3.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.12f);
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
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var savedPipePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (savedPipePrefab != null) EditorGUIUtility.PingObject(savedPipePrefab);
            Debug.Log($"<color=#00FF88>[SlapstickRelicVFXBuilder]</color> 💨 Dựng thành công CỤM KHÓI ĐẶC QUÁNH ĐỨNG YÊN tại: {prefabPath}");
        }

        // 4. R007: Chiếu Trải Hoàng Tuyền (Mộc - Bẫy Ngủ & Đường Trượt Bowling Siêu Tốc)
        [MenuItem("Tools/VFX Generator/Slapstick Relics/4. 🎋 Build W_MAT (Chiếu Cói)", false, 4)]
        public static void BuildSleepingMatVFX()
        {
            EnsureDirectories();
            string prefabPath = $"{PREFAB_DIR}/VFX_Relic_SleepingMat_Decal.prefab";
            string slideHitPrefabPath = $"{PREFAB_DIR}/VFX_Relic_SleepingMat_SlideHit.prefab";

            // Load Textures
            string texMatPath = "Assets/VFX/SkillLibrary/Textures/Tex_SleepingMat_Mat_Clean.png";
            string texZzzPath = "Assets/VFX/SkillLibrary/Textures/Tex_SleepingMat_SleepZzz_Clean.png";
            string texBubblePath = "Assets/VFX/SkillLibrary/Textures/Tex_SleepingMat_Bubble_Clean.png";
            string texMistPath = "Assets/VFX/SkillLibrary/Textures/Tex_SleepingMat_DreamMist.png";
            string texStrikePath = "Assets/VFX/SkillLibrary/Textures/Tex_SleepingMat_StrikeImpact.png";

            Texture2D texMat = AssetDatabase.LoadAssetAtPath<Texture2D>(texMatPath);
            Texture2D texZzz = AssetDatabase.LoadAssetAtPath<Texture2D>(texZzzPath);
            Texture2D texBubble = AssetDatabase.LoadAssetAtPath<Texture2D>(texBubblePath);
            Texture2D texMist = AssetDatabase.LoadAssetAtPath<Texture2D>(texMistPath);
            Texture2D texStrike = AssetDatabase.LoadAssetAtPath<Texture2D>(texStrikePath);

            // Create Materials
            Material matDecal = GetOrCreateTextureMaterial("MAT_VFX_SleepingMat_Decal", texMat, new Color(1.1f, 1.1f, 1.1f, 1f), false);
            Material matZzz = GetOrCreateTextureMaterial("MAT_VFX_SleepingMat_Zzz", texZzz, new Color(0.8f, 1.8f, 1.3f, 1f), true);
            Material matBubble = GetOrCreateTextureMaterial("MAT_VFX_SleepingMat_Bubble", texBubble, new Color(1f, 1.5f, 1.3f, 0.9f), false);
            Material matMist = GetOrCreateTextureMaterial("MAT_VFX_SleepingMat_DreamMist", texMist, new Color(0.6f, 1.4f, 0.9f, 0.65f), false);
            Material matStrike = GetOrCreateTextureMaterial("MAT_VFX_SleepingMat_StrikeImpact", texStrike, new Color(2.5f, 2.0f, 0.8f, 1f), true);

            // =========================================================================
            // PREFAB 1: TẤM CHIẾU CÓI HOÀNG TUYỀN (GROUND DECAL & SLEEP AURA)
            // =========================================================================
            GameObject root = new GameObject("VFX_Relic_SleepingMat_Decal");

            // --- TẦNG 1: TẤM CHIẾU CÓI TRẢI BUNG TRÊN MẶT ĐẤT (NẰM DƯỚI CHÂN NHÂN VẬT) ---
            var ps = root.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 4.0f;
            main.loop = false;
            main.startLifetime = 4.0f;
            main.startSpeed = 0f;
            main.startSize3D = true;
            main.startSizeX = 3.0f;
            main.startSizeY = 1.8f;
            main.startSizeZ = 1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1) });

            var shape = ps.shape;
            shape.enabled = false;

            // Fade in mượt lúc bung chiếu, sáng ổn định, fade out lúc tan biến
            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.9f, 1.1f, 0.95f), 0.9f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.05f), new GradientAlphaKey(1f, 0.9f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            // Unroll Pop-in effect
            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            AnimationCurve curveScale = new AnimationCurve();
            curveScale.AddKey(0f, 0.3f);
            curveScale.AddKey(0.08f, 1.05f);
            curveScale.AddKey(0.12f, 1.0f);
            curveScale.AddKey(1.0f, 1.0f);
            sol.size = new ParticleSystem.MinMaxCurve(1f, curveScale);

            var rend = root.GetComponent<ParticleSystemRenderer>();
            rend.material = matDecal;
            rend.sortingLayerName = "Tilemap_Decals"; // Luôn nằm sát mặt đất dưới chân nhân vật và quái
            rend.sortingOrder = 1;

            // --- TẦNG 2: LÀN KHÓI HƯƠNG THẢO MỘC RU NGỦ (DREAM MIST AURA) ---
            GameObject mistObj = new GameObject("DreamMist_Aura");
            mistObj.transform.SetParent(root.transform, false);
            var psMist = mistObj.AddComponent<ParticleSystem>();
            var mainMist = psMist.main;
            mainMist.duration = 4.0f;
            mainMist.loop = false;
            mainMist.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 2.0f);
            mainMist.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
            mainMist.startSize = new ParticleSystem.MinMaxCurve(1.0f, 1.5f);
            mainMist.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            mainMist.simulationSpace = ParticleSystemSimulationSpace.World;

            var emissMist = psMist.emission;
            emissMist.rateOverTime = 4;

            var shapeMist = psMist.shape;
            shapeMist.enabled = true;
            shapeMist.shapeType = ParticleSystemShapeType.Box;
            shapeMist.scale = new Vector3(2.6f, 1.4f, 0.1f);

            var colMist = psMist.colorOverLifetime;
            colMist.enabled = true;
            Gradient gradMist = new Gradient();
            gradMist.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.5f, 1.3f, 0.8f), 0f), new GradientColorKey(new Color(0.7f, 1.2f, 0.6f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.5f, 0.3f), new GradientAlphaKey(0.4f, 0.7f), new GradientAlphaKey(0f, 1f) }
            );
            colMist.color = gradMist;

            var rendMist = mistObj.GetComponent<ParticleSystemRenderer>();
            rendMist.material = matMist;
            rendMist.sortingLayerName = "Tilemap_Decals";
            rendMist.sortingOrder = 3;

            // --- TẦNG 3: KÝ TỰ COMIC \"Zzz\" BAY BỒNG BỀNH (FLOATING ZZZ) ---
            GameObject zzzObj = new GameObject("Sleep_Zzz_Particles");
            zzzObj.transform.SetParent(root.transform, false);
            var psZzz = zzzObj.AddComponent<ParticleSystem>();
            var mainZzz = psZzz.main;
            mainZzz.duration = 5.0f;
            mainZzz.loop = false;
            mainZzz.startLifetime = new ParticleSystem.MinMaxCurve(1.6f, 2.2f);
            mainZzz.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
            mainZzz.startSize = new ParticleSystem.MinMaxCurve(0.6f, 0.9f);
            mainZzz.simulationSpace = ParticleSystemSimulationSpace.World;

            var emissZzz = psZzz.emission;
            emissZzz.rateOverTime = 3;

            var shapeZzz = psZzz.shape;
            shapeZzz.enabled = true;
            shapeZzz.shapeType = ParticleSystemShapeType.Rectangle;
            shapeZzz.scale = new Vector3(2.6f, 1.4f, 0.1f);
            shapeZzz.rotation = new Vector3(-90f, 0f, 0f);

            var velZzz = psZzz.velocityOverLifetime;
            velZzz.enabled = true;
            velZzz.x = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
            velZzz.y = new ParticleSystem.MinMaxCurve(0.6f, 1.0f);
            velZzz.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            var colZzz = psZzz.colorOverLifetime;
            colZzz.enabled = true;
            Gradient gradZzz = new Gradient();
            gradZzz.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.7f, 1.5f, 1.1f), 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) }
            );
            colZzz.color = gradZzz;

            var solZzz = psZzz.sizeOverLifetime;
            solZzz.enabled = true;
            AnimationCurve curveZzz = new AnimationCurve();
            curveZzz.AddKey(0f, 0.5f);
            curveZzz.AddKey(0.5f, 1.1f);
            curveZzz.AddKey(1f, 0.8f);
            solZzz.size = new ParticleSystem.MinMaxCurve(1f, curveZzz);

            var rendZzz = zzzObj.GetComponent<ParticleSystemRenderer>();
            rendZzz.material = matZzz;
            rendZzz.sortingLayerName = "Skill";
            rendZzz.sortingOrder = 5;

            // --- TẦNG 4: BONG BÓNG NGỦ PHẬP PHỒNG (SLEEP BUBBLES) ---
            GameObject bubbleObj = new GameObject("Sleep_Bubbles");
            bubbleObj.transform.SetParent(root.transform, false);
            var psBubble = bubbleObj.AddComponent<ParticleSystem>();
            var mainBubble = psBubble.main;
            mainBubble.duration = 5.0f;
            mainBubble.loop = false;
            mainBubble.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 1.8f);
            mainBubble.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            mainBubble.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
            mainBubble.simulationSpace = ParticleSystemSimulationSpace.World;

            var emissBubble = psBubble.emission;
            emissBubble.rateOverTime = 2.5f;

            var shapeBubble = psBubble.shape;
            shapeBubble.enabled = true;
            shapeBubble.shapeType = ParticleSystemShapeType.Rectangle;
            shapeBubble.scale = new Vector3(2.4f, 1.2f, 0.1f);
            shapeBubble.rotation = new Vector3(-90f, 0f, 0f);

            var velBubble = psBubble.velocityOverLifetime;
            velBubble.enabled = true;
            velBubble.x = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);
            velBubble.y = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
            velBubble.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            var colBubble = psBubble.colorOverLifetime;
            colBubble.enabled = true;
            Gradient gradBubble = new Gradient();
            gradBubble.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.85f, 0.2f), new GradientAlphaKey(0.85f, 0.8f), new GradientAlphaKey(0f, 1f) }
            );
            colBubble.color = gradBubble;

            var rendBubble = bubbleObj.GetComponent<ParticleSystemRenderer>();
            rendBubble.material = matBubble;
            rendBubble.sortingLayerName = "Skill";
            rendBubble.sortingOrder = 6;

            // Lưu Prefab 1
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            // =========================================================================
            // PREFAB 2: HIỆU ỨNG ỦI BOWLING STRIKE (SLIDE HIT IMPACT - 0.6S)
            // =========================================================================
            GameObject rootHit = new GameObject("VFX_Relic_SleepingMat_SlideHit");

            // --- TẦNG 1: TIA SAO NỔ VA CHẠM (STRIKE FLASH) ---
            var psHit = rootHit.AddComponent<ParticleSystem>();
            var mainHit = psHit.main;
            mainHit.duration = 0.5f;
            mainHit.loop = false;
            mainHit.startLifetime = 0.35f;
            mainHit.startSpeed = 0f;
            mainHit.startSize = 2.4f;
            mainHit.simulationSpace = ParticleSystemSimulationSpace.Local;

            var emissHit = psHit.emission;
            emissHit.rateOverTime = 0;
            emissHit.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1) });

            var shapeHit = psHit.shape;
            shapeHit.enabled = false;

            var colHit = psHit.colorOverLifetime;
            colHit.enabled = true;
            Gradient gradHit = new Gradient();
            gradHit.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(2.5f, 2.0f, 0.8f), 0f), new GradientColorKey(new Color(2.0f, 0.8f, 0.3f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.9f, 0.5f), new GradientAlphaKey(0f, 1f) }
            );
            colHit.color = gradHit;

            var solHit = psHit.sizeOverLifetime;
            solHit.enabled = true;
            AnimationCurve curveHit = new AnimationCurve();
            curveHit.AddKey(0f, 0.4f);
            curveHit.AddKey(0.2f, 1.2f);
            curveHit.AddKey(1f, 0.9f);
            solHit.size = new ParticleSystem.MinMaxCurve(1f, curveHit);

            var rendHit = rootHit.GetComponent<ParticleSystemRenderer>();
            rendHit.material = matStrike;
            rendHit.sortingLayerName = "Skill";
            rendHit.sortingOrder = 22;

            // --- TẦNG 2: TIA LỬA BẮN TUNG TÓE (HIT SPARKS) ---
            GameObject sparksObj = new GameObject("Strike_Sparks");
            sparksObj.transform.SetParent(rootHit.transform, false);
            var psSparks = sparksObj.AddComponent<ParticleSystem>();
            var mainSparks = psSparks.main;
            mainSparks.duration = 0.5f;
            mainSparks.loop = false;
            mainSparks.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
            mainSparks.startSpeed = new ParticleSystem.MinMaxCurve(4.0f, 8.0f);
            mainSparks.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
            mainSparks.simulationSpace = ParticleSystemSimulationSpace.Local;

            var emissSparks = psSparks.emission;
            emissSparks.rateOverTime = 0;
            emissSparks.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 12) });

            var shapeSparks = psSparks.shape;
            shapeSparks.enabled = true;
            shapeSparks.shapeType = ParticleSystemShapeType.Circle;
            shapeSparks.radius = 0.2f;

            var colSparks = psSparks.colorOverLifetime;
            colSparks.enabled = true;
            colSparks.color = gradHit;

            var rendSparks = sparksObj.GetComponent<ParticleSystemRenderer>();
            rendSparks.material = matStrike;
            rendSparks.sortingLayerName = "Skill";
            rendSparks.sortingOrder = 23;

            // Lưu Prefab 2
            PrefabUtility.SaveAsPrefabAsset(rootHit, slideHitPrefabPath);
            Object.DestroyImmediate(rootHit);

            // =========================================================================
            // GÁN TỰ ĐỘNG VÀO PREFAB VŨ KHÍ R007
            // =========================================================================
            string weaponPrefabPath = "Assets/Prefabs/Weapons/Weapon_R007.prefab";
            if (File.Exists(weaponPrefabPath))
            {
                using (var scope = new PrefabUtility.EditPrefabContentsScope(weaponPrefabPath))
                {
                    GameObject wRoot = scope.prefabContentsRoot;
                    var relicComp = wRoot.GetComponent<Relic_SleepingMat>();
                    if (relicComp != null)
                    {
                        SerializedObject so = new SerializedObject(relicComp);
                        var pMat = so.FindProperty("matVfxPrefab");
                        if (pMat != null) pMat.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                        var pSlide = so.FindProperty("slideHitVfxPrefab");
                        if (pSlide != null) pSlide.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(slideHitPrefabPath);
                        so.ApplyModifiedProperties();
                    }
                }
                Debug.Log($"<color=#00FF88>[SlapstickRelicVFXBuilder]</color> 🔗 Đã tự động kết nối VFX vào: {weaponPrefabPath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var savedMatPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (savedMatPrefab != null) EditorGUIUtility.PingObject(savedMatPrefab);
            Debug.Log($"<color=#00FF88>[SlapstickRelicVFXBuilder]</color> 🎋 Dựng thành công CHIẾU TRẢI HOÀNG TUYỀN (Decal + Slide Hit) tại: {prefabPath}");
        }

        // 5. R008: Chổi Lông Gà (Cơn Lốc Quét Rác & Đàn Gà Loạn Đả 2.5D Slapstick)
        [MenuItem("Tools/VFX Generator/Slapstick Relics/5. 🪶 Build W_BROOM (Chổi Lông Gà)", false, 5)]
        public static void BuildChickenBroomVFX()
        {
            EnsureDirectories();
            string prefabPath = $"{PREFAB_DIR}/VFX_Relic_ChickenBroom_Smash.prefab";
            string stampedePrefabPath = $"{PREFAB_DIR}/VFX_Relic_Chicken_Stampede.prefab";

            // Load New Slapstick Textures
            string texWhirlPath = "Assets/VFX/SkillLibrary/Textures/Tex_VFX_Broom_Whirlwind.png";
            string texBroomPath = "Assets/VFX/SkillLibrary/Textures/Tex_ChickenBroom_Giant_Clean.png";
            string texFeathersPath = "Assets/VFX/SkillLibrary/Textures/Tex_ChickenBroom_SingleFeather_Clean.png";
            string texDustPath = "Assets/VFX/SkillLibrary/Textures/Tex_VFX_Chicken_DustTrail.png";
            string texChickenPath = "Assets/VFX/SkillLibrary/Textures/Tex_VFX_Chibi_Chicken_Run.png";

            Texture2D texWhirl = AssetDatabase.LoadAssetAtPath<Texture2D>(texWhirlPath);
            Texture2D texBroom = AssetDatabase.LoadAssetAtPath<Texture2D>(texBroomPath);
            Texture2D texFeathers = AssetDatabase.LoadAssetAtPath<Texture2D>(texFeathersPath);
            Texture2D texDust = AssetDatabase.LoadAssetAtPath<Texture2D>(texDustPath);
            Texture2D texChicken = AssetDatabase.LoadAssetAtPath<Texture2D>(texChickenPath);

            // Tạo Materials sạch sẽ, phát quang đẹp
            Material matWhirl = GetOrCreateTextureMaterial("MAT_VFX_ChickenBroom_Whirlwind", texWhirl, new Color(2.4f, 2.0f, 0.8f, 1f), true);
            Material matBroom = GetOrCreateTextureMaterial("MAT_VFX_ChickenBroom_Giant", texBroom, Color.white, false);
            Material matFeathers = GetOrCreateTextureMaterial("MAT_VFX_ChickenBroom_Feathers", texFeathers, new Color(1.2f, 1.1f, 0.6f, 1f), false);
            Material matDust = GetOrCreateTextureMaterial("MAT_VFX_ChickenBroom_DustTrail", texDust, new Color(1f, 0.95f, 0.8f, 0.65f), false);
            Material matChicken = GetOrCreateTextureMaterial("MAT_VFX_ChickenBroom_ChibiChicken", texChicken, Color.white, false);

            // =========================================================================
            // =========================================================================
            // PREFAB 1: CÂY CHỔI LÔNG GÀ BAY PHÓNG XÉ GIÓ (FLYING BROOM PROJECTILE)
            // =========================================================================
            GameObject root = new GameObject("VFX_Relic_ChickenBroom_Smash");

            // --- TẦNG 1: CÂY CHỔI XOAY TÍT PHÓNG NHANH ---
            var psBroom = root.AddComponent<ParticleSystem>();
            var mainBroom = psBroom.main;
            mainBroom.duration = 1.4f;
            mainBroom.loop = false;
            mainBroom.startLifetime = 1.4f;
            mainBroom.startSpeed = 0f;
            mainBroom.startSize = 1.25f; // Cây chổi kích thước 1.25m rõ nét, đẹp mắt
            mainBroom.startRotation = 0f;
            mainBroom.simulationSpace = ParticleSystemSimulationSpace.Local;

            var emissBroom = psBroom.emission;
            emissBroom.rateOverTime = 0;
            emissBroom.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 1) });

            var rotBroom = psBroom.rotationOverLifetime;
            rotBroom.enabled = true;
            rotBroom.z = new ParticleSystem.MinMaxCurve(1440f * Mathf.Deg2Rad); // Xoay tít 4 vòng/giây vun vút

            var colBroom = psBroom.colorOverLifetime;
            colBroom.enabled = true;
            Gradient gradBroom = new Gradient();
            gradBroom.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.85f), new GradientAlphaKey(0f, 1f) }
            );
            colBroom.color = gradBroom;

            var rendBroom = root.GetComponent<ParticleSystemRenderer>();
            rendBroom.material = matBroom;
            rendBroom.sortingLayerName = "Skill";
            rendBroom.sortingOrder = 18;

            // --- TẦNG 2: VỆT LÔNG GÀ VÀNG RƠI RỤNG SAU ĐUÔI (FEATHER TRAIL) ---
            GameObject feathersObj = new GameObject("Trailing_Feathers");
            feathersObj.transform.SetParent(root.transform, false);

            var psFeathers = feathersObj.AddComponent<ParticleSystem>();
            var mainFeathers = psFeathers.main;
            mainFeathers.duration = 1.4f;
            mainFeathers.loop = false;
            mainFeathers.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.55f);
            mainFeathers.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            mainFeathers.startSize = new ParticleSystem.MinMaxCurve(0.32f, 0.50f);
            mainFeathers.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            mainFeathers.simulationSpace = ParticleSystemSimulationSpace.World; // Lông gà bay rơi lại trên đường bay

            var emissFeathers = psFeathers.emission;
            emissFeathers.rateOverTime = 22; // Rải đều 22 lông gà/giây trên đường chổi bay

            var shapeFeathers = psFeathers.shape;
            shapeFeathers.shapeType = ParticleSystemShapeType.Sphere;
            shapeFeathers.radius = 0.35f;

            var rotFeathers = psFeathers.rotationOverLifetime;
            rotFeathers.enabled = true;
            rotFeathers.z = new ParticleSystem.MinMaxCurve(-180f * Mathf.Deg2Rad, 180f * Mathf.Deg2Rad);

            var colFeathers = psFeathers.colorOverLifetime;
            colFeathers.enabled = true;
            Gradient gradFeathers = new Gradient();
            gradFeathers.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.9f, 0.4f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.9f, 0.6f), new GradientAlphaKey(0f, 1f) }
            );
            colFeathers.color = gradFeathers;

            var rendFeathers = feathersObj.GetComponent<ParticleSystemRenderer>();
            rendFeathers.material = matFeathers;
            rendFeathers.sortingLayerName = "Skill";
            rendFeathers.sortingOrder = 16;

            // --- TẦNG 3: BỤI KHÓI SLAPSTICK CUỘN ĐẰNG SAU ---
            GameObject dustObj = new GameObject("Trailing_Dust");
            dustObj.transform.SetParent(root.transform, false);

            var psDust = dustObj.AddComponent<ParticleSystem>();
            var mainDust = psDust.main;
            mainDust.duration = 1.4f;
            mainDust.loop = false;
            mainDust.startLifetime = 0.35f;
            mainDust.startSpeed = 0.3f;
            mainDust.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.55f);
            mainDust.simulationSpace = ParticleSystemSimulationSpace.World;

            var emissDust = psDust.emission;
            emissDust.rateOverTime = 14;

            var shapeDust = psDust.shape;
            shapeDust.shapeType = ParticleSystemShapeType.Sphere;
            shapeDust.radius = 0.25f;

            var colDust = psDust.colorOverLifetime;
            colDust.enabled = true;
            Gradient gradDust = new Gradient();
            gradDust.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.5f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colDust.color = gradDust;

            var rendDust = dustObj.GetComponent<ParticleSystemRenderer>();
            rendDust.material = matDust;
            rendDust.sortingLayerName = "Skill";
            rendDust.sortingOrder = 5;

            // Gắn Level Scaler
            var scaler = root.AddComponent<ProjectZombie.Features.Shared.VFX.VFXLevelScaler>();
            scaler.tier2SubLayers = new GameObject[] { feathersObj };
            scaler.tier3UltimateLayers = new GameObject[] { dustObj };
            scaler.baseLevel1Scale = 0.85f;
            scaler.scalePerLevel = 0.05f;

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            // =========================================================================
            // PREFAB 2: ĐÀN GÀ CHIBI NỔI LOẠN (CHICKEN STAMPEDE)
            // =========================================================================
            GameObject stampedeRoot = new GameObject("VFX_Relic_Chicken_Stampede");

            var psStampede = stampedeRoot.AddComponent<ParticleSystem>();
            var mainStampede = psStampede.main;
            mainStampede.duration = 1.0f;
            mainStampede.loop = false;
            mainStampede.startLifetime = 0.85f;
            mainStampede.startSpeed = new ParticleSystem.MinMaxCurve(5.5f, 7.5f); // Chạy ào về phía trước
            mainStampede.startSize = new ParticleSystem.MinMaxCurve(0.55f, 0.70f); // Gà Chibi 0.6m
            mainStampede.simulationSpace = ParticleSystemSimulationSpace.World;

            var emissStampede = psStampede.emission;
            emissStampede.rateOverTime = 0;
            emissStampede.SetBursts(new ParticleSystem.Burst[] { 
                new ParticleSystem.Burst(0.0f, 3),
                new ParticleSystem.Burst(0.1f, 3),
                new ParticleSystem.Burst(0.2f, 2)
            });

            var shapeStampede = psStampede.shape;
            shapeStampede.shapeType = ParticleSystemShapeType.Cone;
            shapeStampede.angle = 20f;
            shapeStampede.radius = 0.6f;

            var colStampede = psStampede.colorOverLifetime;
            colStampede.enabled = true;
            Gradient gradStampede = new Gradient();
            gradStampede.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) }
            );
            colStampede.color = gradStampede;

            var rendStampede = stampedeRoot.GetComponent<ParticleSystemRenderer>();
            rendStampede.material = matChicken;
            rendStampede.sortingLayerName = "Skill";
            rendStampede.sortingOrder = 16;

            // Bụi khói dưới chân đàn gà
            GameObject stampedeDust = new GameObject("Chicken_Run_Dust");
            stampedeDust.transform.SetParent(stampedeRoot.transform, false);

            var psSDust = stampedeDust.AddComponent<ParticleSystem>();
            var mainSDust = psSDust.main;
            mainSDust.duration = 1.0f;
            mainSDust.loop = false;
            mainSDust.startLifetime = 0.4f;
            mainSDust.startSpeed = 1.0f;
            mainSDust.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.45f);
            mainSDust.simulationSpace = ParticleSystemSimulationSpace.World;

            var emissSDust = psSDust.emission;
            emissSDust.rateOverTime = 20;

            var rendSDust = stampedeDust.GetComponent<ParticleSystemRenderer>();
            rendSDust.material = matDust;
            rendSDust.sortingLayerName = "Skill";
            rendSDust.sortingOrder = 5;

            PrefabUtility.SaveAsPrefabAsset(stampedeRoot, stampedePrefabPath);
            Object.DestroyImmediate(stampedeRoot);

            // =========================================================================
            // PREFAB 3: GÀ CON HỘ VỆ (COMPANION CHICKEN MINION - CHUẨN ANIMATOR & SPRITE)
            // =========================================================================
            string minionPrefabPath = $"{PREFAB_DIR}/Companion_Chicken_Minion.prefab";
            string idleSpritePath = "Assets/Art/GaChoi/GaChoi-Idle.png";
            string controllerPath = "Assets/Art/GaChoi/GaChoi.controller";

            GameObject minionRoot = new GameObject("Companion_Chicken_Minion");
            var minionSr = minionRoot.AddComponent<SpriteRenderer>();
            
            // Load sub-sprite GaChoi-Idle_0
            Object[] allSprites = AssetDatabase.LoadAllAssetsAtPath(idleSpritePath);
            Sprite idleSprite = null;
            foreach (var obj in allSprites)
            {
                if (obj is Sprite spr)
                {
                    idleSprite = spr;
                    break;
                }
            }

            if (idleSprite != null) minionSr.sprite = idleSprite;
            minionSr.sortingLayerName = "Entities";
            minionSr.sortingOrder = 10;

            // Gán Animator Controller
            var anim = minionRoot.AddComponent<Animator>();
            RuntimeAnimatorController rac = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            if (rac != null) anim.runtimeAnimatorController = rac;

            minionRoot.AddComponent<ChickenMinionCompanion>();

            PrefabUtility.SaveAsPrefabAsset(minionRoot, minionPrefabPath);
            Object.DestroyImmediate(minionRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"<color=#00FF88>[SlapstickRelicVFXBuilder]</color> 🪶 Dựng thành công Prefab Cơn Lốc Quét Rác, Đàn Gà Loạn Đả & Gà Con Hộ Vệ (Animator Fully Wired)!");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(PREFAB_DIR)) Directory.CreateDirectory(PREFAB_DIR);
            if (!Directory.Exists(MAT_DIR)) Directory.CreateDirectory(MAT_DIR);
        }

        // 1.2 W_SLIPPER: Đạn Dép Bay Boomerang (Sprite Chiếc Dép + Vệt Gió)
        public static void BuildSlipperProjectileVFX()
        {
            string prefabPath = $"{PREFAB_DIR}/VFX_Relic_Slipper_Projectile.prefab";
            string texSlipperPath = "Assets/VFX/SkillLibrary/Textures/Tex_Slipper_Projectile.png";
            string texTrailPath = "Assets/VFX/SkillLibrary/Textures/Tex_Slipper_VortexArc.png";

            Texture2D texSlipper = AssetDatabase.LoadAssetAtPath<Texture2D>(texSlipperPath);
            Texture2D texTrail = AssetDatabase.LoadAssetAtPath<Texture2D>(texTrailPath);

            Material matSlipper = GetOrCreateTextureMaterial("MAT_VFX_Slipper_Item", texSlipper, Color.white, false);
            Material matTrail = GetOrCreateTextureMaterial("MAT_VFX_Slipper_Arc", texTrail, new Color(2.0f, 1.8f, 0.5f, 1f), true);

            GameObject root = new GameObject("VFX_Relic_Slipper_Projectile");

            // Sprite Renderer hiển thị chiếc dép bay
            var sr = root.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texSlipperPath);
            sr.sortingLayerName = "Skill";
            sr.sortingOrder = 10;
            root.transform.localScale = Vector3.one * 0.55f;

            // Trail Particle theo sau
            GameObject trailObj = new GameObject("Trail");
            trailObj.transform.SetParent(root.transform, false);
            var ps = trailObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 1.0f;
            main.loop = true;
            main.startLifetime = 0.25f;
            main.startSpeed = 0f;
            main.startSize = 0.8f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emiss = ps.emission;
            emiss.rateOverTime = 16;

            var rend = trailObj.GetComponent<ParticleSystemRenderer>();
            rend.material = matTrail;
            rend.sortingLayerName = "Skill";
            rend.sortingOrder = 9;

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[SlapstickRelicVFXBuilder] 🩴 Dựng thành công Prefab Đạn Dép Bay: {prefabPath}");
        }

        private static Material GetOrCreateTextureMaterial(string matName, Texture2D tex, Color tintColor, bool isAdditive)
        {
            string path = $"{MAT_DIR}/{matName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            
            Shader shader = isAdditive 
                ? (Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("ProjectZombie/VFX/Slash_Additive"))
                : (Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Sprites/Default"));

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
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tintColor);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", tintColor);
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", tintColor);
                
                // Cấu hình Blend Modes chuẩn xác cho URP
                if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f); // Transparent
                if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", isAdditive ? 1f : 0f); // 0=Alpha, 1=Additive
                if (mat.HasProperty("_SrcBlend")) mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", isAdditive ? (float)UnityEngine.Rendering.BlendMode.One : (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 0f);

                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                EditorUtility.SetDirty(mat);
            }
            return mat;
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
