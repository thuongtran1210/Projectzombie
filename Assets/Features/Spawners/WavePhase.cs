using UnityEngine;
using System.Collections.Generic;

namespace ProjectZombie.Features.Spawners
{
    [System.Serializable]
    public struct PillarConfig
    {
        [Tooltip("Prefab của Trụ sẽ được tạo ra.")]
        public GameObject pillarPrefab;
        
        [Header("Pillar Settings")]
        [Tooltip("Prefab của loại Quái mà Trụ này sẽ sinh ra.")]
        public GameObject enemyPrefab;
        
        [Tooltip("Tổng số lượng Quái mà Trụ sẽ sinh ra trước khi tự hủy.")]
        public int totalEnemiesToSpawn;
        
        [Tooltip("Khoảng thời gian chờ giữa mỗi lần nhả Quái của Trụ.")]
        public float enemySpawnInterval;
        
        [Tooltip("Người chơi có thể tấn công và phá hủy Trụ này không?")]
        public bool isAttackable;
    }

    [System.Serializable]
    public struct SpawnPillarConfig
    {
        public string configName;
        
        [Tooltip("Thời điểm bắt đầu rơi Trụ trong phase (tính bằng giây).")]
        public float startPillarTime;
        
        [Tooltip("Thời điểm kết thúc rơi Trụ trong phase (tính bằng giây).")]
        public float endPillarTime;
        
        [Tooltip("Khoảng thời gian chờ giữa mỗi lần rơi Trụ (tính bằng giây).")]
        public float pillarSpawnInterval;

        [Header("Pillar Setup")]
        public PillarConfig pillarSetup;
    }

    [CreateAssetMenu(fileName = "NewWavePhase", menuName = "ProjectZombie/Wave Phase")]
    public class WavePhase : ScriptableObject
    {
        [Header("Phase Information")]
        public string phaseName = "Phase 1";
        
        [Tooltip("Thời điểm Phase này bắt đầu tính từ lúc ván đấu bắt đầu (giây).")]
        public float startTime = 0f;

        [Tooltip("Tông màu không khí (Color Grading Palette-Swap - GDD 7.0).")]
        public Color atmosphereColor = Color.cyan;

        [Header("Continuous Background Spawns")]
        [Tooltip("Các prefab kẻ địch thường xuất hiện liên tục trong Phase này.")]
        public List<GameObject> continuousSpawnPrefabs = new List<GameObject>();

        [Header("Waves In Phase")]
        [Tooltip("Danh sách các đợt spawn quái (SpawnWaveConfig) nằm trong Phase này.")]
        public List<Enemies.SpawnWaveConfig> waveConfigs = new List<Enemies.SpawnWaveConfig>();

        [Header("Optional Pillar Settings")]
        public List<SpawnPillarConfig> pillarConfigs = new List<SpawnPillarConfig>();
    }
}
