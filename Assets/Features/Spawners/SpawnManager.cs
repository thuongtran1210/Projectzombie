using UnityEngine;
using System.Collections.Generic;

namespace ProjectZombie.Features.Spawners
{
    public class SpawnManager : MonoBehaviour
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
        public int MaxSpawnsPerFrame => maxSpawnsPerFrame;
        public float SpawnDelayBetweenFrames => spawnDelayBetweenFrames;

        private bool _isMatchActive = false;
        private Transform _playerTransform;
        private float[] _pillarSpawnTimers;

        public float MatchTime => matchTime;
        public WavePhase CurrentPhase => currentPhaseIndex >= 0 && currentPhaseIndex < phases.Count ? phases[currentPhaseIndex] : null;

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
            
            // Prewarm quái vật từ Wave phases
            if (EnemyPoolManager.Instance != null)
            {
                foreach (var phase in phases)
                {
                    if (phase.pillarConfigs != null)
                    {
                        foreach (var pillar in phase.pillarConfigs)
                        {
                            PrewarmConfig(pillar.pillarSetup);
                        }
                    }
                }
            }

            // Auto start the match
            StartMatch();
        }
        
        private void PrewarmConfig(PillarConfig config)
        {
            if (config.enemyPrefab != null)
            {
                int amount = config.totalEnemiesToSpawn > 0 ? config.totalEnemiesToSpawn : 20;
                EnemyPoolManager.Instance.PrewarmPool(config.enemyPrefab, amount);
            }
        }

        public void StartMatch()
        {
            matchTime = 0f;
            currentPhaseIndex = -1;
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
            HandlePillarSpawning();
        }

        private void CheckForPhaseChange()
        {
            int nextPhaseIndex = currentPhaseIndex + 1;
            
            if (nextPhaseIndex < phases.Count)
            {
                if (matchTime >= phases[nextPhaseIndex].startTime)
                {
                    currentPhaseIndex = nextPhaseIndex;
                    Debug.Log($"[SpawnManager] Entered Phase {currentPhaseIndex + 1}: {CurrentPhase.phaseName}");
                    
                    if (CurrentPhase.pillarConfigs != null)
                    {
                        _pillarSpawnTimers = new float[CurrentPhase.pillarConfigs.Count];
                        for (int i = 0; i < _pillarSpawnTimers.Length; i++)
                        {
                            _pillarSpawnTimers[i] = CurrentPhase.pillarConfigs[i].pillarSpawnInterval; 
                        }
                    }
                    else
                    {
                        _pillarSpawnTimers = new float[0];
                    }
                }
            }
        }

        private void HandlePillarSpawning()
        {
            if (CurrentPhase == null || CurrentPhase.pillarConfigs == null || _pillarSpawnTimers == null) return;

            float timeInPhase = matchTime - CurrentPhase.startTime;

            for (int i = 0; i < CurrentPhase.pillarConfigs.Count; i++)
            {
                var config = CurrentPhase.pillarConfigs[i];

                if (timeInPhase >= config.startPillarTime && timeInPhase <= config.endPillarTime)
                {
                    _pillarSpawnTimers[i] += Time.deltaTime;

                    if (_pillarSpawnTimers[i] >= config.pillarSpawnInterval)
                    {
                        _pillarSpawnTimers[i] = 0f;
                        SpawnPillar(config.pillarSetup);
                    }
                }
            }
        }

        public void SpawnPillar(PillarConfig config)
        {
            if (config.pillarPrefab == null) return;

            Vector3 spawnPos = GetSpawnPosition();
            GameObject pillarObj = Instantiate(config.pillarPrefab, spawnPos, Quaternion.identity);
            
            SpawnPillar pillar = pillarObj.GetComponent<SpawnPillar>();
            if (pillar != null)
            {
                pillar.Initialize(config);
            }
            else
            {
                Debug.LogWarning($"[SpawnManager] SpawnPillar component is missing on prefab {config.pillarPrefab.name}!");
            }
        }

        private Vector3 GetSpawnPosition()
        {
            Vector3 center = _playerTransform != null ? _playerTransform.position : Vector3.zero;

            for (int i = 0; i < 10; i++)
            {
                float angle = Random.Range(0f, 360f);
                float radius = Random.Range(minSpawnRadius, maxSpawnRadius);

                Vector2 randomDir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                Vector2 spawnPos = (Vector2)center + randomDir * radius;

                if (obstacleLayer.value != 0)
                {
                    Collider2D hit = Physics2D.OverlapCircle(spawnPos, 0.5f, obstacleLayer);
                    if (hit != null) continue;
                }

                return spawnPos;
            }

            float fallbackAngle = Random.Range(0f, 360f);
            Vector2 fallbackDir = new Vector2(Mathf.Cos(fallbackAngle * Mathf.Deg2Rad), Mathf.Sin(fallbackAngle * Mathf.Deg2Rad));
            return (Vector2)center + fallbackDir * minSpawnRadius;
        }
    }
}
