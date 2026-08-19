#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Projectiles.Data;

namespace ProjectZombie.Features.Projectiles.Editor
{
    /// <summary>
    /// Editor Tool tự động tạo 12 Prefab đạn chuẩn (Rigidbody2D Kinematic + CircleCollider2D IsTrigger + Visual_Root)
    /// trong Assets/_Prefabs/Projectiles/ và liên kết trực tiếp vào LogicPrefab trên các ScriptableObject ProjectileData.
    /// Menu: ProjectZombie > Projectiles > Generate 12 MVP Projectile Prefabs
    /// </summary>
    public static class ProjectilePrefabGenerator
    {
        private struct ProjInfo
        {
            public string id;
            public string prefabName;
            public float radius;

            public ProjInfo(string id, string prefabName, float radius)
            {
                this.id = id;
                this.prefabName = prefabName;
                this.radius = radius;
            }
        }

        [MenuItem("ProjectZombie/Projectiles/Generate 12 MVP Projectile Prefabs")]
        public static void GenerateAllProjectilePrefabs()
        {
            string prefabFolder = "Assets/_Prefabs/Projectiles";
            string dataFolder = "Assets/_Data/Projectiles/Data";

            if (!AssetDatabase.IsValidFolder("Assets/_Prefabs")) AssetDatabase.CreateFolder("Assets", "_Prefabs");
            if (!AssetDatabase.IsValidFolder(prefabFolder)) AssetDatabase.CreateFolder("Assets/_Prefabs", "Projectiles");

            ProjInfo[] projectiles = new ProjInfo[]
            {
                new ProjInfo("Proj_W001", "Proj_W001_NoThan", 0.4f),
                new ProjInfo("Proj_W002", "Proj_W002_ButPhanQuan", 1.8f),
                new ProjInfo("Proj_W003", "Proj_W003_BuaTranYeu", 0.5f),
                new ProjInfo("Proj_W004", "Proj_W004_CuuViHoTrao", 0.5f),
                new ProjInfo("Proj_W005", "Proj_W005_TrongDongDongSon", 1.0f),
                new ProjInfo("Proj_W006", "Proj_W006_LuuDanThanSa", 0.25f),
                new ProjInfo("Proj_W007", "Proj_W007_CungThachSanh", 0.4f),
                new ProjInfo("Proj_W008", "Proj_W008_DaoCuuVi", 0.35f),
                new ProjInfo("Proj_W009", "Proj_W009_TruongLongVuong", 0.6f),
                new ProjInfo("Proj_W010", "Proj_W010_LinhPhuMaDa", 2.5f),
                new ProjInfo("Proj_W011", "Proj_W011_NuocThanhChuaHuong", 3.0f),
                new ProjInfo("Proj_W012", "Proj_W012_PhiTieuBatQuai", 0.5f)
            };

            int successCount = 0;
            foreach (var info in projectiles)
            {
                string prefabPath = $"{prefabFolder}/{info.prefabName}.prefab";
                GameObject prefabObj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (prefabObj == null)
                {
                    // Tạo GameObject đạn chuẩn theo thiết kế Lego Layer (Section 5 PROJECTILE_SYSTEM_DOC.md)
                    GameObject rootGo = new GameObject(info.prefabName);
                    
                    var rb = rootGo.AddComponent<Rigidbody2D>();
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

                    var collider = rootGo.AddComponent<CircleCollider2D>();
                    collider.isTrigger = true;
                    collider.radius = info.radius;

                    GameObject visualRoot = new GameObject("Visual_Root");
                    visualRoot.transform.SetParent(rootGo.transform, false);

                    prefabObj = PrefabUtility.SaveAsPrefabAsset(rootGo, prefabPath);
                    Object.DestroyImmediate(rootGo);
                    Debug.Log($"[ProjectilePrefabGenerator] Đã tạo Prefab đạn chuẩn: {prefabPath}");
                }

                // Gán Prefab vào ô LogicPrefab trên ScriptableObject ProjectileData
                string[] dataGuids = AssetDatabase.FindAssets($"{info.id}_ t:ProjectileData", new string[] { dataFolder });
                if (dataGuids.Length > 0)
                {
                    string dataPath = AssetDatabase.GUIDToAssetPath(dataGuids[0]);
                    var dataAsset = AssetDatabase.LoadAssetAtPath<ProjectileData>(dataPath);
                    if (dataAsset != null)
                    {
                        SerializedObject so = new SerializedObject(dataAsset);
                        so.FindProperty("LogicPrefab").objectReferenceValue = prefabObj;
                        so.ApplyModifiedProperties();
                        successCount++;
                        Debug.Log($"[ProjectilePrefabGenerator] Đã gán LogicPrefab '{info.prefabName}' vào ProjectileData '{dataAsset.name}'");
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ProjectilePrefabGenerator] Đã khởi tạo & gán thành công {successCount}/12 Prefabs đạn vào ProjectileData SOs!");
        }
    }
}
#endif
