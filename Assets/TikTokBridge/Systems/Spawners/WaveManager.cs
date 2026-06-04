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
        
        [Header("Spawn Settings")]
        [SerializeField] private float minSpawnRadius = 10f;
        [SerializeField] private float maxSpawnRadius = 15f;
        [SerializeField] private LayerMask obstacleLayer;

        [Header("Performance Settings")]
        [SerializeField] private int maxSpawnsPerFrame = 5;
        [SerializeField] private float spawnDelayBetweenFrames = 0.05f;

        private float _spawnTimer = 0f;
        private bool _isMatchActive = false;
        private Transform _playerTransform;
        private Queue<GameObject> _spawnQueue = new Queue<GameObject>();
        private Coroutine _spawnCoroutine;

        public float MatchTime => matchTime;
        public WavePhase CurrentPhase => currentPhaseIndex >= 0 && currentPhaseIndex < phases.Count ? phases[currentPhaseIndex] : null;

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }

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
            if (CurrentPhase.backgroundEnemyPrefab != null)
            {
                EnqueueSpawns(CurrentPhase.backgroundEnemyPrefab, amount);
            }
        }

        private void EnqueueSpawns(GameObject prefab, int amount)
        {
            if (prefab == null) return;
            
            for (int i = 0; i < amount; i++)
            {
                _spawnQueue.Enqueue(prefab);
            }

            if (_spawnCoroutine == null)
            {
                _spawnCoroutine = StartCoroutine(ProcessSpawnQueue());
            }
        }

        private System.Collections.IEnumerator ProcessSpawnQueue()
        {
            while (_spawnQueue.Count > 0)
            {
                int spawnCount = Mathf.Min(maxSpawnsPerFrame, _spawnQueue.Count);
                for (int i = 0; i < spawnCount; i++)
                {
                    GameObject prefabToSpawn = _spawnQueue.Dequeue();
                    EnemyPoolManager.Instance.SpawnEnemy(prefabToSpawn, GetSpawnPosition(), Quaternion.identity);
                }

                if (spawnDelayBetweenFrames > 0)
                {
                    yield return new WaitForSeconds(spawnDelayBetweenFrames);
                }
                else
                {
                    yield return null;
                }
            }
            
            _spawnCoroutine = null;
        }

        private Vector3 GetSpawnPosition()
        {
            Vector3 center = _playerTransform != null ? _playerTransform.position : Vector3.zero;

            // Thử tối đa 10 lần để tìm vị trí không bị kẹt vào tường
            for (int i = 0; i < 10; i++)
            {
                float angle = Random.Range(0f, 360f);
                float radius = Random.Range(minSpawnRadius, maxSpawnRadius);

                Vector2 randomDir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                Vector2 spawnPos = (Vector2)center + randomDir * radius;

                if (obstacleLayer.value != 0)
                {
                    Collider2D hit = Physics2D.OverlapCircle(spawnPos, 0.5f, obstacleLayer);
                    if (hit != null) continue; // Bị kẹt
                }

                return spawnPos;
            }

            // Fallback
            float fallbackAngle = Random.Range(0f, 360f);
            Vector2 fallbackDir = new Vector2(Mathf.Cos(fallbackAngle * Mathf.Deg2Rad), Mathf.Sin(fallbackAngle * Mathf.Deg2Rad));
            return (Vector2)center + fallbackDir * minSpawnRadius;
        }
    }
}
