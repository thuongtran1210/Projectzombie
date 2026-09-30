using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Spawners.Core;

namespace ProjectZombie.Features.Spawners.Strategies
{
    /// <summary>
    /// Spawns a burst quota at its timeline marker, optionally pacing enemies by spawnInterval.
    /// </summary>
    public class BurstWaveSpawnStrategy : ISpawnPatternStrategy
    {
        public TimelineEventType HandledType => TimelineEventType.BurstWave;
        private readonly List<PendingWave> _pendingWaves = new List<PendingWave>();

        private sealed class PendingWave
        {
            public GameObject Prefab;
            public string PoolKey;
            public int Remaining;
            public float Interval;
            public float Timer;
        }

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

            if (actualSpawn <= 0) return;

            if (evt.spawnInterval <= 0f)
            {
                var immediateWave = new PendingWave { Prefab = prefab, PoolKey = poolKey, Remaining = actualSpawn };
                while (immediateWave.Remaining > 0 && tracker.CanSpawnMore)
                    SpawnOne(immediateWave, locator, tracker, playerTransform, spawnCallback);
                return;
            }

            var wave = new PendingWave
            {
                Prefab = prefab,
                PoolKey = poolKey,
                Remaining = actualSpawn,
                Interval = Mathf.Max(0f, evt.spawnInterval)
            };
            SpawnOne(wave, locator, tracker, playerTransform, spawnCallback);
            if (wave.Remaining > 0) _pendingWaves.Add(wave);
        }

        public void OnUpdate(
            float deltaTime,
            float timeMultiplier,
            ISpawnPositionLocator locator,
            IEnemyPopulationTracker tracker,
            Transform playerTransform,
            Func<GameObject, Vector3, string, GameObject> spawnCallback)
        {
            for (int i = _pendingWaves.Count - 1; i >= 0; i--)
            {
                PendingWave wave = _pendingWaves[i];
                if (wave.Remaining <= 0)
                {
                    _pendingWaves.RemoveAt(i);
                    continue;
                }
                if (!tracker.CanSpawnMore) continue;

                wave.Timer += deltaTime;
                if (wave.Timer < wave.Interval) continue;
                wave.Timer -= wave.Interval;
                SpawnOne(wave, locator, tracker, playerTransform, spawnCallback);
                if (wave.Remaining <= 0) _pendingWaves.RemoveAt(i);
            }
        }

        public void ResetStrategy()
        {
            _pendingWaves.Clear();
        }

        private static void SpawnOne(
            PendingWave wave,
            ISpawnPositionLocator locator,
            IEnemyPopulationTracker tracker,
            Transform playerTransform,
            Func<GameObject, Vector3, string, GameObject> spawnCallback)
        {
            if (wave == null || wave.Remaining <= 0 || !tracker.CanSpawnMore) return;
            Vector3 position = locator.GetSpawnPosition(playerTransform, 10f, 16f);
            spawnCallback?.Invoke(wave.Prefab, position, wave.PoolKey);
            wave.Remaining--;
        }
    }
}
