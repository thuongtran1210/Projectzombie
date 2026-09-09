#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Enemies;

namespace ProjectZombie.Features.Enemies.Editor
{
    /// <summary>
    /// Editor Tool tự động tạo 15 Wave Config ScriptableObjects bám sát 100% GDD v4.0 Timeline
    /// trong Assets/_Data/Waves/
    /// Menu: ProjectZombie > Waves > Generate 15 Minute Wave Configs (GDD 4.0 Timeline)
    /// </summary>
    public static class WaveDataGenerator
    {
        private struct WaveDef
        {
            public string fileName;
            public float triggerSeconds;
            public int spawnCount;
            public float interval;
            public bool isElite;
            public bool isBoss;
            public float hpMultiplier;

            public WaveDef(string fileName, float triggerSeconds, int spawnCount, float interval, bool isElite = false, bool isBoss = false, float hpMultiplier = 0f)
            {
                this.fileName = fileName;
                this.triggerSeconds = triggerSeconds;
                this.spawnCount = spawnCount;
                this.interval = interval;
                this.isElite = isElite;
                this.isBoss = isBoss;
                this.hpMultiplier = hpMultiplier;
            }
        }

        [MenuItem("ProjectZombie/Waves/Generate 15 Minute Wave Configs (GDD 4.0 Timeline)")]
        public static void GenerateAllWaves()
        {
            string folderPath = "Assets/_Data/Waves";

            if (!AssetDatabase.IsValidFolder("Assets/_Data")) AssetDatabase.CreateFolder("Assets", "_Data");
            if (!AssetDatabase.IsValidFolder(folderPath)) AssetDatabase.CreateFolder("Assets/_Data", "Waves");

            WaveDef[] waveTimeline = new WaveDef[]
            {
                new WaveDef("Wave_Minute_01", 30f, 15, 0.6f),
                new WaveDef("Wave_Minute_02", 90f, 20, 0.5f),
                new WaveDef("Wave_Minute_03", 150f, 25, 0.4f),
                new WaveDef("Wave_Minute_04", 210f, 30, 0.35f),
                new WaveDef("Wave_Minute_05", 270f, 1, 0f, true, false, 2.0f), // Elite 1: Quỷ Nhập Tràng (04:30)
                new WaveDef("Wave_Minute_06", 330f, 30, 0.3f),
                new WaveDef("Wave_Minute_07", 390f, 35, 0.3f),
                new WaveDef("Wave_Minute_08", 450f, 1, 0f, false, true, 5.0f),  // Mid-Boss: Ngưu Đầu Mã Diện (07:30)
                new WaveDef("Wave_Minute_09", 510f, 40, 0.25f),
                new WaveDef("Wave_Minute_10", 570f, 45, 0.25f, true),            // Mini Swarm (Bão Yêu 45 mob - 09:30)
                new WaveDef("Wave_Minute_11", 630f, 45, 0.25f),
                new WaveDef("Wave_Minute_12", 690f, 50, 0.2f),
                new WaveDef("Wave_Minute_13", 750f, 50, 0.2f),
                new WaveDef("Wave_Minute_14", 810f, 50, 0.15f, true),            // Pre-Boss Multi-Elite Rush (13:30)
                new WaveDef("Wave_Minute_15", 900f, 1, 0f, false, true, 15.0f)  // Final Boss: Diêm Vương (15:00)
            };

            int successCount = 0;
            System.Collections.Generic.HashSet<string> generatedFileNames = new System.Collections.Generic.HashSet<string>();

            foreach (var def in waveTimeline)
            {
                generatedFileNames.Add($"{def.fileName}.asset");
                string assetPath = $"{folderPath}/{def.fileName}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<SpawnWaveConfig>(assetPath);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<SpawnWaveConfig>();
                    AssetDatabase.CreateAsset(asset, assetPath);
                }

                SerializedObject so = new SerializedObject(asset);
                so.FindProperty("triggerTimeSeconds").floatValue = def.triggerSeconds;
                so.FindProperty("baseSpawnCount").intValue = def.spawnCount;
                so.FindProperty("spawnInterval").floatValue = def.interval;
                so.FindProperty("isEliteWave").boolValue = def.isElite;
                so.FindProperty("isBossWave").boolValue = def.isBoss;
                so.FindProperty("hpMultiplierOverride").floatValue = def.hpMultiplier;
                so.ApplyModifiedProperties();

                EditorUtility.SetDirty(asset);
                successCount++;
            }

            // Tự động quét và dọn sạch các file ScriptableObject cũ không còn nằm trong Timeline (ví dụ: Wave_Minute_16 đến 20)
            int deletedCount = 0;
            string[] existingGuids = AssetDatabase.FindAssets("t:SpawnWaveConfig", new string[] { folderPath });
            foreach (var guid in existingGuids)
            {
                string existingPath = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = System.IO.Path.GetFileName(existingPath);
                if (!generatedFileNames.Contains(fileName))
                {
                    AssetDatabase.DeleteAsset(existingPath);
                    deletedCount++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string reportMsg = $"Đã tạo/cập nhật thành công {successCount} Wave Config SOs (15 Phút) trong {folderPath}!\n" +
                              (deletedCount > 0 ? $"Đã tự động xóa sạch {deletedCount} Wave SO cũ thừa (Wave 16-20)." : "Không có file thừa cần xóa.");

            Debug.Log($"[WaveDataGenerator] {reportMsg}");
            EditorUtility.DisplayDialog("Wave Data Generator", reportMsg, "Tuyệt vời");
        }
    }
}
#endif
