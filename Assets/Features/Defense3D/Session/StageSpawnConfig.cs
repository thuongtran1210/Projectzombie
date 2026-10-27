using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    [Serializable]
    public sealed class DefenseSpawnEventConfig
    {
        public string EventId;
        [Min(0f)] public float TriggerTime;
        public DefenseEnemyDataSO Enemy;
        public string ZoneId;
        [Min(1)] public int Count = 1;
        [Tooltip("Optional replacement for slot 0. Does not add an extra enemy.")]
        public DefenseEnemyDataSO LeadEnemy;
    }

    [Serializable]
    public sealed class DefenseSpawnPointConfig
    {
        public Vector3 Offset;
        [Range(0, 2)] public int Lane = 1;
    }

    [Serializable]
    public sealed class DefenseSpawnZoneConfig
    {
        public string ZoneId;
        public bool RelativeToEastMarker = true;
        public Vector3 Origin;
        public DefenseSpawnPointConfig[] Points = Array.Empty<DefenseSpawnPointConfig>();
    }

    /// <summary>Authoring data only. Compile validates and copies it for a run.</summary>
    [CreateAssetMenu(fileName = "StageSpawnConfig", menuName = "TREBRAVE/Defense3D/Stage Spawn Config")]
    public sealed class StageSpawnConfig : ScriptableObject
    {
        public const string DefaultResource = "Defense3D/Stages/Stage01_Production";
        [SerializeField] private string profileVersion = "stage01-v1";
        [SerializeField] private DefenseSpawnEventConfig[] events = Array.Empty<DefenseSpawnEventConfig>();
        [SerializeField] private DefenseSpawnZoneConfig[] zones = Array.Empty<DefenseSpawnZoneConfig>();
        [Header("Arena boss (independent of ordinary events)")]
        [SerializeField] private DefenseEnemyDataSO arenaBoss;
        [SerializeField] private string bossZoneId = "arenaEast";
        [SerializeField, Min(0f)] private float bossSpawnTime = 510f;
        [SerializeField, Min(0f)] private float arenaDuration = 45f;
        [SerializeField, Min(1f)] private float bossHealth = 1200f;
        [Header("Spawn queue limits")]
        [SerializeField, Min(1)] private int maxActiveEnemies = 32;
        [SerializeField, Min(1)] private int maxPendingSpawns = 40;
        [SerializeField, Min(1)] private int spawnBudgetPerFrame = 4;
        [SerializeField, Min(0.1f)] private float jobLifetime = 25f;
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
