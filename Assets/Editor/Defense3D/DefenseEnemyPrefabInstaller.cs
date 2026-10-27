using System.IO;
using ProjectZombie.Features.Defense3D;
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.EditorTools.Defense3D
{
    public static class DefenseEnemyPrefabInstaller
    {
        [MenuItem("TREBRAVE/Defense 3D/Create Missing Enemy Prefabs")]
        public static void CreateMissingPrefabs()
        {
            if (EditorApplication.isPlaying)
                throw new System.InvalidOperationException("Exit Play Mode before creating enemy prefabs.");
            Directory.CreateDirectory("Assets/Resources/Defense3D");
            AssetDatabase.Refresh();

            // Default fallback prefabs
            Create(DefenseEnemyPrefab3D.EnemyResource, new Color(0.78f, 0.14f, 0.23f), Vector3.one);
            Create(DefenseEnemyPrefab3D.BossResource, new Color(0.55f, 0.10f, 0.8f), new Vector3(2.2f, 2.2f, 2.2f));

            // Dedicated archetype prefabs with distinctive visual colors & scales
            Create(DefenseEnemyPrefab3D.MagiapResource, new Color(0.78f, 0.14f, 0.23f), new Vector3(1.0f, 1.0f, 1.0f));
            Create(DefenseEnemyPrefab3D.ThuyQuaiResource, new Color(0.15f, 0.55f, 0.82f), new Vector3(0.9f, 0.7f, 1.3f));
            Create(DefenseEnemyPrefab3D.HuyetDoiResource, new Color(0.55f, 0.05f, 0.05f), new Vector3(0.7f, 0.65f, 0.7f));
            Create(DefenseEnemyPrefab3D.QuaiDuResource, new Color(0.45f, 0.35f, 0.30f), new Vector3(0.85f, 1.0f, 0.85f));
            Create(DefenseEnemyPrefab3D.PhiCuongResource, new Color(0.48f, 0.12f, 0.64f), new Vector3(1.2f, 1.3f, 1.2f));
            Create(DefenseEnemyPrefab3D.MagiapBossResource, new Color(0.90f, 0.15f, 0.10f), new Vector3(1.7f, 1.7f, 1.7f));

            CreateWoodDrop();
            CreateDefinitions();
            WireArchetypeDefinitions();

            // Upgrade fallback and archetype prefabs
            UpgradePrefab(DefenseEnemyPrefab3D.EnemyResource, DefenseEnemyRegistry.ID_MAGIAP);
            UpgradePrefab(DefenseEnemyPrefab3D.BossResource, DefenseEnemyRegistry.ID_YAKSHA_BOSS);
            UpgradePrefab(DefenseEnemyPrefab3D.MagiapResource, DefenseEnemyRegistry.ID_MAGIAP);
            UpgradePrefab(DefenseEnemyPrefab3D.ThuyQuaiResource, DefenseEnemyRegistry.ID_THUYQUAI);
            UpgradePrefab(DefenseEnemyPrefab3D.HuyetDoiResource, DefenseEnemyRegistry.ID_HUYETDOI);
            UpgradePrefab(DefenseEnemyPrefab3D.QuaiDuResource, DefenseEnemyRegistry.ID_QUAIDU);
            UpgradePrefab(DefenseEnemyPrefab3D.PhiCuongResource, DefenseEnemyRegistry.ID_PHICUONG);
            UpgradePrefab(DefenseEnemyPrefab3D.MagiapBossResource, DefenseEnemyRegistry.ID_MAGIAP_BOSS);

            AssetDatabase.SaveAssets();
        }

        private static void CreateDefinitions()
        {
            Directory.CreateDirectory("Assets/Resources/Defense3D/Enemies");
            AssetDatabase.Refresh();
            string[] ids = { DefenseEnemyRegistry.ID_MAGIAP, DefenseEnemyRegistry.ID_THUYQUAI,
                DefenseEnemyRegistry.ID_HUYETDOI, DefenseEnemyRegistry.ID_QUAIDU,
                DefenseEnemyRegistry.ID_PHICUONG, DefenseEnemyRegistry.ID_MAGIAP_BOSS,
                DefenseEnemyRegistry.ID_YAKSHA_BOSS };
            foreach (string id in ids)
            {
                string path = "Assets/Resources/" + DefenseEnemyFactory3D.DefinitionsFolder + id + ".asset";
                if (File.Exists(path)) continue;
                var stats = DefenseEnemyRegistry.GetStats(id);
                var definition = ScriptableObject.CreateInstance<DefenseEnemyDataSO>();
                var serialized = new SerializedObject(definition);
                serialized.FindProperty("enemyId").stringValue = stats.EnemyId;
                serialized.FindProperty("displayName").stringValue = stats.DisplayName;
                serialized.FindProperty("locomotionType").enumValueIndex = (int)stats.Locomotion;
                serialized.FindProperty("isBossOrMiniboss").boolValue = stats.IsBoss;
                serialized.FindProperty("maxHP").floatValue = stats.MaxHp;
                serialized.FindProperty("moveSpeed").floatValue = stats.Speed;
                serialized.FindProperty("attackDamage").floatValue = stats.Damage;
                serialized.FindProperty("attackInterval").floatValue = stats.AttackInterval;
                serialized.FindProperty("attackRange").floatValue = stats.AttackRange;
                serialized.FindProperty("woodDrop").intValue = stats.WoodReward;
                serialized.FindProperty("expDrop").intValue = stats.ExpReward;
                string resource = DefenseEnemyPrefab3D.GetResourceForArchetype(id);
                serialized.FindProperty("prefab").objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/" + resource + ".prefab");
                serialized.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.CreateAsset(definition, path);
            }
        }

        private static void WireArchetypeDefinitions()
        {
            string[] ids = { DefenseEnemyRegistry.ID_MAGIAP, DefenseEnemyRegistry.ID_THUYQUAI,
                DefenseEnemyRegistry.ID_HUYETDOI, DefenseEnemyRegistry.ID_QUAIDU,
                DefenseEnemyRegistry.ID_PHICUONG, DefenseEnemyRegistry.ID_MAGIAP_BOSS,
                DefenseEnemyRegistry.ID_YAKSHA_BOSS };
            foreach (string id in ids)
            {
                string path = "Assets/Resources/" + DefenseEnemyFactory3D.DefinitionsFolder + id + ".asset";
                var definition = AssetDatabase.LoadAssetAtPath<DefenseEnemyDataSO>(path);
                if (definition == null) continue;
                string resource = DefenseEnemyPrefab3D.GetResourceForArchetype(id);
                var targetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/" + resource + ".prefab");
                if (targetPrefab != null && definition.Prefab != targetPrefab)
                {
                    var serialized = new SerializedObject(definition);
                    serialized.FindProperty("prefab").objectReferenceValue = targetPrefab;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(definition);
                }
            }
        }

        private static void UpgradePrefab(string resource, string enemyId)
        {
            string path = "Assets/Resources/" + resource + ".prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var actor = root.GetComponent<DefenseEnemy3D>() ?? root.AddComponent<DefenseEnemy3D>();
                if (root.GetComponent<DefenseGroundMovement3D>() == null) root.AddComponent<DefenseGroundMovement3D>();
                if (root.GetComponent<DefenseEnemyAttack3D>() == null) root.AddComponent<DefenseEnemyAttack3D>();
                if (root.GetComponent<DefenseEnemyDrop3D>() == null) root.AddComponent<DefenseEnemyDrop3D>();
                if (actor.Definition == null)
                {
                    var serialized = new SerializedObject(actor);
                    serialized.FindProperty("definition").objectReferenceValue =
                        AssetDatabase.LoadAssetAtPath<DefenseEnemyDataSO>("Assets/Resources/" +
                            DefenseEnemyFactory3D.DefinitionsFolder + enemyId + ".asset");
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void CreateWoodDrop()
        {
            string path = "Assets/Resources/" + DefenseWoodDrop3D.ResourcePath + ".prefab";
            if (File.Exists(path)) return;
            var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                root.name = "DefenseWoodDrop3D";
                root.transform.localScale = new Vector3(0.42f, 0.28f, 0.35f);
                Object.DestroyImmediate(root.GetComponent<Collider>());
                root.AddComponent<DefenseWoodDrop3D>();
                string materialPath = Path.ChangeExtension(path, ".mat");
                var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material == null)
                {
                    var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                    material = new Material(shader) { color = new Color(0.65f, 0.38f, 0.14f) };
                    AssetDatabase.CreateAsset(material, materialPath);
                }
                root.GetComponent<Renderer>().sharedMaterial = material;
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static void Create(string resource, Color color)
        {
            Create(resource, color, Vector3.one);
        }

        private static void Create(string resource, Color color, Vector3 scale)
        {
            string path = "Assets/Resources/" + resource + ".prefab";
            if (File.Exists(path)) return; // Preserve authored prefab edits.
            var root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            try
            {
                root.name = Path.GetFileNameWithoutExtension(path);
                root.transform.localScale = scale;
                Object.DestroyImmediate(root.GetComponent<Collider>());
                root.AddComponent<DefenseEnemyBridgeWalker3D>();
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                string materialPath = Path.ChangeExtension(path, ".mat");
                var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material == null)
                {
                    material = new Material(shader) { color = color };
                    AssetDatabase.CreateAsset(material, materialPath);
                }
                root.GetComponent<Renderer>().sharedMaterial = material;
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
