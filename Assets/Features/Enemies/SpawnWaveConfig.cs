using UnityEngine;
using ProjectZombie.Core.ScriptableObjects;

namespace ProjectZombie.Features.Enemies
{
    /// <summary>
    /// Cấu hình cho một đợt spawn (Wave) trong trận.
    /// Designer tạo các SO này và kéo vào EnemySpawner để thiết lập timeline của trận đấu.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSpawnWave", menuName = "ProjectZombie/Spawn Wave Config")]
    public class SpawnWaveConfig : ScriptableObject
    {
        [Header("Trigger")]
        [Tooltip("Thời điểm kích hoạt wave (giây kể từ đầu trận). VD: 30 = 0:30, 300 = 5:00.")]
        public float triggerTimeSeconds = 30f;

        [Header("Spawn Content")]
        [Tooltip("Danh sách Prefab kẻ địch có thể spawn trong đợt này.")]
        public GameObject[] enemyPrefabs;

        [Tooltip("Số lượng kẻ địch spawn trong đợt (trước khi Difficulty Scaling).")]
        public int baseSpawnCount = 10;

        [Tooltip("Khoảng thời gian (giây) giữa mỗi lần spawn trong đợt. 0 = spawn tất cả cùng lúc.")]
        public float spawnInterval = 0.3f;

        [Header("Wave Type")]
        [Tooltip("Đây có phải đợt Elite không? Dùng để thông báo UI và scale khó hơn.")]
        public bool isEliteWave = false;

        [Tooltip("Đây có phải đợt Boss không? Chỉ spawn 1 Boss duy nhất.")]
        public bool isBossWave = false;

        [Header("Optional Scaling Override")]
        [Tooltip("Nếu > 0, ghi đè hệ số HP scale tự động của Difficulty Scaling cho wave này.")]
        public float hpMultiplierOverride = 0f;
    }
}
