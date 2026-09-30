using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Spawners.Core;

namespace ProjectZombie.Features.Spawners.Strategies
{
    /// <summary>
    /// Spawns a finite event quota one enemy at a time using the configured spawn interval.
    /// </summary>
    public class ContinuousSpawnStrategy : ISpawnPatternStrategy
    {
        public TimelineEventType HandledType => TimelineEventType.Continuous;

        private readonly List<TimelineEvent> _activeEvents = new List<TimelineEvent>();
        private readonly Dictionary<TimelineEvent, float> _eventTimers = new Dictionary<TimelineEvent, float>();
        private readonly Dictionary<TimelineEvent, int> _remainingSpawns = new Dictionary<TimelineEvent, int>();

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
                _remainingSpawns[evt] = Mathf.Max(1, evt.spawnCount);
                if (SpawnOne(evt, locator, tracker, playerTransform, spawnCallback))
                    _remainingSpawns[evt]--;
                if (_remainingSpawns[evt] <= 0) RemoveEventAt(_activeEvents.Count - 1);
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
            if (_activeEvents.Count == 0) return;

            for (int i = 0; i < _activeEvents.Count; i++)
            {
                var evt = _activeEvents[i];
                if (!tracker.CanSpawnMore) continue;

                _eventTimers[evt] += deltaTime;

                float interval = Mathf.Max(0.1f, evt.spawnInterval);
                if (_eventTimers[evt] >= interval)
                {
                    _eventTimers[evt] -= interval;
                    if (SpawnOne(evt, locator, tracker, playerTransform, spawnCallback))
                        _remainingSpawns[evt]--;
                    if (_remainingSpawns[evt] <= 0)
                    {
                        RemoveEventAt(i);
                        i--;
                    }
                }
            }
        }

        private static bool SpawnOne(
            TimelineEvent evt,
            ISpawnPositionLocator locator,
            IEnemyPopulationTracker tracker,
            Transform playerTransform,
            Func<GameObject, Vector3, string, GameObject> spawnCallback)
        {
            if (evt == null || !tracker.CanSpawnMore) return false;
            GameObject prefab = evt.GetPrefabOrLoad();
            Vector3 position = locator.GetSpawnPosition(playerTransform, 10f, 16f);
            spawnCallback?.Invoke(prefab, position, evt.GetPoolKey());
            return true;
        }

        private void RemoveEventAt(int index)
        {
            if (index < 0 || index >= _activeEvents.Count) return;
            TimelineEvent evt = _activeEvents[index];
            _activeEvents.RemoveAt(index);
            _eventTimers.Remove(evt);
            _remainingSpawns.Remove(evt);
        }

        public void ResetStrategy()
        {
            _activeEvents.Clear();
            _eventTimers.Clear();
            _remainingSpawns.Clear();
        }
    }
}
