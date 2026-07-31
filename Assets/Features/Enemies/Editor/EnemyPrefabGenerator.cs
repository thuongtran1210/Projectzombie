#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectZombie.Core.ScriptableObjects;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Enemies.Behaviors;
using ProjectZombie.Features.Boss;

namespace ProjectZombie.Features.Enemies.Editor
{
    /// <summary>
    /// Editor Tool tự động tạo và cấu hình đầy đủ FSM, Strategies, Behaviors, Boss Controllers cho toàn bộ Enemy Prefabs.
    /// Menu: ProjectZombie > Enemies > Generate All Enemy & Boss Prefabs
    /// </summary>
    public static class EnemyPrefabGenerator
    {
        [MenuItem("ProjectZombie/Enemies/Generate All Enemy & Boss Prefabs")]
        public static void GenerateAllPrefabs()
        {
            string folderPath = "Assets/_Prefabs/Characters/Enemies";
            if (!AssetDatabase.IsValidFolder("Assets/_Prefabs")) AssetDatabase.CreateFolder("Assets", "_Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/_Prefabs/Characters")) AssetDatabase.CreateFolder("Assets/_Prefabs", "Characters");
            if (!AssetDatabase.IsValidFolder(folderPath)) AssetDatabase.CreateFolder("Assets/_Prefabs/Characters", "Enemies");

            // Load ExpGem prefab
            GameObject expGemPrefab = null;
            string[] expGuids = AssetDatabase.FindAssets("ExpGem t:Prefab", new string[] { "Assets/_Prefabs" });
            if (expGuids.Length > 0)
            {
                expGemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(expGuids[0]));
            }

            // 1. Tạo Prefabs Yêu Ma Thường & Elite với đầy đủ FSM & Strategy
            CreateEnemyPrefab(folderPath, "E_MAGIAP", "Ma Giáp", new Color(0.7f, 0.8f, 0.9f), 1.0f, isRanged: false, expGemPrefab: expGemPrefab);
            CreateEnemyPrefab(folderPath, "E_MATROI", "Ma Trơi", new Color(1.0f, 0.3f, 0.2f), 0.8f, isRanged: false, expGemPrefab: expGemPrefab);
            CreateEnemyPrefab(folderPath, "E_MADA", "Ma Da", new Color(0.2f, 0.6f, 1.0f), 0.9f, isRanged: true, expGemPrefab: expGemPrefab);
            CreateEnemyPrefab(folderPath, "E_HOALYTINH", "Hồ Ly Tinh Nhỏ", new Color(1.0f, 0.5f, 0.0f), 0.7f, isRanged: false, isSuicide: true, expGemPrefab: expGemPrefab);
            CreateEnemyPrefab(folderPath, "E_QUYNHAPTRANG", "Quỷ Nhập Tràng", new Color(0.6f, 0.5f, 0.2f), 1.5f, isRanged: false, isElite: true, expGemPrefab: expGemPrefab);

            // 2. Tạo Prefabs Bosses đầy đủ Boss Controllers & Dynamic Elements
            CreateBossPrefab(folderPath, "Boss_NguuDauMaDien", "Ngưu Đầu Mã Diện", new Color(0.8f, 0.2f, 0.2f), 2.2f, isFinalBoss: false, expGemPrefab: expGemPrefab);
            CreateBossPrefab(folderPath, "Boss_DiemVuong", "Diêm Vương", new Color(0.4f, 0.1f, 0.6f), 2.8f, isFinalBoss: true, expGemPrefab: expGemPrefab);

