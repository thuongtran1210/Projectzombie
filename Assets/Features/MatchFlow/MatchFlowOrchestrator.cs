using System;
using System.Threading.Tasks;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Maps;
using ProjectZombie.Features.Spawners;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Upgrades;

namespace ProjectZombie.Features.MatchFlow
{
    /// <summary>
    /// Nhạc trưởng điều phối luồng chuẩn bị trận đấu (Match Flow Orchestration).
    /// Thực thi tuần tự, dứt điểm từng giai đoạn: Nạp Map -> Preload Quái -> Setup Player -> Chuyển State Playing.
    /// Giúp tách biệt hoàn toàn Logic Gameplay ra khỏi tầng UI Presentation.
    /// </summary>
    public static class MatchFlowOrchestrator
    {
        private static GameObject _currentMapInstance;

        public static async Task ExecuteCombatPreparationAsync(
            StageDefinitionSO stage,
            GameplayBootstrapper gameplayBootstrapper,
            Action<float, string> reportProgress = null)
        {
            // -------------------------------------------------------------
            // BƯỚC 1: Cấu hình Timeline Ải
            // -------------------------------------------------------------
            if (stage != null && stage.timelineConfig != null && SpawnManager.Instance != null)
            {
                SpawnManager.Instance.SetTimelineConfig(stage.timelineConfig);
            }

            // -------------------------------------------------------------
            // BƯỚC 2: Khởi tạo Map Tilemap (Addressables -> Fallback Resources)
            // -------------------------------------------------------------
            string stageMsg = stage != null ? $"Đang khai mở {stage.stageName}..." : "Đang chuẩn bị chiến trường...";
            reportProgress?.Invoke(0.2f, stageMsg);

            await LoadMapAsync(stage);
            await Task.Yield();

            // -------------------------------------------------------------
            // BƯỚC 3: Khởi tạo Player & UI In-Game
            // -------------------------------------------------------------
            reportProgress?.Invoke(0.4f, "Đang triệu hồi chân thân Tướng...");
            if (gameplayBootstrapper != null)
            {
                gameplayBootstrapper.StartMatchFlow();
            }
            await Task.Yield();

            // -------------------------------------------------------------
            // BƯỚC 4: Preload Quái Vật & Khởi Tạo Pool Ngầm
            // -------------------------------------------------------------
            reportProgress?.Invoke(0.6f, "Đang nạp dữ liệu quái vật cõi âm...");
            if (SpawnManager.Instance != null)
            {
                await SpawnManager.Instance.StartMatchAsync();
            }
            await Task.Yield();

            // -------------------------------------------------------------
            // BƯỚC 5: Nạp Thẻ Nâng Cấp & Chuẩn Bị Âm Thanh Trận Đấu
            // -------------------------------------------------------------
            reportProgress?.Invoke(0.85f, "Đang ngưng tụ linh khí ngũ hành...");
            UpgradeManager.Instance?.AutoPopulateUpgradesIfEmpty();

            var phaseAudio = UnityEngine.Object.FindObjectOfType<global::Core.Audio.PhaseAudioController>();
            if (phaseAudio != null)
            {
                phaseAudio.ForceInitialPhaseAudio();
            }
            await Task.Yield();

            // -------------------------------------------------------------
            // BƯỚC 6: Bắt Đầu Trận Đấu (Chuyển GameState)
            // -------------------------------------------------------------
            reportProgress?.Invoke(1.0f, "Chiến trường đã sẵn sàng!");
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(GameState.Playing);
            }
        }

        private static async Task LoadMapAsync(StageDefinitionSO stage)
        {
            if (stage == null || string.IsNullOrEmpty(stage.mapPrefabAddress)) return;

            // 1. Dọn dẹp map cũ
            DestroyCurrentMapInstance();

            bool loadedFromAddressables = false;
            try
            {
                var locHandle = UnityEngine.AddressableAssets.Addressables.LoadResourceLocationsAsync(stage.mapPrefabAddress);
                await locHandle.Task;
                if (locHandle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded &&
                    locHandle.Result != null && locHandle.Result.Count > 0)
                {
                    var handle = UnityEngine.AddressableAssets.Addressables.InstantiateAsync(stage.mapPrefabAddress);
                    await handle.Task;
                    if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                    {
                        _currentMapInstance = handle.Result;
                        SpawnManager.Instance?.RefreshMapReferences();
                        loadedFromAddressables = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MatchFlowOrchestrator] Addressable load failed: {ex.Message}");
            }

            // 2. Fallback sang Resources nếu offline hoặc Addressables chưa build
            if (!loadedFromAddressables)
            {
                var mapPrefab = Resources.Load<GameObject>($"Maps/{stage.mapPrefabAddress}");
                if (mapPrefab != null)
                {
                    _currentMapInstance = UnityEngine.Object.Instantiate(mapPrefab);
                    _currentMapInstance.name = stage.mapPrefabAddress;
                    SpawnManager.Instance?.RefreshMapReferences();
                    Debug.Log($"<color=#00FF88>[MatchFlowOrchestrator] Đã nạp thành công Map '{stage.mapPrefabAddress}' từ Resources!</color>");
                }
                else
                {
                    Debug.Log($"[MatchFlowOrchestrator] Không tìm thấy Map '{stage.mapPrefabAddress}' trong Resources/Maps, sử dụng Tilemap mặc định trong Scene.");
                }
            }
        }

        public static void DestroyCurrentMapInstance()
        {
            if (_currentMapInstance != null)
            {
                bool releasedByAddressables = false;
                try
                {
                    releasedByAddressables = UnityEngine.AddressableAssets.Addressables.ReleaseInstance(_currentMapInstance);
                }
                catch { }

                if (!releasedByAddressables && _currentMapInstance != null)
                {
                    UnityEngine.Object.Destroy(_currentMapInstance);
                }
                _currentMapInstance = null;
            }

            // Dọn dẹp các instance tàn dư nếu có
            string[] mapNames = new string[] { "Environment_SanDinhLangCo", "Map_SanDinhLangCo", "Map_BambooForest", "Map_AncientCitadel", "Map_CinnabarSwamp" };
            foreach (var mName in mapNames)
            {
                var existing = GameObject.Find(mName);
                if (existing != null) UnityEngine.Object.Destroy(existing);
                var existingClone = GameObject.Find(mName + "(Clone)");
                if (existingClone != null) UnityEngine.Object.Destroy(existingClone);
            }
        }
    }
}
