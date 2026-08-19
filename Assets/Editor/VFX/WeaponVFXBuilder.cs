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
    /// - W003 Bùa Trấn Yêu (Dải lụa Ribbon Trail phát sáng + Sóng đẩy lùi linh khí)
    /// - W012 Phi Tiêu Bát Quái (Vệt gió xoáy lốc Wind Vortex + Tia sáng xé gió)
    /// </summary>
    public static class WeaponVFXBuilder
    {
        private const string PREFAB_FOLDER = "Assets/VFX/SkillLibrary/Prefabs";
        private const string MATERIAL_FOLDER = "Assets/VFX/SkillLibrary/Materials";
        private const string WEAPONS_PREFAB_FOLDER = "Assets/_Prefabs/Weapons";
        private const string PROJECTILES_PREFAB_FOLDER = "Assets/_Prefabs/Projectiles";

        [MenuItem("Tools/VFX Generator/Build All Weapon VFX Prefabs", false, 10)]
        public static void BuildAllWeaponVFX()
        {
            // 0. Đảm bảo toàn bộ Materials URP và Texture đã được sinh đầy đủ
            VFXMaterialGenerator.GenerateAllVFXMaterials();

            if (!Directory.Exists(PREFAB_FOLDER))
            {
                Directory.CreateDirectory(PREFAB_FOLDER);
                AssetDatabase.Refresh();
            }

            // --- 1. VŨ KHÍ W002 BÚT PHÁN QUAN ---
            GameObject penSlash = CreatePenSlashPrefab();
            string penSlashPath = $"{PREFAB_FOLDER}/VFX_W002_PenSlash.prefab";
            GameObject penSlashAsset = PrefabUtility.SaveAsPrefabAsset(penSlash, penSlashPath);
            GameObject.DestroyImmediate(penSlash);

            GameObject groundDecal = CreateGroundDecalPrefab();
            string decalPath = $"{PREFAB_FOLDER}/VFX_W002_GroundDecal.prefab";
            GameObject groundDecalAsset = PrefabUtility.SaveAsPrefabAsset(groundDecal, decalPath);
            GameObject.DestroyImmediate(groundDecal);

            GameObject hitSparks = CreateHitSparksPrefab();
            string sparksPath = $"{PREFAB_FOLDER}/VFX_HitSparks_General.prefab";
            GameObject hitSparksAsset = PrefabUtility.SaveAsPrefabAsset(hitSparks, sparksPath);
            GameObject.DestroyImmediate(hitSparks);

            WireW002WeaponPrefab(penSlashAsset, groundDecalAsset, hitSparksAsset);

            // --- 2. VŨ KHÍ W008 ĐAO CỬU VĨ ---
            GameObject foxFlame = CreateFoxFlameStreamPrefab();
            string foxFlamePath = $"{PREFAB_FOLDER}/VFX_W008_FoxFlameStream.prefab";
            GameObject foxFlameAsset = PrefabUtility.SaveAsPrefabAsset(foxFlame, foxFlamePath);
            GameObject.DestroyImmediate(foxFlame);

            WireW008ProjectilePrefab(foxFlameAsset);

            // --- 3. VŨ KHÍ W003 BÙA TRẤN YÊU ---
            GameObject talismanTrail = CreateTalismanTrailPrefab();
            string talismanTrailPath = $"{PREFAB_FOLDER}/VFX_W003_TalismanTrail.prefab";
            GameObject talismanTrailAsset = PrefabUtility.SaveAsPrefabAsset(talismanTrail, talismanTrailPath);
            GameObject.DestroyImmediate(talismanTrail);

            WireW003ProjectilePrefab(talismanTrailAsset);

            // --- 4. VŨ KHÍ W012 PHI TIÊU BÁT QUÁI ---
            GameObject windVortex = CreateWindVortexPrefab();
            string windVortexPath = $"{PREFAB_FOLDER}/VFX_W012_WindVortex.prefab";
            GameObject windVortexAsset = PrefabUtility.SaveAsPrefabAsset(windVortex, windVortexPath);
            GameObject.DestroyImmediate(windVortex);

            WireW012ProjectilePrefab(windVortexAsset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=#00FF00>[Weapon VFX Builder]</color> Đã tạo & Auto-Wire thành công toàn bộ VFX cho W002, W008, W003 và W012!");
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
            }
        }

        private static void WireW008ProjectilePrefab(GameObject flameVfxAsset)
        {
            string projPath = $"{PROJECTILES_PREFAB_FOLDER}/Proj_W008_DaoCuuVi.prefab";
            GameObject projPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(projPath);
            if (projPrefab == null || flameVfxAsset == null) return;

            using (var scope = new PrefabUtility.EditPrefabContentsScope(projPath))
            {
                GameObject root = scope.prefabContentsRoot;
                var sr = root.GetComponent<SpriteRenderer>();
                if (sr != null) sr.enabled = false;

                Transform existingVFX = root.transform.Find("Flame_VFX");
                if (existingVFX == null)
                {
                    GameObject vfxInstance = (GameObject)PrefabUtility.InstantiatePrefab(flameVfxAsset, root.transform);
                    vfxInstance.name = "Flame_VFX";
                    vfxInstance.transform.localPosition = Vector3.zero;
                    vfxInstance.transform.localRotation = Quaternion.identity;
                }
            }
        }

        private static void WireW003ProjectilePrefab(GameObject trailVfxAsset)
        {
            string projPath = $"{PROJECTILES_PREFAB_FOLDER}/Proj_W003_BuaTranYeu.prefab";
            GameObject projPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(projPath);
            if (projPrefab == null || trailVfxAsset == null) return;

            using (var scope = new PrefabUtility.EditPrefabContentsScope(projPath))
            {
                GameObject root = scope.prefabContentsRoot;

                Transform existingVFX = root.transform.Find("Talisman_Trail_VFX");
                if (existingVFX == null)
                {
                    GameObject vfxInstance = (GameObject)PrefabUtility.InstantiatePrefab(trailVfxAsset, root.transform);
                    vfxInstance.name = "Talisman_Trail_VFX";
                    vfxInstance.transform.localPosition = Vector3.zero;
                    vfxInstance.transform.localRotation = Quaternion.identity;
                }
            }
        }

        private static void WireW012ProjectilePrefab(GameObject vortexVfxAsset)
        {
            string projPath = $"{PROJECTILES_PREFAB_FOLDER}/Proj_W012_PhiTieuBatQuai.prefab";
            GameObject projPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(projPath);
            if (projPrefab == null || vortexVfxAsset == null) return;

            using (var scope = new PrefabUtility.EditPrefabContentsScope(projPath))
            {
                GameObject root = scope.prefabContentsRoot;

                Transform existingVFX = root.transform.Find("Wind_Vortex_VFX");
                if (existingVFX == null)
                {
                    GameObject vfxInstance = (GameObject)PrefabUtility.InstantiatePrefab(vortexVfxAsset, root.transform);
                    vfxInstance.name = "Wind_Vortex_VFX";
                    vfxInstance.transform.localPosition = Vector3.zero;
                    vfxInstance.transform.localRotation = Quaternion.identity;
                }
            }
        }

        private static GameObject CreatePenSlashPrefab()
        {
            GameObject root = new GameObject("VFX_W002_PenSlash");
            root.AddComponent<VFXPoolResetter>();

            ParticleSystem mainPS = root.AddComponent<ParticleSystem>();
            var main = mainPS.main;
            main.duration = 0.2f;
            main.loop = false;
            main.startLifetime = 0.18f;
            main.startSpeed = 0f;
            main.startSize = 3.2f;
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
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(new Color(1f, 0.85f, 0.4f), 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 0.7f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            colorOverLife.color = grad;

            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingLayerName = "VFX_Front";
            renderer.sortingOrder = 10;

            Material slashMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_InkSlash_Arc.mat");
            if (slashMat != null) renderer.sharedMaterial = slashMat;

            // Layer Sparks
            GameObject sparksObj = new GameObject("Ink_Splash_Sparks");
            sparksObj.transform.SetParent(root.transform, false);

            ParticleSystem sparksPS = sparksObj.AddComponent<ParticleSystem>();
            var sparksMain = sparksPS.main;
            sparksMain.duration = 0.2f;
            sparksMain.loop = false;
            sparksMain.startLifetime = new ParticleSystem.MinMaxCurve(0.1f, 0.18f);
            sparksMain.startSpeed = new ParticleSystem.MinMaxCurve(10f, 18f);
            sparksMain.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.3f);
            sparksMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var sparksEmission = sparksPS.emission;
            sparksEmission.rateOverTime = 0;
            sparksEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 14) });

            var sparksShape = sparksPS.shape;
            sparksShape.enabled = true;
            sparksShape.shapeType = ParticleSystemShapeType.Cone;
            sparksShape.angle = 35f;
            sparksShape.radius = 0.2f;

            var sparksRenderer = sparksObj.GetComponent<ParticleSystemRenderer>();
            sparksRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            sparksRenderer.velocityScale = 0.03f;
            sparksRenderer.lengthScale = 1.2f;
            sparksRenderer.sortingLayerName = "VFX_Front";
            sparksRenderer.sortingOrder = 12;

            Material sparksMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_FireSlash_Sparks.mat");
            if (sparksMat != null) sparksRenderer.sharedMaterial = sparksMat;

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
            main.startLifetime = 0.4f;
            main.startSpeed = 0f;
            main.startSize = 2.4f;
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
            if (decalMat != null) renderer.sharedMaterial = decalMat;

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
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.08f, 0.16f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(6f, 14f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 10) });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.2f;

            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = 0.03f;
            renderer.lengthScale = 1.0f;
            renderer.sortingLayerName = "VFX_Front";
            renderer.sortingOrder = 15;

            Material sparksMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_FireSlash_Sparks.mat");
            if (sparksMat != null) renderer.sharedMaterial = sparksMat;

            return root;
        }

        private static GameObject CreateFoxFlameStreamPrefab()
        {
            GameObject root = new GameObject("VFX_W008_FoxFlameStream");
            root.AddComponent<VFXPoolResetter>();

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
            if (flameMat != null) renderer.sharedMaterial = flameMat;

            // Layer Ember Sparks
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
            if (sparksMat != null) emberRenderer.sharedMaterial = sparksMat;

            return root;
        }

        private static GameObject CreateTalismanTrailPrefab()
        {
            GameObject root = new GameObject("VFX_W003_TalismanTrail");
            root.AddComponent<VFXPoolResetter>();

            // 1. Trail Renderer (Dải lụa phát sáng vàng kim)
            TrailRenderer trail = root.AddComponent<TrailRenderer>();
            trail.time = 0.25f;
            trail.startWidth = 0.6f;
            trail.endWidth = 0.05f;
            trail.minVertexDistance = 0.05f;
            trail.autodestruct = false;
            trail.emitting = true;
            trail.sortingLayerName = "VFX_Front";
            trail.sortingOrder = 8;

            AnimationCurve widthCurve = new AnimationCurve();
            widthCurve.AddKey(0.0f, 1.0f);
            widthCurve.AddKey(0.7f, 0.5f);
            widthCurve.AddKey(1.0f, 0.0f);
            trail.widthCurve = widthCurve;

            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(new Color(1f, 0.84f, 0f), 0.5f), new GradientColorKey(new Color(1f, 0.4f, 0f), 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0.0f), new GradientAlphaKey(0.6f, 0.6f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            trail.colorGradient = grad;

            Material trailMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_Talisman_Ribbon_Trail.mat");
            if (trailMat != null) trail.sharedMaterial = trailMat;

            // 2. Hào quang lấp lánh (Sparkle Aura)
            GameObject sparkleObj = new GameObject("Sparkle_Aura");
            sparkleObj.transform.SetParent(root.transform, false);

            ParticleSystem ps = sparkleObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.5f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 15f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.25f;

            var psRenderer = sparkleObj.GetComponent<ParticleSystemRenderer>();
            psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            psRenderer.sortingLayerName = "VFX_Front";
            psRenderer.sortingOrder = 9;

            Material sparksMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_FireSlash_Sparks.mat");
            if (sparksMat != null) psRenderer.sharedMaterial = sparksMat;

            return root;
        }

        private static GameObject CreateWindVortexPrefab()
        {
            GameObject root = new GameObject("VFX_W012_WindVortex");
            root.AddComponent<VFXPoolResetter>();

            ParticleSystem ps = root.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.3f;
            main.loop = true;
            main.startLifetime = 0.2f;
            main.startSpeed = 0f;
            main.startSize = 1.6f;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            var rotOverLife = ps.rotationOverLifetime;
            rotOverLife.enabled = true;
            rotOverLife.z = new ParticleSystem.MinMaxCurve(720f * Mathf.Deg2Rad);

            var emission = ps.emission;
            emission.rateOverTime = 8f;

            var shape = ps.shape;
            shape.enabled = false;

            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingLayerName = "VFX_Front";
            renderer.sortingOrder = 9;

            Material vortexMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_BatQuai_Wind_Vortex.mat");
            if (vortexMat != null) renderer.sharedMaterial = vortexMat;

            return root;
        }
    }
}
