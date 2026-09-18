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
    public class SpawnManager : MonoBehaviour, ISpawnService, ProjectZombie.Core.Architecture.IResettableStatic
    {
        public static SpawnManager Instance { get; private set; }

        public void ResetStaticState()
        {
            Instance = null;
            OnWaveTriggered = null;
            OnTimelineProgressUpdated = null;
            ProjectZombie.Core.Architecture.ServiceContext.Unregister<ISpawnService>();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            OnWaveTriggered = null;
            OnTimelineProgressUpdated = null;
            ProjectZombie.Core.Architecture.ServiceContext.Unregister<ISpawnService>();
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



        [Header("Auto Start (For Quick Play / Testing)")]
        [Tooltip("Chỉ tự động bắt đầu trận khi test riêng lẻ trong Unity Editor và GameState là Playing")]
        [SerializeField] private bool autoStartOnPlay = false;

        // Sub-modules (Tách biệt đơn trách nhiệm)
        private ArenaBoundaryContext _boundaryContext;
        private ISpawnPositionLocator _spawnLocator;
        private IEnemyPopulationTracker _populationTracker;
        private readonly WaveScheduler _waveScheduler = new WaveScheduler();
        private readonly Dictionary<TimelineEventType, ISpawnPatternStrategy> _strategies = new Dictionary<TimelineEventType, ISpawnPatternStrategy>();

        private Transform _playerTransform;
        private Camera _mainCamera;
        private WavePreloader _wavePreloader;

        // Public Properties giữ 100% tương thích ngược qua WaveScheduler sub-module
        public float MatchTime => _waveScheduler.MatchTime;
        public int CurrentEnemyCount => _populationTracker != null ? _populationTracker.CurrentEnemyCount : 0;
        public bool IsMatchActive => _waveScheduler.IsMatchActive;
        public LevelTimelineConfig TimelineConfig => timelineConfig;
        public float LevelDuration => _waveScheduler.LevelDuration;
        public float MatchProgress => _waveScheduler.MatchProgress;
        public int CurrentWaveIndex => _waveScheduler.CurrentWaveIndex;
        public int TotalWaves => _waveScheduler.TotalWaves;
        public string CurrentStageName => (timelineConfig != null && !string.IsNullOrEmpty(timelineConfig.levelName)) ? timelineConfig.levelName : "Màn 1: U Minh Giới";
        public TimelineEvent CurrentActiveEvent => (timelineConfig != null && timelineConfig.events != null && _waveScheduler.NextEventIndex > 0 && _waveScheduler.NextEventIndex <= timelineConfig.events.Count) ? timelineConfig.events[_waveScheduler.NextEventIndex - 1] : null;
        public TimelineEvent NextUpcomingEvent => (timelineConfig != null && timelineConfig.events != null && _waveScheduler.NextEventIndex < timelineConfig.events.Count) ? timelineConfig.events[_waveScheduler.NextEventIndex] : null;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            ProjectZombie.Core.Architecture.ServiceContext.Register<ISpawnService>(this);
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

            if (autoStartOnPlay && isSandboxTesting && !IsMatchActive)
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
                _boundaryContext.AssignMapInstance(mapInstance, gTilemap, oTilemap);
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
            // Trong Multiplayer: Chỉ có Chủ Phòng (Host) mới được quyền kích hoạt đợt quái
            if (ProjectZombie.Core.Architecture.ServiceContext.TryGet<ProjectZombie.Features.Multiplayer.Core.INetworkSessionService>(out var session))
            {
                if (session.IsInRoom && !session.IsHost)
                {
                    Debug.Log("<color=#888888>[SpawnManager]</color> Bỏ qua StartMatch trên Client (Host là máy chủ điều phối duy nhất).");
                    return;
                }
            }

            EnsureDependencies();
            _waveScheduler.Initialize(timelineConfig);

            if (_populationTracker != null)
            {
                int playerCount = Player.PlayerProvider.Registry != null ? Mathf.Max(1, Player.PlayerProvider.Registry.ActivePlayers.Count) : 1;
                int scaledCap = Mathf.RoundToInt(maxEnemyCap * (1f + (playerCount - 1) * 0.5f));

                _populationTracker.MaxEnemyCap = scaledCap;
                _populationTracker.MinEnemyFloor = minEnemyFloor;
                _populationTracker.ResetCount();
            }

            foreach (var strategy in _strategies.Values)
            {
                strategy.ResetStrategy();
            }

            _waveScheduler.StartMatch();

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
            ProcessDueEvents(_waveScheduler.Tick(0f, out _));
        }

        public void StartMatch()
        {
            _ = StartMatchAsync();
        }

        public void StopMatch()
        {
            _waveScheduler.StopMatch();
        }

        /// <summary>
        /// Dừng trận đấu và dọn sạch toàn bộ quái vật, boss, minion, tiền và ngọc rơi trên bản đồ.
        /// </summary>
        public void StopMatchAndClearAllEnemies()
        {
            _waveScheduler.StopMatch();

            if (_populationTracker != null)
            {
                _populationTracker.ResetCount();
            }

            foreach (var strategy in _strategies.Values)
            {
                strategy.ResetStrategy();
            }

            // Dọn sạch toàn bộ Enemies qua ActiveEnemies registry O(1)
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
            if (!_waveScheduler.IsMatchActive || timelineConfig == null) return;

            // Trong Multiplayer: Chỉ có Chủ Phòng (Host) mới được quyền chạy nhịp sinh quái vật
            if (ProjectZombie.Core.Architecture.ServiceContext.TryGet<ProjectZombie.Features.Multiplayer.Core.INetworkSessionService>(out var session))
            {
                if (session.IsInRoom && !session.IsHost)
                {
                    return; // Client không tự chạy spawner tránh lệch nhịp quái
                }
            }

            // 1. Cập nhật thời gian và nhận danh sách các sự kiện wave đến hạn
            var dueEvents = _waveScheduler.Tick(Time.deltaTime, out _);

            // 2. Phát sự kiện tiến trình cho HUD / Wave Banner
            OnTimelineProgressUpdated?.Invoke(_waveScheduler.MatchTime, _waveScheduler.LevelDuration, _waveScheduler.MatchProgress);

            // 3. Kích hoạt các wave event
            ProcessDueEvents(dueEvents);

            // 4. Chạy cập nhật các Strategy đang kích hoạt (Luân phiên phân bổ quái quanh các người chơi còn sống)
            float timeMultiplier = _populationTracker != null ? _populationTracker.CalculateAdaptiveMultiplier(adaptiveCatchupRate) : 1f;
            Transform activeTarget = GetTargetPlayerTransform();

            foreach (var strategy in _strategies.Values)
            {
                strategy.OnUpdate(Time.deltaTime, timeMultiplier, _spawnLocator, _populationTracker, activeTarget, SpawnAtPosition);
            }
        }

        private Transform GetTargetPlayerTransform()
        {
            var registry = Player.PlayerProvider.Registry;
            if (registry != null && registry.ActivePlayers.Count > 0)
            {
                int count = registry.ActivePlayers.Count;
                int startIndex = UnityEngine.Random.Range(0, count);
                for (int i = 0; i < count; i++)
                {
                    var p = registry.ActivePlayers[(startIndex + i) % count];
                    if (p != null && p.Transform != null && p.IsAlive)
                    {
                        return p.Transform;
                    }
                }
            }
            return _playerTransform != null ? _playerTransform : Player.PlayerProvider.PlayerTransform;
        }

        private void ProcessDueEvents(List<TimelineEvent> dueEvents)
        {
            if (dueEvents == null) return;
            for (int i = 0; i < dueEvents.Count; i++)
            {
                TriggerEvent(dueEvents[i]);
            }
        }

        private void TriggerEvent(TimelineEvent evt)
        {
            if (evt == null) return;

            Debug.Log($"[SpawnManager] Kích hoạt Timeline Event: '{evt.eventName}' (Key: {evt.GetPoolKey()}, Type: {evt.eventType}) tại phút {(MatchTime / 60f):F2}");

            // Phát sự kiện cho tầng UI (Wave Banner Widget) cập nhật
            string stageName = timelineConfig != null ? timelineConfig.levelName : "Chiến Trường";
            int totalEvents = TotalWaves;
            int currentWave = CurrentWaveIndex;
            Sprite waveIcon = evt.GetIcon();
            OnWaveTriggered?.Invoke(new ProjectZombie.Features.UI.HUD.WaveInfo(stageName, evt.eventName, currentWave, totalEvents, evt.eventType, evt.timestampSeconds, waveIcon));

            // Dọn dẹp quái cũ ở xa khi chuyển Wave hoặc dọn sân khi Boss xuất hiện
            bool isBossWave = evt.eventType == TimelineEventType.BossSpawn;
            CullDistantEnemies(maxDistance: isBossWave ? 18f : 24f, cullAllDistant: isBossWave);

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

        /// <summary>
        /// Thu hồi các quái vật ở quá xa người chơi về Pool khi chuyển Wave để chống nghẽn sĩ số và tụt FPS.
        /// </summary>
        public void CullDistantEnemies(float maxDistance = 24f, bool cullAllDistant = false)
        {
            if (Enemies.Enemy.ActiveEnemies == null || Enemies.Enemy.ActiveEnemies.Count == 0) return;

            var registry = Player.PlayerProvider.Registry;
            var activePlayers = registry != null ? registry.ActivePlayers : null;
            float maxDistSq = maxDistance * maxDistance;

            var toRemove = new List<Enemies.Enemy>();

            foreach (var enemy in Enemies.Enemy.ActiveEnemies)
            {
                if (enemy == null || enemy.IsBoss) continue;

                // Kiểm tra xem quái có nằm quá xa TẤT CẢ người chơi không
                bool isFarFromAll = true;
                if (activePlayers != null && activePlayers.Count > 0)
                {
                    for (int i = 0; i < activePlayers.Count; i++)
                    {
                        var p = activePlayers[i];
                        if (p != null && p.Transform != null && p.IsAlive)
                        {
                            if (((Vector2)enemy.transform.position - (Vector2)p.Transform.position).sqrMagnitude <= maxDistSq)
                            {
                                isFarFromAll = false;
                                break;
                            }
                        }
                    }
                }
                else if (_playerTransform != null)
                {
                    if (((Vector2)enemy.transform.position - (Vector2)_playerTransform.position).sqrMagnitude <= maxDistSq)
                    {
                        isFarFromAll = false;
                    }
                }

                if (isFarFromAll || (cullAllDistant && UnityEngine.Random.value > 0.4f))
                {
                    toRemove.Add(enemy);
                }
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                var e = toRemove[i];
                if (e != null && e.gameObject != null)
                {
                    if (e.TryGetComponent<EnemyPoolConfig>(out var pc) && pc.Pool != null)
                    {
                        pc.ReturnToPool();
                    }
                    else
                    {
                        Destroy(e.gameObject);
                    }
                }
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
