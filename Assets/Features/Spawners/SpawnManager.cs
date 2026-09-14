using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Spawners.Core;
using ProjectZombie.Features.Spawners.Spatial;
using ProjectZombie.Features.Spawners.Strategies;

namespace ProjectZombie.Features.Spawners
{
    /// <summary>
    /// Facade & Nhạc trưởng điều phối toàn bộ vòng đời trận đấu (Timeline Director).
    /// Ủy quyền xử lý Không gian cho ArenaBoundaryContext,
    /// Thuật toán vị trí cho CameraAwareSpawnLocator,
    /// Sĩ số & Pacing cho EnemyPopulationTracker,
    /// Và các kiểu spawn cho ISpawnPatternStrategy.
    /// </summary>
    public class SpawnManager : MonoBehaviour, ProjectZombie.Core.Architecture.IResettableStatic
    {
        public static SpawnManager Instance { get; private set; }

        public void ResetStaticState()
        {
            Instance = null;
            OnWaveTriggered = null;
            OnTimelineProgressUpdated = null;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            OnWaveTriggered = null;
            OnTimelineProgressUpdated = null;
        }

        /// <summary>
        /// Sự kiện phát ra khi một đợt quái / mốc Timeline mới được kích hoạt.
        /// </summary>
        public static event Action<ProjectZombie.Features.UI.HUD.WaveInfo> OnWaveTriggered;

        /// <summary>
        /// Sự kiện cập nhật tiến trình thời gian của trận đấu (matchTime, maxDuration, progress 0..1).
        /// </summary>
        public static event Action<float, float, float> OnTimelineProgressUpdated;

        [Header("Timeline Configuration")]
        [SerializeField] private LevelTimelineConfig timelineConfig;

        [Header("Spawn Settings & Limits")]
        [SerializeField] private int maxEnemyCap = 50;
        [Tooltip("Số lượng quái tối thiểu cần duy trì trên sàn đấu để tránh khoảng lặng khi người chơi quét sạch quái")]
        [SerializeField] private int minEnemyFloor = 8;
        [Tooltip("Hệ số gia tốc spawn bù quái khi số quái trên sân thấp hơn minEnemyFloor")]
        [SerializeField] private float adaptiveCatchupRate = 3.0f;
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

        [Header("Auto Start (For Quick Play / Testing)")]
        [Tooltip("Chỉ tự động bắt đầu trận khi test riêng lẻ trong Unity Editor và GameState là Playing")]
        [SerializeField] private bool autoStartOnPlay = false;

        // Sub-modules (Tách biệt đơn trách nhiệm)
        private ArenaBoundaryContext _boundaryContext;
        private ISpawnPositionLocator _spawnLocator;
        private IEnemyPopulationTracker _populationTracker;
        private readonly Dictionary<TimelineEventType, ISpawnPatternStrategy> _strategies = new Dictionary<TimelineEventType, ISpawnPatternStrategy>();

        private Transform _playerTransform;
        private Camera _mainCamera;
        private WavePreloader _wavePreloader;
        private int _nextEventIndex = 0;

        // Public Properties giữ 100% tương thích ngược
        public float MatchTime => matchTime;
        public int CurrentEnemyCount => _populationTracker != null ? _populationTracker.CurrentEnemyCount : 0;
        public bool IsMatchActive => isMatchActive;
        public LevelTimelineConfig TimelineConfig => timelineConfig;
        public float LevelDuration => (timelineConfig != null && timelineConfig.maxLevelDuration > 0) ? timelineConfig.maxLevelDuration : 900f;
        public float MatchProgress => Mathf.Clamp01(matchTime / Mathf.Max(1f, LevelDuration));
        public int CurrentWaveIndex => Mathf.Max(1, _nextEventIndex);
        public int TotalWaves => (timelineConfig != null && timelineConfig.events != null) ? Mathf.Max(1, timelineConfig.events.Count) : 1;
        public string CurrentStageName => (timelineConfig != null && !string.IsNullOrEmpty(timelineConfig.levelName)) ? timelineConfig.levelName : "Man 1: U Minh Gioi";
        public TimelineEvent CurrentActiveEvent => (timelineConfig != null && timelineConfig.events != null && _nextEventIndex > 0 && _nextEventIndex <= timelineConfig.events.Count) ? timelineConfig.events[_nextEventIndex - 1] : null;
        public TimelineEvent NextUpcomingEvent => (timelineConfig != null && timelineConfig.events != null && _nextEventIndex < timelineConfig.events.Count) ? timelineConfig.events[_nextEventIndex] : null;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            _mainCamera = Camera.main;
            InitializeSubModules();
        }

