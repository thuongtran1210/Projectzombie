using UnityEngine;

namespace TikTokBridge.Models
{
    [CreateAssetMenu(fileName = "NewWavePhase", menuName = "ProjectZombie/Wave Phase")]
    public class WavePhase : ScriptableObject
    {
        [Header("Phase Information")]
        public string phaseName = "Phase 1";
        
        [Tooltip("Time in seconds from the start of the match when this phase begins.")]
        public float startTime = 0f;

        [Header("Spawn Settings")]
        [Tooltip("Time interval in seconds between background enemy spawns.")]
        public float baseSpawnInterval = 5f;

        [Tooltip("The enemy prefab to spawn during this phase.")]
        public GameObject backgroundEnemyPrefab;

        [Tooltip("Number of enemies to spawn each time the interval is reached.")]
        public int amountPerSpawn = 1;
    }
}
