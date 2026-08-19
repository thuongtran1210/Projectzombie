using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Spawners
{
    /// <summary>
    /// Manager tập trung duy nhất quản lý toàn bộ nhịp độ Spawn, Timeline trận đấu và kiểm soát Enemy Cap.
    /// </summary>
    public class SpawnManager : MonoBehaviour
    {
        public static SpawnManager Instance { get; private set; }

        [Header("Timeline Configuration")]
        [SerializeField] private LevelTimelineConfig timelineConfig;

        [Header("Spawn Settings & Limits")]
        [SerializeField] private int maxEnemyCap = 200; // Khống chế tối đa 200 quái trên màn hình (GDD Performance)
        [SerializeField] private float minSpawnRadius = 12f;
        [SerializeField] private float maxSpawnRadius = 18f;

        [Header("Debug Info")]
        [SerializeField] private float matchTime = 0f;
        [SerializeField] private bool isMatchActive = false;
        [SerializeField] private int currentEnemyCount = 0;

        private Transform _playerTransform;
        private Camera _mainCamera;
        private readonly List<TimelineEvent> _activeContinuousEvents = new List<TimelineEvent>();
        private readonly Dictionary<TimelineEvent, float> _eventTimers = new Dictionary<TimelineEvent, float>();
        private int _nextEventIndex = 0;

        public float MatchTime => matchTime;
        public int CurrentEnemyCount => currentEnemyCount;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            _mainCamera = Camera.main;
        }

        private WavePreloader _wavePreloader;

        private void OnEnable()
        {
            Player.PlayerProvider.OnPlayerSpawned += HandlePlayerSpawned;
            Player.PlayerProvider.OnPlayerDespawned += HandlePlayerDespawned;
        }

        private void OnDisable()
        {
            Player.PlayerProvider.OnPlayerSpawned -= HandlePlayerSpawned;
            Player.PlayerProvider.OnPlayerDespawned -= HandlePlayerDespawned;
        }

        private void HandlePlayerSpawned(Transform playerTf, Shared.HealthSystem playerHp)
        {
            _playerTransform = playerTf;
        }

        private void HandlePlayerDespawned()
        {
            _playerTransform = null;
        }

        private void Start()
        {
            _wavePreloader = GetComponent<WavePreloader>();
            if (_wavePreloader == null)
            {
                _wavePreloader = gameObject.AddComponent<WavePreloader>();
            }

            if (Player.PlayerProvider.HasPlayer)
            {
                _playerTransform = Player.PlayerProvider.PlayerTransform;
                _ = StartMatchAsync();
            }
        }

        public async System.Threading.Tasks.Task StartMatchAsync()
        {
            matchTime = 0f;
            _nextEventIndex = 0;
            _activeContinuousEvents.Clear();
            _eventTimers.Clear();

            // 1. Tự động Async Preload tất cả Prefabs trong Timeline qua WavePreloader
            if (timelineConfig != null && _wavePreloader != null)
            {
                await _wavePreloader.PreloadTimelineAssetsAsync(timelineConfig);
            }

            isMatchActive = true;
        }

        public void StartMatch()
        {
            _ = StartMatchAsync();
        }


        public void StopMatch()
        {
            isMatchActive = false;
        }

        private void Update()
        {
            if (!isMatchActive || timelineConfig == null) return;

            matchTime += Time.deltaTime;

            // 1. Kiểm tra kích hoạt Timeline Event mới
            CheckTimelineEvents();

            // 2. Chạy các Continuous Spawn Event
            HandleContinuousSpawns();
        }

        private void CheckTimelineEvents()
        {
            var events = timelineConfig.events;
            while (_nextEventIndex < events.Count && matchTime >= events[_nextEventIndex].timestampSeconds)
            {
                TimelineEvent evt = events[_nextEventIndex];
                TriggerEvent(evt);
                _nextEventIndex++;
            }
        }

        private void TriggerEvent(TimelineEvent evt)
        {
            string poolKey = evt.GetPoolKey();
            if (string.IsNullOrEmpty(poolKey) && evt.spawnPrefab == null) return;

            Debug.Log($"[SpawnManager] Kích hoạt Timeline Event: '{evt.eventName}' (Key: {poolKey}) tại phút {(matchTime / 60f):F2}");

            switch (evt.eventType)
            {
                case TimelineEventType.Continuous:
                    if (!_activeContinuousEvents.Contains(evt))
                    {
                        _activeContinuousEvents.Add(evt);
                        _eventTimers[evt] = 0f;
                    }
                    break;

                case TimelineEventType.BurstWave:
                    SpawnBurstWave(evt.spawnPrefab, evt.spawnCount);
                    break;

                case TimelineEventType.BossSpawn:
                    ClearSmallEnemiesAround(20f); // Dọn sạch quái nhỏ trong bán kính 20m khi Boss xuất hiện
                    SpawnAtPosition(evt.spawnPrefab, GetSpawnPositionOutsideCamera());
                    break;

                case TimelineEventType.SpawnPillar:
                    if (evt.spawnPrefab != null)
                    {
                        SpawnPillar(evt.spawnPrefab);
                    }
                    break;
            }
        }

        /// <summary>
        /// Phương thức hỗ trợ Spawn Trụ (Debug UI hoặc Timeline Event).
        /// </summary>
        public void SpawnPillar(PillarConfig config)
        {
            if (config.pillarPrefab == null) return;
            Vector3 spawnPos = GetSpawnPositionOutsideCamera();
            GameObject pillarObj = Instantiate(config.pillarPrefab, spawnPos, Quaternion.identity);

            SpawnPillar pillar = pillarObj.GetComponent<SpawnPillar>();
            if (pillar != null)
            {
                pillar.Initialize(config);
            }
        }

        public void SpawnPillar(GameObject pillarPrefab)
        {
            if (pillarPrefab == null) return;
            Vector3 spawnPos = GetSpawnPositionOutsideCamera();
            Instantiate(pillarPrefab, spawnPos, Quaternion.identity);
        }


        private void HandleContinuousSpawns()
        {
            if (currentEnemyCount >= maxEnemyCap) return;

            for (int i = 0; i < _activeContinuousEvents.Count; i++)
            {
                var evt = _activeContinuousEvents[i];
                _eventTimers[evt] += Time.deltaTime;

                if (_eventTimers[evt] >= evt.spawnInterval)
                {
                    _eventTimers[evt] = 0f;
                    if (currentEnemyCount < maxEnemyCap)
                    {
                        SpawnAtPosition(evt.spawnPrefab, GetSpawnPositionOutsideCamera());
                    }
                }
            }
        }

        public void SpawnBurstWave(GameObject prefab, int count)
        {
            int actualSpawn = Mathf.Min(count, maxEnemyCap - currentEnemyCount);
            for (int i = 0; i < actualSpawn; i++)
            {
                SpawnAtPosition(prefab, GetSpawnPositionOutsideCamera());
            }
        }

        private GameObject SpawnAtPosition(GameObject prefab, Vector3 position)
        {
            if (EnemyPoolManager.Instance != null)
            {
                GameObject enemy = EnemyPoolManager.Instance.SpawnEnemy(prefab, position, Quaternion.identity);
                if (enemy != null) currentEnemyCount++;
                return enemy;
            }

            GameObject spawned = Instantiate(prefab, position, Quaternion.identity);
            currentEnemyCount++;
            return spawned;
        }


        public void OnEnemyDied()
        {
            currentEnemyCount = Mathf.Max(0, currentEnemyCount - 1);
        }

        private void ClearSmallEnemiesAround(float radius)
        {
            if (_playerTransform == null) return;
            Collider2D[] hits = Physics2D.OverlapCircleAll(_playerTransform.position, radius);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy") && !hit.name.Contains("Boss"))
                {
                    hit.gameObject.SetActive(false);
                    OnEnemyDied();
                }
            }
        }

        public Vector3 GetSpawnPositionOutsideCamera()
        {
            Vector3 center = _playerTransform != null ? _playerTransform.position : Vector3.zero;
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(minSpawnRadius, maxSpawnRadius);

            Vector3 spawnPos = center + new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0f);
            return spawnPos;
        }
    }
}
