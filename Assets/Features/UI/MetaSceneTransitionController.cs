using System;
using System.Collections;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Spawners;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Bộ điều phối chuyển cảnh mượt mà Hướng A (All-in-One) giữa Sảnh Menu (Meta Hub) và Gameplay.
    /// Tích hợp trực tiếp với GameStateManager gốc trong Shared.
    /// </summary>
    public class MetaSceneTransitionController : MonoBehaviour
    {
        public static MetaSceneTransitionController Instance { get; private set; }

        [Header("Canvas Group Overlay")]
        [SerializeField] private CanvasGroup _fadeOverlayCanvasGroup;

        [Header("UI Subsystems")]
        [SerializeField] private MetaUIManager _metaUIManager;
        [SerializeField] private GameplayUIManager _gameplayUIManager;

        [Header("Systems")]
        [SerializeField] private MainHubPresenter _mainHubPresenter;
        [SerializeField] private SpawnManager _spawnManager;
        [SerializeField] private GameplayBootstrapper _gameplayBootstrapper;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (_fadeOverlayCanvasGroup == null)
            {
                _fadeOverlayCanvasGroup = GetComponent<CanvasGroup>();
            }

            EnsureReferences();

            // Thiết lập trạng thái hiển thị ban đầu
            bool isMainMenu = GameStateManager.Instance == null || GameStateManager.Instance.CurrentState == GameState.MainMenu;
            ApplyStateVisuals(isMainMenu);
        }

        private void Start()
        {
            EnsureReferences();
        }

        private void EnsureReferences()
        {
            if (_metaUIManager == null) _metaUIManager = MetaUIManager.Instance ?? FindObjectOfType<MetaUIManager>(true);
            if (_gameplayUIManager == null) _gameplayUIManager = GameplayUIManager.Instance ?? FindObjectOfType<GameplayUIManager>(true);
            if (_mainHubPresenter == null) _mainHubPresenter = FindObjectOfType<MainHubPresenter>(true);
            if (_spawnManager == null) _spawnManager = SpawnManager.Instance ?? FindObjectOfType<SpawnManager>(true);
            if (_gameplayBootstrapper == null) _gameplayBootstrapper = FindObjectOfType<GameplayBootstrapper>(true);

            // Đăng ký lắng nghe sự kiện Chọn Ải từ StageSelectUIPresenter để bắt đầu trận đấu
            StageSelect.StageSelectUIPresenter.OnStageSelectedForBattle -= HandleStageSelectedForBattle;
            StageSelect.StageSelectUIPresenter.OnStageSelectedForBattle += HandleStageSelectedForBattle;
        }

        private void OnDestroy()
        {
            StageSelect.StageSelectUIPresenter.OnStageSelectedForBattle -= HandleStageSelectedForBattle;
        }

        private void HandleStageSelectedForBattle(Maps.StageDefinitionSO stage)
        {
            TransitionToCombat(stage);
        }

        private Maps.StageDefinitionSO _selectedStage;
        public Maps.StageDefinitionSO SelectedStage => _selectedStage;
        private GameObject _currentInstantiatedMap;

        private void DestroyCurrentMapInstance()
        {
            if (_currentInstantiatedMap != null)
            {
                // Giải phóng an toàn qua Addressables nếu đối tượng được sinh từ Addressables
                bool releasedByAddressables = false;
                try
                {
                    releasedByAddressables = UnityEngine.AddressableAssets.Addressables.ReleaseInstance(_currentInstantiatedMap);
                }
                catch { }

                if (!releasedByAddressables && _currentInstantiatedMap != null)
                {
                    Destroy(_currentInstantiatedMap);
                }
                _currentInstantiatedMap = null;
            }

            // Dọn dẹp cả các map cũ do tool dựng sẵn hoặc spawn trước đó trong Scene nếu có
            string[] mapNames = new string[] { "Environment_SanDinhLangCo", "Map_SanDinhLangCo", "Map_BambooForest", "Map_AncientCitadel", "Map_CinnabarSwamp" };
            foreach (var mName in mapNames)
            {
                var existing = GameObject.Find(mName);
                if (existing != null) Destroy(existing);
                var existingClone = GameObject.Find(mName + "(Clone)");
                if (existingClone != null) Destroy(existingClone);
            }
        }

        public void TransitionToCombat(Maps.StageDefinitionSO stage = null)
        {
            _selectedStage = stage;
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            }
            StartCoroutine(TransitionToCombatRoutine());
        }

        public void StartRun()
        {
            TransitionToCombat(null);
        }

        public void ReturnToMetaHub()
        {
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            }
            StartCoroutine(TransitionToMetaHubRoutine());
        }

        private IEnumerator TransitionToCombatRoutine()
        {
            Time.timeScale = 1f;

            // Nếu có cấu hình ải được chọn, gán Timeline cho SpawnManager
            if (_selectedStage != null && _selectedStage.timelineConfig != null)
            {
                if (Spawners.SpawnManager.Instance != null)
                {
                    Spawners.SpawnManager.Instance.SetTimelineConfig(_selectedStage.timelineConfig);
                }
            }

            if (LoadingScreenPresenter.Instance != null)
            {
                bool loadingFinished = false;

                LoadingScreenPresenter.Instance.ShowTaskLoading(async (reportProgress) =>
                {
                    // 1. (20%) Khởi tạo hoặc tải Map Tilemap từ Addressables
                    string stageMsg = _selectedStage != null ? $"Đang khai mở {_selectedStage.stageName}..." : "Đang triệu hồi chân thân Tướng...";
                    reportProgress?.Invoke(0.2f, stageMsg);

                    // Nếu có chỉ định Addressables Map Key cho Ải, tiến hành dọn map cũ và nạp map mới qua Addressables hoặc Resources
                    if (_selectedStage != null && !string.IsNullOrEmpty(_selectedStage.mapPrefabAddress))
                    {
                        try
                        {
                            // 1. Dọn dẹp map cũ trước đó nếu có
                            DestroyCurrentMapInstance();

                            bool loadedFromAddressables = false;
                            try
                            {
                                var locHandle = UnityEngine.AddressableAssets.Addressables.LoadResourceLocationsAsync(_selectedStage.mapPrefabAddress);
                                await locHandle.Task;
                                if (locHandle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded && locHandle.Result != null && locHandle.Result.Count > 0)
                                {
                                    var handle = UnityEngine.AddressableAssets.Addressables.InstantiateAsync(_selectedStage.mapPrefabAddress);
                                    await handle.Task;
                                    if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                                    {
                                        _currentInstantiatedMap = handle.Result;
                                        Spawners.SpawnManager.Instance?.RefreshMapReferences();
                                        loadedFromAddressables = true;
                                    }
                                }
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogWarning($"[MetaSceneTransitionController] Addressable load failed: {ex.Message}");
                            }

                            // 2. Fallback sang Resources nội bộ trong APK nếu Addressables chưa tải hoặc chạy Offline trên Android
                            if (!loadedFromAddressables)
                            {
                                var mapPrefab = Resources.Load<GameObject>($"Maps/{_selectedStage.mapPrefabAddress}");
                                if (mapPrefab != null)
                                {
                                    _currentInstantiatedMap = Instantiate(mapPrefab);
                                    _currentInstantiatedMap.name = _selectedStage.mapPrefabAddress;
                                    Spawners.SpawnManager.Instance?.RefreshMapReferences();
                                    Debug.Log($"<color=#00FF88>[MetaSceneTransitionController] Đã nạp thành công Map '{_selectedStage.mapPrefabAddress}' từ Resources!</color>");
                                }
                                else
                                {
                                    Debug.Log($"[MetaSceneTransitionController] Không tìm thấy Map '{_selectedStage.mapPrefabAddress}' trong Resources/Maps, sử dụng Tilemap mặc định có sẵn trong Scene.");
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogWarning($"[MetaSceneTransitionController] Map '{_selectedStage.mapPrefabAddress}' không khả dụng: {ex.Message}.");
                        }
                    }

                    if (_gameplayBootstrapper != null)
                    {
                        _gameplayBootstrapper.StartMatchFlow();
                    }
                    await System.Threading.Tasks.Task.Yield();

                    // 2. (50%) Preload Quái vật & Khởi tạo Object Pool ngầm
                    reportProgress?.Invoke(0.5f, "Đang nạp dữ liệu quái vật cõi âm...");
                    if (Spawners.SpawnManager.Instance != null)
                    {
                        await Spawners.SpawnManager.Instance.StartMatchAsync();
                    }
                    await System.Threading.Tasks.Task.Yield();

                    // 3. (80%) Nạp sẵn Database Thẻ Nâng Cấp, VFX & Nhạc Trận Đấu
                    reportProgress?.Invoke(0.8f, "Đang ngưng tụ linh khí ngũ hành...");
                    Upgrades.UpgradeManager.Instance?.AutoPopulateUpgradesIfEmpty();
                    
                    // Phát trước BGM Trận Đấu ngầm trong Loading Screen để tránh LoadFMODSound lúc vào trận
                    var phaseAudio = FindObjectOfType<global::Core.Audio.PhaseAudioController>();
                    if (phaseAudio != null)
                    {
                        phaseAudio.ForceInitialPhaseAudio();
                    }
                    await System.Threading.Tasks.Task.Yield();

                    // 4. (100%) Chuyển trạng thái Game sang Playing
                    reportProgress?.Invoke(1.0f, "Chiến trường đã sẵn sàng!");
                    if (GameStateManager.Instance != null)
                    {
                        GameStateManager.Instance.ChangeState(GameState.Playing);
                    }
                }, () =>
                {
                    ApplyStateVisuals(false);
                    loadingFinished = true;
                }, "Đang khai mở cửa Hoàng Tuyền...");

                while (!loadingFinished) yield return null;
            }
            else
            {
                // 1. Fade Out tối dần
                yield return StartCoroutine(FadeRoutine(0f, 1f, 0.25f));

                // 2. Chuyển UI
                ApplyStateVisuals(false);

                // 3. Bắt đầu trận đấu
                if (_gameplayBootstrapper != null)
                {
                    _gameplayBootstrapper.StartMatchFlow();
                }
                if (Spawners.SpawnManager.Instance != null)
                {
                    _ = Spawners.SpawnManager.Instance.StartMatchAsync();
                }
                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.ChangeState(GameState.Playing);
                }

                // 4. Fade In sáng lại
                yield return StartCoroutine(FadeRoutine(1f, 0f, 0.25f));
            }
        }

        private IEnumerator TransitionToMetaHubRoutine()
        {
            Time.timeScale = 1f;

            // 1. Fade Out tối dần
            yield return StartCoroutine(FadeRoutine(0f, 1f, 0.25f));

            // 2. Dọn sạch quái, projectiles, VFX và reset Player đứng tại Sảnh
            if (_spawnManager != null)
            {
                _spawnManager.StopMatchAndClearAllEnemies();
            }

            // Dọn sạch toàn bộ đạn / pháp bảo đang bay
            ProjectZombie.Features.Projectiles.Core.ProjectileSystem.Instance?.DespawnAllProjectiles();

            // Dọn sạch các hiệu ứng VFX / Particle đang phát
            ProjectZombie.Features.Shared.VFX.GlobalVFXPoolManager.Instance?.ClearAllActiveEffects();
            ProjectZombie.Core.Pooling.VFXPoolManager.ClearPools();

            // Dọn sạch các vùng Trận Đồ / Zone Decals còn sót lại
            var allZones = FindObjectsOfType<ProjectZombie.Features.Skills.Zones.BatQuaiTranZone>();
            for (int i = 0; i < allZones.Length; i++)
            {
                if (allZones[i] != null && allZones[i].gameObject != null) Destroy(allZones[i].gameObject);
            }

            if (_gameplayBootstrapper != null)
            {
                _gameplayBootstrapper.ResetPlayerToHub();
            }

            // 3. Chuyển UI về Meta Hub
            ApplyStateVisuals(true);

            if (_metaUIManager != null)
            {
                _metaUIManager.OpenScreen(MetaScreenType.MainHub);
            }
            else if (MetaUIManager.Instance != null)
            {
                MetaUIManager.Instance.OpenScreen(MetaScreenType.MainHub);
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(GameState.MainMenu);
            }

            // 4. Fade In sáng lại
            yield return StartCoroutine(FadeRoutine(1f, 0f, 0.25f));
        }

        private IEnumerator FadeRoutine(float fromAlpha, float toAlpha, float duration)
        {
            if (_fadeOverlayCanvasGroup == null) yield break;

            _fadeOverlayCanvasGroup.gameObject.SetActive(true);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _fadeOverlayCanvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration);
                yield return null;
            }

            _fadeOverlayCanvasGroup.alpha = toAlpha;
            if (Mathf.Approximately(toAlpha, 0f))
            {
                _fadeOverlayCanvasGroup.gameObject.SetActive(false);
            }
        }

        private void ApplyStateVisuals(bool isMeta)
        {
            if (_metaUIManager != null)
            {
                _metaUIManager.SetMetaCanvasActive(isMeta);
            }
            else if (MetaUIManager.Instance != null)
            {
                MetaUIManager.Instance.SetMetaCanvasActive(isMeta);
            }

            if (_gameplayUIManager != null)
            {
                _gameplayUIManager.SetGameplayCanvasActive(!isMeta);
            }
            else if (GameplayUIManager.Instance != null)
            {
                GameplayUIManager.Instance.SetGameplayCanvasActive(!isMeta);
            }

            if (_spawnManager != null)
            {
                _spawnManager.gameObject.SetActive(!isMeta);
            }

            if (_fadeOverlayCanvasGroup != null)
            {
                _fadeOverlayCanvasGroup.alpha = 0f;
                _fadeOverlayCanvasGroup.gameObject.SetActive(false);
            }
        }
    }
}
