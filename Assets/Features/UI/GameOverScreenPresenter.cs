using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Presenter điều phối dữ liệu cho Màn hình Kết quả Game Over / Chiến thắng.
    /// Lấy dữ liệu từ RunStatsTracker (Model), định dạng thành chuỗi và cập nhật cho View.
    /// </summary>
    public class GameOverScreenPresenter : MonoBehaviour
    {
        [Header("View Reference")]
        [SerializeField] private GameOverScreenView view;

        [Header("Colors")]
        [SerializeField] private Color victoryColor = new Color(1f, 0.84f, 0f);
        [SerializeField] private Color defeatColor = new Color(0.8f, 0.1f, 0.1f);

        [Header("Scene Names")]
        [SerializeField] private string gameSceneName = "GameScene";
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Animation Settings")]
        [SerializeField] private bool animateCurrencyCount = true;

        [Header("Player Model Reference")]
        [SerializeField] private HealthSystem playerHealth;

        private int _currencyEarned;
        private float _currencyDisplayTimer;
        private bool _animatingCurrency;
        private bool _lastIsVictory;

        private bool _isConstructed = false;

        public void Construct(HealthSystem health)
        {
            if (_isConstructed)
            {
                UnsubscribeEvents();
            }

            playerHealth = health;

            SubscribeEvents();

            _isConstructed = true;
        }

        private void Start()
        {
            if (view == null)
            {
                view = GetComponent<GameOverScreenView>();
            }

            if (view != null)
            {
                view.SetActive(false);
                view.OnPlayAgainClicked += HandlePlayAgain;
                view.OnMainMenuClicked += HandleMainMenu;
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnStateChanged += HandleStateChanged;
            }

            // Tương thích ngược: nếu đã kéo thả trong Inspector thì tự động Construct luôn
            if (playerHealth != null)
            {
                Construct(playerHealth);
            }
        }

        private void OnDestroy()
        {
            if (view != null)
            {
                view.OnPlayAgainClicked -= HandlePlayAgain;
                view.OnMainMenuClicked -= HandleMainMenu;
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
            }

            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDied += HandlePlayerDied;
            }
        }

        private void UnsubscribeEvents()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDied -= HandlePlayerDied;
            }
        }

        private void HandlePlayerDied()
        {
            Show(isVictory: false);
        }

        private void HandleStateChanged(GameState newState)
        {
            if (view == null) return;

            if (newState == GameState.GameOver)
            {
                view.SetActive(true);
                
                if (RunStatsTracker.Instance != null)
                {
                    RunStatsTracker.Instance.StopTracking();
                }

                PopulateStats(_lastIsVictory);
            }
            else
            {
                view.SetActive(false);
            }
        }

        private void Update()
        {
            if (!_animatingCurrency || view == null) return;

            _currencyDisplayTimer += Time.unscaledDeltaTime * 50f;
            int displayed = Mathf.Min(Mathf.FloorToInt(_currencyDisplayTimer), _currencyEarned);
            
            view.SetCurrency($"+{displayed} 🪙");

            if (displayed >= _currencyEarned)
            {
                _animatingCurrency = false;
            }
        }

        /// <summary>
        /// Hiển thị màn hình Game Over bằng cách chuyển trạng thái game.
        /// </summary>
        /// <param name="isVictory">Đạt điều kiện thắng lợi (true) hay bị hạ gục (false)</param>
        public void Show(bool isVictory = false)
        {
            _lastIsVictory = isVictory;

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(GameState.GameOver);
            }
            else
            {
                // Fallback hoạt động độc lập nếu không có GameStateManager trong scene
                Time.timeScale = 0f;

                if (view != null)
                {
                    view.SetActive(true);
                }

                if (RunStatsTracker.Instance != null)
                {
                    RunStatsTracker.Instance.StopTracking();
                }

                PopulateStats(isVictory);
            }
        }

        private void PopulateStats(bool isVictory)
        {
            if (view == null) return;

            // Thiết lập tiêu đề
            string titleText = isVictory ? "CHIẾN THẮNG!" : "ĐÃ NGÃ XUỐNG";
            Color titleColor = isVictory ? victoryColor : defeatColor;
            view.SetTitle(titleText, titleColor);

            var tracker = RunStatsTracker.Instance;
            if (tracker == null)
            {
                Debug.LogWarning("[GameOverScreenPresenter] RunStatsTracker.Instance là null — không có dữ liệu thống kê.");
                return;
            }

            // Gửi dữ liệu đã định dạng xuống View
            view.SetTimeAlive($"Thời gian sống: {tracker.GetFormattedTime()}");
            view.SetKillCount($"Zombie đã hạ: {tracker.KillCount}");
            view.SetMaxLevel($"Cấp độ đạt: {tracker.MaxLevelReached}");
            view.SetDamageDealt($"Sát thương gây ra: {tracker.TotalDamageDealt:F0}");

            // Tính toán Currency nhận được
            _currencyEarned = tracker.CalculateMetaCurrency();

            // Kích hoạt animation đếm số hoặc hiển thị trực tiếp
            if (animateCurrencyCount && _currencyEarned > 0)
            {
                _currencyDisplayTimer = 0f;
                _animatingCurrency = true;
            }
            else
            {
                view.SetCurrency($"+{_currencyEarned} 🪙");
            }
        }

        private void HandlePlayAgain()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameSceneName);
        }

        private void HandleMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
