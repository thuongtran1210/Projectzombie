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

        private readonly Queue<DefenseSpawnJobEntry> pendingQueue = new Queue<DefenseSpawnJobEntry>();
        private readonly List<DefenseSpawnJobEntry> allJobs = new List<DefenseSpawnJobEntry>();

        public int CurrentActiveCount { get; private set; }
        public int PendingCount => pendingQueue.Count;

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
            if (pendingQueue.Count >= MaxPendingSpawns)
            {
                var entry = new DefenseSpawnJobEntry(job, currentTime) { Status = DefenseSpawnJobStatus.Overflow };
                allJobs.Add(entry);
                JobOverflowed?.Invoke(job);
                return false;
            }

            var newEntry = new DefenseSpawnJobEntry(job, currentTime);
            pendingQueue.Enqueue(newEntry);
            allJobs.Add(newEntry);
            return true;
        }

        public void NotifyEnemyDefeatedOrRemoved()
        {
            CurrentActiveCount = Math.Max(0, CurrentActiveCount - 1);
        }

        public int ProcessSpawns(float currentTime, Func<DefenseSpawnJob, bool> canSpawnPredicate = null)
        {
            int spawnedThisBatch = 0;
            int countToInspect = pendingQueue.Count;

            for (int i = 0; i < countToInspect; i++)
            {
                if (pendingQueue.Count == 0) break;
                if (spawnedThisBatch >= SpawnBudgetPerFrame) break;

                var entry = pendingQueue.Peek();

                // Check TTL expiry
                if (currentTime - entry.EnqueuedTime > JobTTL)
                {
                    pendingQueue.Dequeue();
                    entry.Status = DefenseSpawnJobStatus.Expired;
                    JobExpired?.Invoke(entry.Job);
                    continue;
                }

                // Check active cap
                if (CurrentActiveCount >= MaxActiveEnemies)
                {
                    break;
                }

                // Check predicate (e.g. safe distance from hero or valid navmesh)
                if (canSpawnPredicate != null && !canSpawnPredicate(entry.Job))
                {
                    // Cannot spawn safely right now, leave in queue
                    break;
                }

                // Successful spawn
                pendingQueue.Dequeue();
                entry.Status = DefenseSpawnJobStatus.Spawned;
                CurrentActiveCount++;
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
        }
    }
}
