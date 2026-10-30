using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// [BỘ ĐIỀU PHỐI VÒNG LẶP TRẬN ĐẤU 600 GIÂY - 10 PHÚT]
    /// Quản lý toàn bộ tiến trình diễn ra trận đấu:
    /// - Đếm ngược thời gian từ 0 -> 600 giây.
    /// - Quản lý triệu hồi Wave quái, cảnh báo quái xuất hiện.
    /// - Kích hoạt sự kiện Boss Dạ Xoa xuất hiện (Yaksha Encounter).
    /// - Bắt các điều kiện Thắng (sống sót qua 600s/diệt Boss) hoặc Thua (Căn cứ sập).
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public sealed class DefenseRun6003D : MonoBehaviour
    {
        [Header("=== 1. ĐỊA HÌNH & CĂN CỨ PHÒNG THỦ ===")]
        [Tooltip("Chứa các điểm Spawn chính: Tướng xuất hiện ở đâu, quái xuất hiện ở đâu")]
        [SerializeField] private DefenseSceneInfo3D sceneInfo;
        [Tooltip("Căn Cứ Bờ Tây cần bảo vệ (Máu căn cứ)")]
        [SerializeField] private DefenseBasePrototype3D defenseBase;

        [Header("=== 2. CẤU HÌNH ĐỢT QUÁI (STAGE & WAVE PLAN) ===")]
        [Tooltip("Cấu hình danh sách các Wave quái theo mốc thời gian giây")]
        [SerializeField] private StageSpawnConfig stageSpawnConfig;
        [Tooltip("Blueprint của ải (nếu dùng ScriptableObject thay cho component trên scene)")]
        [SerializeField] private DefenseStageBlueprintSO stageBlueprint;

        [Header("=== 3. QUẢN LÝ XÂY THÁP & HIỂN THỊ HUD ===")]
        [Tooltip("Presenter điều khiển giao diện HUD thời gian và thanh máu trong trận")]
        [SerializeField] private UI.DefenseMatchPresenter matchUI;
        [Tooltip("Bộ điều khiển cho phép người chơi đứng gần Socket để bấm nút Xây Tháp")]
        [SerializeField] private DefenseTowerBuildPrototype3D towerBuilder;
        [Tooltip("Bật hiển thị thông tin debug trên màn hình")]
        [SerializeField] private bool showDebugOverlay;
        [Tooltip("Gốc chứa các đối tượng động được spawn lúc chơi")]
        [SerializeField] private DefenseRuntimeRoots3D _runtimeRoots;

        [Header("=== 4. CẢNH BÁO QUÁI & HIỆU ỨNG TẤN CÔNG ===")]
        [Tooltip("Presenter hiển thị vệt đỏ/mũi tên cảnh báo trước khi đợt quái lớn ập đến")]
        [SerializeField] private UI.DefenseSpawnWarningPresenter3D _spawnWarnings;
        [Tooltip("Cấu hình hiệu ứng đòn đánh thường của quái")]
        [SerializeField] private DefenseBasicAttackEffectSetSO _enemyAttackStyle;
        private DefenseSession _session;
        private DefenseMatchRuntime _runtime;
        private DefenseSpawnRuntime _spawns;
        private DefenseYakshaEncounter3D _encounter;
        private DefenseHeroHealth3D _heroHealth;
        private DefenseWoodDropPool _woodDropPool;
        private DefenseEnemyPoolManager _enemyPoolManager;

        public float ElapsedTime => _session != null ? _session.ElapsedSeconds : 0f;
        public bool IsEnded => _session != null && _session.IsTerminal;
        public bool IsWon => _session != null && _session.Phase == DefenseGamePhase.Win;
        public bool IsOvertime => _session != null && _session.IsOvertime;
        public int ProcessedEventsCount => _spawns != null ? _spawns.ProcessedEvents : 0;
        public int TotalEventsCount => _spawns != null ? _spawns.TotalEvents : 0;
        public int EnemiesDefeatedCount => _spawns != null ? _spawns.EnemiesDefeatedCount : 0;
        public bool BossSpawned => _encounter != null && _encounter.HasSpawned;
        public bool AssaultStarted => _encounter != null && _encounter.AssaultStarted;
        public DefenseEnemyBridgeWalker3D Yaksha => _encounter != null ? _encounter.Boss : null;
        public DefenseG1RuntimeAdapter RuntimeAdapter => _session != null ? _session.RuntimeAdapter : null;
        public DefenseBasePrototype3D Base => defenseBase;
        public DefenseHeroHealth3D HeroHealth => _heroHealth;
        public IDefenseWoodDrops WoodDrops => _woodDropPool;
        public event Action<bool> MatchEnded;

        /// <summary>Preserves authored bindings while creating only C# runtime services.</summary>
        public bool InitializeSession(DefenseSession session, DefenseWoodWallet3D wallet, ProjectZombie.Features.UI.DamageText.ICombatTextService combatText = null, IDefenseEffectService effects = null)
        {
            if (_session != null) throw new InvalidOperationException("Run is already initialized.");
            try
            {
                if (session == null || wallet == null || sceneInfo == null || sceneInfo.HeroSpawn == null ||
                    sceneInfo.EastEnemySpawn == null || defenseBase == null || matchUI == null || (stageSpawnConfig == null && stageBlueprint == null))
                    throw new InvalidOperationException("Assign Session, Wallet, SceneInfo, Hero, Base, Match UI and Stage Spawn Config or Stage Blueprint.");
                _heroHealth = sceneInfo.HeroSpawn.GetComponent<DefenseHeroHealth3D>();
                if (_heroHealth == null) throw new InvalidOperationException("Hero requires DefenseHeroHealth3D.");
                if (_runtimeRoots == null || _runtimeRoots.gameObject.scene != gameObject.scene)
                    throw new InvalidOperationException("Assign authored Runtime Roots in this scene.");
                _runtimeRoots.ValidateBindings();
                if (effects != null && (_enemyAttackStyle == null || !_enemyAttackStyle.IsValid))
                    throw new InvalidOperationException("Assign valid enemy basic-attack VFX profiles.");
                var plan = stageBlueprint != null ? stageBlueprint.Compile() : stageSpawnConfig.Compile();
                _encounter = new DefenseYakshaEncounter3D(session, defenseBase, _heroHealth, plan.ArenaDuration, plan.BossHealth);
                var placement = new DefenseSpawnPlacement3D(plan, sceneInfo.EastEnemySpawn, sceneInfo.HeroSpawn,defenseBase.transform,_runtimeRoots.WaterNavigation,_runtimeRoots.AirNavigation);
                _runtimeRoots.FriendlyTargets.Bind(session);
                // Session sở hữu pool trước runtime để dispose runtime/factory trước pool theo thứ tự ngược.
                var woodDropPrefab = Resources.Load<GameObject>(DefenseWoodDrop3D.ResourcePath);
                if (woodDropPrefab == null) throw new InvalidOperationException("Missing wood pickup prefab.");
                _woodDropPool = new DefenseWoodDropPool(woodDropPrefab, _runtimeRoots.PickupPools, gameplay: session);
                _enemyPoolManager = new DefenseEnemyPoolManager(_runtimeRoots.EnemyPools);
                session.Own(_woodDropPool);
                session.Own(_enemyPoolManager);
                session.RunCleanup += _woodDropPool.Clear;
                foreach (var shop in _runtimeRoots.GetComponentsInChildren<DefenseVillageShop3D>(true)) shop.BindWoodDrops(_woodDropPool);
                // Shop có thể được đặt ngoài Runtime; composition root cũng truyền dependency cho scene participant.
                var factory = new DefenseEnemySessionFactory3D(session,_runtimeRoots,wallet,session.TargetRegistry,combatText,_enemyPoolManager,_woodDropPool,effects,_enemyAttackStyle);
                _spawns = new DefenseSpawnRuntime(plan, placement, session, defenseBase, wallet, _encounter, session.TargetRegistry,factory,_runtimeRoots.CannonBattery);
                _spawnWarnings?.Configure(_spawns.Warnings);
                _runtimeRoots.CannonBattery.Bind(session,_spawns);

                _runtime = new DefenseMatchRuntime(session, defenseBase, _heroHealth, towerBuilder, _spawns, _encounter);
                session.Own(_runtime);
                _runtime.MatchEnded += ForwardResult;
                session.RuntimeAdapter.RegisterOwner(_runtime.Cleanup, _runtime.Reset);
                _session = session;
                return true;
            }
            catch (Exception exception)
            {
                try
                {
                    if (_runtime != null) _runtime.Dispose();
                    else _spawns?.Dispose();
                }
                finally
                {
                    _encounter?.Dispose();
                    _enemyPoolManager?.Dispose();
                    _woodDropPool?.Dispose();
                }
                Debug.LogError("[TREBRAVE Defense3D] Run initialization failed: " + exception.Message, this);
                enabled = false;
                return false;
            }
        }

        /// <summary>Applies selected stage rules before the run acquires any resources.</summary>
        public void ConfigureStage(MatchLaunchRequest request)
        {
            if (_session != null) throw new InvalidOperationException("Cannot change rules during a run.");
            var stage = request?.Stage;
            if (stage == null || !stage.CanLaunchDefense) throw new ArgumentException("Invalid stage.", nameof(stage));
            stageBlueprint = stage.DefenseBlueprint;
            stageSpawnConfig = request.ResolvedRules;
        }

        public void RestartRun() => _session?.Restart();

        private void Update()
        {
            _runtime?.Tick(Time.deltaTime);
            _enemyPoolManager?.FlushReturns();
            _woodDropPool?.FlushReturns();
        }

        private void OnDisable()
        {
            if (_session != null && _session.AdvancesGameplay) _session.Pause();
        }

        private void OnDestroy()
        {
            if (_runtime != null) _runtime.MatchEnded -= ForwardResult;
            // Session là owner duy nhất quyết định thứ tự cleanup và dispose các service.
            _session?.Dispose();
        }

        private void ForwardResult(bool won) => MatchEnded?.Invoke(won);

        private void OnGUI()
        {
            if (showDebugOverlay) DefenseRunDebugOverlay.Draw(this);
        }
    }
}
