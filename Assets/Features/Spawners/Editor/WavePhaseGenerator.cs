#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Spawners;
using System.Collections.Generic;

namespace ProjectZombie.Features.Spawners.Editor
{
    /// <summary>
    /// Editor Tool sinh dữ liệu mẫu cho WavePhase (Pillar System) và tự động gán vào SpawnManager GameObject trong Scene.
    /// Menu: ProjectZombie > Spawners > Generate WavePhase Data & Assign to SpawnManager
    /// </summary>
    public static class WavePhaseGenerator
    {
        [MenuItem("ProjectZombie/Spawners/Generate WavePhase Data & Assign to SpawnManager")]
        public static void GenerateAndAssignWavePhases()
        {
            string folderPath = "Assets/_Data/PillarPhases";

            if (!AssetDatabase.IsValidFolder("Assets/_Data")) AssetDatabase.CreateFolder("Assets", "_Data");
            if (!AssetDatabase.IsValidFolder(folderPath)) AssetDatabase.CreateFolder("Assets/_Data", "PillarPhases");

            // 1. Tìm Prefabs quái mẫu để gán vào PillarConfig
            GameObject maGiapPrefab = null;
            string[] enemyGuids = AssetDatabase.FindAssets("E_MAGIAP t:Prefab", new string[] { "Assets/_Prefabs/Enemies" });
            if (enemyGuids.Length > 0)
            {
                maGiapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(enemyGuids[0]));
            }

            // 2. Tạo 3 WavePhase SOs đại diện cho 3 Phase chính (GDD 7.0)
            WavePhase phase1 = CreateOrGetPhase($"{folderPath}/Phase1_SuongMoULinh.asset", "Phase 1: Sương Mờ U Linh", 0f, new Color(0.2f, 0.4f, 0.6f), maGiapPrefab, 10, 2f);
            WavePhase phase2 = CreateOrGetPhase($"{folderPath}/Phase2_AmPhongHoangTuyen.asset", "Phase 2: Âm Phong Hoàng Tuyền", 300f, new Color(0.8f, 0.7f, 0.2f), maGiapPrefab, 20, 1.5f);
            WavePhase phase3 = CreateOrGetPhase($"{folderPath}/Phase3_BaoHacKhiHuyếtNguyet.asset", "Phase 3: Bão Hắc Khí Huyết Nguyệt", 600f, new Color(0.8f, 0.1f, 0.1f), maGiapPrefab, 30, 1f);

            // Gom 20 WaveConfig SOs vào 3 Phase tương ứng
            PopulateWavesIntoPhase(phase1, "Assets/_Data/Waves", 1, 5);  // Phút 1-5
            PopulateWavesIntoPhase(phase2, "Assets/_Data/Waves", 6, 10); // Phút 6-10
            PopulateWavesIntoPhase(phase3, "Assets/_Data/Waves", 11, 20); // Phút 11-20

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 3. Tự động tìm SpawnManager trong Scene và gán danh sách WavePhase
            SpawnManager spawnManager = Object.FindObjectOfType<SpawnManager>();
            if (spawnManager != null)
            {
                SerializedObject so = new SerializedObject(spawnManager);
                SerializedProperty phasesProp = so.FindProperty("phases");
                phasesProp.ClearArray();

                phasesProp.InsertArrayElementAtIndex(0);
                phasesProp.GetArrayElementAtIndex(0).objectReferenceValue = phase1;

                phasesProp.InsertArrayElementAtIndex(1);
                phasesProp.GetArrayElementAtIndex(1).objectReferenceValue = phase2;

                phasesProp.InsertArrayElementAtIndex(2);
                phasesProp.GetArrayElementAtIndex(2).objectReferenceValue = phase3;

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(spawnManager);
                Debug.Log($"[WavePhaseGenerator] ✅ Đã gán 3 WavePhase vào SpawnManager GameObject trên Scene!");
            }
            else
            {
                Debug.LogWarning("[WavePhaseGenerator] ⚠️ Đã tạo 3 file WavePhase SOs nhưng không tìm thấy GameObject SpawnManager trong Scene hiện tại.");
            }
        }

        private static WavePhase CreateOrGetPhase(string path, string phaseName, float startTime, Color color, GameObject enemyPrefab, int totalEnemies, float interval)
        {
            var phase = AssetDatabase.LoadAssetAtPath<WavePhase>(path);
            if (phase == null)
            {
                phase = ScriptableObject.CreateInstance<WavePhase>();
                AssetDatabase.CreateAsset(phase, path);
            }

            phase.phaseName = phaseName;
            phase.startTime = startTime;
            phase.atmosphereColor = color;
            if (enemyPrefab != null)
            {
                if (phase.continuousSpawnPrefabs == null) phase.continuousSpawnPrefabs = new List<GameObject>();
                if (!phase.continuousSpawnPrefabs.Contains(enemyPrefab))
                {
                    phase.continuousSpawnPrefabs.Add(enemyPrefab);
                }
            }

            phase.pillarConfigs = new List<SpawnPillarConfig>
            {
                new SpawnPillarConfig
                {
                    configName = $"{phaseName} - Pillar Alpha",
                    startPillarTime = 0f,
                    endPillarTime = 120f,
                    pillarSpawnInterval = 10f,
                    pillarSetup = new PillarConfig
                    {
                        enemyPrefab = enemyPrefab,
                        totalEnemiesToSpawn = totalEnemies,
                        enemySpawnInterval = interval,
                        isAttackable = true
                    }
                }
            };

            EditorUtility.SetDirty(phase);
            return phase;
        }

        private static void PopulateWavesIntoPhase(WavePhase phase, string waveFolder, int startMinute, int endMinute)
        {
            if (phase == null) return;
            phase.waveConfigs.Clear();

            for (int m = startMinute; m <= endMinute; m++)
            {
                string fileName = $"Wave_Minute_{m:00}";
                string[] guids = AssetDatabase.FindAssets($"{fileName} t:SpawnWaveConfig", new string[] { waveFolder });
                if (guids.Length > 0)
                {
                    string wavePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    var waveSO = AssetDatabase.LoadAssetAtPath<Enemies.SpawnWaveConfig>(wavePath);
                    if (waveSO != null && !phase.waveConfigs.Contains(waveSO))
                    {
                        phase.waveConfigs.Add(waveSO);
                    }
                }
            }
            EditorUtility.SetDirty(phase);
        }
    }
}
#endif
