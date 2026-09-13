using System;
using UnityEngine;
using ProjectZombie.Features.Spawners.Core;

namespace ProjectZombie.Features.Spawners.Strategies
{
    /// <summary>
    /// Chiến thuật xuất hiện Boss: Dọn sạch quái nhỏ trong bán kính an toàn 20m,
    /// rồi triệu hồi Boss ngoài tầm nhìn camera.
    /// Sẵn sàng mở rộng thêm Boss Phase, Cutscene hoặc Boss Arena Lock trong tương lai.
    /// </summary>
    public class BossEncounterStrategy : ISpawnPatternStrategy
    {
        public TimelineEventType HandledType => TimelineEventType.BossSpawn;

        // Bộ đệm tĩnh tránh allocation rác khi quét dọn quái nhỏ quanh Boss (0 GC Alloc)
        private static readonly Collider2D[] _clearEnemiesBuffer = new Collider2D[64];

        public void OnEventTriggered(
            TimelineEvent evt,
            ISpawnPositionLocator locator,
            IEnemyPopulationTracker tracker,
            Transform playerTransform,
            Func<GameObject, Vector3, string, GameObject> spawnCallback)
        {
            if (evt == null) return;

            // 1. Dọn sạch quái thường trong bán kính 20m quanh Player để tạo võ đài đấu Boss
            ClearSmallEnemiesAround(playerTransform, 20f, tracker);

            // 2. Spawn Boss
            GameObject prefab = evt.GetPrefabOrLoad();
            string poolKey = evt.GetPoolKey();
            Vector3 spawnPos = locator.GetSpawnPosition(playerTransform, 10f, 16f);

            spawnCallback?.Invoke(prefab, spawnPos, poolKey);
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

        private void ClearSmallEnemiesAround(Transform center, float radius, IEnemyPopulationTracker tracker)
        {
            if (center == null) return;

            int hitCount = Physics2D.OverlapCircleNonAlloc(center.position, radius, _clearEnemiesBuffer);
            for (int i = 0; i < hitCount; i++)
            {
                var hit = _clearEnemiesBuffer[i];
                if (hit != null && hit.CompareTag("Enemy") && !hit.name.Contains("Boss"))
                {
                    hit.gameObject.SetActive(false);
                    tracker?.OnEnemyDied();
                }
            }
        }
    }
}
