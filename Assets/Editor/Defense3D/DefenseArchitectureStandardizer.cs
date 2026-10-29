#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using ProjectZombie.Features.Defense3D;

namespace ProjectZombie.EditorTools.Defense3D
{
    /// <summary>
    /// Sổ tay Kiến trúc 3D: Công cụ Editor chuẩn hóa kiến trúc Data-Driven và Prefab Hierarchy.
    /// Tự động sinh Data Assets mẫu (Tower & Effects) và Base Prefabs theo chuẩn phân quyền Base/Variant.
    /// </summary>
    public static class DefenseArchitectureStandardizer
    {
        private const string DATA_ROOT = "Assets/_Data/Defense3D";
        private const string PREFAB_ROOT = "Assets/_Prefabs/Defense3D";

        [MenuItem("TREBRAVE/Architecture/1. Tạo Sample Data Assets (Chòi & Hiệu ứng)")]
        public static void GenerateSampleDataAssets()
        {
            EnsureDirectories();

            // 1. Tạo SO_SlowEffect
            string slowPath = $"{DATA_ROOT}/Effects/SO_Effect_Slow_CoBan.asset";
            var slowEffect = AssetDatabase.LoadAssetAtPath<SO_SlowEffect>(slowPath);
            if (slowEffect == null)
            {
                slowEffect = ScriptableObject.CreateInstance<SO_SlowEffect>();
                AssetDatabase.CreateAsset(slowEffect, slowPath);
                Debug.Log($"[Architecture] Đã tạo Asset: {slowPath}");
            }

            // 2. Tạo SO_StunEffect
            string stunPath = $"{DATA_ROOT}/Effects/SO_Effect_Stun_CoBan.asset";
            var stunEffect = AssetDatabase.LoadAssetAtPath<SO_StunEffect>(stunPath);
            if (stunEffect == null)
            {
                stunEffect = ScriptableObject.CreateInstance<SO_StunEffect>();
                AssetDatabase.CreateAsset(stunEffect, stunPath);
                Debug.Log($"[Architecture] Đã tạo Asset: {stunPath}");
            }

            // 3. Load Targeting Profile có sẵn
            var targeting = AssetDatabase.LoadAssetAtPath<DefenseAttackTargetingProfileSO>("Assets/Resources/Defense3D/Targeting_Tower.asset");

            // 4. Tạo SO_Tower_NoThan (Chòi Cung Thủ Nỏ Thần)
            string towerArcherPath = $"{DATA_ROOT}/Towers/SO_Tower_NoThan_Archer.asset";
            var towerArcher = AssetDatabase.LoadAssetAtPath<SO_TowerData>(towerArcherPath);
            if (towerArcher == null)
            {
                towerArcher = ScriptableObject.CreateInstance<SO_TowerData>();

                // Gán serialized values thông qua SerializedObject để bảo toàn dữ liệu
                var so = new SerializedObject(towerArcher);
                so.FindProperty("_id").stringValue = "tower_nothan";
                so.FindProperty("_towerName").stringValue = "Nỏ Thần Bắn Tỉa";
                if (targeting != null)
                {
                    so.FindProperty("_targetingStrategy").objectReferenceValue = targeting;
                }

                // Cấu hình 3 cấp độ
                var levelsProp = so.FindProperty("_levels");
                levelsProp.arraySize = 3;

                // Cấp 1
                var lv1 = levelsProp.GetArrayElementAtIndex(0);
                lv1.FindPropertyRelative("buildCost").intValue = 25;
                lv1.FindPropertyRelative("damage").floatValue = 22f;
                lv1.FindPropertyRelative("attackRange").floatValue = 8f;
                lv1.FindPropertyRelative("attackRate").floatValue = 1.1f;

                // Cấp 2
                var lv2 = levelsProp.GetArrayElementAtIndex(1);
                lv2.FindPropertyRelative("buildCost").intValue = 40;
                lv2.FindPropertyRelative("damage").floatValue = 35f;
                lv2.FindPropertyRelative("attackRange").floatValue = 9.5f;
                lv2.FindPropertyRelative("attackRate").floatValue = 1.4f;

                // Cấp 3
                var lv3 = levelsProp.GetArrayElementAtIndex(2);
                lv3.FindPropertyRelative("buildCost").intValue = 60;
                lv3.FindPropertyRelative("damage").floatValue = 55f;
                lv3.FindPropertyRelative("attackRange").floatValue = 11f;
                lv3.FindPropertyRelative("attackRate").floatValue = 1.8f;

                so.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.CreateAsset(towerArcher, towerArcherPath);
                Debug.Log($"[Architecture] Đã tạo Asset Tháp Nỏ Thần: {towerArcherPath}");
            }

            // 5. Tạo SO_Tower_BangHa (Chòi Băng có kèm hiệu ứng làm chậm)
            string towerIcePath = $"{DATA_ROOT}/Towers/SO_Tower_BangHa_Ice.asset";
            var towerIce = AssetDatabase.LoadAssetAtPath<SO_TowerData>(towerIcePath);
            if (towerIce == null)
            {
                towerIce = ScriptableObject.CreateInstance<SO_TowerData>();
                var so = new SerializedObject(towerIce);
                so.FindProperty("_id").stringValue = "tower_bangha";
                so.FindProperty("_towerName").stringValue = "Hàn Băng Tháp";
                if (targeting != null)
                {
                    so.FindProperty("_targetingStrategy").objectReferenceValue = targeting;
                }

                var effectsProp = so.FindProperty("_attackEffects");
                effectsProp.arraySize = 1;
                effectsProp.GetArrayElementAtIndex(0).objectReferenceValue = slowEffect;

                var levelsProp = so.FindProperty("_levels");
                levelsProp.arraySize = 3;

                var lv1 = levelsProp.GetArrayElementAtIndex(0);
                lv1.FindPropertyRelative("buildCost").intValue = 30;
                lv1.FindPropertyRelative("damage").floatValue = 15f;
                lv1.FindPropertyRelative("attackRange").floatValue = 7.5f;
                lv1.FindPropertyRelative("attackRate").floatValue = 0.9f;

                var lv2 = levelsProp.GetArrayElementAtIndex(1);
                lv2.FindPropertyRelative("buildCost").intValue = 50;
                lv2.FindPropertyRelative("damage").floatValue = 25f;
                lv2.FindPropertyRelative("attackRange").floatValue = 8.5f;
                lv2.FindPropertyRelative("attackRate").floatValue = 1.1f;

                var lv3 = levelsProp.GetArrayElementAtIndex(2);
                lv3.FindPropertyRelative("buildCost").intValue = 75;
                lv3.FindPropertyRelative("damage").floatValue = 40f;
                lv3.FindPropertyRelative("attackRange").floatValue = 10f;
                lv3.FindPropertyRelative("attackRate").floatValue = 1.3f;

                so.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.CreateAsset(towerIce, towerIcePath);
                Debug.Log($"[Architecture] Đã tạo Asset Tháp Hàn Băng: {towerIcePath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("TREBRAVE Architecture", "Đã tạo thành công bộ Data Assets mẫu (Tháp & Hiệu Ứng)!", "OK");
        }

        [MenuItem("TREBRAVE/Architecture/2. Tạo Base Prefabs Chuẩn (P_Tower_Base & P_Enemy_Base)")]
        public static void GenerateBasePrefabs()
        {
            EnsureDirectories();

            // 1. Tạo P_Tower_Base.prefab
            string towerBasePath = $"{PREFAB_ROOT}/Towers/P_Tower_Base.prefab";
            if (!File.Exists(towerBasePath))
            {
                var towerRoot = new GameObject("P_Tower_Base");
                var collider = towerRoot.AddComponent<CapsuleCollider>();
                collider.center = new Vector3(0f, 1f, 0f);
                collider.height = 2f;
                collider.radius = 0.5f;

                var towerController = towerRoot.AddComponent<DefenseTowerPrototype3D>();
                towerRoot.AddComponent<DefenseSupportTarget3D>();

                var rotator = new GameObject("Visual_Rotator");
                rotator.transform.SetParent(towerRoot.transform, false);

                var model3D = new GameObject("Model_3D");
                model3D.transform.SetParent(rotator.transform, false);

                var firePoint = new GameObject("Socket_FirePoint");
                firePoint.transform.SetParent(rotator.transform, false);
                firePoint.transform.localPosition = new Vector3(0f, 1.8f, 0.4f);

                // Gán socket vào tower controller
                var so = new SerializedObject(towerController);
                so.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
                so.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(towerRoot, towerBasePath);
                Object.DestroyImmediate(towerRoot);
                Debug.Log($"[Architecture] Đã tạo Base Prefab: {towerBasePath}");
            }

            // 2. Tạo P_Enemy_Base.prefab
            string enemyBasePath = $"{PREFAB_ROOT}/Enemies/P_Enemy_Base.prefab";
            if (File.Exists(enemyBasePath))
            {
                AssetDatabase.DeleteAsset(enemyBasePath);
            }
            {
                var enemyRoot = new GameObject("P_Enemy_Base");
                var agent = enemyRoot.AddComponent<NavMeshAgent>();
                agent.speed = 2f;
                agent.stoppingDistance = 0.2f;
                agent.radius = 0.5f;
                agent.height = 2f;

                var rb = enemyRoot.AddComponent<Rigidbody>();
                rb.isKinematic = true;

                var col = enemyRoot.AddComponent<CapsuleCollider>();
                col.center = new Vector3(0f, 1f, 0f);
                col.height = 2f;
                col.radius = 0.5f;

                var enemy3D = enemyRoot.AddComponent<DefenseEnemy3D>();
                var status = enemyRoot.AddComponent<DefenseEnemyStatus3D>();
                var anchors = enemyRoot.AddComponent<DefenseActorAnchors3D>();
                var walker = enemyRoot.AddComponent<DefenseEnemyBridgeWalker3D>();
                var movement = enemyRoot.AddComponent<DefenseGroundMovement3D>();
                var attack = enemyRoot.AddComponent<DefenseEnemyAttack3D>();

                var model3D = new GameObject("Model_3D");
                model3D.transform.SetParent(enemyRoot.transform, false);

                var sockets = new GameObject("Sockets");
                sockets.transform.SetParent(enemyRoot.transform, false);

                var hit = new GameObject("Socket_HitPoint");
                hit.transform.SetParent(sockets.transform, false);
                hit.transform.localPosition = new Vector3(0f, 1f, 0f);

                var head = new GameObject("Socket_Head");
                head.transform.SetParent(sockets.transform, false);
                head.transform.localPosition = new Vector3(0f, 2.1f, 0f);

                var cast = new GameObject("Socket_Cast");
                cast.transform.SetParent(sockets.transform, false);
                cast.transform.localPosition = new Vector3(0f, 1.2f, 0.5f);

                var ground = new GameObject("Socket_Ground");
                ground.transform.SetParent(sockets.transform, false);
                ground.transform.localPosition = new Vector3(0f, 0.05f, 0f);

                // Wire up anchors
                var so = new SerializedObject(anchors);
                so.FindProperty("_visualRoot").objectReferenceValue = model3D.transform;
                so.FindProperty("_hit").objectReferenceValue = hit.transform;
                so.FindProperty("_head").objectReferenceValue = head.transform;
                so.FindProperty("_cast").objectReferenceValue = cast.transform;
                so.FindProperty("_ground").objectReferenceValue = ground.transform;
                so.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(enemyRoot, enemyBasePath);
                Object.DestroyImmediate(enemyRoot);
                Debug.Log($"[Architecture] Đã tạo Base Prefab: {enemyBasePath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("TREBRAVE Architecture", "Đã tạo thành công bộ Base Prefabs (P_Tower_Base & P_Enemy_Base)!", "OK");
        }

        [MenuItem("TREBRAVE/Architecture/0. Khôi Phục Composed Prefabs Gốc (Fix Lỗi Ngay)")]
        public static void RestoreOriginalEnemyDefinitions()
        {
            var mapping = new (string id, string resource)[]
            {
                ("magiap", "Defense3D/DefenseEnemy_Magiap"),
                ("thuyquai", "Defense3D/DefenseEnemy_ThuyQuai"),
                ("huyetdoi", "Defense3D/DefenseEnemy_HuyetDoi"),
                ("quaidu", "Defense3D/DefenseEnemy_QuaiDu"),
                ("phicuong", "Defense3D/DefenseEnemy_PhiCuong"),
                ("magiap_boss", "Defense3D/DefenseEnemy_MagiapBoss"),
                ("yaksha_boss", "Defense3D/DefenseYaksha3D")
            };

            int count = 0;
            foreach (var item in mapping)
            {
                string assetPath = $"Assets/Resources/Defense3D/Enemies/{item.id}.asset";
                var enemyData = AssetDatabase.LoadAssetAtPath<DefenseEnemyDataSO>(assetPath);
                var originalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Resources/{item.resource}.prefab");
                if (enemyData != null && originalPrefab != null)
                {
                    var so = new SerializedObject(enemyData);
                    so.FindProperty("prefab").objectReferenceValue = originalPrefab;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(enemyData);
                    count++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("TREBRAVE Architecture", $"Đã khôi phục thành công {count} Enemy Definitions về các Composed Prefabs gốc có sẵn đầy đủ BridgeWalker!", "OK");
        }

        [MenuItem("TREBRAVE/Architecture/3. Tạo Prefab Variants Cho Quái & Chòi")]
        public static void GeneratePrefabVariants()
        {
            EnsureDirectories();

            // 1. Kiểm tra Base Tower
            string towerBasePath = $"{PREFAB_ROOT}/Towers/P_Tower_Base.prefab";
            var baseTower = AssetDatabase.LoadAssetAtPath<GameObject>(towerBasePath);
            if (baseTower == null)
            {
                GenerateBasePrefabs();
                baseTower = AssetDatabase.LoadAssetAtPath<GameObject>(towerBasePath);
            }

            if (baseTower != null)
            {
                // Tạo Variant Tháp Nỏ Thần
                string noThanVariantPath = $"{PREFAB_ROOT}/Towers/P_Tower_NoThan_Archer.prefab";
                var noThanData = AssetDatabase.LoadAssetAtPath<SO_TowerData>($"{DATA_ROOT}/Towers/SO_Tower_NoThan_Archer.asset");
                CreateTowerVariant(baseTower, noThanData, noThanVariantPath);

                // Tạo Variant Tháp Hàn Băng
                string bangHaVariantPath = $"{PREFAB_ROOT}/Towers/P_Tower_BangHa_Ice.prefab";
                var bangHaData = AssetDatabase.LoadAssetAtPath<SO_TowerData>($"{DATA_ROOT}/Towers/SO_Tower_BangHa_Ice.asset");
                CreateTowerVariant(baseTower, bangHaData, bangHaVariantPath);
            }

            // 2. Tạo Enemy Variants từ các Composed Prefabs gốc đầy đủ Visual, Collision & Animations
            string enemyBasePath = $"{PREFAB_ROOT}/Enemies/P_Enemy_Base.prefab";
            var baseEnemy = AssetDatabase.LoadAssetAtPath<GameObject>(enemyBasePath);

            var enemySourcePrefabs = new (string id, string originalName, string variantName)[]
            {
                ("magiap", "DefenseEnemy_Magiap", "P_Enemy_Magiap"),
                ("thuyquai", "DefenseEnemy_ThuyQuai", "P_Enemy_ThuyQuai"),
                ("huyetdoi", "DefenseEnemy_HuyetDoi", "P_Enemy_HuyetDoi"),
                ("quaidu", "DefenseEnemy_QuaiDu", "P_Enemy_QuaiDu"),
                ("phicuong", "DefenseEnemy_PhiCuong", "P_Enemy_PhiCuong")
            };

            foreach (var item in enemySourcePrefabs)
            {
                string originalPath = $"Assets/Resources/Defense3D/{item.originalName}.prefab";
                var sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(originalPath) ?? baseEnemy;
                if (sourcePrefab != null)
                {
                    CreateEnemyVariant(sourcePrefab, item.id, $"{PREFAB_ROOT}/Enemies/{item.variantName}.prefab");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("TREBRAVE Architecture", "Đã tạo thành công bộ Prefab Variants cho Chòi & Quái!", "OK");
        }

        private static void CreateTowerVariant(GameObject basePrefab, SO_TowerData data, string variantPath)
        {
            if (basePrefab == null || File.Exists(variantPath)) return;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
            if (instance.TryGetComponent<DefenseTowerPrototype3D>(out var controller))
            {
                var so = new SerializedObject(controller);
                so.FindProperty("towerData").objectReferenceValue = data;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(instance, variantPath);
            Object.DestroyImmediate(instance);
            Debug.Log($"[Architecture] Đã tạo Tower Prefab Variant: {variantPath}");
        }

        private static void CreateEnemyVariant(GameObject basePrefab, string enemyId, string variantPath)
        {
            if (basePrefab == null) return;
            if (File.Exists(variantPath))
            {
                AssetDatabase.DeleteAsset(variantPath);
            }

            var enemyData = AssetDatabase.LoadAssetAtPath<DefenseEnemyDataSO>($"Assets/Resources/Defense3D/Enemies/{enemyId}.asset");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
            if (instance.TryGetComponent<DefenseEnemy3D>(out var enemy3D))
            {
                var so = new SerializedObject(enemy3D);
                so.FindProperty("definition").objectReferenceValue = enemyData;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // Gắn hoặc đảm bảo cấu hình đặc thù theo Archetype
            if (enemyId == "huyetdoi" || enemyId == "phicuong")
            {
                var airMove = instance.GetComponent<DefenseAirMovement3D>() ?? instance.AddComponent<DefenseAirMovement3D>();
                var aggro = AssetDatabase.LoadAssetAtPath<DefenseAggroProfileSO>("Assets/Resources/Defense3D/Aggro_SongQue.asset");
                var soAir = new SerializedObject(airMove);
                var aggroProp = soAir.FindProperty("_aggroProfile");
                if (aggroProp != null)
                {
                    aggroProp.objectReferenceValue = aggro;
                    soAir.ApplyModifiedPropertiesWithoutUndo();
                }

                // Authored spherical trigger hitbox cho Air flight
                if (!instance.TryGetComponent<SphereCollider>(out var sphere) || !sphere.isTrigger)
                {
                    if (sphere == null) sphere = instance.AddComponent<SphereCollider>();
                    sphere.isTrigger = true;
                    sphere.center = Vector3.zero;
                    sphere.radius = 0.5f;
                }
            }
            else if (enemyId == "thuyquai")
            {
                if (instance.GetComponent<DefenseWaterMovement3D>() == null)
                    instance.AddComponent<DefenseWaterMovement3D>();
            }

            PrefabUtility.SaveAsPrefabAsset(instance, variantPath);
            Object.DestroyImmediate(instance);
            Debug.Log($"[Architecture] Đã tạo Enemy Prefab Variant: {variantPath}");
        }

        [MenuItem("TREBRAVE/Architecture/4. Cập Nhật Enemy Definitions Dùng Prefab Variants")]
        public static void LinkEnemyDefinitionsToVariants()
        {
            var mapping = new (string id, string prefabName)[]
            {
                ("magiap", "P_Enemy_Magiap"),
                ("thuyquai", "P_Enemy_ThuyQuai"),
                ("huyetdoi", "P_Enemy_HuyetDoi"),
                ("quaidu", "P_Enemy_QuaiDu"),
                ("phicuong", "P_Enemy_PhiCuong")
            };

            int updated = 0;
            foreach (var item in mapping)
            {
                string assetPath = $"Assets/Resources/Defense3D/Enemies/{item.id}.asset";
                string variantPath = $"{PREFAB_ROOT}/Enemies/{item.prefabName}.prefab";

                var enemyData = AssetDatabase.LoadAssetAtPath<DefenseEnemyDataSO>(assetPath);
                var variantPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(variantPath);

                if (enemyData != null && variantPrefab != null)
                {
                    var so = new SerializedObject(enemyData);
                    so.FindProperty("prefab").objectReferenceValue = variantPrefab;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(enemyData);
                    updated++;
                    Debug.Log($"[Architecture] Đã liên kết {item.id} -> {item.prefabName}.prefab");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("TREBRAVE Architecture", $"Đã cập nhật thành công {updated} Enemy Definitions trỏ sang Prefab Variants mới!", "OK");
        }

        [MenuItem("TREBRAVE/Architecture/5. Chuẩn Hóa Hệ Thống Đạn (Projectiles & Object Pooling)")]
        public static void StandardizeProjectilesAndPooling()
        {
            EnsureDirectories();
            string projDir = $"{PREFAB_ROOT}/Projectiles";

            // 1. Tìm hoặc nạp Trail Material
            var trailMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/VFX/Defense3D/ArrowUnlit.mat");
            if (trailMat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
                trailMat = new Material(shader);
                AssetDatabase.CreateAsset(trailMat, $"{DATA_ROOT}/MAT_Projectile_Trail.mat");
            }

            // 2. Tạo P_Projectile_Base.prefab
            string basePath = $"{projDir}/P_Projectile_Base.prefab";
            var root = new GameObject("P_Projectile_Base");
            var core = root.AddComponent<ProjectileCore3D>();

            var visualObj = new GameObject("Visual");
            visualObj.transform.SetParent(root.transform);
            visualObj.transform.localPosition = Vector3.zero;
            var visual = visualObj.AddComponent<ProjectileVisual3D>();

            var trail = visualObj.AddComponent<TrailRenderer>();
            trail.time = 0.22f;
            trail.startWidth = 0.12f;
            trail.endWidth = 0.01f;
            trail.sharedMaterial = trailMat;
            trail.enabled = false;

            var modelObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            modelObj.name = "Model";
            modelObj.transform.SetParent(visualObj.transform);
            modelObj.transform.localPosition = Vector3.zero;
            modelObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            modelObj.transform.localScale = new Vector3(0.06f, 0.25f, 0.06f);
            var col = modelObj.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            var visualSo = new SerializedObject(visual);
            visualSo.FindProperty("_trailRenderer").objectReferenceValue = trail;
            visualSo.FindProperty("_meshModel").objectReferenceValue = modelObj;
            visualSo.ApplyModifiedProperties();

            var coreSo = new SerializedObject(core);
            coreSo.FindProperty("_visual").objectReferenceValue = visual;
            coreSo.ApplyModifiedProperties();

            var basePrefab = PrefabUtility.SaveAsPrefabAsset(root, basePath);
            Object.DestroyImmediate(root);
            Debug.Log($"[Architecture] Đã tạo Base Projectile Prefab: {basePath}");

            // 3. Tạo P_Proj_Arrow_Archer.prefab (Mũi tên vàng kim Nỏ Thần)
            string archerProjPath = $"{projDir}/P_Proj_Arrow_Archer.prefab";
            var archerInstance = PrefabUtility.InstantiatePrefab(basePrefab) as GameObject;
            archerInstance.name = "P_Proj_Arrow_Archer";
            var archerTrail = archerInstance.GetComponentInChildren<TrailRenderer>();
            if (archerTrail != null)
            {
                var goldColor = new Color(1f, 0.85f, 0.2f, 1f);
                var grad = new Gradient();
                grad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(goldColor, 0f), new GradientColorKey(goldColor, 1f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
                );
                archerTrail.colorGradient = grad;
                archerTrail.startWidth = 0.14f;
            }
            var archerPrefab = PrefabUtility.SaveAsPrefabAsset(archerInstance, archerProjPath);
            Object.DestroyImmediate(archerInstance);
            Debug.Log($"[Architecture] Đã tạo Archer Projectile Prefab: {archerProjPath}");

            // 4. Tạo P_Proj_IceShard_BangHa.prefab (Băng hà tinh thể)
            string iceProjPath = $"{projDir}/P_Proj_IceShard_BangHa.prefab";
            var iceInstance = PrefabUtility.InstantiatePrefab(basePrefab) as GameObject;
            iceInstance.name = "P_Proj_IceShard_BangHa";
            var iceTrail = iceInstance.GetComponentInChildren<TrailRenderer>();
            if (iceTrail != null)
            {
                var cyanColor = new Color(0.2f, 0.9f, 1f, 1f);
                var grad = new Gradient();
                grad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(cyanColor, 0f), new GradientColorKey(cyanColor, 1f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
                );
                iceTrail.colorGradient = grad;
                iceTrail.startWidth = 0.18f;
            }
            var iceModel = iceInstance.transform.Find("Visual/Model");
            if (iceModel != null)
            {
                iceModel.localScale = new Vector3(0.12f, 0.18f, 0.12f);
            }
            var icePrefab = PrefabUtility.SaveAsPrefabAsset(iceInstance, iceProjPath);
            Object.DestroyImmediate(iceInstance);
            Debug.Log($"[Architecture] Đã tạo Ice Projectile Prefab: {iceProjPath}");

            // 5. Cập nhật SO_TowerData với Prefab Đạn mới
            string towerArcherPath = $"{DATA_ROOT}/Towers/SO_Tower_NoThan_Archer.asset";
            var towerArcher = AssetDatabase.LoadAssetAtPath<SO_TowerData>(towerArcherPath);
            if (towerArcher != null)
            {
                var so = new SerializedObject(towerArcher);
                so.FindProperty("_projectilePrefab").objectReferenceValue = archerPrefab;
                so.FindProperty("_projectileSpeed").floatValue = 26f;
                so.FindProperty("_projectileArcHeight").floatValue = 0.4f;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(towerArcher);
                Debug.Log($"[Architecture] Đã liên kết SO_Tower_NoThan_Archer -> P_Proj_Arrow_Archer.prefab");
            }

            string towerIcePath = $"{DATA_ROOT}/Towers/SO_Tower_BangHa_Ice.asset";
            var towerIce = AssetDatabase.LoadAssetAtPath<SO_TowerData>(towerIcePath);
            if (towerIce != null)
            {
                var so = new SerializedObject(towerIce);
                so.FindProperty("_projectilePrefab").objectReferenceValue = icePrefab;
                so.FindProperty("_projectileSpeed").floatValue = 18f;
                so.FindProperty("_projectileArcHeight").floatValue = 0.6f;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(towerIce);
                Debug.Log($"[Architecture] Đã liên kết SO_Tower_BangHa_Ice -> P_Proj_IceShard_BangHa.prefab");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("TREBRAVE Architecture", "Đã chuẩn hóa thành công Hệ Thống Đạn Đạo 3D:\n- P_Projectile_Base.prefab\n- P_Proj_Arrow_Archer.prefab\n- P_Proj_IceShard_BangHa.prefab\n- Cập nhật SO_TowerData liên kết đạn tương ứng!", "OK");
        }

        [MenuItem("TREBRAVE/Architecture/6. Tích Hợp Projectile Pool Vào Scene Prototype")]
        public static void AddProjectilePoolToScene()
        {
            string scenePath = "Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity";
            var currentScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            if (currentScene.path != scenePath)
            {
                currentScene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
            }

            var pool = Object.FindObjectOfType<DefenseProjectilePool3D>();
            if (pool == null)
            {
                var poolObj = new GameObject("[DEFENSE_PROJECTILE_POOL]");
                pool = poolObj.AddComponent<DefenseProjectilePool3D>();

                string archerProjPath = $"{PREFAB_ROOT}/Projectiles/P_Proj_Arrow_Archer.prefab";
                var archerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(archerProjPath);
                if (archerPrefab != null)
                {
                    var so = new SerializedObject(pool);
                    so.FindProperty("_defaultProjectilePrefab").objectReferenceValue = archerPrefab;
                    so.ApplyModifiedProperties();
                }

                var compRoot = Object.FindObjectOfType<DefenseSceneCompositionRoot>();
                if (compRoot != null)
                {
                    var compSo = new SerializedObject(compRoot);
                    var listProp = compSo.FindProperty("_sceneParticipants");
                    if (listProp != null)
                    {
                        listProp.arraySize++;
                        listProp.GetArrayElementAtIndex(listProp.arraySize - 1).objectReferenceValue = pool;
                        compSo.ApplyModifiedProperties();
                        Debug.Log("[Architecture] Đã thêm DefenseProjectilePool3D vào _sceneParticipants của CompositionRoot!");
                    }
                }

                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(currentScene);
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(currentScene);
                Debug.Log("[Architecture] Đã tạo và lưu [DEFENSE_PROJECTILE_POOL] vào Scene!");
            }
            else
            {
                Debug.Log("[Architecture] Scene đã có [DEFENSE_PROJECTILE_POOL]!");
            }

            EditorUtility.DisplayDialog("TREBRAVE Architecture", "Đã tích hợp thành công DefenseProjectilePool3D vào Scene MapSongQue3D_Prototype!", "OK");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(DATA_ROOT)) Directory.CreateDirectory(DATA_ROOT);
            if (!Directory.Exists($"{DATA_ROOT}/Towers")) Directory.CreateDirectory($"{DATA_ROOT}/Towers");
            if (!Directory.Exists($"{DATA_ROOT}/Effects")) Directory.CreateDirectory($"{DATA_ROOT}/Effects");
            if (!Directory.Exists(PREFAB_ROOT)) Directory.CreateDirectory(PREFAB_ROOT);
            if (!Directory.Exists($"{PREFAB_ROOT}/Towers")) Directory.CreateDirectory($"{PREFAB_ROOT}/Towers");
            if (!Directory.Exists($"{PREFAB_ROOT}/Enemies")) Directory.CreateDirectory($"{PREFAB_ROOT}/Enemies");
            if (!Directory.Exists($"{PREFAB_ROOT}/Projectiles")) Directory.CreateDirectory($"{PREFAB_ROOT}/Projectiles");
        }
    }
}
#endif

