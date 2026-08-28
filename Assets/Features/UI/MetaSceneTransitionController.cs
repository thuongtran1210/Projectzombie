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

            // Đăng ký lắng nghe sự kiện Xuất Trận từ Presenter
            if (_mainHubPresenter != null)
            {
                _mainHubPresenter.OnStartRunRequested -= StartRun;
                _mainHubPresenter.OnStartRunRequested += StartRun;
            }
        }

        private void OnDestroy()
        {
            if (_mainHubPresenter != null)
            {
                _mainHubPresenter.OnStartRunRequested -= StartRun;
            }
        }

        public void StartRun()
        {
            StartCoroutine(TransitionToCombatRoutine());
        }

        public void ReturnToMetaHub()
        {
            StartCoroutine(TransitionToMetaHubRoutine());
        }

        private IEnumerator TransitionToCombatRoutine()
        {
            if (LoadingScreenPresenter.Instance != null)
            {
                bool loadingFinished = false;
                LoadingScreenPresenter.Instance.ShowLoading(0.85f, () =>
                {
                    ApplyStateVisuals(false);

                    if (_gameplayBootstrapper != null)
                    {
                        _gameplayBootstrapper.StartMatchFlow();
                    }
                    else if (GameStateManager.Instance != null)
                    {
                        GameStateManager.Instance.ChangeState(GameState.Playing);
                    }
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
                else if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.ChangeState(GameState.Playing);
                }

                // 4. Fade In sáng lại
                yield return StartCoroutine(FadeRoutine(1f, 0f, 0.25f));
            }
        }

        private IEnumerator TransitionToMetaHubRoutine()
        {
            // 1. Fade Out tối dần
            yield return StartCoroutine(FadeRoutine(0f, 1f, 0.25f));

            // 2. Chuyển UI về Meta
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

            // 3. Fade In sáng lại
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
