using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using ProjectZombie.Features.Maps;

namespace ProjectZombie.Core.Architecture
{
    /// <summary>Owns application services in Bootstrap; content scenes own no persistent managers.</summary>
    [DefaultExecutionOrder(-2000)]
    public sealed class AppCompositionRoot : MonoBehaviour
    {
        public const string BOOTSTRAP_SCENE = "Bootstrap";
        public static AppCompositionRoot Instance { get; private set; }
        [SerializeField] private AssetReference _hubScene;
        [SerializeField] private WorldStageDatabaseSO _stages;
        [SerializeField] private AppLoadingView _loading;
        [SerializeField] private ProjectZombie.Features.Defense3D.Loadout.DefenseTowerLoadoutCatalogSO _towerMetadata;
        public AppFlowController Flow { get; private set; }
        public WorldStageDatabaseSO Stages => _stages;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            var active = SceneManager.GetActiveScene();
            Flow = new AppFlowController(new AddressableSceneLoader(active.name == BOOTSTRAP_SCENE ? default : active), _hubScene, _loading);
            ServiceContext.Register<AppFlowController>(Flow);
            if (_towerMetadata != null) ServiceContext.Register(_towerMetadata);
            CoreBootstrapper.InitializeCoreServices(gameObject);
            if (!TryGetComponent<Features.Shared.GameStateManager>(out _))
                gameObject.AddComponent<Features.Shared.GameStateManager>();
            _loading.Hide();
        }

        private async void Start()
        {
            if (Instance == this && SceneManager.GetActiveScene().name == BOOTSTRAP_SCENE)
                await Flow.ReturnToHubAsync();
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
            ServiceContext.Unregister<AppFlowController>();
            ServiceContext.Unregister<ProjectZombie.Features.Defense3D.Loadout.DefenseTowerLoadoutCatalogSO>();
        }
    }
}
