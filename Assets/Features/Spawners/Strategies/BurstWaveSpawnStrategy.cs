using System;
using UnityEngine;
using ProjectZombie.Features.Spawners.Core;

namespace ProjectZombie.Features.Spawners.Strategies
{
    /// <summary>
    /// Chiến thuật spawn bộc phát (Burst Wave) tại một mốc thời gian xác định.
    /// </summary>
    public class BurstWaveSpawnStrategy : ISpawnPatternStrategy
    {
        public TimelineEventType HandledType => TimelineEventType.BurstWave;

        public void OnEventTriggered(
            TimelineEvent evt,
            ISpawnPositionLocator locator,
            IEnemyPopulationTracker tracker,
            Transform playerTransform,
            Func<GameObject, Vector3, string, GameObject> spawnCallback)
        {
            if (evt == null) return;

            GameObject prefab = evt.GetPrefabOrLoad();
            string poolKey = evt.GetPoolKey();
            int actualSpawn = Mathf.Min(evt.spawnCount, tracker.MaxEnemyCap - tracker.CurrentEnemyCount);

            for (int i = 0; i < actualSpawn; i++)
            {
                if (!tracker.CanSpawnMore) break;
                Vector3 pos = locator.GetSpawnPosition(playerTransform, 10f, 16f);
                spawnCallback?.Invoke(prefab, pos, poolKey);
            }
        }

        public void OnUpdate(
            float deltaTime,
            float timeMultiplier,
            ISpawnPositionLocator locator,
            IEnemyPopulationTracker tracker,
            Transform playerTransform,
            Func<GameObject, Vector3, string, GameObject> spawnCallback)
        {
            // Burst wave là sự kiện one-shot, không cần chạy định kỳ mỗi frame
        }

        public void ResetStrategy()
        {
        }
    }
}
