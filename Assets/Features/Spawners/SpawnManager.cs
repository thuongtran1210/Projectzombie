using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
        [SerializeField] private int maxEnemyCap = 50; // Khống chế 30-50 quái cho không gian Combo & Dash (GDD v5.0 Action RPG)
        [SerializeField] private float minSpawnRadius = 10f;
        [SerializeField] private float maxSpawnRadius = 16f;

        [Header("Map Boundary & Walkable Area")]
        [Tooltip("Collider bao quanh khu vực có thể di chuyển trên sàn đấu (Tùy chọn)")]
        [SerializeField] private Collider2D walkableAreaCollider;
        [Tooltip("Tilemap sàn gạch/mặt đất để xác thực ô đi được")]
        [SerializeField] private Tilemap groundTilemap;
        [Tooltip("Tự động tìm Tilemap_Ground trong Scene nếu chưa gán thủ công")]
        [SerializeField] private bool autoFindGroundTilemap = true;
        [Tooltip("Khoảng cách đệm ngoài rìa Camera")]
        [SerializeField] private float cameraPadding = 1.5f;

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

            // Tự động tìm kiếm Tilemap_Ground nếu chưa được gán thủ công
            if (autoFindGroundTilemap && groundTilemap == null && walkableAreaCollider == null)
            {
                var groundObj = GameObject.Find("Tilemap_Ground");
                if (groundObj != null)
                {
                    groundTilemap = groundObj.GetComponent<Tilemap>();
                }
                else
                {
                    groundTilemap = FindObjectOfType<Tilemap>();
                }
            }

            // Trì hoãn việc khởi động Match sang Frame tiếp theo để tránh dồn gánh nặng vào Frame 1
            StartCoroutine(RoutineStartMatchDelayed());
        }

        private IEnumerator RoutineStartMatchDelayed()
        {
            yield return null; // Nhường 1 frame cho Scene và UI khởi tạo xong

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

        /// <summary>
        /// Dừng trận đấu và dọn sạch toàn bộ quái vật, boss, minion còn sót lại trên bản đồ.
        /// </summary>
        public void StopMatchAndClearAllEnemies()
        {
            isMatchActive = false;
            matchTime = 0f;
            _nextEventIndex = 0;
            _activeContinuousEvents.Clear();
            _eventTimers.Clear();

            var allEnemies = FindObjectsOfType<Enemies.Enemy>();
            for (int i = 0; i < allEnemies.Length; i++)
            {
                if (allEnemies[i] != null && allEnemies[i].gameObject != null)
                {
                    Destroy(allEnemies[i].gameObject);
                }
            }

            // Dọn sạch tiền và ngọc EXP còn sót lại trên sàn
            var allCoins = FindObjectsOfType<Collectibles.CoinDrop>();
            for (int i = 0; i < allCoins.Length; i++)
            {
                if (allCoins[i] != null && allCoins[i].gameObject != null) Destroy(allCoins[i].gameObject);
            }

            var allGems = FindObjectsOfType<Collectibles.ExpGem>();
            for (int i = 0; i < allGems.Length; i++)
            {
                if (allGems[i] != null && allGems[i].gameObject != null) Destroy(allGems[i].gameObject);
            }

            Collectibles.CoinPoolManager.Instance?.ClearPools();
            Collectibles.ExpGemPoolManager.Instance?.ClearPools();
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
            int obstacleMask = LayerMask.GetMask("Obstacle", "Water");
            if (obstacleMask == 0) obstacleMask = LayerMask.GetMask("Obstacle");

            if (_mainCamera == null) _mainCamera = Camera.main;

            float effectiveMin = minSpawnRadius;
            float effectiveMax = maxSpawnRadius;

            // Tự động thích ứng bán kính theo kích thước Camera nếu có
            if (_mainCamera != null && _mainCamera.orthographic)
            {
                float camHalfH = _mainCamera.orthographicSize + cameraPadding;
                float camHalfW = _mainCamera.orthographicSize * _mainCamera.aspect + cameraPadding;
                float camDiagonal = Mathf.Sqrt(camHalfW * camHalfW + camHalfH * camHalfH);

                // Đảm bảo bán kính tối thiểu vượt ra ngoài góc chéo Camera
                effectiveMin = Mathf.Min(minSpawnRadius, camDiagonal);
                effectiveMax = Mathf.Max(effectiveMin + 2f, maxSpawnRadius);
            }

            // Giai đoạn 1: Thử lấy mẫu vị trí hợp lệ (Vừa ngoài Camera vừa trong Sàn đấu)
            for (int i = 0; i < 16; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = Random.Range(effectiveMin, effectiveMax);
                Vector3 candidatePos = center + new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0f);

                // 1. Phải nằm trong sàn đấu Walkable Area
                if (!IsInsideWalkableArea(candidatePos))
                    continue;

                // 2. Không được dính Obstacle / Tường
                Collider2D hit = Physics2D.OverlapCircle(candidatePos, 0.6f, obstacleMask);
                if (hit != null)
                    continue;

                // 3. Phải nằm ngoài tầm nhìn Camera
                if (!IsOutsideCameraViewport(candidatePos))
                    continue;

                return candidatePos;
            }

            // Giai đoạn 2: Smart Fallback (Nếu Player đứng sát góc tường hoặc Map nhỏ)
            for (int i = 0; i < 8; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = Random.Range(effectiveMin * 0.7f, effectiveMin);
                Vector3 candidatePos = center + new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0f);

                if (IsInsideWalkableArea(candidatePos))
                {
                    Collider2D hit = Physics2D.OverlapCircle(candidatePos, 0.6f, obstacleMask);
                    if (hit == null)
                    {
                        return candidatePos;
                    }
                }
            }

            // Fallback cuối cùng: Lấy vị trí gần Player nhất còn thuộc sàn đấu
            if (walkableAreaCollider != null)
            {
                return walkableAreaCollider.ClosestPoint(center + (Vector3)(Random.insideUnitCircle * effectiveMin));
            }

            float fallbackAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            return center + new Vector3(Mathf.Cos(fallbackAngle) * minSpawnRadius, Mathf.Sin(fallbackAngle) * minSpawnRadius, 0f);
        }

        private bool IsInsideWalkableArea(Vector3 position)
        {
            if (walkableAreaCollider != null)
            {
                return walkableAreaCollider.OverlapPoint(position);
            }

            if (groundTilemap != null)
            {
                Vector3Int cellPos = groundTilemap.WorldToCell(position);
                return groundTilemap.HasTile(cellPos);
            }

            // Nếu không có bất kỳ cấu hình ranh giới nào, mặc định cho phép
            return true;
        }

        private bool IsOutsideCameraViewport(Vector3 position)
        {
            if (_mainCamera == null) return true;
            Vector3 vp = _mainCamera.WorldToViewportPoint(position);
            return vp.x < -0.05f || vp.x > 1.05f || vp.y < -0.05f || vp.y > 1.05f;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 center = _playerTransform != null ? _playerTransform.position : transform.position;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, minSpawnRadius);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(center, maxSpawnRadius);

            if (walkableAreaCollider != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(walkableAreaCollider.bounds.center, walkableAreaCollider.bounds.size);
            }
        }
    }
}
