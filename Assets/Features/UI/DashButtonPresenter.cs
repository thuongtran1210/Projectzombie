using UnityEngine;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Presenter điều phối giữa PlayerController / PlayerStats (Model) và DashButtonView (View).
    /// Tuân thủ MVP (Section 12 Rules): Quản lý vòng đời subscribe/unsubscribe và format dữ liệu trước khi đẩy sang View.
    /// </summary>
    public class DashButtonPresenter : MonoBehaviour
    {
        [Header("View Reference")]
        [SerializeField] private DashButtonView _view;

        [Header("Model References (Optional - Tự Auto-Detect khi Player Spawn)")]
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private PlayerStats _playerStats;

        private float _lastDashTime;
        private float _dashCooldown;

        private void Awake()
        {
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

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.OnButtonClicked -= OnButtonClicked;
            }

            if (_playerController != null)
            {
                _playerController.OnDashed -= OnPlayerDashed;
            }
        }

        private void Update()
        {
            if (_playerStats == null || _playerController == null)
            {
                TryBindPlayer();
                return;
            }

            _dashCooldown = _playerStats.DashCooldown;
            float timePassed = Time.time - _lastDashTime;
            float remaining = Mathf.Max(0f, _dashCooldown - timePassed);

            string formattedText = remaining > 0f ? $"{remaining:F1}s" : string.Empty;
            _view.SetCooldown(remaining, _dashCooldown, formattedText);
            _view.SetInteractable(remaining <= 0f);
        }

        private void TryBindPlayer()
        {
            if (PlayerController.Instance != null)
            {
                _playerController = PlayerController.Instance;
                _playerStats = _playerController.GetComponent<PlayerStats>();

                _playerController.OnDashed -= OnPlayerDashed;
                _playerController.OnDashed += OnPlayerDashed;

                _lastDashTime = _playerController.LastDashTime;
            }
        }

        private void OnPlayerDashed()
        {
            if (_playerController != null)
            {
                _lastDashTime = _playerController.LastDashTime;
            }
        }

        private void OnButtonClicked()
        {
            if (_playerController != null)
            {
                _playerController.PerformDash();
            }
        }
    }
}
