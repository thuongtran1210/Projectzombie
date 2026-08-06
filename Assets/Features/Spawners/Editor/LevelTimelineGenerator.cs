#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.Features.Spawners.Editor
{
    public static class LevelTimelineGenerator
    {
        [MenuItem("Tools/ProjectZombie/Spawners/Generate Level 1 Timeline Asset")]
        public static void GenerateLevel1Timeline()
        {
            string folderPath = "Assets/_Data/Levels";
            if (!AssetDatabase.IsValidFolder("Assets/_Data"))
            {
                AssetDatabase.CreateFolder("Assets", "_Data");
            }
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/_Data", "Levels");
            }

            LevelTimelineConfig config = ScriptableObject.CreateInstance<LevelTimelineConfig>();
            config.levelName = "Màn 1: U Minh Giới";
            config.maxLevelDuration = 1200f; // 20 phút

            // Thêm các mốc sự kiện mẫu theo GDD
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 00:00 - Quỷ Xương xuất hiện nền",
                timestampSeconds = 0f,
                eventType = TimelineEventType.Continuous,
                spawnCount = 150,
                spawnInterval = 1.5f
            });

            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 03:00 - Bầy Quỷ Xương bao vây (Burst)",
                timestampSeconds = 180f,
                eventType = TimelineEventType.BurstWave,
                spawnCount = 30
            });

            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 10:00 - BOSS 1 NGƯU ĐẦU MÃ DIỆN XUẤT HIỆN",
                timestampSeconds = 600f,
                eventType = TimelineEventType.BossSpawn
            });

            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 20:00 - FINAL BOSS DIÊM VƯƠNG XUẤT HIỆN",
                timestampSeconds = 1200f,
                eventType = TimelineEventType.BossSpawn
            });

            string assetPath = $"{folderPath}/Level1_Timeline.asset";
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[LevelTimelineGenerator] Đã tạo thành công Level Timeline Asset tại: {assetPath}!");
        }
    }
}
#endif
