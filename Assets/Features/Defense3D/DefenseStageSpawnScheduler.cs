using System;
using System.Collections.Generic;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Gameplay-time event cursor. Knows no Unity scene, prefab or spawn queue.</summary>
    public sealed class DefenseStageSpawnScheduler
    {
        private readonly IReadOnlyList<DefenseScheduledSpawn> events;
        public int ProcessedEvents { get; private set; }
        public DefenseStageSpawnScheduler(IReadOnlyList<DefenseScheduledSpawn> events)
        { this.events = events ?? throw new ArgumentNullException(nameof(events)); }
        public void Reset() => ProcessedEvents = 0;
        public void EnqueueDue(float gameplaySeconds, Action<DefenseSpawnJob> enqueue)
        {
            if (enqueue == null) throw new ArgumentNullException(nameof(enqueue));
            if (float.IsNaN(gameplaySeconds) || float.IsInfinity(gameplaySeconds) || gameplaySeconds < 0f)
                throw new ArgumentOutOfRangeException(nameof(gameplaySeconds));
            while (ProcessedEvents < events.Count && events[ProcessedEvents].TriggerTime <= gameplaySeconds)
            {
                var evt = events[ProcessedEvents++];
                for (int slot = 0; slot < evt.Count; slot++)
                    enqueue(new DefenseSpawnJob(evt.EventId, slot, evt.EnemyId,
                        slot == 0 && !string.IsNullOrEmpty(evt.LeadEnemyId) ? evt.LeadEnemyId : evt.EnemyId,
                        evt.ZoneId, evt.TriggerTime, evt.Kind));
            }
        }
    }
}