        private void InitializeSubModules()
        {
            if (_boundaryContext == null)
            {
                _boundaryContext = new ArenaBoundaryContext(walkableAreaCollider, groundTilemap, null, autoFindGroundTilemap, 1.0f);
            }

            if (_spawnLocator == null)
            {
                _spawnLocator = new CameraAwareSpawnLocator(_boundaryContext, _mainCamera, cameraPadding);
            }

            if (_populationTracker == null)
            {
                _populationTracker = new EnemyPopulationTracker(maxEnemyCap, minEnemyFloor);
            }

            if (_strategies.Count == 0)
            {
                RegisterStrategy(new ContinuousSpawnStrategy());
                RegisterStrategy(new BurstWaveSpawnStrategy());
                RegisterStrategy(new BossEncounterStrategy());
                RegisterStrategy(new PillarSpawnStrategy());
            }
        }

        public void RegisterStrategy(ISpawnPatternStrategy strategy)
        {
            if (strategy != null)
            {
                _strategies[strategy.HandledType] = strategy;
            }
        }

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
            bool isSandboxTesting = Application.isEditor && 
                                    (Shared.GameStateManager.Instance == null || Shared.GameStateManager.Instance.CurrentState == Shared.GameState.Playing);

            if (autoStartOnPlay && isSandboxTesting && !isMatchActive)
            {
                StartMatch();
            }
        }

        public void SetTimelineConfig(LevelTimelineConfig config)
        {
            if (config != null)
            {
                timelineConfig = config;
            }
        }

        private void EnsureDependencies()
        {
            InitializeSubModules();

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

            if (_playerTransform == null && Player.PlayerProvider.HasPlayer)
            {
                _playerTransform = Player.PlayerProvider.PlayerTransform;
            }

            _boundaryContext.EnsureDependencies();
        }

        /// <summary>
        /// Cấu hình trực tiếp Map Instance vừa sinh ra từ MatchFlowOrchestrator, triệt tiêu hoàn toàn Find trong runtime.
        /// </summary>
        public void ConfigureMapInstance(GameObject mapInstance)
        {
            if (mapInstance == null) return;

            Tilemap gTilemap = null;
            Tilemap oTilemap = null;
            Collider2D wCollider = mapInstance.GetComponentInChildren<Collider2D>();

            var tilemaps = mapInstance.GetComponentsInChildren<Tilemap>();
            foreach (var tm in tilemaps)
            {
                string lname = tm.name.ToLower();
                if (gTilemap == null && (lname.Contains("ground") || lname.Contains("floor")))
                {
                    gTilemap = tm;
                }
                else if (oTilemap == null && (lname.Contains("obstacle") || lname.Contains("wall")))
                {
                    oTilemap = tm;
                }
            }

            if (gTilemap == null && tilemaps.Length > 0) gTilemap = tilemaps[0];

            groundTilemap = gTilemap;
            walkableAreaCollider = wCollider;

            if (_boundaryContext != null)
            {
                _boundaryContext.AssignExplicitTilemaps(gTilemap, oTilemap, wCollider);
            }
        }

        /// <summary>
        /// Làm mới tham chiếu Tilemap & ranh giới sàn đấu khi nạp một Map mới từ Addressables.
        /// </summary>
        public void RefreshMapReferences()
        {
            groundTilemap = null;
            walkableAreaCollider = null;
            if (_boundaryContext != null)
            {
                _boundaryContext.RefreshMapReferences();
            }
            EnsureDependencies();
        }

        public async Task StartMatchAsync()
        {
            EnsureDependencies();
            matchTime = 0f;
            _nextEventIndex = 0;

            if (_populationTracker != null)
            {
                _populationTracker.MaxEnemyCap = maxEnemyCap;
                _populationTracker.MinEnemyFloor = minEnemyFloor;
                _populationTracker.ResetCount();
            }

            foreach (var strategy in _strategies.Values)
            {
                strategy.ResetStrategy();
            }

            isMatchActive = true;

            // 1. Tự động Async Preload tất cả Prefabs trong Timeline qua WavePreloader
            if (timelineConfig != null && _wavePreloader != null)
            {
                try
                {
                    await _wavePreloader.PreloadTimelineAssetsAsync(timelineConfig);
                }
                catch (Exception ex)
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
        /// Dừng trận đấu và dọn sạch toàn bộ quái vật, boss, minion, tiền và ngọc rơi trên bản đồ.
        /// </summary>
        public void StopMatchAndClearAllEnemies()
        {
            isMatchActive = false;
            matchTime = 0f;
            _nextEventIndex = 0;

            if (_populationTracker != null)
            {
                _populationTracker.ResetCount();
            }

            foreach (var strategy in _strategies.Values)
            {
                strategy.ResetStrategy();
            }

            // Dọn sạch toàn bộ Enenies qua ActiveEnemies registry O(1)
            var activeEnemies = new System.Collections.Generic.List<Enemies.Enemy>(Enemies.Enemy.ActiveEnemies);
            for (int i = 0; i < activeEnemies.Count; i++)
            {
                if (activeEnemies[i] != null && activeEnemies[i].gameObject != null)
                {
                    Destroy(activeEnemies[i].gameObject);
                }
            }
            Enemies.Enemy.ActiveEnemies.Clear();

            Collectibles.CoinPoolManager.Instance?.ClearPools();
            Collectibles.ExpGemPoolManager.Instance?.ClearPools();
        }

        private void Update()
        {
            if (!isMatchActive || timelineConfig == null) return;

            matchTime += Time.deltaTime;

            // 1. Cập nhật tiến trình thời gian cho HUD / Wave Banner
            float maxDuration = LevelDuration;
            float progress = Mathf.Clamp01(matchTime / Mathf.Max(1f, maxDuration));
            OnTimelineProgressUpdated?.Invoke(matchTime, maxDuration, progress);

            // 2. Kiểm tra kích hoạt Timeline Event mới
            CheckTimelineEvents();

            // 3. Chạy cập nhật các Strategy đang kích hoạt
            float timeMultiplier = _populationTracker != null ? _populationTracker.CalculateAdaptiveMultiplier(adaptiveCatchupRate) : 1f;

            foreach (var strategy in _strategies.Values)
            {
                strategy.OnUpdate(Time.deltaTime, timeMultiplier, _spawnLocator, _populationTracker, _playerTransform, SpawnAtPosition);
            }
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

            Debug.Log($"[SpawnManager] Kích hoạt Timeline Event: '{evt.eventName}' (Key: {evt.GetPoolKey()}, Type: {evt.eventType}) tại phút {(matchTime / 60f):F2}");

            // Phát sự kiện cho tầng UI (Wave Banner Widget) cập nhật
            string stageName = timelineConfig != null ? timelineConfig.levelName : "Chiến Trường";
            int totalEvents = timelineConfig != null && timelineConfig.events != null ? timelineConfig.events.Count : 1;
            int currentWave = Mathf.Min(_nextEventIndex + 1, totalEvents);
            Sprite waveIcon = evt.GetIcon();
            OnWaveTriggered?.Invoke(new ProjectZombie.Features.UI.HUD.WaveInfo(stageName, evt.eventName, currentWave, totalEvents, evt.eventType, evt.timestampSeconds, waveIcon));

            // Chuyển giao thực thi cho Strategy tương ứng
            if (_strategies.TryGetValue(evt.eventType, out var strategy))
            {
                strategy.OnEventTriggered(evt, _spawnLocator, _populationTracker, _playerTransform, SpawnAtPosition);
            }
            else
            {
                Debug.LogWarning($"[SpawnManager] Chưa có strategy cho loại TimelineEventType: {evt.eventType}");
            }
        }

        public GameObject SpawnAtPosition(GameObject prefab, Vector3 position, string poolKey = null)
        {
            GameObject enemy = null;

            if (EnemyPoolManager.Instance != null)
            {
                if (prefab != null)
                {
                    enemy = EnemyPoolManager.Instance.SpawnEnemy(prefab, position, Quaternion.identity);
                }
                else if (!string.IsNullOrEmpty(poolKey))
                {
                    enemy = EnemyPoolManager.Instance.SpawnEnemy(poolKey, position, Quaternion.identity);
                }
            }
            else if (prefab != null)
            {
                enemy = Instantiate(prefab, position, Quaternion.identity);
            }

            if (enemy == null && prefab == null)
            {
                Debug.LogWarning($"[SpawnManager] Không thể spawn quái vì cả Prefab và PoolKey ('{poolKey}') đều không tìm thấy đối tượng hợp lệ!");
                return null;
            }

            if (enemy != null)
            {
                _populationTracker?.RegisterSpawn(1);
                enemy.SetActive(true);
            }
            return enemy;
        }

        /// <summary>
        /// Đồng bộ giảm sĩ số quái vật khi có quái bị tiêu diệt hoặc trả về Pool.
        /// </summary>
        public void OnEnemyDied()
        {
            _populationTracker?.OnEnemyDied(1);
        }

        /// <summary>
        /// Tiện ích lấy tọa độ ngoài camera cho các hệ thống ngoại vi (vd: Spawn Trụ tế đàn).
        /// </summary>
        public Vector3 GetSpawnPositionOutsideCamera()
        {
            if (_spawnLocator == null) EnsureDependencies();
            return _spawnLocator.GetSpawnPosition(_playerTransform, minSpawnRadius, maxSpawnRadius);
        }

        public void SpawnBurstWave(GameObject prefab, int count, string poolKey = null)
        {
            int actualSpawn = Mathf.Min(count, _populationTracker != null ? _populationTracker.MaxEnemyCap - _populationTracker.CurrentEnemyCount : count);
            for (int i = 0; i < actualSpawn; i++)
            {
                if (_populationTracker != null && !_populationTracker.CanSpawnMore) break;
                SpawnAtPosition(prefab, GetSpawnPositionOutsideCamera(), poolKey);
            }
        }

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
            else if (_boundaryContext != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(_boundaryContext.SafeMapBounds.center, _boundaryContext.SafeMapBounds.size);
            }
        }
    }
}
