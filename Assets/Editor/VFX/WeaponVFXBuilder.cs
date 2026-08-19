using UnityEngine;
using UnityEditor;
using System.IO;
using ProjectZombie.VFX;
using ProjectZombie.Features.Weapons;

namespace ProjectZombie.Editor.VFX
{
    /// <summary>
    /// Editor Tool chuyên dụng tự động tạo và cấu hình các Prefab VFX chuẩn Anime 2D URP cho TOÀN BỘ 12 PHÁP BẢO.
    /// Tích hợp 100% Sprite RGBA trong suốt, gán SpriteRenderer vào Visual_Root của Đạn và Auto-Wiring vào Prefabs.
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

            // --- 1. W002 BÚT PHÁN QUAN ---
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

            // --- 2. W008 ĐAO CỬU VĨ ---
            GameObject foxFlame = CreateFoxFlameStreamPrefab();
            string foxFlamePath = $"{PREFAB_FOLDER}/VFX_W008_FoxFlameStream.prefab";
            GameObject foxFlameAsset = PrefabUtility.SaveAsPrefabAsset(foxFlame, foxFlamePath);
            GameObject.DestroyImmediate(foxFlame);

            WireW008ProjectilePrefab(foxFlameAsset);

            // --- 3. W003 BÙA TRẤN YÊU ---
            GameObject talismanTrail = CreateTalismanTrailPrefab();
            string talismanTrailPath = $"{PREFAB_FOLDER}/VFX_W003_TalismanTrail.prefab";
            GameObject talismanTrailAsset = PrefabUtility.SaveAsPrefabAsset(talismanTrail, talismanTrailPath);
            GameObject.DestroyImmediate(talismanTrail);

            WireW003ProjectilePrefab(talismanTrailAsset);

            // --- 4. W012 PHI TIÊU BÁT QUÁI ---
            GameObject windVortex = CreateWindVortexPrefab();
            string windVortexPath = $"{PREFAB_FOLDER}/VFX_W012_WindVortex.prefab";
            GameObject windVortexAsset = PrefabUtility.SaveAsPrefabAsset(windVortex, windVortexPath);
            GameObject.DestroyImmediate(windVortex);

            WireW012ProjectilePrefab(windVortexAsset);

            // --- 5. W005 TRỐNG ĐỒNG ĐÔNG SƠN ---
            GameObject dongSon = CreateDongSonShockwavePrefab();
            string dongSonPath = $"{PREFAB_FOLDER}/VFX_W005_DongSonShockwave.prefab";
            GameObject dongSonAsset = PrefabUtility.SaveAsPrefabAsset(dongSon, dongSonPath);
            GameObject.DestroyImmediate(dongSon);

            WireW005WeaponPrefab(dongSonAsset);

            // --- 6. W006 LỰU ĐẠN THẦN SA ---
            GameObject cinnabarExp = CreateCinnabarExplosionPrefab();
            string cinnabarPath = $"{PREFAB_FOLDER}/VFX_W006_CinnabarExplosion.prefab";
            GameObject cinnabarAsset = PrefabUtility.SaveAsPrefabAsset(cinnabarExp, cinnabarPath);
            GameObject.DestroyImmediate(cinnabarExp);

            WireW006ProjectilePrefab(cinnabarAsset);

            // --- 7. W011 NƯỚC THÁNH CHÙA HƯƠNG ---
            GameObject holyWater = CreateHolyWaterAoEPrefab();
            string holyPath = $"{PREFAB_FOLDER}/VFX_W011_HolyWaterAoE.prefab";
            GameObject holyAsset = PrefabUtility.SaveAsPrefabAsset(holyWater, holyPath);
            GameObject.DestroyImmediate(holyWater);

            WireW011ProjectilePrefab(holyAsset);

            // --- 8. W001 NỎ THẦN & W007 CUNG THẠCH SANH ---
            GameObject goldenArrow = CreateGoldenArrowPrefab();
            string arrowPath = $"{PREFAB_FOLDER}/VFX_W001_GoldenArrow.prefab";
            GameObject arrowAsset = PrefabUtility.SaveAsPrefabAsset(goldenArrow, arrowPath);
            GameObject.DestroyImmediate(goldenArrow);

            WireW001AndW007ProjectilePrefab(arrowAsset);

            // --- 9. W004 CỬU VĨ HỒ TRẢO ---
            GameObject foxClaws = CreateFoxClawsPrefab();
            string clawsPath = $"{PREFAB_FOLDER}/VFX_W004_FoxClaws.prefab";
            GameObject clawsAsset = PrefabUtility.SaveAsPrefabAsset(foxClaws, clawsPath);
            GameObject.DestroyImmediate(foxClaws);

            WireW004ProjectilePrefab(clawsAsset);

