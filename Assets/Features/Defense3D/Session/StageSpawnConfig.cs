using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    [Serializable]
    public sealed class DefenseSpawnEventConfig
    {
        [Tooltip("Mã định danh đợt xuất hiện quái (Ví dụ: benque-01, wave-02)")]
        public string EventId;

        [Tooltip("Thời điểm kích hoạt đợt sóng tính từ lúc bắt đầu trận (giây). Ví dụ: 18s, 108s")]
        [Min(0f)] public float TriggerTime;

        [Tooltip("Loại quái vật xuất hiện chính trong đợt sóng này")]
        public DefenseEnemyDataSO Enemy;

        [Tooltip("Mã vùng xuất hiện trên bản đồ (Ví dụ: eastBank, canalWater, northCanopy, southBank)")]
        public string ZoneId;

        [Tooltip("Số lượng quái vật sinh ra trong đợt này")]
        [Min(1)] public int Count = 1;

        [Tooltip("Quái vật thủ lĩnh đi đầu đàn (tùy chọn: thay thế vị trí con quái đầu tiên trong đàn, không làm tăng tổng số lượng quái)")]
        public DefenseEnemyDataSO LeadEnemy;
    }

    [Serializable]
    public sealed class DefenseSpawnPointConfig
    {
        [Tooltip("Độ lệch tọa độ (X, Y, Z) so với tâm vùng Origin")]
        public Vector3 Offset;

        [Tooltip("Đường hành quân / Làn đường di chuyển (0: Làn Trái / Bắc, 1: Làn Giữa, 2: Làn Phải / Nam)")]
        [Range(0, 2)] public int Lane = 1;
    }

    [Serializable]
    public sealed class DefenseSpawnZoneConfig
    {
        [Tooltip("Mã định danh vùng xuất hiện (Ví dụ: eastBank, canalWater, northCanopy, southBank, arenaEast)")]
        public string ZoneId;

        [Tooltip("Tích chọn: Tọa độ tính tương đối từ mốc Bờ Đông (EastMarker). Bỏ chọn: Tọa độ thế giới tuyệt đối.")]
        public bool RelativeToEastMarker = true;

        [Tooltip("Tọa độ tâm điểm của vùng xuất hiện")]
        public Vector3 Origin;

        [Tooltip("Danh sách các điểm xuất hiện chi tiết trong vùng này")]
        public DefenseSpawnPointConfig[] Points = Array.Empty<DefenseSpawnPointConfig>();
    }

    /// <summary>
    /// Bản cấu hình đợt sóng xuất hiện quái trong Màn chơi (Stage Spawn Configuration).
    /// Hỗ trợ Game Designer thiết lập toàn bộ Timeline quái, Vùng xuất hiện và Giới hạn hiệu năng chống giật lag.
    /// </summary>
    [CreateAssetMenu(fileName = "StageSpawnConfig", menuName = "TREBRAVE/Defense3D/Stage Spawn Config")]
    public sealed class StageSpawnConfig : ScriptableObject
    {
        public const string DefaultResource = "Defense3D/Stages/Stage01_Production";

        [Header("── 1. ĐỊNH DANH CẤU HÌNH (Profile Identity) ──")]
        [Tooltip("Mã phiên bản cấu hình màn chơi (Ví dụ: stage01-v1, stage02-v1)")]
        [SerializeField] private string profileVersion = "stage01-v1";

        [Header("── 2. DÒNG THỜI GIAN ĐỢT QUÁI (Wave Timeline Events) ──")]
        [Tooltip("Danh sách toàn bộ các đợt xuất hiện quái vật theo trình tự thời gian trận đấu")]
        [SerializeField] private DefenseSpawnEventConfig[] events = Array.Empty<DefenseSpawnEventConfig>();

        [Header("── 3. CÁC VÙNG XUẤT HIỆN TRÊN BẢN ĐỒ (Spawn Zones) ──")]
        [Tooltip("Danh sách các vùng xuất hiện (Bờ Đông, Kênh Nước, Tán Rừng...) trên bản đồ")]
        [SerializeField] private DefenseSpawnZoneConfig[] zones = Array.Empty<DefenseSpawnZoneConfig>();

        [Header("── 4. ĐẠI BOSS CUỐI TRẬN (Arena Climax Boss) ──")]
        [Tooltip("Dữ liệu của Đại Boss xuất hiện ở giai đoạn quyết chiến Đấu Trường cuối trận")]
        [SerializeField] private DefenseEnemyDataSO arenaBoss;

        [Tooltip("Mã vùng xuất hiện dành riêng cho Boss (mặc định: arenaEast)")]
        [SerializeField] private string bossZoneId = "arenaEast";

        [Tooltip("Thời điểm Đại Boss xuất hiện (giây). Ví dụ: 510s = 8 phút 30 giây")]
        [SerializeField, Min(0f)] private float bossSpawnTime = 510f;

        [Tooltip("Thời gian tối đa để tiêu diệt Boss trong Đấu Trường (giây) trước khi kết luận thất bại")]
        [SerializeField, Min(0f)] private float arenaDuration = 45f;

        [Tooltip("Lượng máu tối đa của Boss ghi đè riêng cho trận đấu này")]
        [SerializeField, Min(1f)] private float bossHealth = 1200f;

        [Header("── 5. GIỚI HẠN HIỆU NĂNG & CHỐNG GIẬT LAG (Performance Budget) ──")]
        [Tooltip("Số lượng quái vật tối đa đồng thời hoạt động trên sân (chống quá tải CPU/GPU trên di động)")]
        [SerializeField, Min(1)] private int maxActiveEnemies = 32;

        [Tooltip("Số lượng quái vật tối đa được xếp hàng chờ sinh ra trong bộ đệm")]
        [SerializeField, Min(1)] private int maxPendingSpawns = 40;

        [Tooltip("Số lượng quái vật tối đa được phép khởi tạo trong mỗi khung hình (tránh drop FPS đột ngột)")]
        [SerializeField, Min(1)] private int spawnBudgetPerFrame = 4;

        [Tooltip("Thời gian sống tối đa của yêu cầu spawn trong hàng đợi (giây). Nếu vị trí bị chặn quá lâu sẽ tự hủy để tránh tràn bộ đệm.")]
        [SerializeField, Min(0.1f)] private float jobLifetime = 25f;

        [Tooltip("Khoảng cách an toàn tối thiểu với Anh Hùng (mét). Quái sẽ không spawn nếu người chơi đứng quá gần vùng này.")]
        [SerializeField, Min(0f)] private float safeHeroDistance = 3.1f;

        public DefenseStageSpawnPlan Compile()
        {
            if (string.IsNullOrWhiteSpace(profileVersion)) throw new InvalidOperationException("Profile version is required.");
            ValidateTime(bossSpawnTime, "Boss spawn time");
            if (!Finite(arenaDuration) || arenaDuration < 0f || !Finite(bossHealth) || bossHealth <= 0f ||
                !Finite(safeHeroDistance) || safeHeroDistance < 0f)
                throw new InvalidOperationException("Arena duration, boss health and safe distance must be valid finite values.");
            var definitions = new Dictionary<string, DefenseEnemyDataSO>(StringComparer.Ordinal);
            var zoneCopies = new Dictionary<string, DefenseSpawnZoneSnapshot>(StringComparer.Ordinal);
            foreach (var zone in zones ?? Array.Empty<DefenseSpawnZoneConfig>())
            {
                if (zone == null || string.IsNullOrWhiteSpace(zone.ZoneId) || zoneCopies.ContainsKey(zone.ZoneId) || zone.Points == null || zone.Points.Length == 0)
                    throw new InvalidOperationException("Zones need unique IDs and at least one spawn point.");
                if (!Finite(zone.Origin)) throw new InvalidOperationException("Zone origin must be finite: " + zone.ZoneId);
                var points = new DefenseSpawnLocation[zone.Points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    var point = zone.Points[i];
                    if (point == null || !Finite(point.Offset) || point.Lane < 0 || point.Lane > 2)
                        throw new InvalidOperationException("Invalid spawn point in zone: " + zone.ZoneId);
                    points[i] = new DefenseSpawnLocation(point.Offset, point.Lane);
                }
                zoneCopies.Add(zone.ZoneId, new DefenseSpawnZoneSnapshot(zone.RelativeToEastMarker, zone.Origin, points));
            }
            var scheduled = new List<DefenseScheduledSpawn>();
            var eventIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in events ?? Array.Empty<DefenseSpawnEventConfig>())
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.EventId) || !eventIds.Add(entry.EventId) || entry.EventId == "arena-boss")
                    throw new InvalidOperationException("Events need unique IDs; arena-boss is reserved.");
                ValidateTime(entry.TriggerTime, entry.EventId);
                if (entry.Count < 1 || !zoneCopies.ContainsKey(entry.ZoneId ?? ""))
                    throw new InvalidOperationException("Event needs a positive count and a defined zone: " + entry.EventId);
                AddDefinition(definitions, entry.Enemy);
                if (entry.LeadEnemy != null) AddDefinition(definitions, entry.LeadEnemy);
                scheduled.Add(new DefenseScheduledSpawn(entry.EventId, entry.TriggerTime, entry.Enemy.EnemyId,
                    entry.LeadEnemy != null ? entry.LeadEnemy.EnemyId : null, entry.ZoneId, entry.Count, DefenseSpawnKind.Enemy));
            }
            AddDefinition(definitions, arenaBoss);
            if (!zoneCopies.ContainsKey(bossZoneId ?? "")) throw new InvalidOperationException("Boss zone is undefined.");
            scheduled.Add(new DefenseScheduledSpawn("arena-boss", bossSpawnTime, arenaBoss.EnemyId, null, bossZoneId, 1, DefenseSpawnKind.ArenaBoss));
            return new DefenseStageSpawnPlan(profileVersion, scheduled, definitions, zoneCopies,
                arenaDuration, bossHealth, maxActiveEnemies, maxPendingSpawns, spawnBudgetPerFrame, jobLifetime, safeHeroDistance);
        }

        private static void AddDefinition(Dictionary<string, DefenseEnemyDataSO> definitions, DefenseEnemyDataSO enemy)
        {
            if (enemy == null || string.IsNullOrWhiteSpace(enemy.EnemyId) || enemy.Prefab == null ||
                enemy.Prefab.GetComponent<DefenseEnemyBridgeWalker3D>() == null)
                throw new InvalidOperationException("Every spawn needs an Enemy Definition with a composed prefab.");
            if (!Finite(enemy.MaxHP) || enemy.MaxHP <= 0f || !Finite(enemy.MoveSpeed) || enemy.MoveSpeed <= 0f ||
                !Finite(enemy.AttackDamage) || enemy.AttackDamage < 0f || !Finite(enemy.AttackInterval) || enemy.AttackInterval <= 0f ||
                !Finite(enemy.AttackRange) || enemy.AttackRange <= 0f || enemy.WoodDrop < 0 || enemy.ExpDrop < 0)
                throw new InvalidOperationException("Invalid Enemy Definition stats: " + enemy.EnemyId);
            if (definitions.TryGetValue(enemy.EnemyId, out var existing) && existing != enemy)
                throw new InvalidOperationException("Different definitions share Enemy ID: " + enemy.EnemyId);
            definitions[enemy.EnemyId] = enemy;
        }
        private static void ValidateTime(float value, string label)
        {
            if (!Finite(value) || value < 0f || value >= DefenseGameplayClock.StandardDurationSeconds)
                throw new InvalidOperationException(label + " must be within [0, 600) gameplay seconds.");
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static bool Finite(Vector3 value) => Finite(value.x) && Finite(value.y) && Finite(value.z);
    }
}
