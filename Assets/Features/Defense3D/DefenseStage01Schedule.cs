using System;
using System.Collections.Generic;

namespace ProjectZombie.Features.Defense3D
{
    public struct DefenseSpawnJob
    {
        public string EventId;
        public int SlotIndex;
        public string DeclaredType;
        public string ActualSpawnType;
        public string Zone;
        public float ScheduledTime;

        public DefenseSpawnJob(string eventId, int slotIndex, string declaredType, string actualSpawnType, string zone, float scheduledTime)
        {
            EventId = eventId;
            SlotIndex = slotIndex;
            DeclaredType = declaredType;
            ActualSpawnType = actualSpawnType;
            Zone = zone;
            ScheduledTime = scheduledTime;
        }
    }

    public struct DefenseStageEvent
    {
        public int Index;
        public string EventId;
        public float TriggerTime;
        public int Phase;
        public string DeclaredType;
        public string Zone;
        public int JobCount;

        public DefenseStageEvent(int index, string eventId, float triggerTime, int phase, string declaredType, string zone, int jobCount)
        {
            Index = index;
            EventId = eventId;
            TriggerTime = triggerTime;
            Phase = phase;
            DeclaredType = declaredType;
            Zone = zone;
            JobCount = jobCount;
        }
    }

    /// <summary>
    /// U3D-012: Authoritative schedule generator for Stage01 (34 events / 214 jobs).
    /// Implements D-05 (benque-07 @108s: 1 magiap_boss only) and
    /// D-11/A (20 magiap events -> 20 phicuong lead slots + 108 magiap).
    /// Pure C#; deterministic and frame-independent.
    /// </summary>
    public static class DefenseStage01Schedule
    {
        public const int TotalEvents = 34;
        public const float EventInterval = 18f;

        public static List<DefenseStageEvent> GenerateEvents()
        {
            var events = new List<DefenseStageEvent>(TotalEvents);
            for (int i = 0; i < TotalEvents; i++)
            {
                float at = i * EventInterval;
                int phase = Math.Min(5, (int)(at / 120f) + 1);
                string eventId = $"benque-{(i + 1):D2}";

                if (i == 6) // benque-07 @108s Miniboss
                {
                    events.Add(new DefenseStageEvent(i, eventId, at, phase, DefenseEnemyRegistry.ID_MAGIAP_BOSS, "eastBank", 1));
                    continue;
                }

                // Rotating sequence from source:
                // magiap/eastBank, huyetdoi/northCanopy, thuyquai/canalWater, magiap/eastBank, magiap/southBank
                int rot = i % 5;
                string declaredType;
                string zone;
                switch (rot)
                {
                    case 0:
                        declaredType = DefenseEnemyRegistry.ID_MAGIAP;
                        zone = "eastBank";
                        break;
                    case 1:
                        declaredType = DefenseEnemyRegistry.ID_HUYETDOI;
                        zone = "northCanopy";
                        break;
                    case 2:
                        declaredType = DefenseEnemyRegistry.ID_THUYQUAI;
                        zone = "canalWater";
                        break;
                    case 3:
                        declaredType = DefenseEnemyRegistry.ID_MAGIAP;
                        zone = "eastBank";
                        break;
                    default:
                        declaredType = DefenseEnemyRegistry.ID_MAGIAP;
                        zone = "southBank";
                        break;
                }

                int count = Math.Min(8, 3 + phase + (i % 2));
                events.Add(new DefenseStageEvent(i, eventId, at, phase, declaredType, zone, count));
            }
            return events;
        }

        public static List<DefenseSpawnJob> GenerateAllSpawnJobs()
        {
            var events = GenerateEvents();
            var jobs = new List<DefenseSpawnJob>(214);

            foreach (var evt in events)
            {
                for (int slot = 0; slot < evt.JobCount; slot++)
                {
                    string actualType = evt.DeclaredType;

                    // D-11/A rule: In magiap events, slotIndex == 0 becomes phicuong
                    if (evt.DeclaredType == DefenseEnemyRegistry.ID_MAGIAP && slot == 0)
                    {
                        actualType = DefenseEnemyRegistry.ID_PHICUONG;
                    }

                    jobs.Add(new DefenseSpawnJob(evt.EventId, slot, evt.DeclaredType, actualType, evt.Zone, evt.TriggerTime));
                }
            }
            return jobs;
        }
    }
}
