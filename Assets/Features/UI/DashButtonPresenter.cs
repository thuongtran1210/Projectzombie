using UnityEngine;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Presenter điều phối giữa PlayerController / PlayerStats (Model) và DashButtonView (View).
    /// Tuân thủ MVP (Section 12 Rules): Quản lý vòng đời subscribe/unsubscribe và format dữ liệu trước khi đẩy sang View.
    /// </summary>
    public class DashButtonPresenter : MonoBehaviour, ProjectZombie.Core.Audio.IAudioServiceConsumer
    {
        private ProjectZombie.Core.Audio.IAudioService _audioService;

        public void InjectAudioService(ProjectZombie.Core.Audio.IAudioService audioService)
        {
            _audioService = audioService;
        }
        [Header("View Reference")]
        [SerializeField] private DashButtonView _view;

        [Header("Model References (Optional - Tự Auto-Detect khi Player Spawn)")]
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private PlayerStats _playerStats;

        private float _dashCooldown;

        public static DashButtonPresenter Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            if (_view == null) _view = GetComponent<DashButtonView>();
        }

        private void Start()
        {
            if (_view != null)
            {
                _view.OnButtonClicked += OnButtonClicked;
            }

            TryBindPlayer();
        }

        public void Bind(PlayerController pc, PlayerStats stats)
        {
            if (_playerController != null)
            {
                _playerController.OnDashCooldownUpdated -= RefreshCooldown;
            }

            _playerController = pc;
            _playerStats = stats;

            if (_playerController != null)
            {
                _playerController.OnDashCooldownUpdated += RefreshCooldown;
                RefreshCooldown(_playerController.DashRemainingCooldown, _playerController.DashMaxCooldown);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (_view != null)
            {
                _view.OnButtonClicked -= OnButtonClicked;
            }

            if (_playerController != null)
            {
                _playerController.OnDashCooldownUpdated -= RefreshCooldown;
            }
        }

        private int _lastFormattedTenths = -1;
        private string _lastFormattedCooldownStr = string.Empty;

        private void Update()
        {
            if (Controls.Customization.CustomizableControlButton.IsAnyInEditMode)
            {
                return;
            }

            if (_playerStats == null || _playerController == null)
            {
                TryBindPlayer();
                return;
            }

            RefreshCooldown(_playerController.DashRemainingCooldown, _playerController.DashMaxCooldown);
        }

        private void RefreshCooldown(float remaining, float maximum)
        {
            _dashCooldown = maximum;
            if (_view == null) return;

            int currentTenths = Mathf.CeilToInt(remaining * 10f);
            if (currentTenths != _lastFormattedTenths)
            {
                _lastFormattedTenths = currentTenths;
                _lastFormattedCooldownStr = remaining > 0f ? $"{remaining:F1}s" : string.Empty;
            }

            _view.SetCooldown(remaining, _dashCooldown, _lastFormattedCooldownStr);
            _view.SetInteractable(remaining <= 0f);
        }

        private void TryBindPlayer()
        {
            if (PlayerProvider.HasPlayer && PlayerProvider.PlayerGameObject != null
                && PlayerProvider.PlayerGameObject.TryGetComponent<PlayerController>(out var controller))
            {
                Bind(controller, controller.GetComponent<PlayerStats>());
            }
        }

        private void OnButtonClicked()
        {
            if (Controls.Customization.CustomizableControlButton.IsAnyInEditMode) return;

            if (_playerController == null || _playerController.DashRemainingCooldown > 0f)
            {
                _audioService?.PlayUIError();
                return;
            }

            _audioService?.PlayUIClick();

            // Tuyến đường duy nhất: Gửi Intent qua PlayerInputReader
            if (PlayerProvider.HasPlayer && PlayerProvider.PlayerGameObject != null && PlayerProvider.PlayerGameObject.TryGetComponent<Player.Input.PlayerInputReader>(out var inputReader))
            {
                inputReader.TriggerDash();
                return;
            }

            if (_playerController != null)
            {
                _playerController.PerformDash();
            }
        }
    }
}
