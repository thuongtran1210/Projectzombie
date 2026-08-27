using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.UI.HUD;
using ProjectZombie.Features.UI.StatsAndSkills;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Bộ điều phối trung tâm quản lý toàn bộ phân hệ UI trong trận (Gameplay Canvas).
    /// Hoạt động theo mô hình State-Driven dựa trên GameStateManager, tách bạch rõ ràng với MetaUIManager.
    /// </summary>
    public class GameplayUIManager : MonoBehaviour
    {
        public static GameplayUIManager Instance { get; private set; }

        [Header("Canvas Group Root")]
        [SerializeField] private CanvasGroup _gameplayCanvasGroup;

        [Header("UI Panels & Screens")]
        [SerializeField] private GameObject _runHUDPanel;
        [SerializeField] private GameObject _mobileControlsPanel;
        [SerializeField] private UpgradeUIView _upgradeUIView;
        [SerializeField] private GameOverScreenView _gameOverScreenView;

        public bool IsInGameplay => _gameplayCanvasGroup != null && _gameplayCanvasGroup.gameObject.activeSelf;

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

            if (_gameplayCanvasGroup == null)
            {
                _gameplayCanvasGroup = GetComponent<CanvasGroup>();
            }
        }

        private void Start()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnStateChanged += HandleGameStateChanged;
                HandleGameStateChanged(GameStateManager.Instance.CurrentState);
            }
        }

        private void OnDestroy()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnStateChanged -= HandleGameStateChanged;
            }
        }

        /// <summary>
        /// Đồng bộ hiển thị của toàn bộ phân hệ UI Gameplay dựa trên trạng thái trò chơi.
        /// </summary>
        public void HandleGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    SetGameplayCanvasActive(false);
                    break;

                case GameState.Playing:
                    SetGameplayCanvasActive(true);
                    SetHUDActive(true);
                    SetMobileControlsActive(true);
                    break;

                case GameState.Paused:
                case GameState.LevelUpSelection:
                    SetGameplayCanvasActive(true);
                    // Giữ HUD hiển thị nhưng khóa/ẩn cụm điều khiển khi đang mở popup
                    SetMobileControlsActive(false);
                    break;

                case GameState.GameOver:
                    SetGameplayCanvasActive(true);
                    SetMobileControlsActive(false);
                    break;
            }
        }

        /// <summary>
        /// Bật/tắt CanvasGroup Gameplay gốc.
        /// </summary>
        public void SetGameplayCanvasActive(bool isActive)
        {
            if (_gameplayCanvasGroup == null) _gameplayCanvasGroup = GetComponent<CanvasGroup>();

            if (_gameplayCanvasGroup != null)
            {
                _gameplayCanvasGroup.gameObject.SetActive(isActive);
                _gameplayCanvasGroup.alpha = isActive ? 1f : 0f;
                _gameplayCanvasGroup.blocksRaycasts = isActive;
                _gameplayCanvasGroup.interactable = isActive;
            }

            SetHUDActive(isActive);
            SetMobileControlsActive(isActive);
        }

        public void SetHUDActive(bool isActive)
        {
            if (_runHUDPanel == null)
            {
                var hudView = GetComponentInChildren<RunHUDView>(true);
                if (hudView != null) _runHUDPanel = hudView.gameObject;
            }

            if (_runHUDPanel != null)
            {
                _runHUDPanel.SetActive(isActive);
            }
        }

        public void SetMobileControlsActive(bool isActive)
        {
            if (_mobileControlsPanel != null)
            {
                _mobileControlsPanel.SetActive(isActive);
            }
        }
    }
}