            // --- 10. W009 TRƯỢNG LONG VƯƠNG ---
            GameObject lightning = CreateLightningChainPrefab();
            string lightningPath = $"{PREFAB_FOLDER}/VFX_W009_LightningChain.prefab";
            GameObject lightningAsset = PrefabUtility.SaveAsPrefabAsset(lightning, lightningPath);
            GameObject.DestroyImmediate(lightning);

            WireW009ProjectilePrefab(lightningAsset);

            // --- 11. W010 LINH PHÙ MA DA ---
            GameObject poisonSwamp = CreatePoisonSwampPrefab();
            string swampPath = $"{PREFAB_FOLDER}/VFX_W010_PoisonSwamp.prefab";
            GameObject swampAsset = PrefabUtility.SaveAsPrefabAsset(poisonSwamp, swampPath);
            GameObject.DestroyImmediate(poisonSwamp);

            // --- 12. THANH ĐỒNG: VÒNG TRẬN TỨ PHỦ, MỒI LỬA & SÓNG PHÁN TRUYỀN ---
            GameObject torchFlame = CreateTorchFlameStreamPrefab();
            string torchFlamePath = $"{PREFAB_FOLDER}/VFX_ThanhDong_TorchFlame.prefab";
            PrefabUtility.SaveAsPrefabAsset(torchFlame, torchFlamePath);
            GameObject.DestroyImmediate(torchFlame);

            GameObject tuPhuAura = CreateTuPhuPossessionCirclePrefab();
            string tuPhuAuraPath = $"{PREFAB_FOLDER}/VFX_ThanhDong_TuPhuPossessionAura.prefab";
            PrefabUtility.SaveAsPrefabAsset(tuPhuAura, tuPhuAuraPath);
            GameObject.DestroyImmediate(tuPhuAura);

            GameObject oracleWave = CreateOracleShockwavePrefab();
            string oracleWavePath = $"{PREFAB_FOLDER}/VFX_ThanhDong_OracleShockwave.prefab";
            PrefabUtility.SaveAsPrefabAsset(oracleWave, oracleWavePath);
            GameObject.DestroyImmediate(oracleWave);

