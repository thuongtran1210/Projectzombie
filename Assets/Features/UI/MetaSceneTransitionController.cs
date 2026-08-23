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

        [Header("Canvas Roots")]
        [SerializeField] private CanvasGroup _metaMenuCanvasGroup;
        [SerializeField] private CanvasGroup _gameplayCanvasGroup;
        [SerializeField] private CanvasGroup _fadeOverlayCanvasGroup;

        [Header("Systems")]
        [SerializeField] private MainHubPresenter _mainHubPresenter;
        [SerializeField] private SpawnManager _spawnManager;

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

            if (_mainHubPresenter == null) _mainHubPresenter = FindObjectOfType<MainHubPresenter>(true);
            if (_spawnManager == null) _spawnManager = FindObjectOfType<SpawnManager>(true);
        }

        private void Start()
        {
            if (_mainHubPresenter != null)
            {
                _mainHubPresenter.OnStartRunRequested += StartRun;
            }

            // Đồng bộ trạng thái khởi đầu
            if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.MainMenu)
            {
                ApplyStateVisuals(true);
            }
            else
            {
                ApplyStateVisuals(false);
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
            // 1. Fade Out tối dần
            yield return StartCoroutine(FadeRoutine(0f, 1f, 0.25f));

            // 2. Chuyển UI
            ApplyStateVisuals(false);

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(GameState.Playing);
            }

            // 3. Fade In sáng lại
            yield return StartCoroutine(FadeRoutine(1f, 0f, 0.25f));
        }

        private IEnumerator TransitionToMetaHubRoutine()
        {
            // 1. Fade Out tối dần
            yield return StartCoroutine(FadeRoutine(0f, 1f, 0.25f));

            // 2. Chuyển UI về Meta
            ApplyStateVisuals(true);

            if (MetaUIManager.Instance != null)
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
            if (_metaMenuCanvasGroup != null)
            {
                _metaMenuCanvasGroup.gameObject.SetActive(isMeta);
                _metaMenuCanvasGroup.alpha = isMeta ? 1f : 0f;
                _metaMenuCanvasGroup.blocksRaycasts = isMeta;
                _metaMenuCanvasGroup.interactable = isMeta;
            }

            if (_gameplayCanvasGroup != null)
            {
                _gameplayCanvasGroup.gameObject.SetActive(!isMeta);
                _gameplayCanvasGroup.alpha = !isMeta ? 1f : 0f;
                _gameplayCanvasGroup.blocksRaycasts = !isMeta;
                _gameplayCanvasGroup.interactable = !isMeta;
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
