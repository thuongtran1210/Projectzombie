using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.MetaProgression;

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

        [Header("Audio Settings")]
        [Tooltip("Âm thanh báo hiệu kết thúc trận")]
        [SerializeField] private AudioClip gameOverStingerClip;
        [Tooltip("Âm thanh nhảy số Cổ Tiền")]
        [SerializeField] private AudioClip coinTickClip;

        [Header("Player Model Reference")]
        [SerializeField] private HealthSystem playerHealth;

        private int _currencyEarned;
        private float _currencyDisplayTimer;
        private int _lastPlayedCoinTick = -1;
        private bool _animatingCurrency;
        private bool _lastIsVictory;
        private AudioSource _uiAudioSource;

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

        private void Awake()
        {
            if (view == null)
            {
                view = GetComponent<GameOverScreenView>();
            }

            _uiAudioSource = GetComponent<AudioSource>();
            if (_uiAudioSource == null)
            {
                _uiAudioSource = gameObject.AddComponent<AudioSource>();
            }
            _uiAudioSource.playOnAwake = false;
            _uiAudioSource.ignoreListenerPause = true;

            PlayerLogic.OnPlayerDeathSequenceCompleted -= HandleDeathSequenceCompleted;
            PlayerLogic.OnPlayerDeathSequenceCompleted += HandleDeathSequenceCompleted;
        }

        private void Start()
        {
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

            PlayerLogic.OnPlayerDeathSequenceCompleted -= HandleDeathSequenceCompleted;

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
            // Nếu Player có PlayerLogic điều phối Death Sequence thì chờ sự kiện OnPlayerDeathSequenceCompleted
            if (playerHealth != null && playerHealth.GetComponent<PlayerLogic>() != null)
            {
                return;
            }

            // Fallback khi không có PlayerLogic trong entity
            Show(isVictory: false);
        }

        private void HandleDeathSequenceCompleted()
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

            view.SetCurrency(displayed);

            // Phát âm thanh cheng cheng mỗi khi số coin tăng lên
            if (displayed != _lastPlayedCoinTick && coinTickClip != null && _uiAudioSource != null)
            {
                _lastPlayedCoinTick = displayed;
                _uiAudioSource.pitch = Random.Range(0.95f, 1.05f);
                _uiAudioSource.PlayOneShot(coinTickClip, 0.7f);
            }

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

            // Phát âm thanh Stinger kết thúc trận
            if (gameOverStingerClip != null && _uiAudioSource != null)
            {
                _uiAudioSource.pitch = 1.0f;
                _uiAudioSource.PlayOneShot(gameOverStingerClip, 0.85f);
            }

            // Thiết lập tiêu đề & Banner Cổ Phong
            string titleText = isVictory ? "CHIẾN THẮNG!" : "ĐÃ NGÃ XUỐNG";
            Color titleColor = isVictory ? victoryColor : defeatColor;
            view.SetTitle(titleText, titleColor);

            #if UNITY_EDITOR
            string bannerPath = isVictory ? "Assets/Art/UI/Badges/Banner_GameOver_Victory.png" : "Assets/Art/UI/Badges/Banner_GameOver_Defeat.png";
            Sprite bannerSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(bannerPath);
            if (bannerSprite != null) view.SetBanner(bannerSprite);
            #endif

            var tracker = RunStatsTracker.Instance;
            if (tracker == null)
            {
                Debug.LogWarning("[GameOverScreenPresenter] RunStatsTracker.Instance là null — không có dữ liệu thống kê.");
                return;
            }

            // Gửi dữ liệu đã định dạng xuống View (Chỉ gửi số hoặc giá trị sạch)
            view.SetTimeAlive(tracker.GetFormattedTime());
            view.SetKillCount($"{tracker.KillCount}");
            view.SetMaxLevel($"Lv.{tracker.MaxLevelReached}");
            view.SetDamageDealt($"{tracker.TotalDamageDealt:N0}");

            // Tính toán Currency nhận được & Ghi nhận tiến trình lưu game
            _currencyEarned = tracker.CalculateMetaCurrency(isVictory);
            if (ProjectZombie.Core.Save.GameManager.Instance != null)
            {
                ProjectZombie.Core.Save.GameManager.Instance.OnRunCompleted(tracker.ElapsedTime, tracker.KillCount, _currencyEarned);
            }
            else if (MetaCurrencyManager.Instance != null)
            {
                MetaCurrencyManager.Instance.AddCurrency(_currencyEarned);
            }

            // Kích hoạt animation đếm số hoặc hiển thị trực tiếp
            if (animateCurrencyCount && _currencyEarned > 0)
            {
                _currencyDisplayTimer = 0f;
                _animatingCurrency = true;
            }
            else
            {
                view.SetCurrency($"+{_currencyEarned} Cổ Tiền");
            }
        }

        private void HandlePlayAgain()
        {
            Time.timeScale = 1f;
            if (view != null) view.SetActive(false);

            if (MetaSceneTransitionController.Instance != null)
            {
                MetaSceneTransitionController.Instance.StartRun();
            }
            else
            {
                SceneManager.LoadScene(gameSceneName);
            }
        }

        private void HandleMainMenu()
        {
            Time.timeScale = 1f;
            if (view != null) view.SetActive(false);

            if (MetaSceneTransitionController.Instance != null)
            {
                MetaSceneTransitionController.Instance.ReturnToMetaHub();
            }
            else
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }
    }
}