            // --- 13. GÁN SPRITE 2D VÀO VISUAL_ROOT CHO TOÀN BỘ ĐẠN ---
            SetupProjectileVisualRoots();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=#00FF00>[Weapon VFX Builder]</color> ĐÃ HOÀN TẤT SETUP SPRITE VISUAL_ROOT VÀ AUTO-WIRE TRỌN BỘ PHÁP BẢO & KỸ NĂNG THANH ĐỒNG!");
        }

        private static void SetupProjectileVisualRoots()
        {
            // Thiết lập Sprite và Texture cho Visual_Root của các đạn
            ConfigureVisualRootSprite("Proj_W007_CungThachSanh", "Arrow_ThachSanh.png", new Vector3(0.8f, 0.8f, 1f));
            ConfigureVisualRootSprite("Proj_W001_NoThan", "Arrow_NoThan.png", new Vector3(0.8f, 0.8f, 1f));
            ConfigureVisualRootSprite("Proj_W012_PhiTieuBatQuai", "PhiTieu_BatQuai.png", new Vector3(0.9f, 0.9f, 1f));
            ConfigureVisualRootSprite("Proj_W003_BuaTranYeu", "buatruyeu.png", new Vector3(1f, 1f, 1f));
            ConfigureVisualRootSprite("Proj_W005_TrongDongDongSon", "DongSon_Wave_Bullet.png", new Vector3(1.2f, 1.2f, 1f));
        }

        private static void ConfigureVisualRootSprite(string projPrefabName, string spriteFileName, Vector3 localScale)
        {
            string projPath = $"{PROJECTILES_PREFAB_FOLDER}/{projPrefabName}.prefab";
            GameObject projPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(projPath);
            if (projPrefab == null) return;

            // Tìm Sprite tương ứng
            Sprite targetSprite = LoadSpriteAsset(spriteFileName);
            if (targetSprite == null) return;

            using (var scope = new PrefabUtility.EditPrefabContentsScope(projPath))
            {
                GameObject root = scope.prefabContentsRoot;
                Transform visualRoot = root.transform.Find("Visual_Root");
                if (visualRoot == null)
                {
                    GameObject vrGo = new GameObject("Visual_Root");
                    vrGo.transform.SetParent(root.transform, false);
                    visualRoot = vrGo.transform;
                }

                visualRoot.localPosition = Vector3.zero;
                visualRoot.localRotation = Quaternion.identity;
                visualRoot.localScale = localScale;

                var sr = visualRoot.GetComponent<SpriteRenderer>();
                if (sr == null) sr = visualRoot.gameObject.AddComponent<SpriteRenderer>();

                sr.sprite = targetSprite;
                sr.sortingLayerName = "VFX_Front";
                sr.sortingOrder = 10;
            }

            Debug.Log($"<color=#00FF00>[Weapon VFX Builder]</color> Đã gán Sprite '{spriteFileName}' vào Visual_Root của {projPrefabName}!");
        }

        private static Sprite LoadSpriteAsset(string fileName)
        {
            string[] candidatePaths = new string[]
            {
                $"Assets/Art/Projectiles/{fileName}",
                $"Assets/Art/Skills/{fileName}",
                $"Assets/Art/Weapons/{fileName}"
            };

            foreach (var path in candidatePaths)
            {
                if (File.Exists(path))
                {
                    // Đảm bảo Texture được import dạng Sprite
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer != null && importer.textureType != TextureImporterType.Sprite)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.alphaIsTransparency = true;
                        importer.mipmapEnabled = false;
                        importer.SaveAndReimport();
                    }

                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sprite != null) return sprite;
                }
            }
            return null;
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
                if (slashAsset != null) so.FindProperty("directionalSlashPrefab").objectReferenceValue = slashAsset.GetComponent<ParticleSystem>();
                if (decalAsset != null) so.FindProperty("groundDecalPrefab").objectReferenceValue = decalAsset.GetComponent<ParticleSystem>();
                if (sparkAsset != null) so.FindProperty("hitSparkPrefab").objectReferenceValue = sparkAsset;
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
                }
            }
        }

        private static void WireW005WeaponPrefab(GameObject shockwaveAsset)
        {
            string weaponPath = $"{WEAPONS_PREFAB_FOLDER}/Weapon_W005_TrongDongDongSon.prefab";
            GameObject weaponPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(weaponPath);
            if (weaponPrefab == null || shockwaveAsset == null) return;

            Sprite weaponIcon = LoadSpriteAsset("Icon_W005_TrongDong.png");

            using (var scope = new PrefabUtility.EditPrefabContentsScope(weaponPath))
            {
                GameObject root = scope.prefabContentsRoot;
                
                // Xóa instance VFX tĩnh nếu có để tránh phát 1 lần lúc spawn
                Transform existingVFX = root.transform.Find("Shockwave_VFX");
                if (existingVFX != null)
                {
                    GameObject.DestroyImmediate(existingVFX.gameObject);
                }

                Weapon_Shotgun shotgunComp = root.GetComponent<Weapon_Shotgun>();
                if (shotgunComp != null)
                {
                    SerializedObject so = new SerializedObject(shotgunComp);
                    var shockwaveProp = so.FindProperty("shockwavePrefab");
                    if (shockwaveProp != null)
                    {
                        shockwaveProp.objectReferenceValue = shockwaveAsset;
                    }
                    if (weaponIcon != null)
                    {
                        var iconProp = so.FindProperty("icon");
                        if (iconProp != null)
                        {
                            iconProp.objectReferenceValue = weaponIcon;
                        }
                    }
                    so.ApplyModifiedProperties();
                }
            }

            // Gán SpawnVFXPrefab vào ProjectileData của Trống Đồng
            string projDataPath = "Assets/_Data/Projectiles/Data/Proj_W005_Trống.asset";
            var projData = AssetDatabase.LoadAssetAtPath<Features.Projectiles.Data.ProjectileData>(projDataPath);
            if (projData != null)
            {
                SerializedObject pSo = new SerializedObject(projData);
                var vfxConfigProp = pSo.FindProperty("VFXConfig");
                if (vfxConfigProp != null)
                {
                    var spawnVfxProp = vfxConfigProp.FindPropertyRelative("SpawnVFXPrefab");
                    if (spawnVfxProp != null)
                    {
                        spawnVfxProp.objectReferenceValue = shockwaveAsset;
                    }
                }
                pSo.ApplyModifiedProperties();
            }

            Debug.Log("<color=#00FF00>[Weapon VFX Builder]</color> Đã wire Shockwave VFX & Icon vào Weapon_W005_TrongDongDongSon và Proj_W005_Trống.asset!");
        }

        private static void WireW006ProjectilePrefab(GameObject cinnabarExpAsset)
        {
            string projPath = $"{PROJECTILES_PREFAB_FOLDER}/Proj_W006_LuuDanThanSa.prefab";
            GameObject projPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(projPath);
            if (projPrefab == null || cinnabarExpAsset == null) return;

            using (var scope = new PrefabUtility.EditPrefabContentsScope(projPath))
            {
                GameObject root = scope.prefabContentsRoot;
                Transform existingVFX = root.transform.Find("Explosion_VFX");
                if (existingVFX == null)
                {
                    GameObject vfxInstance = (GameObject)PrefabUtility.InstantiatePrefab(cinnabarExpAsset, root.transform);
                    vfxInstance.name = "Explosion_VFX";
                    vfxInstance.transform.localPosition = Vector3.zero;
                }
            }
        }

        private static void WireW011ProjectilePrefab(GameObject holyAoEAsset)
        {
            string projPath = $"{PROJECTILES_PREFAB_FOLDER}/Proj_W011_NuocThanhChuaHuong.prefab";
            GameObject projPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(projPath);
            if (projPrefab == null || holyAoEAsset == null) return;

            using (var scope = new PrefabUtility.EditPrefabContentsScope(projPath))
            {
                GameObject root = scope.prefabContentsRoot;
                Transform existingVFX = root.transform.Find("HolyPuddle_VFX");
                if (existingVFX == null)
                {
                    GameObject vfxInstance = (GameObject)PrefabUtility.InstantiatePrefab(holyAoEAsset, root.transform);
                    vfxInstance.name = "HolyPuddle_VFX";
                    vfxInstance.transform.localPosition = Vector3.zero;
                }
            }
        }

        private static void WireW001AndW007ProjectilePrefab(GameObject arrowAsset)
        {
            string[] projPaths = new string[]
            {
                $"{PROJECTILES_PREFAB_FOLDER}/Proj_W001_NoThan.prefab",
                $"{PROJECTILES_PREFAB_FOLDER}/Proj_W007_CungThachSanh.prefab"
            };

            foreach (var path in projPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null || arrowAsset == null) continue;

                using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
                {
                    GameObject root = scope.prefabContentsRoot;
                    Transform existingVFX = root.transform.Find("GoldenArrow_VFX");
                    if (existingVFX == null)
                    {
                        GameObject vfxInstance = (GameObject)PrefabUtility.InstantiatePrefab(arrowAsset, root.transform);
                        vfxInstance.name = "GoldenArrow_VFX";
                        vfxInstance.transform.localPosition = Vector3.zero;
                    }
                }
            }
        }

        private static void WireW004ProjectilePrefab(GameObject clawsAsset)
        {
            string projPath = $"{PROJECTILES_PREFAB_FOLDER}/Proj_W004_HoTrao.prefab";
            GameObject projPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(projPath);
            if (projPrefab == null || clawsAsset == null) return;

            using (var scope = new PrefabUtility.EditPrefabContentsScope(projPath))
            {
                GameObject root = scope.prefabContentsRoot;
                Transform existingVFX = root.transform.Find("FoxClaws_VFX");
                if (existingVFX == null)
                {
                    GameObject vfxInstance = (GameObject)PrefabUtility.InstantiatePrefab(clawsAsset, root.transform);
                    vfxInstance.name = "FoxClaws_VFX";
                    vfxInstance.transform.localPosition = Vector3.zero;
                }
            }
        }

        private static void WireW009ProjectilePrefab(GameObject lightningAsset)
        {
            string projPath = $"{PROJECTILES_PREFAB_FOLDER}/Proj_W009_TruongLongVuong.prefab";
            GameObject projPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(projPath);
            if (projPrefab == null || lightningAsset == null) return;

            using (var scope = new PrefabUtility.EditPrefabContentsScope(projPath))
            {
                GameObject root = scope.prefabContentsRoot;
                Transform existingVFX = root.transform.Find("Lightning_VFX");
                if (existingVFX == null)
                {
                    GameObject vfxInstance = (GameObject)PrefabUtility.InstantiatePrefab(lightningAsset, root.transform);
                    vfxInstance.name = "Lightning_VFX";
                    vfxInstance.transform.localPosition = Vector3.zero;
                }
            }
        }

        private static void WireW010ProjectilePrefab(GameObject swampAsset)
        {
            string projPath = $"{PROJECTILES_PREFAB_FOLDER}/Proj_W010_LinhPhuMaDa.prefab";
            GameObject projPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(projPath);
            if (projPrefab == null || swampAsset == null) return;

            using (var scope = new PrefabUtility.EditPrefabContentsScope(projPath))
            {
                GameObject root = scope.prefabContentsRoot;
                Transform existingVFX = root.transform.Find("PoisonSwamp_VFX");
                if (existingVFX == null)
                {
                    GameObject vfxInstance = (GameObject)PrefabUtility.InstantiatePrefab(swampAsset, root.transform);
                    vfxInstance.name = "PoisonSwamp_VFX";
                    vfxInstance.transform.localPosition = Vector3.zero;
                }
            }
        }

        // --- BUILDERS ĐA LỚP ---

        private static GameObject CreatePenSlashPrefab()
        {
            GameObject root = new GameObject("VFX_W002_PenSlash");
            root.AddComponent<VFXPoolResetter>();

            // Layer 1: Vệt Cọ Mực Tàu Đen Tuyền (Black Ink Stroke - AlphaBlend)
            ParticleSystem mainPS = root.AddComponent<ParticleSystem>();
            var main = mainPS.main;
            main.duration = 0.22f;
            main.loop = false;
            main.startLifetime = 0.2f;
            main.startSpeed = 0f;
            main.startSize = 3.4f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.playOnAwake = true;

            var emission = mainPS.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 1) });

            var shape = mainPS.shape;
            shape.enabled = false;

            var sizeOverLife = mainPS.sizeOverLifetime;
            sizeOverLife.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0.0f, 0.8f);
            sizeCurve.AddKey(0.3f, 1.05f);
            sizeCurve.AddKey(1.0f, 0.95f);
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1.0f, sizeCurve);

            var colorOverLife = mainPS.colorOverLifetime;
            colorOverLife.enabled = true;
            Gradient inkGrad = new Gradient();
            inkGrad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 0.7f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            colorOverLife.color = inkGrad;

            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.Local;
            renderer.sortingLayerName = "VFX_Front";
            renderer.sortingOrder = 9;

            Material inkBlackMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_Ink_Black_Stroke.mat");
            if (inkBlackMat != null) renderer.sharedMaterial = inkBlackMat;

            // Layer 2: Lưỡi Kiếm Quang Năng Dạ Quang Neon (Neon Laser Edge - Additive)
            GameObject neonObj = new GameObject("Neon_Laser_Edge");
            neonObj.transform.SetParent(root.transform, false);

            ParticleSystem neonPS = neonObj.AddComponent<ParticleSystem>();
            var neonMain = neonPS.main;
            neonMain.duration = 0.22f;
            neonMain.loop = false;
            neonMain.startLifetime = 0.18f;
            neonMain.startSpeed = 0f;
            neonMain.startSize = 3.2f;
            neonMain.simulationSpace = ParticleSystemSimulationSpace.Local;

            var neonEmission = neonPS.emission;
            neonEmission.rateOverTime = 0;
            neonEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 1) });

            var neonShape = neonPS.shape;
            neonShape.enabled = false;

            var neonSizeOverLife = neonPS.sizeOverLifetime;
            neonSizeOverLife.enabled = true;
            neonSizeOverLife.size = new ParticleSystem.MinMaxCurve(1.0f, sizeCurve);

            var neonRenderer = neonObj.GetComponent<ParticleSystemRenderer>();
            neonRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            neonRenderer.alignment = ParticleSystemRenderSpace.Local;
            neonRenderer.sortingLayerName = "VFX_Front";
            neonRenderer.sortingOrder = 11;

            Material neonMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_Ink_Neon_Glow.mat");
            if (neonMat == null) neonMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_InkSlash_Arc.mat");
            if (neonMat != null) neonRenderer.sharedMaterial = neonMat;

            // Layer 3: Hạt Mực Bắn & Tia Lửa (Ink Splatters & Sparks)
            GameObject sparksObj = new GameObject("Ink_Splash_Sparks");
            sparksObj.transform.SetParent(root.transform, false);

            ParticleSystem sparksPS = sparksObj.AddComponent<ParticleSystem>();
            var sparksMain = sparksPS.main;
            sparksMain.duration = 0.2f;
            sparksMain.loop = false;
            sparksMain.startLifetime = new ParticleSystem.MinMaxCurve(0.12f, 0.22f);
            sparksMain.startSpeed = new ParticleSystem.MinMaxCurve(10f, 20f);
            sparksMain.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.09f);
            sparksMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var sparksEmission = sparksPS.emission;
            sparksEmission.rateOverTime = 0;
            sparksEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 18) });

            var sparksShape = sparksPS.shape;
            sparksShape.enabled = true;
            sparksShape.shapeType = ParticleSystemShapeType.Cone;
            sparksShape.rotation = new Vector3(0, 90f, 0);
            sparksShape.angle = 20f;
            sparksShape.radius = 0.3f;

            var sparksRenderer = sparksObj.GetComponent<ParticleSystemRenderer>();
            sparksRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            sparksRenderer.velocityScale = 0.08f;
            sparksRenderer.lengthScale = 3.5f;
            sparksRenderer.sortingLayerName = "VFX_Front";
            sparksRenderer.sortingOrder = 13;

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

        private static GameObject CreateDongSonShockwavePrefab()
        {
            GameObject root = new GameObject("VFX_W005_DongSonShockwave");
            root.AddComponent<VFXPoolResetter>();

            ParticleSystem ps = root.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.4f;
            main.loop = false;
            main.startLifetime = 0.35f;
            main.startSpeed = 0f;
            main.startSize = 1.0f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 1) });

            var shape = ps.shape;
            shape.enabled = false;

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0.0f, 1.0f);
            curve.AddKey(1.0f, 6.0f);
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1.0f, curve);

            var colorOverLife = ps.colorOverLifetime;
            colorOverLife.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(new Color(1f, 0.84f, 0.3f), 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.8f, 0.4f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            colorOverLife.color = grad;

            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingLayerName = "VFX_Front";
            renderer.sortingOrder = 8;

            Material shockMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_Shockwave_DongSon.mat");
            if (shockMat != null) renderer.sharedMaterial = shockMat;

            GameObject dustObj = new GameObject("Earth_Debris");
            dustObj.transform.SetParent(root.transform, false);

            ParticleSystem dustPS = dustObj.AddComponent<ParticleSystem>();
            var dustMain = dustPS.main;
            dustMain.duration = 0.4f;
            dustMain.loop = false;
            dustMain.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.35f);
            dustMain.startSpeed = new ParticleSystem.MinMaxCurve(6f, 12f);
            dustMain.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            dustMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var dustEmission = dustPS.emission;
            dustEmission.rateOverTime = 0;
            dustEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 16) });

            var dustShape = dustPS.shape;
            dustShape.enabled = true;
            dustShape.shapeType = ParticleSystemShapeType.Circle;
            dustShape.radius = 0.5f;

            var dustRenderer = dustObj.GetComponent<ParticleSystemRenderer>();
            dustRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            dustRenderer.velocityScale = 0.02f;
            dustRenderer.lengthScale = 1.0f;
            dustRenderer.sortingLayerName = "VFX_Front";
            dustRenderer.sortingOrder = 9;

            Material sparksMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_FireSlash_Sparks.mat");
            if (sparksMat != null) dustRenderer.sharedMaterial = sparksMat;

            return root;
        }

        private static GameObject CreateCinnabarExplosionPrefab()
        {
            GameObject root = new GameObject("VFX_W006_CinnabarExplosion");
            root.AddComponent<VFXPoolResetter>();

            ParticleSystem ps = root.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = 0.45f;
            main.startSpeed = 0f;
            main.startSize = 3.5f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 1) });

            var shape = ps.shape;
            shape.enabled = false;

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0.0f, 0.5f);
            curve.AddKey(0.2f, 1.2f);
            curve.AddKey(1.0f, 1.5f);
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1.0f, curve);

            var colorOverLife = ps.colorOverLifetime;
            colorOverLife.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(new Color(1f, 0.3f, 0.0f), 0.5f), new GradientColorKey(new Color(0.3f, 0.05f, 0.05f), 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.9f, 0.6f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            colorOverLife.color = grad;

            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingLayerName = "VFX_Front";
            renderer.sortingOrder = 10;

            Material pillarMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_Fire_Pillar.mat");
            if (pillarMat != null) renderer.sharedMaterial = pillarMat;

            GameObject embersObj = new GameObject("Cinnabar_Embers");
            embersObj.transform.SetParent(root.transform, false);

            ParticleSystem emberPS = embersObj.AddComponent<ParticleSystem>();
            var emberMain = emberPS.main;
            emberMain.duration = 0.5f;
            emberMain.loop = false;
            emberMain.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
            emberMain.startSpeed = new ParticleSystem.MinMaxCurve(8f, 16f);
            emberMain.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
            emberMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var emberEmission = emberPS.emission;
            emberEmission.rateOverTime = 0;
            emberEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 20) });

            var emberShape = emberPS.shape;
            emberShape.enabled = true;
            emberShape.shapeType = ParticleSystemShapeType.Sphere;
            emberShape.radius = 0.4f;

            var emberRenderer = embersObj.GetComponent<ParticleSystemRenderer>();
            emberRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            emberRenderer.velocityScale = 0.03f;
            emberRenderer.lengthScale = 1.2f;
            emberRenderer.sortingLayerName = "VFX_Front";
            emberRenderer.sortingOrder = 12;

            Material sparksMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_FireSlash_Sparks.mat");
            if (sparksMat != null) emberRenderer.sharedMaterial = sparksMat;

            return root;
        }

        private static GameObject CreateHolyWaterAoEPrefab()
        {
            GameObject root = new GameObject("VFX_W011_HolyWaterAoE");
            root.AddComponent<VFXPoolResetter>();

            ParticleSystem ps = root.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 2.0f;
            main.loop = true;
            main.startLifetime = 1.0f;
            main.startSpeed = 0f;
            main.startSize = 3.8f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 2f;

            var shape = ps.shape;
            shape.enabled = false;

            var colorOverLife = ps.colorOverLifetime;
            colorOverLife.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(new Color(0.4f, 0.9f, 1f), 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.0f), new GradientAlphaKey(0.8f, 0.3f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            colorOverLife.color = grad;

            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingLayerName = "VFX_Back";
            renderer.sortingOrder = -40;

            Material puddleMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_Decal_HolyWaterPuddle.mat");
            if (puddleMat != null) renderer.sharedMaterial = puddleMat;

            GameObject bubblesObj = new GameObject("Holy_Bubbles");
            bubblesObj.transform.SetParent(root.transform, false);

            ParticleSystem bubblePS = bubblesObj.AddComponent<ParticleSystem>();
            var bubbleMain = bubblePS.main;
            bubbleMain.duration = 2.0f;
            bubbleMain.loop = true;
            bubbleMain.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
            bubbleMain.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            bubbleMain.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.28f);
            bubbleMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var bubbleEmission = bubblePS.emission;
            bubbleEmission.rateOverTime = 12f;

            var bubbleShape = bubblePS.shape;
            bubbleShape.enabled = true;
            bubbleShape.shapeType = ParticleSystemShapeType.Circle;
            bubbleShape.radius = 1.6f;

            var bubbleRenderer = bubblesObj.GetComponent<ParticleSystemRenderer>();
            bubbleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            bubbleRenderer.sortingLayerName = "VFX_Front";
            bubbleRenderer.sortingOrder = 10;

            Material bubbleMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_Holy_Bubble.mat");
            if (bubbleMat != null) bubbleRenderer.sharedMaterial = bubbleMat;

            return root;
        }

        private static GameObject CreateGoldenArrowPrefab()
        {
            GameObject root = new GameObject("VFX_W001_GoldenArrow");
            root.AddComponent<VFXPoolResetter>();

            ParticleSystem ps = root.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.3f;
            main.loop = true;
            main.startLifetime = 0.18f;
            main.startSpeed = 0f;
            main.startSize = 2.8f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            var emission = ps.emission;
            emission.rateOverTime = 20f;

            var shape = ps.shape;
            shape.enabled = false;

            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.Local;
            renderer.sortingLayerName = "VFX_Front";
            renderer.sortingOrder = 11;

            Material arrowMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_ThachSanh_SonicArrow.mat");
            if (arrowMat == null) arrowMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_Arrow_Golden_Beam.mat");
            if (arrowMat != null) renderer.sharedMaterial = arrowMat;

            GameObject ringObj = new GameObject("Wind_Ring");
            ringObj.transform.SetParent(root.transform, false);

            ParticleSystem ringPS = ringObj.AddComponent<ParticleSystem>();
            var ringMain = ringPS.main;
            ringMain.duration = 0.3f;
            ringMain.loop = true;
            ringMain.startLifetime = 0.2f;
            ringMain.startSpeed = 0f;
            ringMain.startSize = 1.2f;
            ringMain.simulationSpace = ParticleSystemSimulationSpace.Local;

            var ringEmission = ringPS.emission;
            ringEmission.rateOverTime = 10f;

            var ringShape = ringPS.shape;
            ringShape.enabled = false;

            var ringSizeOverLife = ringPS.sizeOverLifetime;
            ringSizeOverLife.enabled = true;
            AnimationCurve rCurve = new AnimationCurve();
            rCurve.AddKey(0.0f, 0.4f);
            rCurve.AddKey(1.0f, 1.8f);
            ringSizeOverLife.size = new ParticleSystem.MinMaxCurve(1.0f, rCurve);

            var ringRenderer = ringObj.GetComponent<ParticleSystemRenderer>();
            ringRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            ringRenderer.sortingLayerName = "VFX_Front";
            ringRenderer.sortingOrder = 10;

            Material ringMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_Wind_Pierce_Ring.mat");
            if (ringMat != null) ringRenderer.sharedMaterial = ringMat;

            return root;
        }

        private static GameObject CreateFoxClawsPrefab()
        {
            GameObject root = new GameObject("VFX_W004_FoxClaws");
            root.AddComponent<VFXPoolResetter>();

            ParticleSystem ps = root.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.25f;
            main.loop = false;
            main.startLifetime = 0.22f;
            main.startSpeed = 0f;
            main.startSize = 2.8f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.playOnAwake = true;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 1) });

            var shape = ps.shape;
            shape.enabled = false;

            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.Local;
            renderer.sortingLayerName = "VFX_Front";
            renderer.sortingOrder = 12;

            Material clawsMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_Fox_Claws_Slash.mat");
            if (clawsMat != null) renderer.sharedMaterial = clawsMat;

            GameObject orbObj = new GameObject("Soul_Drain");
            orbObj.transform.SetParent(root.transform, false);

            ParticleSystem orbPS = orbObj.AddComponent<ParticleSystem>();
            var orbMain = orbPS.main;
            orbMain.duration = 0.25f;
            orbMain.loop = false;
            orbMain.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
            orbMain.startSpeed = new ParticleSystem.MinMaxCurve(4f, 10f);
            orbMain.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            orbMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var orbEmission = orbPS.emission;
            orbEmission.rateOverTime = 0;
            orbEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 8) });

            var orbShape = orbPS.shape;
            orbShape.enabled = true;
            orbShape.shapeType = ParticleSystemShapeType.Circle;
            orbShape.radius = 0.3f;

            var orbRenderer = orbObj.GetComponent<ParticleSystemRenderer>();
            orbRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            orbRenderer.sortingLayerName = "VFX_Front";
            orbRenderer.sortingOrder = 14;

            Material orbMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_Soul_Drain_Orb.mat");
            if (orbMat != null) orbRenderer.sharedMaterial = orbMat;

            return root;
        }

        private static GameObject CreateLightningChainPrefab()
        {
            GameObject root = new GameObject("VFX_W009_LightningChain");
            root.AddComponent<VFXPoolResetter>();

            ParticleSystem ps = root.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.25f;
            main.loop = false;
            main.startLifetime = 0.2f;
            main.startSpeed = 0f;
            main.startSize = 3.0f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 1) });

            var shape = ps.shape;
            shape.enabled = false;

            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingLayerName = "VFX_Front";
            renderer.sortingOrder = 12;

            Material lightningMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_Lightning_Bolt.mat");
            if (lightningMat != null) renderer.sharedMaterial = lightningMat;

            return root;
        }

        private static GameObject CreatePoisonSwampPrefab()
        {
            GameObject root = new GameObject("VFX_W010_PoisonSwamp");
            root.AddComponent<VFXPoolResetter>();

            ParticleSystem ps = root.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 2.5f;
            main.loop = true;
            main.startLifetime = 1.2f;
            main.startSpeed = 0f;
            main.startSize = 4.2f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 2f;

            var shape = ps.shape;
            shape.enabled = false;

            var colorOverLife = ps.colorOverLifetime;
            colorOverLife.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(new Color(0.7f, 0.2f, 0.9f), 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.0f), new GradientAlphaKey(0.8f, 0.3f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            colorOverLife.color = grad;

            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingLayerName = "VFX_Back";
            renderer.sortingOrder = -45;

            Material swampMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_Decal_PoisonSwamp.mat");
            if (swampMat != null) renderer.sharedMaterial = swampMat;

            return root;
        }

        public static GameObject CreateTorchFlameStreamPrefab()
        {
            GameObject root = new GameObject("VFX_ThanhDong_TorchFlame");
            root.AddComponent<VFXPoolResetter>();

            ParticleSystem ps = root.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 1.0f;
            main.loop = false;
            main.startLifetime = 0.45f;
            main.startSpeed = 8f;
            main.startSize = 1.2f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 25f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25f;
            shape.radius = 0.2f;

            var colorOverLife = ps.colorOverLifetime;
            colorOverLife.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(new Color(1f, 0.4f, 0.1f), 0.6f), new GradientColorKey(new Color(0.8f, 0.1f, 0.05f), 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0.0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            colorOverLife.color = grad;

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 0.4f);
            sizeCurve.AddKey(1f, 1.4f);
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingLayerName = "VFX_Front";
            renderer.sortingOrder = 10;

            Material flameMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_TorchFlame_Bullet.mat");
            if (flameMat != null) renderer.sharedMaterial = flameMat;

            return root;
        }

        public static GameObject CreateTuPhuPossessionCirclePrefab()
        {
            GameObject root = new GameObject("VFX_ThanhDong_TuPhuPossessionAura");
            root.AddComponent<VFXPoolResetter>();

            ParticleSystem ps = root.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 8.0f;
            main.loop = false;
            main.startLifetime = 8.0f;
            main.startSpeed = 0f;
            main.startSize = 5.0f;
            main.startRotation = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1) });

            var rotOverLife = ps.rotationOverLifetime;
            rotOverLife.enabled = true;
            rotOverLife.z = new ParticleSystem.MinMaxCurve(1.5f); // Xoay vòng tròn linh lực

            var colorOverLife = ps.colorOverLifetime;
            colorOverLife.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(new Color(1f, 0.85f, 0.3f), 0.5f), new GradientColorKey(new Color(1f, 0.5f, 0.2f), 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.0f), new GradientAlphaKey(0.9f, 0.15f), new GradientAlphaKey(0.9f, 0.85f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            colorOverLife.color = grad;

            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingLayerName = "VFX_Back";
            renderer.sortingOrder = -40;

            Material circleMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_TuPhu_PossessionCircle.mat");
            if (circleMat != null) renderer.sharedMaterial = circleMat;

            return root;
        }

        public static GameObject CreateOracleShockwavePrefab()
        {
            GameObject root = new GameObject("VFX_ThanhDong_OracleShockwave");
            root.AddComponent<VFXPoolResetter>();

            ParticleSystem ps = root.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.6f;
            main.loop = false;
            main.startLifetime = 0.5f;
            main.startSpeed = 0f;
            main.startSize = 1.0f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1) });

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 1f);
            sizeCurve.AddKey(1f, 15f); // Sóng lan tỏa toàn map
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var colorOverLife = ps.colorOverLifetime;
            colorOverLife.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(new Color(1f, 0.9f, 0.4f), 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            colorOverLife.color = grad;

            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingLayerName = "VFX_Front";
            renderer.sortingOrder = 20;

            Material waveMat = AssetDatabase.LoadAssetAtPath<Material>($"{MATERIAL_FOLDER}/MAT_Oracle_Shockwave.mat");
            if (waveMat != null) renderer.sharedMaterial = waveMat;

            return root;
        }
    }
}
