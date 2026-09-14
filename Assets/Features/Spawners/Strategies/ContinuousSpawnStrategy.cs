using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Spawners.Core;

namespace ProjectZombie.Features.Spawners.Strategies
{
    /// <summary>
    /// Chiến thuật spawn duy trì liên tục với chu kỳ spawnInterval và tích hợp Adaptive Catchup.
    /// </summary>
    public class ContinuousSpawnStrategy : ISpawnPatternStrategy
    {
        public TimelineEventType HandledType => TimelineEventType.Continuous;

        private readonly List<TimelineEvent> _activeEvents = new List<TimelineEvent>();
        private readonly Dictionary<TimelineEvent, float> _eventTimers = new Dictionary<TimelineEvent, float>();

        public void OnEventTriggered(
            TimelineEvent evt,
            ISpawnPositionLocator locator,
            IEnemyPopulationTracker tracker,
            Transform playerTransform,
            Func<GameObject, Vector3, string, GameObject> spawnCallback)
        {
            if (!_activeEvents.Contains(evt))
            {
                _activeEvents.Add(evt);
                _eventTimers[evt] = 0f;

                // Spawn tức thì đợt quái ban đầu
                GameObject prefab = evt.GetPrefabOrLoad();
                string poolKey = evt.GetPoolKey();
                int initialCount = Mathf.Max(1, evt.spawnCount);

                for (int s = 0; s < initialCount; s++)
                {
                    if (!tracker.CanSpawnMore) break;
                    Vector3 pos = locator.GetSpawnPosition(playerTransform, 10f, 16f);
                    spawnCallback?.Invoke(prefab, pos, poolKey);
                }
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
            if (!tracker.CanSpawnMore || _activeEvents.Count == 0) return;

            for (int i = 0; i < _activeEvents.Count; i++)
            {
                var evt = _activeEvents[i];
                if (!_eventTimers.ContainsKey(evt)) _eventTimers[evt] = 0f;

                _eventTimers[evt] += deltaTime * timeMultiplier;

                if (_eventTimers[evt] >= evt.spawnInterval)
                {
                    _eventTimers[evt] = 0f;

                    if (tracker.CanSpawnMore)
                    {
                        int maxPossible = tracker.MaxEnemyCap - tracker.CurrentEnemyCount;
                        int desiredCount = evt.spawnCount > 0 ? evt.spawnCount : 1;
                        int spawnBatch = Mathf.Clamp(desiredCount, 1, maxPossible);

                        GameObject prefab = evt.GetPrefabOrLoad();
                        string poolKey = evt.GetPoolKey();

                        for (int b = 0; b < spawnBatch; b++)
                        {
                            Vector3 pos = locator.GetSpawnPosition(playerTransform, 10f, 16f);
                            spawnCallback?.Invoke(prefab, pos, poolKey);
                        }
                    }
                }
            }
        }

        public void ResetStrategy()
        {
            _activeEvents.Clear();
            _eventTimers.Clear();
        }
    }
}
