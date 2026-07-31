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
                new WaveDef("Wave_Minute_01", 30f, 15, 0.5f),
                new WaveDef("Wave_Minute_02", 90f, 25, 0.4f),
                new WaveDef("Wave_Minute_03", 150f, 35, 0.3f, true), // Elite Wave 1
                new WaveDef("Wave_Minute_04", 210f, 45, 0.3f),
                new WaveDef("Wave_Minute_05", 300f, 1, 0f, false, true, 5.0f), // Boss 1: Ngưu Đầu Mã Diện
                new WaveDef("Wave_Minute_06", 360f, 50, 0.25f),
                new WaveDef("Wave_Minute_07", 420f, 60, 0.25f),
                new WaveDef("Wave_Minute_08", 480f, 100, 0.1f, true), // Horde Burst Wave
                new WaveDef("Wave_Minute_09", 540f, 75, 0.2f),
                new WaveDef("Wave_Minute_10", 600f, 85, 0.2f, true), // Elite Wave 2
                new WaveDef("Wave_Minute_11", 660f, 100, 0.15f),
                new WaveDef("Wave_Minute_12", 720f, 120, 0.15f),
                new WaveDef("Wave_Minute_13", 780f, 140, 0.1f, true), // Elite Wave 3
                new WaveDef("Wave_Minute_14", 840f, 160, 0.1f),
                new WaveDef("Wave_Minute_15", 900f, 1, 0f, false, true, 12.0f) // Final Boss: Diêm Vương
            };

            int successCount = 0;
            foreach (var def in waveTimeline)
            {
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

                successCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[WaveDataGenerator] Đã tạo/cập nhật thành công {successCount} Wave Config SOs trong {folderPath} bám sát GDD v4.0 Timeline!");
        }
    }
}
#endif
