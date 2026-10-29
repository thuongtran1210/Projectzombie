using System;
using System.Collections.Generic;

namespace ProjectZombie.Features.Defense3D
{
    public enum DefenseSpawnJobStatus
    {
        Pending,
        Spawned,
        Expired,
        Overflow
    }

    public sealed class DefenseSpawnPlanner
    {
        public const int MaxActiveEnemies = 32;
        public const int MaxPendingSpawns = 40;
        public const int SpawnBudgetPerFrame = 4;
        public const float JobTTL = 25f;
        private readonly int activeLimit, pendingLimit, frameBudget;
        private readonly float jobLifetime;

        public DefenseSpawnPlanner(int maxActive = MaxActiveEnemies, int maxPending = MaxPendingSpawns,
            int budget = SpawnBudgetPerFrame, float lifetime = JobTTL)
        {
            if (maxActive < 1 || maxPending < 1 || budget < 1 || lifetime <= 0f || float.IsNaN(lifetime) || float.IsInfinity(lifetime))
                throw new ArgumentOutOfRangeException(nameof(maxActive), "Spawn limits must be positive and lifetime finite.");
            activeLimit = maxActive; pendingLimit = maxPending; frameBudget = budget; jobLifetime = lifetime;
        }

        private readonly Queue<DefenseSpawnJobEntry> pendingQueue = new Queue<DefenseSpawnJobEntry>();
        private readonly List<DefenseSpawnJobEntry> allJobs = new List<DefenseSpawnJobEntry>();

        public int CurrentActiveCount { get; private set; }
        public int PendingCount => pendingQueue.Count;
        public int ActiveLimit => activeLimit;
        public int PendingLimit => pendingLimit;
        public int ExpiredJobCount { get; private set; }
        public int OverflowJobCount { get; private set; }
        public int LastPredicateBlocked { get; private set; }
        public int LastFactoryFailed { get; private set; }
        public int SpawnedJobCount { get; private set; }
        public int TotalJobCount => allJobs.Count;
        private bool _expiring;
        private int _resetVersion;

        public event Action<DefenseSpawnJob> JobSpawned;
        public event Action<DefenseSpawnJob> JobExpired;
        public event Action<DefenseSpawnJob> JobOverflowed;

        public sealed class DefenseSpawnJobEntry
        {
            public DefenseSpawnJob Job;
            public float EnqueuedTime;
            public DefenseSpawnJobStatus Status;

            public DefenseSpawnJobEntry(DefenseSpawnJob job, float enqueuedTime)
            {
                Job = job;
                EnqueuedTime = enqueuedTime;
                Status = DefenseSpawnJobStatus.Pending;
            }
        }

        public bool EnqueueJob(DefenseSpawnJob job, float currentTime)
        {
            ExpirePending(currentTime);
            if (pendingQueue.Count >= pendingLimit)
            {
                var entry = new DefenseSpawnJobEntry(job, currentTime) { Status = DefenseSpawnJobStatus.Overflow };
                allJobs.Add(entry);
                OverflowJobCount++;
                JobOverflowed?.Invoke(job);
                return false;
            }

            var newEntry = new DefenseSpawnJobEntry(job, currentTime);
            pendingQueue.Enqueue(newEntry);
            allJobs.Add(newEntry);
            return true;
        }

        public int EnemiesDefeatedCount { get; private set; }

        public void NotifyEnemyDefeatedOrRemoved()
        {
            CurrentActiveCount = Math.Max(0, CurrentActiveCount - 1);
            EnemiesDefeatedCount++;
        }

        /// <summary>Expiry is independent of active capacity and queue rotation order.</summary>
        public int ExpirePending(float currentTime)
        {
            if (float.IsNaN(currentTime) || float.IsInfinity(currentTime) || currentTime<0f)
                throw new ArgumentOutOfRangeException(nameof(currentTime));
            if (_expiring) return 0;
            _expiring=true;
            int expired=0;
            int count=pendingQueue.Count;
            int version=_resetVersion;
            try
            {
                for (int i=0;i<count && pendingQueue.Count>0 && version==_resetVersion;i++)
                {
                    var entry=pendingQueue.Dequeue();
                    if (currentTime-entry.EnqueuedTime>jobLifetime)
                    {
                        entry.Status=DefenseSpawnJobStatus.Expired;
                        ExpiredJobCount++;
                        expired++;
                        JobExpired?.Invoke(entry.Job);
                    }
                    else pendingQueue.Enqueue(entry);
                }
            }
            finally { _expiring=false; }
            return expired;
        }

        public int ProcessSpawns(float currentTime, Func<DefenseSpawnJob, bool> canSpawnPredicate = null,
            Func<DefenseSpawnJob, bool> trySpawn = null)
        {
            ExpirePending(currentTime);
            LastPredicateBlocked=0;
            LastFactoryFailed=0;
            int spawnedThisBatch = 0;
            int attemptsThisBatch = 0;
            int countToInspect = pendingQueue.Count;

            for (int i = 0; i < countToInspect; i++)
            {
                if (pendingQueue.Count == 0) break;
                if (attemptsThisBatch >= frameBudget) break;

                var entry = pendingQueue.Peek();

                // Check active cap
                if (CurrentActiveCount >= activeLimit)
                {
                    break;
                }

                // Check predicate (e.g. safe distance from hero or valid navmesh)
                if (canSpawnPredicate != null && !canSpawnPredicate(entry.Job))
                {
                    LastPredicateBlocked++;
                    // A blocked zone must not starve jobs for other safe zones.
                    pendingQueue.Enqueue(pendingQueue.Dequeue());
                    continue;
                }

                // Execute before committing the slot. A failed factory keeps its
                // original job queued; TTL and identity remain unchanged.
                attemptsThisBatch++;
                if (trySpawn != null && !trySpawn(entry.Job))
                {
                    LastFactoryFailed++;
                    pendingQueue.Enqueue(pendingQueue.Dequeue());
                    continue;
                }

                // Successful spawn
                pendingQueue.Dequeue();
                entry.Status = DefenseSpawnJobStatus.Spawned;
                CurrentActiveCount++;
                SpawnedJobCount++;
                spawnedThisBatch++;
                JobSpawned?.Invoke(entry.Job);
            }

            return spawnedThisBatch;
        }

        public void Reset()
        {
            pendingQueue.Clear();
            allJobs.Clear();
            CurrentActiveCount = 0;
            _resetVersion++;
            SpawnedJobCount=ExpiredJobCount=OverflowJobCount=LastPredicateBlocked=LastFactoryFailed=0;
            EnemiesDefeatedCount = 0;
        }
    }
}
