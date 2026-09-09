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
            config.maxLevelDuration = 900f; // 15 phút (900 giây)
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

            // 00:00 (0s) - Quái nền: Ma Giáp (Quỷ Binh)
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 00:00 - Khởi đầu: Ma Giáp quỷ binh xuất hiện nền",
                timestampSeconds = 0f,
                eventType = TimelineEventType.Continuous,
                spawnPrefab = maGiapPrefab,
                enemyAddress = "E_MAGIAP",
                spawnCount = 3,
                spawnInterval = 2.5f
            });

            // 01:00 (60s) - Ma Da trơn trượt tăng tốc áp sát
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 01:00 - Ma Da trơn trượt tăng tốc áp sát",
                timestampSeconds = 60f,
                eventType = TimelineEventType.Continuous,
                spawnPrefab = maDaPrefab,
                enemyAddress = "E_MADA",
                spawnCount = 4,
                spawnInterval = 2.5f
            });

            // 02:00 (120s) - Ma Trơi bay lơ lửng phóng ma hỏa
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 02:00 - Ma Trơi bay lơ lửng phóng ma hỏa",
                timestampSeconds = 120f,
                eventType = TimelineEventType.Continuous,
                spawnPrefab = maTroiPrefab,
                enemyAddress = "E_MATROI",
                spawnCount = 3,
                spawnInterval = 3.0f
            });

            // 03:00 (180s) - Bầy Ma Da tràn lên bao vây (Burst Wave)
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 03:00 - Bầy Ma Da tràn lên bao vây (Burst Wave)",
                timestampSeconds = 180f,
                eventType = TimelineEventType.BurstWave,
                spawnPrefab = maDaPrefab,
                enemyAddress = "E_MADA",
                spawnCount = 15,
                spawnInterval = 0.1f
            });

            // 04:30 (270s) - Elite Quỷ Nhập Tràng xuất hiện (Thịt đè người)
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 04:30 - ELITE QUỶ NHẬP TRÀNG XUẤT HIỆN",
                timestampSeconds = 270f,
                eventType = TimelineEventType.BurstWave,
                spawnPrefab = quyNhapTrangPrefab,
                enemyAddress = "E_QUYNHAPTRANG",
                spawnCount = 1,
                spawnInterval = 0.0f
            });

            // 06:00 (360s) - Ma Đòi Nợ & Hồ Ly Tinh tinh quái xuất hiện
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 06:00 - Ma Đòi Nợ & Hồ Ly Tinh tinh quái",
                timestampSeconds = 360f,
                eventType = TimelineEventType.Continuous,
                spawnPrefab = hoaLyTinhPrefab,
                enemyAddress = "E_HOALYTINH",
                spawnCount = 4,
                spawnInterval = 3.0f
            });

            // 06:45 (405s) - Ma Đòi Nợ lén lút thó tiền chạy trốn
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 06:45 - Ma Đòi Nợ lén lút thó tiền chạy trốn",
                timestampSeconds = 405f,
                eventType = TimelineEventType.Continuous,
                spawnPrefab = maDoiNoPrefab,
                enemyAddress = "E_MADOINO",
                spawnCount = 2,
                spawnInterval = 8.0f
            });

            // 07:30 (450s) - Mid-Boss Ngưu Đầu Mã Diện xuất hiện (Chính giữa trận đấu 15 phút)
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 07:30 - MID-BOSS NGƯU ĐẦU MÃ DIỆN XUẤT HIỆN",
                timestampSeconds = 450f,
                eventType = TimelineEventType.BossSpawn,
                spawnPrefab = nguuDauMaDienPrefab,
                enemyAddress = "Boss_NguuDauMaDien",
                spawnCount = 1,
                spawnInterval = 0.0f
            });

            // 09:30 (570s) - Bão Ma Hỏa & Hồ Ly Tinh bao vây (Burst Wave)
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 09:30 - Bão Ma Hỏa & Hồ Ly Tinh bao vây (Burst Wave)",
                timestampSeconds = 570f,
                eventType = TimelineEventType.BurstWave,
                spawnPrefab = hoaLyTinhPrefab,
                enemyAddress = "E_HOALYTINH",
                spawnCount = 20,
                spawnInterval = 0.1f
            });

            // 11:30 (690s) - Đội hình Quỷ Binh & Cương Thi tổng lực
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 11:30 - Đội hình Quỷ Binh & Cương Thi tổng lực",
                timestampSeconds = 690f,
                eventType = TimelineEventType.Continuous,
                spawnPrefab = quyNhapTrangPrefab,
                enemyAddress = "E_QUYNHAPTRANG",
                spawnCount = 3,
                spawnInterval = 4.0f
            });

            // 13:30 (810s) - Đại Bão Yêu Ma Pre-Boss Rush (Burst Wave)
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 13:30 - Đại Bão Yêu Ma Pre-Boss Rush (Burst Wave)",
                timestampSeconds = 810f,
                eventType = TimelineEventType.BurstWave,
                spawnPrefab = maGiapPrefab,
                enemyAddress = "E_MAGIAP",
                spawnCount = 30,
                spawnInterval = 0.05f
            });

            // 15:00 (900s) - Final Boss Diêm Vương giáng lâm
            config.events.Add(new TimelineEvent
            {
                eventName = "Phút 15:00 - FINAL BOSS DIÊM VƯƠNG GIÁNG LÂM",
                timestampSeconds = 900f,
                eventType = TimelineEventType.BossSpawn,
                spawnPrefab = diemVuongPrefab,
                enemyAddress = "Boss_DiemVuong",
                spawnCount = 1,
                spawnInterval = 0.0f
            });

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Highlight/Ping Asset trong Project Window để Designer thấy ngay kết quả
            EditorGUIUtility.PingObject(config);
            Selection.activeObject = config;

            string summaryMsg = $"Đã cập nhật thành công Level Timeline Asset (15 Phút - 900s)!\n\n" +
                               $"- Màn chơi: {config.levelName}\n" +
                               $"- Tổng số Event: {config.events.Count} sự kiện\n" +
                               $"- Mid-Boss: Phút 07:30 (450s)\n" +
                               $"- Final Boss: Phút 15:00 (900s)\n" +
                               $"- Vị trí lưu: {assetPath}";

            Debug.Log($"[LevelTimelineGenerator] ✅ {summaryMsg}");
            EditorUtility.DisplayDialog("Level Timeline Generator", summaryMsg, "Xác nhận");
        }
    }
}
#endif

