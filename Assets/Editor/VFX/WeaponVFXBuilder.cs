using UnityEngine;
using UnityEditor;
using System.IO;
using ProjectZombie.VFX;
using ProjectZombie.Features.Weapons;

namespace ProjectZombie.Editor.VFX
{
    /// <summary>
    /// Editor Tool chuyên dụng tự động tạo và cấu hình các Prefab VFX chuẩn Anime 2D URP cho:
    /// - W002 Bút Phán Quan (Vệt chém thư họa mực đen lõi trắng + tàn mực)
    /// - W008 Đao Cửu Vĩ (Luồng rồng lửa nón xoắn + tàn than hồng)
    /// Và tự động wire vào các Prefab vũ khí tương ứng.
    /// </summary>
    public static class WeaponVFXBuilder
    {
        private const string PREFAB_FOLDER = "Assets/VFX/SkillLibrary/Prefabs";
        private const string MATERIAL_FOLDER = "Assets/VFX/SkillLibrary/Materials";
        private const string WEAPONS_PREFAB_FOLDER = "Assets/_Prefabs/Weapons";
        private const string PROJECTILES_PREFAB_FOLDER = "Assets/_Prefabs/Projectiles";

        [MenuItem("Tools/VFX Generator/Build W002 & W008 Weapon VFX Prefabs", false, 15)]
        public static void BuildSlashAndFlameVFX()
        {
            if (!Directory.Exists(PREFAB_FOLDER))
            {
                Directory.CreateDirectory(PREFAB_FOLDER);
                AssetDatabase.Refresh();
            }

            // 1. Tạo Prefab VFX W002 Bút Phán Quan
            GameObject penSlash = CreatePenSlashPrefab();
            string penSlashPath = $"{PREFAB_FOLDER}/VFX_W002_PenSlash.prefab";
            GameObject penSlashAsset = PrefabUtility.SaveAsPrefabAsset(penSlash, penSlashPath);
            GameObject.DestroyImmediate(penSlash);

            // 2. Tạo Prefab Vết Xém Đất W002
            GameObject groundDecal = CreateGroundDecalPrefab();
            string decalPath = $"{PREFAB_FOLDER}/VFX_W002_GroundDecal.prefab";
            GameObject groundDecalAsset = PrefabUtility.SaveAsPrefabAsset(groundDecal, decalPath);
            GameObject.DestroyImmediate(groundDecal);

            // 3. Tạo Prefab Hit Sparks
            GameObject hitSparks = CreateHitSparksPrefab();
            string sparksPath = $"{PREFAB_FOLDER}/VFX_HitSparks_General.prefab";
            GameObject hitSparksAsset = PrefabUtility.SaveAsPrefabAsset(hitSparks, sparksPath);
            GameObject.DestroyImmediate(hitSparks);

            // 4. Tạo Prefab VFX Luồng Lửa W008 Đao Cửu Vĩ
            GameObject foxFlame = CreateFoxFlameStreamPrefab();
            string foxFlamePath = $"{PREFAB_FOLDER}/VFX_W008_FoxFlameStream.prefab";
            GameObject foxFlameAsset = PrefabUtility.SaveAsPrefabAsset(foxFlame, foxFlamePath);
            GameObject.DestroyImmediate(foxFlame);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 5. Tự động Wire vào Weapon_W002_ButPhanQuan.prefab
            WireW002WeaponPrefab(penSlashAsset, groundDecalAsset, hitSparksAsset);

            // 6. Tự động Wire vào Proj_W008_DaoCuuVi.prefab
            WireW008ProjectilePrefab(foxFlameAsset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=#00FF00>[Weapon VFX Builder]</color> Đã tạo & Auto-Wire thành công toàn bộ VFX cho Bút Phán Quan (W002) và Đao Cửu Vĩ (W008)!");
        }

        private static void WireW002WeaponPrefab(GameObject slashAsset, GameObject decalAsset, GameObject sparkAsset)
        {
            string weaponPath = $"{WEAPONS_PREFAB_FOLDER}/Weapon_W002_ButPhanQuan.prefab";
            GameObject weaponPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(weaponPath);
            if (weaponPrefab == null) return;

            var dualSlash = weaponPrefab.GetComponent<Weapon_DualSlash>();
            if (dualSlash != null)
            {
                var so = new SerializedObject(dualSlash);
                if (slashAsset != null)
                {
                    so.FindProperty("directionalSlashPrefab").objectReferenceValue = slashAsset.GetComponent<ParticleSystem>();
                }
                if (decalAsset != null)
                {
                    so.FindProperty("groundDecalPrefab").objectReferenceValue = decalAsset.GetComponent<ParticleSystem>();
                }
                if (sparkAsset != null)
                {
                    so.FindProperty("hitSparkPrefab").objectReferenceValue = sparkAsset;
                }
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(weaponPrefab);
                Debug.Log("<color=#00FF00>[Weapon VFX Builder]</color> Đã Auto-Wire VFX vào Weapon_W002_ButPhanQuan.prefab!");
            }
        }

        private static void WireW008ProjectilePrefab(GameObject flameVfxAsset)
        {
            string projPath = $"{PROJECTILES_PREFAB_FOLDER}/Proj_W008_DaoCuuVi.prefab";
            GameObject projPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(projPath);
            if (projPrefab == null || flameVfxAsset == null) return;

            // Mở prefab để chỉnh sửa cấu trúc
            using (var scope = new PrefabUtility.EditPrefabContentsScope(projPath))
            {
                GameObject root = scope.prefabContentsRoot;

                // Xóa hoặc ẩn SpriteRenderer mặc định của đạn nếu có
                var sr = root.GetComponent<SpriteRenderer>();
                if (sr != null) sr.enabled = false;

                // Kiểm tra xem đã có VFX con chưa
                Transform existingVFX = root.transform.Find("Flame_VFX");
                if (existingVFX == null)
                {
                    GameObject vfxInstance = (GameObject)PrefabUtility.InstantiatePrefab(flameVfxAsset, root.transform);
                    vfxInstance.name = "Flame_VFX";
                    vfxInstance.transform.localPosition = Vector3.zero;
                    vfxInstance.transform.localRotation = Quaternion.identity;
                    vfxInstance.transform.localScale = Vector3.one;
                }
            }

            Debug.Log("<color=#00FF00>[Weapon VFX Builder]</color> Đã Auto-Wire Luồng Lửa vào Proj_W008_DaoCuuVi.prefab!");
        }

        private static GameObject CreatePenSlashPrefab()
        {
            GameObject root = new GameObject("VFX_W002_PenSlash");
            root.AddComponent<VFXPoolResetter>();

            // 1. Layer Main Slash Arc (Vệt Chém Chính)
            ParticleSystem mainPS = root.AddComponent<ParticleSystem>();
            var main = mainPS.main;
            main.duration = 0.2f;
            main.loop = false;
            main.startLifetime = 0.18f;
            main.startSpeed = 0f;
            main.startSize = 3.2f;
            main.startRotation = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;

            var emission = mainPS.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 1) });