            // 3. Tạo Prefabs Rương Phần Thưởng
            CreateChestPrefab("Assets/_Prefabs/Chests", "Chest_UMinh", "Rương U Minh", new Color(0.3f, 0.8f, 1.0f));
            CreateChestPrefab("Assets/_Prefabs/Chests", "Chest_DauThai", "Rương Đầu Thai", new Color(1.0f, 0.85f, 0.2f));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EnemyPrefabGenerator] ✅ Đã cấu hình và khởi tạo đầy đủ FSM/Strategy/Behaviors cho toàn bộ Prefabs Yêu Ma & Boss!");
        }

        private static void CreateEnemyPrefab(string folderPath, string enemyId, string displayName, Color tintColor, float scale, bool isRanged = false, bool isSuicide = false, bool isElite = false, GameObject expGemPrefab = null)
        {
            string prefabPath = $"{folderPath}/{enemyId}.prefab";

            GameObject root = new GameObject(enemyId);
            root.tag = "Enemy";
            root.layer = LayerMask.NameToLayer("Enemy") != -1 ? LayerMask.NameToLayer("Enemy") : 0;
            root.transform.localScale = Vector3.one * scale;

            // Rigidbody2D
            var rb = root.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.mass = isElite ? 10f : 1f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // Collider2D
            var col = root.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            // HealthSystem
            var health = root.AddComponent<HealthSystem>();

            // Visual Child
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform);
            visual.transform.localPosition = Vector3.zero;

            var sr = visual.AddComponent<SpriteRenderer>();
            sr.color = tintColor;

            // Animator
            var animator = visual.AddComponent<Animator>();
            var enemyAnim = visual.AddComponent<EnemyAnimator>();

            // Strategy Component Setup (GDD Section 11)
            if (isRanged)
            {
                root.AddComponent<RangedMovementStrategy>();
                root.AddComponent<RangedAttackStrategy>();
            }
            else
            {
                root.AddComponent<MeleeMovementStrategy>();
                root.AddComponent<MeleeAttackStrategy>();
            }

            // Special Behaviors
            if (isSuicide)
            {
                root.AddComponent<SuicideExplodeBehavior>();
            }

            // FSM Main Component
            var enemyFSM = root.AddComponent<Enemy>();

            // Gán Config ScriptableObject
            string[] configGuids = AssetDatabase.FindAssets($"{enemyId} t:EnemyConfig", new string[] { "Assets/_Data/Enemies" });
            if (configGuids.Length > 0)
            {
                var enemyConfig = AssetDatabase.LoadAssetAtPath<EnemyConfig>(AssetDatabase.GUIDToAssetPath(configGuids[0]));
                if (enemyConfig != null)
                {
                    enemyFSM.Config = enemyConfig;
                    health.SetMaxHealth(enemyConfig.maxHealth, true);
                }
            }

            if (expGemPrefab != null)
            {
                SerializedObject enemySO = new SerializedObject(enemyFSM);
                var expProp = enemySO.FindProperty("expGemPrefab");
                if (expProp != null)
                {
                    expProp.objectReferenceValue = expGemPrefab;
                    enemySO.ApplyModifiedProperties();
                }
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
        }

        private static void CreateBossPrefab(string folderPath, string bossId, string displayName, Color tintColor, float scale, bool isFinalBoss = false, GameObject expGemPrefab = null)
        {
            string prefabPath = $"{folderPath}/{bossId}.prefab";

            GameObject root = new GameObject(bossId);
            root.tag = "Boss";
            root.layer = LayerMask.NameToLayer("Enemy") != -1 ? LayerMask.NameToLayer("Enemy") : 0;
            root.transform.localScale = Vector3.one * scale;

            var rb = root.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.mass = 50f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            var col = root.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.8f;

            var health = root.AddComponent<HealthSystem>();

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform);
            visual.transform.localPosition = Vector3.zero;

            var sr = visual.AddComponent<SpriteRenderer>();
            sr.color = tintColor;

            var animator = visual.AddComponent<Animator>();
            visual.AddComponent<EnemyAnimator>();

            // Strategies
            root.AddComponent<MeleeMovementStrategy>();
            root.AddComponent<MeleeAttackStrategy>();

            // Boss Dynamic Elements & Controllers
            root.AddComponent<BossElementController>();

            if (isFinalBoss)
            {
                root.AddComponent<AbominationBossController>();
            }
            else
            {
                root.AddComponent<SkeletonKingBossController>();
            }

            // FSM Component
            var enemyFSM = root.AddComponent<Enemy>();

            string[] configGuids = AssetDatabase.FindAssets($"{bossId} t:EnemyConfig", new string[] { "Assets/_Data/Enemies" });
            if (configGuids.Length > 0)
            {
                var enemyConfig = AssetDatabase.LoadAssetAtPath<EnemyConfig>(AssetDatabase.GUIDToAssetPath(configGuids[0]));
                if (enemyConfig != null)
                {
                    enemyFSM.Config = enemyConfig;
                    health.SetMaxHealth(enemyConfig.maxHealth, true);
                }
            }

            if (expGemPrefab != null)
            {
                SerializedObject enemySO = new SerializedObject(enemyFSM);
                var expProp = enemySO.FindProperty("expGemPrefab");
                if (expProp != null)
                {
                    expProp.objectReferenceValue = expGemPrefab;
                    enemySO.ApplyModifiedProperties();
                }
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
        }

        private static void CreateChestPrefab(string folderPath, string chestId, string displayName, Color tintColor)
        {
            if (!AssetDatabase.IsValidFolder(folderPath)) AssetDatabase.CreateFolder("Assets/_Prefabs", "Chests");
            string prefabPath = $"{folderPath}/{chestId}.prefab";

            GameObject root = new GameObject(chestId);
            root.tag = "Untagged";
            root.transform.localScale = Vector3.one * 1.2f;

            var col = root.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 1f);

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform);
            visual.transform.localPosition = Vector3.zero;

            var sr = visual.AddComponent<SpriteRenderer>();
            sr.color = tintColor;

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
        }
    }
}
#endif
