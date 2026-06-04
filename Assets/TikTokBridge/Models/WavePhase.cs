using UnityEngine;
using System.Collections.Generic;

namespace TikTokBridge.Models
{
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

    [CreateAssetMenu(fileName = "NewWavePhase", menuName = "ProjectZombie/Wave Phase")]
    public class WavePhase : ScriptableObject
    {
        [Header("Phase Information")]
        public string phaseName = "Phase 1";
        
        [Tooltip("Thời điểm Phase này bắt đầu tính từ lúc ván đấu bắt đầu (giây).")]
        public float startTime = 0f;

        [Header("Spawn Pillar Settings")]
        public List<SpawnPillarConfig> pillarConfigs = new List<SpawnPillarConfig>();
    }
}
