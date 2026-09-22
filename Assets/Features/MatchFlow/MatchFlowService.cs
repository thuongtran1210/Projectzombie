using System;
using System.Threading.Tasks;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Maps;
using ProjectZombie.Features.Spawners;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Upgrades;
using ProjectZombie.Core.Architecture;
using ProjectZombie.Core.Services.Addressables;

namespace ProjectZombie.Features.MatchFlow
{
    /// <summary>
    /// Dịch vụ điều phối luồng nạp và chuẩn bị trận đấu.
    /// Sử dụng IAssetProvider để nạp tài nguyên đồng bộ với chiến lược Dev vs Release (Mục 3.2 & 3.3 AGENTS.md).
    /// </summary>
    public class MatchFlowService : IMatchFlowService
    {
        private static MatchFlowService _default;
        public static MatchFlowService Default => _default ??= new MatchFlowService();

        private readonly IAssetProvider _assetProvider;
        private GameObject _currentMapInstance;

        public GameObject CurrentMapInstance => _currentMapInstance;

        public MatchFlowService(IAssetProvider assetProvider = null)
        {
            _assetProvider = assetProvider ?? ServiceContext.Get<IAssetProvider>() ?? AddressableAssetManager.Instance;
        }

        public async Task ExecuteCombatPreparationAsync(
            StageDefinitionSO stage,
            GameplayBootstrapper gameplayBootstrapper,
            Action<float, string> reportProgress = null)
        {
            // Tự động resolve stage mặc định nếu chưa chọn bằng AssetProvider
            if (stage == null)
            {
                var db = await _assetProvider.LoadAssetAsync<WorldStageDatabaseSO>("WorldStageDatabase");
                if (db != null && db.Stages != null && db.Stages.Count > 0)
                {
                    stage = db.Stages[0];
                }

                if (stage == null)
                {
                    stage = await _assetProvider.LoadAssetAsync<StageDefinitionSO>("Stage_01_BambooForest");
                }
            }

            // BƯỚC 1: Cấu hình Timeline Ải
            if (stage != null && stage.timelineConfig != null && SpawnManager.Instance != null)
            {
                SpawnManager.Instance.SetTimelineConfig(stage.timelineConfig);
            }

            // BƯỚC 2: Khởi tạo Map Tilemap qua AssetProvider
            string stageMsg = stage != null ? $"Đang khai mở {stage.stageName}..." : "Đang chuẩn bị chiến trường...";
            reportProgress?.Invoke(0.2f, stageMsg);

            await LoadMapAsync(stage);
            await Task.Yield();

            // BƯỚC 3: Khởi tạo Player & UI In-Game
            reportProgress?.Invoke(0.4f, "Đang triệu hồi chân thân Tướng...");
            if (gameplayBootstrapper != null)
            {
                gameplayBootstrapper.StartMatchFlow();
            }
            await Task.Yield();

            // BƯỚC 4: Preload Quái Vật & Khởi Tạo Pool Ngầm
            reportProgress?.Invoke(0.6f, "Đang nạp dữ liệu quái vật cõi âm...");
            if (SpawnManager.Instance != null && !SpawnManager.Instance.IsMatchActive)
            {
                await SpawnManager.Instance.StartMatchAsync();
            }
            await Task.Yield();

            // BƯỚC 5: Nạp Thẻ Nâng Cấp & Chuẩn Bị Âm Thanh Trận Đấu
            reportProgress?.Invoke(0.85f, "Đang ngưng tụ linh khí ngũ hành...");
            if (UpgradeManager.Instance != null)
            {
                await UpgradeManager.Instance.AutoPopulateUpgradesIfEmptyAsync();
            }

            var phaseAudio = global::Core.Audio.PhaseAudioController.Instance;
            if (phaseAudio != null)
            {
                phaseAudio.ForceInitialPhaseAudio();
            }
            await Task.Yield();

            // BƯỚC 6: Bắt Đầu Trận Đấu (Chuyển GameState)
            reportProgress?.Invoke(1.0f, "Chiến trường đã sẵn sàng!");
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(GameState.Playing);
            }
        }

        private async Task LoadMapAsync(StageDefinitionSO stage)
        {
            if (stage == null || string.IsNullOrEmpty(stage.mapPrefabAddress))
            {
                EnableStaticSceneEnvironments();
                return;
            }

            DestroyCurrentMapInstance();
            DisableStaticSceneEnvironments();

            var mapInstance = await _assetProvider.InstantiateAsync(stage.mapPrefabAddress, Vector3.zero, Quaternion.identity);
            if (mapInstance != null)
            {
                _currentMapInstance = mapInstance;
                _currentMapInstance.name = stage.mapPrefabAddress;
                _currentMapInstance.transform.position = Vector3.zero;
                _currentMapInstance.SetActive(true);
                SpawnManager.Instance?.ConfigureMapInstance(_currentMapInstance);
                Debug.Log($"<color=#00FF88>[MatchFlowService] Đã nạp thành công Map '{stage.mapPrefabAddress}'!</color>");
            }
            else
            {
                Debug.LogWarning($"[MatchFlowService] Không thể nạp Map '{stage.mapPrefabAddress}', sử dụng Tilemap mặc định trong Scene.");
                EnableStaticSceneEnvironments();
            }
        }

        public void DestroyCurrentMapInstance()
        {
            if (_currentMapInstance != null)
            {
                UnityEngine.Object.Destroy(_currentMapInstance);
                _currentMapInstance = null;
            }
        }

        private static void DisableStaticSceneEnvironments()
        {
            var staticSanDinh = GameObject.Find("Environment_SanDinhLangCo");
            if (staticSanDinh != null)
            {
                staticSanDinh.SetActive(false);
            }
        }

        private static void EnableStaticSceneEnvironments()
        {
            var grids = UnityEngine.Object.FindObjectsOfType<Grid>(true);
            foreach (var grid in grids)
            {
                if (grid.gameObject.name.Contains("SanDinh") || grid.gameObject.name.Contains("Environment"))
                {
                    grid.gameObject.SetActive(true);
                    SpawnManager.Instance?.ConfigureMapInstance(grid.gameObject);
                    break;
                }
            }
        }
    }
}