            var shape = mainPS.shape;
            shape.enabled = false;

            var sizeOverLife = mainPS.sizeOverLifetime;
            sizeOverLife.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0.0f, 0.7f);
            sizeCurve.AddKey(0.3f, 1.1f);
            sizeCurve.AddKey(1.0f, 0.9f);
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1.0f, sizeCurve);

            var colorOverLife = mainPS.colorOverLifetime;
            colorOverLife.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(new Color(0.9f, 0.8f, 0.5f), 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 0.7f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            colorOverLife.color = grad;

            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingLayerName = "VFX_Front";
            renderer.sortingOrder = 10;

            Material slashMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_FireSlash_Arc.mat");
            if (slashMat != null) renderer.material = slashMat;

            // 2. Layer Ink Splash / Sparks (Giọt mực & Tia sáng văng)
            GameObject sparksObj = new GameObject("Ink_Splash_Sparks");
            sparksObj.transform.SetParent(root.transform, false);

            ParticleSystem sparksPS = sparksObj.AddComponent<ParticleSystem>();
            var sparksMain = sparksPS.main;
            sparksMain.duration = 0.2f;
            sparksMain.loop = false;
            sparksMain.startLifetime = new ParticleSystem.MinMaxCurve(0.12f, 0.22f);
            sparksMain.startSpeed = new ParticleSystem.MinMaxCurve(12f, 22f);
            sparksMain.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
            sparksMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var sparksEmission = sparksPS.emission;
            sparksEmission.rateOverTime = 0;
            sparksEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 16) });

            var sparksShape = sparksPS.shape;
            sparksShape.enabled = true;
            sparksShape.shapeType = ParticleSystemShapeType.Cone;
            sparksShape.angle = 45f;
            sparksShape.radius = 0.3f;

            var sparksRenderer = sparksObj.GetComponent<ParticleSystemRenderer>();
            sparksRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            sparksRenderer.velocityScale = 0.08f;
            sparksRenderer.lengthScale = 2.0f;
            sparksRenderer.sortingLayerName = "VFX_Front";
            sparksRenderer.sortingOrder = 12;

            Material sparksMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_FireSlash_Sparks.mat");
            if (sparksMat != null) sparksRenderer.material = sparksMat;

            return root;
        }

        private static GameObject CreateGroundDecalPrefab()
        {
            GameObject root = new GameObject("VFX_W002_GroundDecal");
            root.AddComponent<VFXPoolResetter>();

            ParticleSystem ps = root.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = 0.45f;
            main.startSpeed = 0f;
            main.startSize = 2.8f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 1) });

            var shape = ps.shape;
            shape.enabled = false;

            var colorOverLife = ps.colorOverLifetime;
            colorOverLife.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0.0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            colorOverLife.color = grad;

            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingLayerName = "VFX_Back";
            renderer.sortingOrder = -50;

            Material decalMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_Decal_CrackedEarth.mat");
            if (decalMat != null) renderer.material = decalMat;

            return root;
        }

        private static GameObject CreateHitSparksPrefab()
        {
            GameObject root = new GameObject("VFX_HitSparks_General");
            root.AddComponent<VFXPoolResetter>();

            ParticleSystem ps = root.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.25f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.1f, 0.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(8f, 16f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.3f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 12) });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.2f;

            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = 0.06f;
            renderer.lengthScale = 1.5f;
            renderer.sortingLayerName = "VFX_Front";
            renderer.sortingOrder = 15;

            Material sparksMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_FireSlash_Sparks.mat");
            if (sparksMat != null) renderer.material = sparksMat;

            return root;
        }

        private static GameObject CreateFoxFlameStreamPrefab()
        {
            GameObject root = new GameObject("VFX_W008_FoxFlameStream");
            root.AddComponent<VFXPoolResetter>();

            // 1. Layer Lửa Nón Chính (Main Flame Stream)
            ParticleSystem flamePS = root.AddComponent<ParticleSystem>();
            var main = flamePS.main;
            main.duration = 0.4f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(10f, 18f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.8f, 1.8f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = flamePS.emission;
            emission.rateOverTime = 30f;

            var shape = flamePS.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25f;
            shape.radius = 0.2f;

            var sizeOverLife = flamePS.sizeOverLifetime;
            sizeOverLife.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0.0f, 0.4f);
            sizeCurve.AddKey(0.4f, 1.2f);
            sizeCurve.AddKey(1.0f, 1.8f);
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1.0f, sizeCurve);

            var colorOverLife = flamePS.colorOverLifetime;
            colorOverLife.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(Color.white, 0.0f), 
                    new GradientColorKey(new Color(1f, 0.6f, 0.1f), 0.3f), 
                    new GradientColorKey(new Color(0.9f, 0.15f, 0.05f), 0.7f),
                    new GradientColorKey(new Color(0.2f, 0.05f, 0.1f), 1.0f)
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1.0f, 0.0f), 
                    new GradientAlphaKey(0.9f, 0.6f), 
                    new GradientAlphaKey(0.0f, 1.0f) 
                }
            );
            colorOverLife.color = grad;

            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingLayerName = "VFX_Front";
            renderer.sortingOrder = 10;

            Material flameMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_FireSlash_Arc.mat");
            if (flameMat != null) renderer.material = flameMat;

            // 2. Layer Tàn Than Hồng (Ember Sparks)
            GameObject emberObj = new GameObject("Ember_Sparks");
            emberObj.transform.SetParent(root.transform, false);

            ParticleSystem emberPS = emberObj.AddComponent<ParticleSystem>();
            var emberMain = emberPS.main;
            emberMain.duration = 0.4f;
            emberMain.loop = true;
            emberMain.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
            emberMain.startSpeed = new ParticleSystem.MinMaxCurve(6f, 14f);
            emberMain.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
            emberMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var emberEmission = emberPS.emission;
            emberEmission.rateOverTime = 20f;

            var emberShape = emberPS.shape;
            emberShape.enabled = true;
            emberShape.shapeType = ParticleSystemShapeType.Cone;
            emberShape.angle = 35f;
            emberShape.radius = 0.2f;

            var noise = emberPS.noise;
            noise.enabled = true;
            noise.strength = 0.4f;
            noise.frequency = 0.8f;

            var emberRenderer = emberObj.GetComponent<ParticleSystemRenderer>();
            emberRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            emberRenderer.sortingLayerName = "VFX_Front";
            emberRenderer.sortingOrder = 12;

            Material sparksMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_FireSlash_Sparks.mat");
            if (sparksMat != null) emberRenderer.material = sparksMat;

            return root;
        }
    }
}
