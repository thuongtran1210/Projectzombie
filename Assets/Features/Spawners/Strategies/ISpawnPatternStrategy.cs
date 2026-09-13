using System;
using UnityEngine;
using ProjectZombie.Features.Spawners.Core;

namespace ProjectZombie.Features.Spawners.Strategies
{
    /// <summary>
    /// Giao diện chuẩn cho các chiến thuật/quy luật Spawn quái (Strategy Pattern).
    /// </summary>
    public interface ISpawnPatternStrategy
    {
        TimelineEventType HandledType { get; }

        /// <summary>
        /// Kích hoạt khi timeline event bắt đầu.
        /// </summary>
        void OnEventTriggered(
            TimelineEvent evt,
            ISpawnPositionLocator locator,
            IEnemyPopulationTracker tracker,
            Transform playerTransform,
            Func<GameObject, Vector3, string, GameObject> spawnCallback);

        /// <summary>
        /// Cập nhật định kỳ mỗi frame (nếu là continuous hoặc multi-phase).
        /// </summary>
        void OnUpdate(
            float deltaTime,
            float timeMultiplier,
            ISpawnPositionLocator locator,
            IEnemyPopulationTracker tracker,
            Transform playerTransform,
            Func<GameObject, Vector3, string, GameObject> spawnCallback);

        /// <summary>
        /// Dọn dẹp trạng thái khi kết thúc trận đấu.
        /// </summary>
        void ResetStrategy();
    }
}
