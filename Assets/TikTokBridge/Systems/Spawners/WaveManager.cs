using UnityEngine;
using System.Collections.Generic;
using TikTokBridge.Models;

namespace TikTokBridge.Systems.Spawners
{
    public class WaveManager : MonoBehaviour
    {
        [Header("Wave Configuration")]
        [Tooltip("List of phases, MUST be sorted by startTime in ascending order.")]
        [SerializeField] private List<WavePhase> phases = new List<WavePhase>();

        [Header("Debug Info")]
        [SerializeField] private float matchTime = 0f;
        [SerializeField] private int currentPhaseIndex = -1;
        
        private float _spawnTimer = 0f;
        private bool _isMatchActive = false;

        public float MatchTime => matchTime;
        public WavePhase CurrentPhase => currentPhaseIndex >= 0 && currentPhaseIndex < phases.Count ? phases[currentPhaseIndex] : null;

        private void Start()
        {
            // Auto start the match for now
            StartMatch();
        }

        public void StartMatch()
        {
            matchTime = 0f;
            currentPhaseIndex = -1;
            _spawnTimer = 0f;
            _isMatchActive = true;
            
            CheckForPhaseChange();
        }

        public void StopMatch()
        {
            _isMatchActive = false;
        }

        private void Update()
        {
            if (!_isMatchActive || phases.Count == 0) return;

            matchTime += Time.deltaTime;
            
            CheckForPhaseChange();
            HandleSpawning();
        }

        private void CheckForPhaseChange()
        {
            // If there's a next phase and we've reached its start time
            int nextPhaseIndex = currentPhaseIndex + 1;
            
            if (nextPhaseIndex < phases.Count)
            {
                if (matchTime >= phases[nextPhaseIndex].startTime)
                {
                    currentPhaseIndex = nextPhaseIndex;
                    Debug.Log($"[WaveManager] Entered Phase {currentPhaseIndex + 1}: {CurrentPhase.phaseName}");
                    
                    // Khởi tạo sẵn (Prewarm) quái vật của Phase này để tránh giật lag khi Spawn
                    if (CurrentPhase.backgroundEnemyPrefab != null && EnemyPoolManager.Instance != null)
                    {
                        EnemyPoolManager.Instance.PrewarmPool(CurrentPhase.backgroundEnemyPrefab, 50);
                    }
                    
                    // Reset spawn timer when entering a new phase to spawn immediately
                    _spawnTimer = CurrentPhase.baseSpawnInterval; 
                }
            }
        }

        private void HandleSpawning()
        {
            if (CurrentPhase == null || CurrentPhase.backgroundEnemyPrefab == null) return;

            _spawnTimer += Time.deltaTime;

            if (_spawnTimer >= CurrentPhase.baseSpawnInterval)
            {
                _spawnTimer = 0f;
                SpawnBackgroundEnemies();
            }
        }

        private void SpawnBackgroundEnemies()
        {
            if (EnemyPoolManager.Instance == null)
            {
                Debug.LogWarning("[WaveManager] EnemyPoolManager.Instance is null! Cannot spawn enemies.");
                return;
            }

            int amount = CurrentPhase.amountPerSpawn;
            for (int i = 0; i < amount; i++)
            {
                EnemyPoolManager.Instance.SpawnEnemy(
                    CurrentPhase.backgroundEnemyPrefab, 
                    GetSpawnPosition(), 
                    Quaternion.identity
                );
            }
        }

        private Vector3 GetSpawnPosition()
        {
            // Simple random position around the center, could be expanded to use spawn points
            return new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
        }
    }
}
