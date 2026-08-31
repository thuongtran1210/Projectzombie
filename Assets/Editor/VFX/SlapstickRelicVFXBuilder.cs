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
            Debug.Log($"[SlapstickRelicVFXBuilder] 🩴 Dựng thành công Prefab Lốc Dép Vạn Năng (High Polish): {prefabPath}");
        }

        // 2. W_POT: Nồi Cơm Hút Quái & Đại Bác (Thổ - Chuẩn Multi-Layer AAA Anime VFX)
        [MenuItem("Tools/VFX Generator/🍚 Rebuild Pot Suction VFX (High Polish)", false, 2)]
        public static void BuildPotVFX()
        {
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
            Debug.Log($"[SlapstickRelicVFXBuilder] 🍚 Dựng thành công Prefab Nồi Cơm Hút Chân Không (High Polish): {prefabPath}");
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
