using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Định nghĩa cấu hình một đợt xuất hiện quái vật trên dòng thời gian trận đấu (Timeline Wave Entry).
    /// </summary>
    [Serializable]
    public sealed class DefenseStageWaveEntry
    {
        [Tooltip("Mã định danh đợt xuất hiện quái (Ví dụ: benque-01, wave-02)")]
        public string eventId = "wave-01";

        [Tooltip("Thời điểm đợt quái xuất hiện tính từ đầu trận (giây). Ví dụ: 18s, 108s")]
        [Min(0f)] public float triggerTime;

        [Tooltip("Loại quái vật xuất hiện chính trong đợt sóng này")]
        public DefenseEnemyDataSO enemy;

        [Tooltip("Vùng xuất hiện trên bản đồ (chọn qua danh mục Dropdown an toàn, chống gõ sai chính tả)")]
        public DefenseSpawnZoneType zone = DefenseSpawnZoneType.EastBank;

        [Tooltip("Số lượng quái vật sinh ra trong đợt này")]
        [Min(1)] public int count = 1;

        [Tooltip("Quái vật thủ lĩnh đi đầu đàn (tùy chọn: thay thế con quái đầu tiên, không làm tăng tổng số lượng quái)")]
        public DefenseEnemyDataSO leadEnemy;
    }

    /// <summary>
    /// Cấu hình tọa độ và các điểm xuất hiện chi tiết của một Vùng trên bản đồ.
    /// </summary>
    [Serializable]
    public sealed class DefenseSpawnZoneBlueprint
    {
        [Tooltip("Vùng xuất hiện (chọn qua Dropdown)")]
        public DefenseSpawnZoneType zoneType = DefenseSpawnZoneType.EastBank;

        [Tooltip("Tích chọn: Tọa độ tính tương đối từ mốc Bờ Đông (EastMarker). Bỏ chọn: Tọa độ tuyệt đối.")]
        public bool relativeToEastMarker = true;

        [Tooltip("Tọa độ gốc tâm điểm của vùng xuất hiện")]
        public Vector3 origin;

        [Tooltip("Danh sách các điểm xuất hiện chi tiết trong vùng này")]
        public DefenseSpawnPointConfig[] points = Array.Empty<DefenseSpawnPointConfig>();
    }

    /// <summary>
    /// BẢN THIẾT KẾ ẢI CHIẾN TỔNG THỂ (Master Stage Blueprint).
    /// Trung tâm dữ liệu duy nhất (Single Source of Truth) kết hợp toàn bộ:
    /// - Thông tin Màn chơi & Thưởng vượt ải
    /// - Dòng thời gian đợt quái (Data-driven Timeline Waves)
    /// - Vị trí các Vùng xuất hiện trên bản đồ (Spawn Zones)
    /// - Đại Boss Đấu Trường quyết chiến (Arena Climax Boss)
    /// - Giới hạn hiệu năng chống giật lag trên thiết bị di động
    /// </summary>
    [CreateAssetMenu(fileName = "StageBlueprint_New", menuName = "ProjectZombie/Defense3D/Stage Blueprint (Bản Thiết Kế Ải)")]
    public sealed class DefenseStageBlueprintSO : ScriptableObject
    {
        [Header("── 1. THÔNG TIN ẢI CHIẾN (Stage Profile) ──")]
        [Tooltip("Mã định danh màn chơi (Ví dụ: STAGE_01, STAGE_02)")]
        public string stageId = "STAGE_01";

        [Tooltip("Tên màn chơi hiển thị thuần Việt Cổ Phong")]
        public string stageName = "Sông Quê Âm Ty";

        [Tooltip("Mô tả bối cảnh và cốt truyện ngắn của màn chơi")]
        [TextArea(2, 4)]
        public string description = "Nơi âm khí tích tụ ngàn năm dưới những rặng tre ma, đàn thủy quái và cương thi bắt đầu trỗi dậy.";

        [Tooltip("Hình ảnh minh họa màn chơi trên giao diện Chọn Ải")]
        public Sprite stageThumbnail;

        [Tooltip("Thời lượng tiêu chuẩn của trận chiến (giây). Mặc định: 600s = 10 phút")]
        [Min(60f)] public float stageDurationSeconds = 600f;

        [Header("── 2. TÀI NGUYÊN & PHẦN THƯỞNG (Assets & Rewards) ──")]
        [Tooltip("Địa chỉ Addressable nạp Prefab Bản đồ hoặc tên Scene")]
        public string mapAddress = "MapSongQue3D_Prototype";

        [Tooltip("Địa chỉ Addressable nạp Nhạc nền trận chiến")]
        public string bgmAddress = "BGM_SongQue";

        [Tooltip("Số Cổ Tiền cơ bản nhận được khi vượt ải thành công")]
        [Min(0)] public int baseRewardCoins = 500;

        [Tooltip("Điểm kinh nghiệm cơ bản nhận được khi vượt ải thành công")]
        [Min(0)] public int baseRewardExp = 250;

        [Header("── 3. CÁC VÙNG XUẤT HIỆN TRÊN BẢN ĐỒ (Spawn Zones) ──")]
        [Tooltip("Danh sách cấu hình tọa độ cho từng vùng trên bản đồ")]
        public DefenseSpawnZoneBlueprint[] spawnZones = Array.Empty<DefenseSpawnZoneBlueprint>();

        [Header("── 4. DÒNG THỜI GIAN ĐỢT QUÁI (Timeline Waves) ──")]
        [Tooltip("Danh sách toàn bộ các đợt xuất hiện quái vật từ phút 00:00 đến khi Boss xuất hiện")]
        public List<DefenseStageWaveEntry> timelineWaves = new List<DefenseStageWaveEntry>();

        [Header("── 5. ĐẠI BOSS CUỐI TRẬN (Arena Climax Boss) ──")]
        [Tooltip("Dữ liệu của Đại Boss xuất hiện ở giai đoạn quyết chiến Đấu Trường cuối trận")]
        public DefenseEnemyDataSO arenaBoss;

        [Tooltip("Vùng xuất hiện dành riêng cho Boss (mặc định: Đấu Trường Quyết Chiến)")]
        public DefenseSpawnZoneType bossZone = DefenseSpawnZoneType.ArenaEast;

        [Tooltip("Thời điểm Đại Boss xuất hiện (giây). Mặc định: 510s = 8 phút 30 giây")]
        [Min(0f)] public float bossSpawnTime = 510f;

        [Tooltip("Thời gian tối đa để tiêu diệt Boss trong Đấu Trường (giây) trước khi kết luận thất bại")]
        [Min(10f)] public float arenaDuration = 45f;

        [Tooltip("Lượng máu tối đa của Boss áp dụng riêng cho màn chơi này")]
        [Min(1f)] public float bossHealthOverride = 1200f;

        [Header("── 6. GIỚI HẠN HIỆU NĂNG & CHỐNG GIẬT LAG (Performance Budget) ──")]
        [Tooltip("Số lượng quái vật tối đa đồng thời hoạt động trên sân (chống quá tải CPU/GPU trên di động)")]
        [Min(1)] public int maxActiveEnemies = 32;

        [Tooltip("Số lượng quái vật tối đa được xếp hàng chờ sinh ra trong bộ đệm")]
        [Min(1)] public int maxPendingSpawns = 40;

        [Tooltip("Số lượng quái vật tối đa được phép khởi tạo trong mỗi khung hình (tránh drop FPS đột ngột)")]
        [Min(1)] public int spawnBudgetPerFrame = 4;

        [Tooltip("Thời gian sống tối đa của yêu cầu spawn trong hàng đợi (giây). Quá thời gian này sẽ tự hủy để chống nghẽn hàng.")]
        [Min(1f)] public float jobLifetime = 25f;

        [Tooltip("Khoảng cách an toàn tối thiểu với Anh Hùng (mét). Quái sẽ không spawn nếu người chơi đứng quá gần vùng này.")]
        [Min(0f)] public float safeHeroDistance = 3.1f;

        /// <summary>
        /// Biên dịch Bản Thiết Kế Ải thành DefenseStageSpawnPlan phục vụ runtime trận đấu.
        /// Đảm bảo tính tương thích 100% với hệ thống runtime của Defense 3D.
        /// </summary>
        public DefenseStageSpawnPlan Compile()
        {
            if (string.IsNullOrWhiteSpace(stageId))
                throw new InvalidOperationException("[DefenseStageBlueprintSO] Mã định danh Ải (StageId) không được để trống.");

            if (arenaBoss == null)
                throw new InvalidOperationException("[DefenseStageBlueprintSO] Chưa gán Đại Boss Đấu Trường (ArenaBoss).");

            if (bossHealthOverride <= 0f || float.IsNaN(bossHealthOverride) || float.IsInfinity(bossHealthOverride))
                throw new InvalidOperationException("[DefenseStageBlueprintSO] Lượng máu của Boss phải là một số dương hữu hạn.");

            var definitions = new Dictionary<string, DefenseEnemyDataSO>(StringComparer.Ordinal);
            var zoneCopies = new Dictionary<string, DefenseSpawnZoneSnapshot>(StringComparer.Ordinal);

            // 1. Biên dịch các vùng Spawn Zones
            foreach (var zoneBp in spawnZones ?? Array.Empty<DefenseSpawnZoneBlueprint>())
            {
                if (zoneBp == null) continue;
                string zoneId = zoneBp.zoneType.ToZoneId();

                if (zoneCopies.ContainsKey(zoneId))
                    throw new InvalidOperationException($"[DefenseStageBlueprintSO] Trùng lặp cấu hình vùng xuất hiện: {zoneBp.zoneType}");

                if (zoneBp.points == null || zoneBp.points.Length == 0)
                    throw new InvalidOperationException($"[DefenseStageBlueprintSO] Vùng '{zoneBp.zoneType}' phải có ít nhất 1 điểm xuất hiện.");

                var points = new DefenseSpawnLocation[zoneBp.points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    var pt = zoneBp.points[i];
                    if (pt == null || pt.Lane < 0 || pt.Lane > 2)
                        throw new InvalidOperationException($"[DefenseStageBlueprintSO] Điểm xuất hiện không hợp lệ tại vùng: {zoneBp.zoneType}");

                    points[i] = new DefenseSpawnLocation(pt.Offset, pt.Lane);
                }

                zoneCopies.Add(zoneId, new DefenseSpawnZoneSnapshot(zoneBp.relativeToEastMarker, zoneBp.origin, points));
            }

            // Đảm bảo có ít nhất vùng cho Boss
            string bossZoneId = bossZone.ToZoneId();
            if (!zoneCopies.ContainsKey(bossZoneId))
            {
                // Fallback mặc định an toàn cho Boss zone nếu chưa cấu hình riêng
                zoneCopies.Add(bossZoneId, new DefenseSpawnZoneSnapshot(true, new Vector3(0f, 1f, 0f),
                    new[] { new DefenseSpawnLocation(Vector3.zero, 1) }));
            }

            // 2. Biên dịch các đợt quái (Timeline Waves)
            var scheduled = new List<DefenseScheduledSpawn>();
            var eventIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (var wave in timelineWaves ?? new List<DefenseStageWaveEntry>())
            {
                if (wave == null) continue;

                if (string.IsNullOrWhiteSpace(wave.eventId))
                    throw new InvalidOperationException("[DefenseStageBlueprintSO] Có một đợt quái bị trống EventId.");

                if (!eventIds.Add(wave.eventId))
                    throw new InvalidOperationException($"[DefenseStageBlueprintSO] Trùng lặp EventId '{wave.eventId}'. Mỗi đợt phải có mã duy nhất.");

                if (wave.enemy == null)
                    throw new InvalidOperationException($"[DefenseStageBlueprintSO] Đợt '{wave.eventId}' chưa gán loại Quái Vật chính.");

                if (wave.count < 1)
                    throw new InvalidOperationException($"[DefenseStageBlueprintSO] Đợt '{wave.eventId}' có số lượng quái nhỏ hơn 1.");

                string zoneId = wave.zone.ToZoneId();
                if (!zoneCopies.ContainsKey(zoneId))
                    throw new InvalidOperationException($"[DefenseStageBlueprintSO] Đợt '{wave.eventId}' chỉ định vùng '{wave.zone}' chưa được định nghĩa trong SpawnZones.");

                AddDefinition(definitions, wave.enemy);
                if (wave.leadEnemy != null) AddDefinition(definitions, wave.leadEnemy);

                scheduled.Add(new DefenseScheduledSpawn(
                    wave.eventId,
                    wave.triggerTime,
                    wave.enemy.EnemyId,
                    wave.leadEnemy != null ? wave.leadEnemy.EnemyId : null,
                    zoneId,
                    wave.count,
                    DefenseSpawnKind.Enemy));
            }

            // 3. Thêm sự kiện Đại Boss Đấu Trường
            AddDefinition(definitions, arenaBoss);
            scheduled.Add(new DefenseScheduledSpawn(
                "arena-boss",
                bossSpawnTime,
                arenaBoss.EnemyId,
                null,
                bossZoneId,
                1,
                DefenseSpawnKind.ArenaBoss));

            return new DefenseStageSpawnPlan(
                stageId,
                scheduled,
                definitions,
                zoneCopies,
                arenaDuration,
                bossHealthOverride,
                maxActiveEnemies,
                maxPendingSpawns,
                spawnBudgetPerFrame,
                jobLifetime,
                safeHeroDistance);
        }

        private static void AddDefinition(Dictionary<string, DefenseEnemyDataSO> dict, DefenseEnemyDataSO enemy)
        {
            if (enemy == null || string.IsNullOrWhiteSpace(enemy.EnemyId)) return;
            dict[enemy.EnemyId] = enemy;
        }
    }
}
