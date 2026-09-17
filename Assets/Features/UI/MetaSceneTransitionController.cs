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
            if (_metaUIManager == null) _metaUIManager = MetaUIManager.Instance;
            if (_gameplayUIManager == null) _gameplayUIManager = GameplayUIManager.Instance;
            if (_mainHubPresenter == null) _mainHubPresenter = MainHubPresenter.Instance;
            if (_spawnManager == null) _spawnManager = SpawnManager.Instance;
            if (_gameplayBootstrapper == null) _gameplayBootstrapper = GameplayBootstrapper.Instance;

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
            MatchFlow.MatchFlowOrchestrator.DestroyCurrentMapInstance();
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
                    // Ủy quyền toàn bộ luồng nạp Map, Preload Quái, Khởi tạo Player và Chuyển State cho MatchFlowOrchestrator
                    await MatchFlow.MatchFlowOrchestrator.ExecuteCombatPreparationAsync(_selectedStage, _gameplayBootstrapper, reportProgress);
                }, () =>
                {
                    ApplyStateVisuals(false);
                    loadingFinished = true;
                    CheckAndPromptInitialMythicCore();
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

                // 5. Mở bảng chọn Lõi Thần Thoại sau khi màn hình sáng lại
                CheckAndPromptInitialMythicCore();
            }
        }

        private void CheckAndPromptInitialMythicCore()
        {
            var player = PlayerProvider.HasPlayer ? PlayerProvider.PlayerGameObject : null;
            PlayerMythicManager mythicMgr = null;
            bool hasMythicMgr = player != null && player.TryGetComponent(out mythicMgr);
            var archetype = mythicMgr != null ? mythicMgr.CurrentArchetype : Upgrades.MythicArchetype.None;

            Debug.Log($"<color=#FFD700>[DIAG_TRANSITION]</color> CheckAndPromptInitialMythicCore: player = {(player != null ? player.name : "NULL")}, hasMythicMgr = {hasMythicMgr}, archetype = {archetype}");

            if (player != null && hasMythicMgr && archetype == Upgrades.MythicArchetype.None)
            {
                if (GameStateManager.Instance != null)
                {
                    Debug.Log("<color=#FFD700>[MetaSceneTransitionController]</color> Khởi đầu trận đấu: Mở bảng chọn 3 Đại Lõi Thần Thoại (Nhập Đạo Arena)!");
                    GameStateManager.Instance.ChangeState(GameState.LevelUpSelection);
                }
                else
                {
                    Debug.LogError("<color=#FFD700>[DIAG_TRANSITION]</color> GameStateManager.Instance is NULL!");
                }
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
            ProjectZombie.Features.Skills.Zones.BatQuaiTranZone.ClearAllZones();

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

            // Quản lý sân khấu xem trước nhân vật: Ẩn và dọn dẹp khi vào trận, bật lại khi về Hub
            if (CharacterPreviewStage.Instance != null)
            {
                CharacterPreviewStage.Instance.SetStageActive(isMeta);
            }

            if (_fadeOverlayCanvasGroup != null)
            {
                _fadeOverlayCanvasGroup.alpha = 0f;
                _fadeOverlayCanvasGroup.gameObject.SetActive(false);
            }
        }
    }
}
