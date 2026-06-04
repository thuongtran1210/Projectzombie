using UnityEngine;
using System.Collections.Generic;
using TikTokBridge.Models;
using TikTokBridge.Logic;
using TikTokBridge.Core;

namespace TikTokBridge.Systems.Spawners
{
    public class SpawnManager : MonoBehaviour
    {
        [Header("Wave Configuration")]
        [Tooltip("List of phases, MUST be sorted by startTime in ascending order.")]
        [SerializeField] private List<WavePhase> phases = new List<WavePhase>();

        [Header("TikTok Like/Follow Events")]
        [SerializeField] private PillarConfig likePillarConfig;
        [SerializeField] private PillarConfig followPillarConfig;
        [SerializeField] private PillarConfig defaultGiftFallbackConfig;

        [Header("TikTok Gift Mappings")]
        [SerializeField] private List<GiftPillarMapping> giftMappings = new List<GiftPillarMapping>();

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

        private bool _isMatchActive = false;
        private Transform _playerTransform;
        private float[] _pillarSpawnTimers;
        
        private ICommandDispatcher _dispatcher;
        private Dictionary<string, PillarConfig> _giftToPillarMap = new Dictionary<string, PillarConfig>();

        public float MatchTime => matchTime;
        public WavePhase CurrentPhase => currentPhaseIndex >= 0 && currentPhaseIndex < phases.Count ? phases[currentPhaseIndex] : null;

        private void Awake()
        {
            foreach (var mapping in giftMappings)
            {
                if (!string.IsNullOrEmpty(mapping.giftName))
                {
                    _giftToPillarMap[mapping.giftName] = mapping.pillarSetup;
                }
            }
        }

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
            
            // Prewarm quái vật từ TikTok Events để không bị giật
            if (EnemyPoolManager.Instance != null)
            {
                PrewarmConfig(likePillarConfig);
                PrewarmConfig(followPillarConfig);
                PrewarmConfig(defaultGiftFallbackConfig);

                foreach (var mapping in giftMappings)
                {
                    PrewarmConfig(mapping.pillarSetup);
                }
                
                // Prewarm từ Wave phases
                foreach (var phase in phases)
                {
                    foreach (var pillar in phase.pillarConfigs)
                    {
                        PrewarmConfig(pillar.pillarSetup);
                    }
                }
            }

            // Auto start the match for now
            StartMatch();
        }
        
        private void PrewarmConfig(PillarConfig config)
        {
            if (config.enemyPrefab != null)
            {
                // Prewarm amount based on totalEnemiesToSpawn or a default value
                int amount = config.totalEnemiesToSpawn > 0 ? config.totalEnemiesToSpawn : 20;
                EnemyPoolManager.Instance.PrewarmPool(config.enemyPrefab, amount);
            }
        }

        public void Construct(ICommandDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _dispatcher.OnLikeReceived += HandleLike;
            _dispatcher.OnFollowReceived += HandleFollow;
            _dispatcher.OnSpawnEnemy += HandleSpawnEnemy;
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
        
        // --- TikTok Events Handling ---
        private void HandleLike(GameCommandPayload cmd)
        {
            Debug.Log($"[SpawnManager] {cmd.user} liked! Spawning Like Pillar.");
            SpawnPillar(likePillarConfig);
        }

        private void HandleFollow(GameCommandPayload cmd)
        {
            Debug.Log($"[SpawnManager] {cmd.user} followed! Spawning Follow Pillar.");
            SpawnPillar(followPillarConfig);
        }

        private void HandleSpawnEnemy(GameCommandPayload cmd)
        {
            string enemyType = cmd.enemy; 
            if (string.IsNullOrEmpty(enemyType) && cmd.additionalData != null)
            {
                enemyType = cmd.additionalData["enemy"]?.ToString();
            }

            int amount = cmd.amount > 0 ? cmd.amount : 1;
            PillarConfig configToSpawn;

            if (string.IsNullOrEmpty(enemyType))
            {
                configToSpawn = defaultGiftFallbackConfig;
            }
            else if (!_giftToPillarMap.TryGetValue(enemyType, out configToSpawn))
            {
                configToSpawn = defaultGiftFallbackConfig;
                Debug.Log($"[SpawnManager] Unknown gift '{enemyType}' from {cmd.user}. Using Fallback Pillar.");
            }

            // Multiply the total enemies to spawn by the gift amount
            // Make sure we at least spawn 'amount' if totalEnemiesToSpawn is 0 or something
            int baseAmount = configToSpawn.totalEnemiesToSpawn > 0 ? configToSpawn.totalEnemiesToSpawn : 1;
            configToSpawn.totalEnemiesToSpawn = baseAmount * amount;
            
            Debug.Log($"[SpawnManager] {cmd.user} sent Gift! Spawning Pillar (Total Enemies: {configToSpawn.totalEnemiesToSpawn})!");
            SpawnPillar(configToSpawn);
        }
        // ------------------------------

        private void SpawnPillar(PillarConfig config)
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
                    if (hit != null) continue; // Bị kẹt
                }

                return spawnPos;
            }

            // Fallback
            float fallbackAngle = Random.Range(0f, 360f);
            Vector2 fallbackDir = new Vector2(Mathf.Cos(fallbackAngle * Mathf.Deg2Rad), Mathf.Sin(fallbackAngle * Mathf.Deg2Rad));
            return (Vector2)center + fallbackDir * minSpawnRadius;
        }
        
        private void OnDestroy()
        {
            if (_dispatcher != null)
            {
                _dispatcher.OnLikeReceived -= HandleLike;
                _dispatcher.OnFollowReceived -= HandleFollow;
                _dispatcher.OnSpawnEnemy -= HandleSpawnEnemy;
            }
        }
    }
}
