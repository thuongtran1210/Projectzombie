namespace ProjectZombie.Features.Defense3D
{
    public enum DefenseSpawnKind { Enemy, ArenaBoss, Landing }

    /// <summary>Stable identity of one scheduled spawn slot, retained across retries.</summary>
    public struct DefenseSpawnJob
    {
        public string EventId;
        public int SlotIndex;
        public string DeclaredType;
        public string ActualSpawnType;
        public string Zone;
        public float ScheduledTime;
        public DefenseSpawnKind Kind;

        public DefenseSpawnJob(string eventId, int slotIndex, string declaredType, string actualSpawnType,
            string zone, float scheduledTime, DefenseSpawnKind kind = DefenseSpawnKind.Enemy)
        {
            EventId = eventId;
            SlotIndex = slotIndex;
            DeclaredType = declaredType;
            ActualSpawnType = actualSpawnType;
            Zone = zone;
            ScheduledTime = scheduledTime;
            Kind = kind;
        }
    }

    /// <summary>One compiled schedule entry, independent of Unity assets and geometry.</summary>
    public readonly struct DefenseScheduledSpawn
    {
        public readonly string EventId, EnemyId, LeadEnemyId, ZoneId;
        public readonly float TriggerTime;
        public readonly int Count;
        public readonly DefenseSpawnKind Kind;

        public DefenseScheduledSpawn(string id, float time, string enemy, string lead, string zone, int count, DefenseSpawnKind kind)
        {
            EventId = id;
            TriggerTime = time;
            EnemyId = enemy;
            LeadEnemyId = lead;
            ZoneId = zone;
            Count = count;
            Kind = kind;
        }
    }
}
