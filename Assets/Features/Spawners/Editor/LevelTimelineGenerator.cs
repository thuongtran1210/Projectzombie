#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.Features.Spawners.Editor
{
    /// <summary>
    /// Editor Tool tạo và đồng bộ Timeline màn 1 (Màn 1: U Minh Giới) với đầy đủ tất cả Enemy & Boss theo GDD v5.0.
    /// Menu: Tools > ProjectZombie > Spawners > Generate Level 1 Timeline Asset
    /// </summary>
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

            string assetPath = $"{folderPath}/Level1_Timeline.asset";
            LevelTimelineConfig config = AssetDatabase.LoadAssetAtPath<LevelTimelineConfig>(assetPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<LevelTimelineConfig>();
                AssetDatabase.CreateAsset(config, assetPath);
            }

            config.levelName = "Màn 1: U Minh Giới";
            config.maxLevelDuration = 1200f; // 20 phút (1200 giây)
            config.events.Clear();

            // Load Prefabs từ thư mục Enemies
            GameObject maGiapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Characters/Enemies/E_MAGIAP.prefab");
            GameObject maDaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Characters/Enemies/E_MADA.prefab");
            GameObject maTroiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Characters/Enemies/E_MATROI.prefab");
            GameObject hoaLyTinhPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Characters/Enemies/E_HOALYTINH.prefab");
            GameObject maDoiNoPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Characters/Enemies/E_MADOINO.prefab");
            GameObject quyNhapTrangPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Characters/Enemies/E_QUYNHAPTRANG.prefab");
            GameObject nguuDauMaDienPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Characters/Enemies/Boss_NguuDauMaDien.prefab");
            GameObject diemVuongPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Characters/Enemies/Boss_DiemVuong.prefab");

            // 00:00 - Quái nền: Ma Giáp (Quỷ Binh)
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 00:00 - Khởi đầu: Ma Giáp quỷ binh xuất hiện nền",
                timestampSeconds = 0f,
                eventType = TimelineEventType.Continuous,
                spawnPrefab = maGiapPrefab,
                enemyAddress = "E_MAGIAP",
                spawnCount = 3,
                spawnInterval = 4.0f
            });

            // 01:00 - Ma Da trơn trượt áp sát
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 01:00 - Ma Da trơn trượt tăng tốc áp sát",
                timestampSeconds = 60f,
                eventType = TimelineEventType.Continuous,
                spawnPrefab = maDaPrefab,
                enemyAddress = "E_MADA",
                spawnCount = 4,
                spawnInterval = 3.5f
            });

            // 02:00 - Ma Trơi bay lơ lửng phóng ma hỏa
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 02:00 - Ma Trơi bay lơ lửng phóng ma hỏa",
                timestampSeconds = 120f,
                eventType = TimelineEventType.Continuous,
                spawnPrefab = maTroiPrefab,
                enemyAddress = "E_MATROI",
                spawnCount = 3,
                spawnInterval = 4.0f
            });

            // 03:00 - Bầy Ma Da tràn lên bao vây (Burst Wave)
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 03:00 - Bầy Ma Da tràn lên bao vây (Burst Wave)",
                timestampSeconds = 180f,
                eventType = TimelineEventType.BurstWave,
                spawnPrefab = maDaPrefab,
                enemyAddress = "E_MADA",
                spawnCount = 12,
                spawnInterval = 0.2f
            });

            // 04:00 - Hồ Ly Tinh tinh quái lao vào tự nổ AoE
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 04:00 - Bầy Hồ Ly Tinh tinh quái lao vào tự nổ",
                timestampSeconds = 240f,
                eventType = TimelineEventType.Continuous,
                spawnPrefab = hoaLyTinhPrefab,
                enemyAddress = "E_HOALYTINH",
                spawnCount = 4,
                spawnInterval = 4.0f
            });

            // 05:00 - Elite Quỷ Nhập Tràng xuất hiện (Thịt đè người)
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 05:00 - ELITE QUỶ NHẬP TRÀNG XUẤT HIỆN",
                timestampSeconds = 300f,
                eventType = TimelineEventType.BurstWave,
                spawnPrefab = quyNhapTrangPrefab,
                enemyAddress = "E_QUYNHAPTRANG",
                spawnCount = 1,
                spawnInterval = 0.0f
            });

            // 06:00 - Ma Đòi Nợ lén lút thó tiền chạy trốn
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 06:00 - Ma Đòi Nợ lén lút thó tiền chạy trốn",
                timestampSeconds = 360f,
                eventType = TimelineEventType.Continuous,
                spawnPrefab = maDoiNoPrefab,
                enemyAddress = "E_MADOINO",
                spawnCount = 2,
                spawnInterval = 12.0f
            });

            // 08:00 - Bão Ma Hỏa & Hồ Ly Tinh bao vây (Burst Wave)
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 08:00 - Bão Ma Hỏa & Hồ Ly Tinh bao vây (Burst Wave)",
                timestampSeconds = 480f,
                eventType = TimelineEventType.BurstWave,
                spawnPrefab = hoaLyTinhPrefab,
                enemyAddress = "E_HOALYTINH",
                spawnCount = 16,
                spawnInterval = 0.2f
            });

            // 10:00 - Mid-Boss Ngưu Đầu Mã Diện xuất hiện
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 10:00 - MID-BOSS NGƯU ĐẦU MÃ DIỆN XUẤT HIỆN",
                timestampSeconds = 600f,
                eventType = TimelineEventType.BossSpawn,
                spawnPrefab = nguuDauMaDienPrefab,
                enemyAddress = "Boss_NguuDauMaDien",
                spawnCount = 1,
                spawnInterval = 0.0f
            });

            // 12:00 - Đội hình Quỷ Binh & Cương Thi tổng lực
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 12:00 - Đội hình Quỷ Binh & Cương Thi tổng lực",
                timestampSeconds = 720f,
                eventType = TimelineEventType.Continuous,
                spawnPrefab = quyNhapTrangPrefab,
                enemyAddress = "E_QUYNHAPTRANG",
                spawnCount = 2,
                spawnInterval = 6.0f
            });

            // 15:00 - Đại Bão Yêu Ma tổng lực (Multi-Burst Wave)
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 15:00 - Đại Bão Yêu Ma tổng lực (Multi-Burst Wave)",
                timestampSeconds = 900f,
                eventType = TimelineEventType.BurstWave,
                spawnPrefab = maGiapPrefab,
                enemyAddress = "E_MAGIAP",
                spawnCount = 25,
                spawnInterval = 0.1f
            });

            // 20:00 - Final Boss Diêm Vương giáng lâm
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 20:00 - FINAL BOSS DIÊM VƯƠNG GIÁNG LÂM",
                timestampSeconds = 1200f,
                eventType = TimelineEventType.BossSpawn,
                spawnPrefab = diemVuongPrefab,
                enemyAddress = "Boss_DiemVuong",
                spawnCount = 1,
                spawnInterval = 0.0f
            });

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[LevelTimelineGenerator] ✅ Đã cập nhật thành công Level Timeline Asset tại: {assetPath} với đầy đủ tất cả Enemy & Boss!");
        }
    }
}
#endif

