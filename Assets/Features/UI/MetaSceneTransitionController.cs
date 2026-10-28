using System;
using System.Collections;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.Loadout;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Bộ điều phối chuyển cảnh giữa Sảnh Menu (Meta Hub) và Chiến trường Defense 3D.
    /// Đã được tinh giản trong G6: Loại bỏ hoàn toàn các hook và tham chiếu tới gameplay 2D cũ.
    /// </summary>
    public class MetaSceneTransitionController : MonoBehaviour
    {
        public static MetaSceneTransitionController Instance { get; private set; }

        [Header("Canvas Group Overlay")]
        [SerializeField] private CanvasGroup _fadeOverlayCanvasGroup;

        [Header("UI Subsystems")]
        [SerializeField] private MetaUIManager _metaUIManager;

        [Header("Systems")]
        [SerializeField] private MainHubPresenter _mainHubPresenter;

        private Maps.StageDefinitionSO _selectedStage;
        public Maps.StageDefinitionSO SelectedStage => _selectedStage;

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
            if (_mainHubPresenter == null) _mainHubPresenter = MainHubPresenter.Instance;

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

            if (LoadingScreenPresenter.Instance != null)
            {
                bool loadingFinished = false;

                LoadingScreenPresenter.Instance.ShowTaskLoading(async (reportProgress) =>
                {
                    // Nạp cấu hình Loadout hiện tại từ Repository
                    var repo = new DefenseLoadoutRepository();
                    var dto = repo.Load();
                    var snapshot = new DefenseLoadoutSnapshot(
                        dto.HeroId, dto.RelicId, dto.MinionIds, dto.TowerIds);

                    // Điều hướng sang Scene Defense 3D ("MapSongQue3D_Prototype")
                    await DefenseMatchNavigation.LaunchDefenseMatchAsync(snapshot, reportProgress);
                }, () =>
                {
                    ApplyStateVisuals(false);
                    loadingFinished = true;
                }, "Đang khai mở chiến trường Sông Quê 3D...");

                while (!loadingFinished) yield return null;
            }
            else
            {
                // 1. Fade Out tối dần
                yield return StartCoroutine(FadeRoutine(0f, 1f, 0.25f));

                // 2. Chuyển UI
                ApplyStateVisuals(false);

                // 3. Bắt đầu trận đấu Defense 3D
                var repo = new DefenseLoadoutRepository();
                var dto = repo.Load();
                var snapshot = new DefenseLoadoutSnapshot(
                    dto.HeroId, dto.RelicId, dto.MinionIds, dto.TowerIds);
                _ = DefenseMatchNavigation.LaunchDefenseMatchAsync(snapshot);

                // 4. Fade In sáng lại
                yield return StartCoroutine(FadeRoutine(1f, 0f, 0.25f));
            }
        }

        private IEnumerator TransitionToMetaHubRoutine()
        {
            Time.timeScale = 1f;

            // 1. Fade Out tối dần
            yield return StartCoroutine(FadeRoutine(0f, 1f, 0.25f));

            // 2. Nếu đang trong phòng Multiplayer Co-op, dọn dẹp nhân vật mạng
            if (ProjectZombie.Core.Architecture.ServiceContext.TryGet<Multiplayer.Core.INetworkSessionService>(out var netSession) && netSession.IsInRoom)
            {
                if (netSession is Multiplayer.Core.PhotonFusionSessionService)
                {
                    var spawner = FindObjectOfType<Multiplayer.Core.NetworkPlayerSpawner>();
                    if (spawner != null)
                    {
                        spawner.StopMatch();
                    }
                }
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
