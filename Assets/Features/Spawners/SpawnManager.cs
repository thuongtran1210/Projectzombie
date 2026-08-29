using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using ProjectZombie.Features.Shared;

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

        [Header("Auto Start (For Quick Play / Testing)")]
        [Tooltip("Tự động bắt đầu trận đấu khi vào Scene nếu không có UI Meta điều phối")]
        [SerializeField] private bool autoStartOnPlay = true;

        private void Start()
        {
            if (autoStartOnPlay && !isMatchActive)
            {
                StartMatch();
            }
        }

        private Tilemap _obstacleTilemap;
        private Bounds _safeMapBounds;
        private bool _hasCalculatedBounds = false;

        private void EnsureDependencies()
        {
            if (_wavePreloader == null)
            {
                _wavePreloader = GetComponent<WavePreloader>() ?? gameObject.AddComponent<WavePreloader>();
            }

            if (timelineConfig == null)
            {
                timelineConfig = Resources.Load<LevelTimelineConfig>("Levels/Level1_Timeline") ??
                                 Resources.Load<LevelTimelineConfig>("Level1_Timeline");
#if UNITY_EDITOR
                if (timelineConfig == null)
                {
                    timelineConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelTimelineConfig>("Assets/_Data/Levels/Level1_Timeline.asset");
                }
#endif
            }

            if (_playerTransform == null)
            {
                if (Player.PlayerProvider.HasPlayer)
                {
                    _playerTransform = Player.PlayerProvider.PlayerTransform;
                }
                else
                {
                    var playerObj = GameObject.FindGameObjectWithTag("Player") ?? GameObject.Find("Player");
                    if (playerObj != null) _playerTransform = playerObj.transform;
                }
            }

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

            // Tự động tìm Tilemap_Obstacles nội bộ để tránh spawn quái trên nóc tường
            if (_obstacleTilemap == null)
            {
                var obsObj = GameObject.Find("Tilemap_Obstacles");
                if (obsObj != null) _obstacleTilemap = obsObj.GetComponent<Tilemap>();
            }

            CalculateSafeMapBounds();
        }

        /// <summary>
        /// Tự động tính toán hình chữ nhật sàn đấu an toàn (Safe Map Bounds) để Clamping O(1)
        /// </summary>
        private void CalculateSafeMapBounds()
        {
            if (_hasCalculatedBounds) return;

            float margin = 1.0f; // Thụt lề an toàn cách mép vực/tường 1 mét

            if (walkableAreaCollider != null)
            {
                _safeMapBounds = walkableAreaCollider.bounds;
                _safeMapBounds.Expand(-margin * 2f);
                _hasCalculatedBounds = true;
                return;
            }

            if (groundTilemap != null)
            {
                groundTilemap.CompressBounds();
                Bounds localB = groundTilemap.localBounds;
                Vector3 worldMin = groundTilemap.transform.TransformPoint(localB.min);
                Vector3 worldMax = groundTilemap.transform.TransformPoint(localB.max);

                Vector3 center = (worldMin + worldMax) * 0.5f;
                Vector3 size = new Vector3(Mathf.Max(2f, (worldMax.x - worldMin.x) - margin * 2f), Mathf.Max(2f, (worldMax.y - worldMin.y) - margin * 2f), 10f);

                _safeMapBounds = new Bounds(center, size);
                _hasCalculatedBounds = true;
                return;
            }

            // Fallback nếu không có Tilemap: Khung 20x20 tiêu chuẩn
            _safeMapBounds = new Bounds(Vector3.zero, new Vector3(20f, 20f, 10f));
            _hasCalculatedBounds = true;
        }

        public async System.Threading.Tasks.Task StartMatchAsync()
        {
            EnsureDependencies();
            matchTime = 0f;
            _nextEventIndex = 0;
            _activeContinuousEvents.Clear();
            _eventTimers.Clear();

            isMatchActive = true;

            // 1. Tự động Async Preload tất cả Prefabs trong Timeline qua WavePreloader (chạy ngầm không block isMatchActive)
            if (timelineConfig != null && _wavePreloader != null)
            {
                try
                {
                    await _wavePreloader.PreloadTimelineAssetsAsync(timelineConfig);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[SpawnManager] Preload assets warning: {ex.Message}");
                }
            }

            // Kích hoạt ngay sự kiện ban đầu ở giây thứ 0s
            CheckTimelineEvents();
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
            if (evt == null) return;

            // Đảm bảo spawnPrefab luôn có sẵn
            if (evt.spawnPrefab == null && !string.IsNullOrEmpty(evt.enemyAddress))
            {
                var pool = EnemyPoolManager.Instance;
                // Nếu pool có sẵn thì vẫn tiếp tục
            }

            string poolKey = evt.GetPoolKey();
            Debug.Log($"[SpawnManager] Kích hoạt Timeline Event: '{evt.eventName}' (Key: {poolKey}, Type: {evt.eventType}) tại phút {(matchTime / 60f):F2}");

            switch (evt.eventType)
            {
                case TimelineEventType.Continuous:
                    if (!_activeContinuousEvents.Contains(evt))
                    {
                        _activeContinuousEvents.Add(evt);
                        _eventTimers[evt] = 0f;
                        // Spawn tức thì đợt quái đầu tiên ngay khi kích hoạt sự kiện
                        for (int s = 0; s < Mathf.Max(1, evt.spawnCount); s++)
                        {
                            SpawnAtPosition(evt.spawnPrefab, GetSpawnPositionOutsideCamera());
                        }
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
            if (prefab == null)
            {
                Debug.LogWarning("[SpawnManager] Không thể spawn quái vì prefab bị NULL trong TimelineEvent!");
                return null;
            }

            GameObject enemy = null;
            if (EnemyPoolManager.Instance != null)
            {
                enemy = EnemyPoolManager.Instance.SpawnEnemy(prefab, position, Quaternion.identity);
            }
            else
            {
                enemy = Instantiate(prefab, position, Quaternion.identity);
            }

            if (enemy != null)
            {
                currentEnemyCount++;
                enemy.SetActive(true);
            }
            return enemy;
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
            if (_playerTransform == null)
            {
                if (Player.PlayerProvider.HasPlayer) _playerTransform = Player.PlayerProvider.PlayerTransform;
                else
                {
                    var playerObj = GameObject.FindGameObjectWithTag("Player");
                    if (playerObj != null) _playerTransform = playerObj.transform;
                }
            }

            Vector3 center = _playerTransform != null ? _playerTransform.position : Vector3.zero;
            int obstacleMask = LayerMask.GetMask("Obstacle", "Water");
            if (obstacleMask == 0) obstacleMask = LayerMask.GetMask("Obstacle");

            if (_mainCamera == null) _mainCamera = Camera.main;

            float effectiveMin = minSpawnRadius > 0 ? minSpawnRadius : 8f;
            float effectiveMax = maxSpawnRadius > effectiveMin ? maxSpawnRadius : effectiveMin + 6f;

            // Tự động thích ứng bán kính theo kích thước Camera nếu có
            if (_mainCamera != null && _mainCamera.orthographic)
            {
                float camHalfH = _mainCamera.orthographicSize + cameraPadding;
                float camHalfW = _mainCamera.orthographicSize * _mainCamera.aspect + cameraPadding;
                float camDiagonal = Mathf.Sqrt(camHalfW * camHalfW + camHalfH * camHalfH);

                effectiveMin = Mathf.Min(minSpawnRadius, camDiagonal);
                effectiveMax = Mathf.Max(effectiveMin + 2f, maxSpawnRadius);
            }

            // Giai đoạn 1: Lấy mẫu vị trí (Vừa ngoài Camera, vừa nằm trong Safe Bounds và có sàn hợp lệ)
            for (int i = 0; i < 16; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = Random.Range(effectiveMin, effectiveMax);
                Vector3 candidatePos = center + new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0f);

                // 1. Phải nằm trong Safe Bounds và sàn gạch Walkable
                if (!IsInsideWalkableArea(candidatePos))
                    continue;

                // 2. Không được dính Obstacle / Tường
                if (obstacleMask != 0)
                {
                    Collider2D hit = Physics2D.OverlapCircle(candidatePos, 0.5f, obstacleMask);
                    if (hit != null) continue;
                }

                // 3. Phải nằm ngoài tầm nhìn Camera
                if (!IsOutsideCameraViewport(candidatePos))
                    continue;

                return candidatePos;
            }

            // Giai đoạn 2: Smart Math Clamping Fallback (Khi Player đứng sát góc chết mép tường)
            // Lấy ngẫu nhiên các điểm ngoài Camera rồi ép trực tiếp (Clamp) vào trong Safe Map Bounds
            for (int i = 0; i < 8; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = Random.Range(effectiveMin, effectiveMax);
                Vector3 rawPos = center + new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0f);

                Vector3 clampedPos = ClampToSafeBounds(rawPos);

                if (IsInsideWalkableArea(clampedPos))
                {
                    if (obstacleMask != 0 && Physics2D.OverlapCircle(clampedPos, 0.5f, obstacleMask) != null)
                        continue;

                    return clampedPos;
                }
            }

            // Fallback cuối cùng: Clamp điểm quanh Player về mép an toàn bên trong sàn đấu
            Vector3 finalFallback = ClampToSafeBounds(center + (Vector3)(Random.insideUnitCircle.normalized * effectiveMin));
            return finalFallback;
        }

        private Vector3 ClampToSafeBounds(Vector3 targetPos)
        {
            if (!_hasCalculatedBounds) CalculateSafeMapBounds();

            float clampedX = Mathf.Clamp(targetPos.x, _safeMapBounds.min.x, _safeMapBounds.max.x);
            float clampedY = Mathf.Clamp(targetPos.y, _safeMapBounds.min.y, _safeMapBounds.max.y);
            return new Vector3(clampedX, clampedY, 0f);
        }

        private bool IsInsideWalkableArea(Vector3 position)
        {
            // 1. Bắt buộc phải nằm trong Safe Bounds của sàn đấu
            if (_hasCalculatedBounds && !_safeMapBounds.Contains(position))
            {
                return false;
            }

            if (walkableAreaCollider != null)
            {
                if (!walkableAreaCollider.gameObject.name.Contains("Obstacle") && !walkableAreaCollider.OverlapPoint(position))
                {
                    return false;
                }
            }

            // 2. Bắt buộc phải có sàn gạch trên Ground Tilemap
            if (groundTilemap != null)
            {
                Vector3Int cellPos = groundTilemap.WorldToCell(position);
                if (!groundTilemap.HasTile(cellPos)) return false;
            }

            // 3. Tuyệt đối không được trùng với ô tường trên Obstacle Tilemap
            if (_obstacleTilemap != null)
            {
                Vector3Int obsCell = _obstacleTilemap.WorldToCell(position);
                if (_obstacleTilemap.HasTile(obsCell)) return false;
            }

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
