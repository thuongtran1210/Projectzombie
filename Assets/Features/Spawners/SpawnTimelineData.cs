using UnityEngine;
using System.Collections.Generic;

namespace ProjectZombie.Features.Spawners
{
    [System.Serializable]
    public class TimelineWaveEntry
    {
        public string waveName = "Wave 1";
        [Tooltip("Thời điểm bắt đầu wave (tính bằng giây)")]
        public float startTimeInSeconds = 0f;

        [Tooltip("Thời điểm kết thúc wave (tính bằng giây)")]
        public float endTimeInSeconds = 60f;

        [Tooltip("Danh sách quái vật xuất hiện trong wave này")]
        public List<PillarConfig> enemyConfigs = new List<PillarConfig>();

        [Tooltip("Prefab Boss xuất hiện đặc biệt tại mốc này (Nếu có)")]
        public GameObject bossPrefab;

        [Tooltip("Tốc độ sinh quái (mỗi X giây sinh 1 lượt)")]
        public float spawnInterval = 2f;
    }

    [CreateAssetMenu(fileName = "SpawnTimelineData", menuName = "ProjectZombie/Spawners/Spawn Timeline Data")]
    public class SpawnTimelineData : ScriptableObject
    {
        [Header("Timeline Scenario (00:00 -> 20:00)")]
        [Tooltip("Danh sách các Wave tiến trình thời gian")]
        public List<TimelineWaveEntry> waveTimeline = new List<TimelineWaveEntry>();

        public TimelineWaveEntry GetCurrentWave(float elapsedTime)
        {
            if (waveTimeline == null || waveTimeline.Count == 0) return null;

            for (int i = waveTimeline.Count - 1; i >= 0; i--)
            {
                if (elapsedTime >= waveTimeline[i].startTimeInSeconds)
                {
                    return waveTimeline[i];
                }
            }

            return waveTimeline[0];
        }
    }
}
