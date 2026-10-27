using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Serialized scene facade. Composition owns the session; runtime services own match behavior.</summary>
    [DefaultExecutionOrder(-500)]
    public sealed class DefenseRun6003D : MonoBehaviour
    {
        [SerializeField] private DefenseSceneInfo3D sceneInfo;
        [SerializeField] private DefenseBasePrototype3D defenseBase;
        [SerializeField] private StageSpawnConfig stageSpawnConfig;
        [SerializeField] private UI.DefenseMatchPresenter matchUI;
        [SerializeField] private DefenseTowerBuildPrototype3D towerBuilder;
        [SerializeField] private bool showDebugOverlay;
        [SerializeField] private DefenseRuntimeRoots3D _runtimeRoots;
        [SerializeField] private UI.DefenseSpawnWarningPresenter3D _spawnWarnings;
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
        public bool BossSpawned => _encounter != null && _encounter.HasSpawned;
        public bool AssaultStarted => _encounter != null && _encounter.AssaultStarted;
        public DefenseEnemyBridgeWalker3D Yaksha => _encounter != null ? _encounter.Boss : null;
        public DefenseG1RuntimeAdapter RuntimeAdapter => _session != null ? _session.RuntimeAdapter : null;
        public DefenseBasePrototype3D Base => defenseBase;
        public DefenseHeroHealth3D HeroHealth => _heroHealth;
        public event Action<bool> MatchEnded;

        /// <summary>Preserves authored bindings while creating only C# runtime services.</summary>
        public bool InitializeSession(DefenseSession session, DefenseWoodWallet3D wallet)
        {
            if (_session != null) throw new InvalidOperationException("Run is already initialized.");
            try
            {
                if (session == null || wallet == null || sceneInfo == null || sceneInfo.HeroSpawn == null ||
                    sceneInfo.EastEnemySpawn == null || defenseBase == null || matchUI == null || stageSpawnConfig == null)
                    throw new InvalidOperationException("Assign Session, Wallet, SceneInfo, Hero, Base, Match UI and Stage Spawn Config.");
                _heroHealth = sceneInfo.HeroSpawn.GetComponent<DefenseHeroHealth3D>();
                if (_heroHealth == null) throw new InvalidOperationException("Hero requires DefenseHeroHealth3D.");
                if (_runtimeRoots == null || _runtimeRoots.gameObject.scene != gameObject.scene)
                    throw new InvalidOperationException("Assign authored Runtime Roots in this scene.");
                _runtimeRoots.ValidateBindings();
                var plan = stageSpawnConfig.Compile();
                _spawnWarnings?.Configure(plan, sceneInfo.EastEnemySpawn);
                _encounter = new DefenseYakshaEncounter3D(session, defenseBase, _heroHealth, plan.ArenaDuration, plan.BossHealth);
                var placement = new DefenseSpawnPlacement3D(plan, sceneInfo.EastEnemySpawn, sceneInfo.HeroSpawn,defenseBase.transform,_runtimeRoots.WaterNavigation,_runtimeRoots.AirNavigation);
                _runtimeRoots.FriendlyTargets.Bind(session);
                var factory = new DefenseEnemySessionFactory3D(session,_runtimeRoots,wallet,session.TargetRegistry);
                _spawns = new DefenseSpawnRuntime(plan, placement, session, defenseBase, wallet, _encounter, session.TargetRegistry,factory,_runtimeRoots.CannonBattery);
                _runtimeRoots.CannonBattery.Bind(session,_spawns);

                var woodDropPrefab = Resources.Load<GameObject>(DefenseWoodDrop3D.ResourcePath);
                if (woodDropPrefab != null)
                {
                    _woodDropPool = new DefenseWoodDropPool(woodDropPrefab, _runtimeRoots.Pools);
                    DefenseWoodDropPool.RegisterSessionPool(_woodDropPool);
                }

                _enemyPoolManager = new DefenseEnemyPoolManager(_runtimeRoots.Pools);
                DefenseEnemyPoolManager.RegisterSessionPool(_enemyPoolManager);

                _runtime = new DefenseMatchRuntime(session, defenseBase, _heroHealth, towerBuilder, _spawns, _encounter);
                session.Own(_runtime);
                _runtime.MatchEnded += ForwardResult;
                session.RuntimeAdapter.RegisterOwner(_runtime.Cleanup, _runtime.Reset);
                _session = session;
                return true;
            }
            catch (Exception exception)
            {
                if (_enemyPoolManager != null)
                {
                    DefenseEnemyPoolManager.UnregisterSessionPool(_enemyPoolManager);
                    _enemyPoolManager.Dispose();
                    _enemyPoolManager = null;
                }
                if (_woodDropPool != null)
                {
                    DefenseWoodDropPool.UnregisterSessionPool(_woodDropPool);
                    _woodDropPool.Dispose();
                    _woodDropPool = null;
                }
                try
                {
                    if (_runtime != null) _runtime.Dispose();
                    else _spawns?.Dispose();
                }
                finally { _encounter?.Dispose(); }
                Debug.LogError("[TREBRAVE Defense3D] Run initialization failed: " + exception.Message, this);
                enabled = false;
                return false;
            }
        }

        public void RestartRun() => _session?.Restart();

        private void Update()
        {
            _runtime?.Tick(Time.deltaTime);
        }

        private void OnDisable()
        {
            if (_session != null && _session.AdvancesGameplay) _session.Pause();
        }

        private void OnDestroy()
        {
            if (_enemyPoolManager != null)
            {
                DefenseEnemyPoolManager.UnregisterSessionPool(_enemyPoolManager);
                _enemyPoolManager.Dispose();
                _enemyPoolManager = null;
            }
            if (_woodDropPool != null)
            {
                DefenseWoodDropPool.UnregisterSessionPool(_woodDropPool);
                _woodDropPool.Dispose();
                _woodDropPool = null;
            }
            if (_runtime == null) return;
            _runtime.MatchEnded -= ForwardResult;
            _runtime.Dispose();
        }

        private void ForwardResult(bool won) => MatchEnded?.Invoke(won);

        private void OnGUI()
        {
            if (showDebugOverlay) DefenseRunDebugOverlay.Draw(this);
        }
    }
}

