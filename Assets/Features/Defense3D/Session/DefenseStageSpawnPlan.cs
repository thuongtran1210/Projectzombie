using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    public readonly struct DefenseSpawnLocation
    {
        public readonly Vector3 Position;
        public readonly int Lane;
        public DefenseSpawnLocation(Vector3 position, int lane) { Position = position; Lane = lane; }
    }

    public sealed class DefenseSpawnZoneSnapshot
    {
        private readonly bool relativeToEast;
        private readonly Vector3 origin;
        private readonly DefenseSpawnLocation[] points;
        public DefenseSpawnZoneSnapshot(bool relative, Vector3 origin, DefenseSpawnLocation[] points)
        { relativeToEast = relative; this.origin = origin; this.points = (DefenseSpawnLocation[])points.Clone(); }
        public DefenseSpawnLocation Resolve(Vector3 eastMarker, int slot)
        {
            var point = points[slot % points.Length];
            return new DefenseSpawnLocation((relativeToEast ? eastMarker : Vector3.zero) + origin + point.Position, point.Lane);
        }
    }

    /// <summary>Validated per-run snapshot. No mutable authoring event/zone objects are retained.</summary>
    public sealed class DefenseStageSpawnPlan
    {
        public string ProfileVersion { get; }
        public IReadOnlyList<DefenseScheduledSpawn> Events { get; }
        private readonly Dictionary<string, DefenseEnemyDataSO> definitions;
        private readonly Dictionary<string, DefenseEnemySpawnDefinition> _snapshots;
        private readonly Dictionary<string, DefenseSpawnZoneSnapshot> zones;
        public float ArenaDuration { get; }
        public float BossHealth { get; }
        public float SafeHeroDistance { get; }
        private readonly int maxActive, maxPending, budget;
        private readonly float lifetime;

        public DefenseStageSpawnPlan(string version, List<DefenseScheduledSpawn> events,
            Dictionary<string, DefenseEnemyDataSO> definitions, Dictionary<string, DefenseSpawnZoneSnapshot> zones,
            float arenaDuration, float bossHealth, int maxActive, int maxPending, int budget, float lifetime, float safeDistance)
        {
            // Validate queue settings during compilation, not on the first gameplay frame.
            new DefenseSpawnPlanner(maxActive, maxPending, budget, lifetime);
            ProfileVersion = version;
            // Stable order for equal-time events, independent of frame boundaries.
            var ordered = new List<DefenseScheduledSpawn>(events);
            ordered.Sort((a, b) => {
                int result = a.TriggerTime.CompareTo(b.TriggerTime);
                return result != 0 ? result : StringComparer.Ordinal.Compare(a.EventId, b.EventId);
            });
            Events = ordered.AsReadOnly();
            this.definitions = new Dictionary<string, DefenseEnemyDataSO>(definitions);
            _snapshots = new Dictionary<string, DefenseEnemySpawnDefinition>(StringComparer.Ordinal);
            foreach (var pair in definitions)
            {
                var snapshot = new DefenseEnemySpawnDefinition(pair.Value);
                if (!StringComparer.Ordinal.Equals(pair.Key, snapshot.EnemyId))
                    throw new ArgumentException("Enemy catalog key does not match definition ID: " + pair.Key);
                _snapshots.Add(pair.Key, snapshot);
            }
            this.zones = new Dictionary<string, DefenseSpawnZoneSnapshot>(zones);
            ArenaDuration = arenaDuration; BossHealth = bossHealth; SafeHeroDistance = safeDistance;
            this.maxActive = maxActive; this.maxPending = maxPending; this.budget = budget; this.lifetime = lifetime;
        }
        [Obsolete("Authoring data is mutable. Use GetEnemySnapshot for runtime spawns.")]
        public DefenseEnemyDataSO GetEnemy(string id) => definitions[id];
        public DefenseEnemySpawnDefinition GetEnemySnapshot(string id) => _snapshots[id];
        public DefenseSpawnLocation Resolve(DefenseSpawnJob job, Vector3 eastMarker) => zones[job.Zone].Resolve(eastMarker, job.SlotIndex);
        public DefenseSpawnPlanner CreatePlanner() => new DefenseSpawnPlanner(maxActive, maxPending, budget, lifetime);
    }
}
