using System;
using UnityEngine;
using ProjectZombie.Features.Spawners.Core;

namespace ProjectZombie.Features.Spawners.Strategies
{
    /// <summary>
    /// Chiến thuật spawn Trụ tế đàn (Pillar) phục vụ cơ chế tương tác đặc thù trong màn chơi.
    /// </summary>
    public class PillarSpawnStrategy : ISpawnPatternStrategy
    {
        public TimelineEventType HandledType => TimelineEventType.SpawnPillar;

        public void OnEventTriggered(
            TimelineEvent evt,
            ISpawnPositionLocator locator,
            IEnemyPopulationTracker tracker,
            Transform playerTransform,
            Func<GameObject, Vector3, string, GameObject> spawnCallback)
        {
            if (evt == null) return;

            GameObject prefab = evt.GetPrefabOrLoad();
            if (prefab != null)
            {
                Vector3 spawnPos = locator.GetSpawnPosition(playerTransform, 10f, 16f);
                GameObject pillarObj = UnityEngine.Object.Instantiate(prefab, spawnPos, Quaternion.identity);
                // Trụ không tính vào giới hạn số lượng quái di động của EnemyPopulationTracker
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
        }

        public void ResetStrategy()
        {
        }
    }
}
