using UnityEngine;
using ProjectZombie.Features.Spawners;

namespace ProjectZombie.Features.UI.HUD
{
    /// <summary>
    /// DTO đại diện cho dữ liệu của một sự kiện Wave / Đợt quái.
    /// Giúp tách rời (Decoupled) dữ liệu giữa SpawnManager và tầng UI.
    /// </summary>
    [System.Serializable]
    public struct WaveInfo
    {
        public string stageName;
        public string waveTitle;
        public int currentWaveIndex; // 1-based (Đợt 1, 2, 3...)
        public int totalWaves;
        public TimelineEventType eventType;
        public float timestampSeconds;
        public Sprite eventIcon;

        public WaveInfo(string stageName, string waveTitle, int currentWaveIndex, int totalWaves, TimelineEventType eventType, float timestampSeconds, Sprite eventIcon = null)
        {
            this.stageName = stageName;
            this.waveTitle = waveTitle;
            this.currentWaveIndex = currentWaveIndex;
            this.totalWaves = totalWaves;
            this.eventType = eventType;
            this.timestampSeconds = timestampSeconds;
            this.eventIcon = eventIcon;
        }
    }
}
