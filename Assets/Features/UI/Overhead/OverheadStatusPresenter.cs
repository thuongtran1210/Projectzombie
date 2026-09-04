// ============================================================================
// FILE: OverheadStatusPresenter.cs — TẦNG PRESENTER (MVP)
// Trách nhiệm: Lắng nghe Model events (HealthSystem, PlayerExperience),
// đẩy dữ liệu sang OverheadStatusView để render.
// Hỗ trợ tái sử dụng cho cả Player, Enemy và Boss.
// ============================================================================

using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.UI.Overhead
{
    /// <summary>
    /// Presenter điều phối dữ liệu từ HealthSystem và PlayerExperience sang OverheadStatusView.
    /// </summary>
    public class OverheadStatusPresenter : MonoBehaviour
    {
        [Header("View Reference")]
        [SerializeField] private OverheadStatusView _view;

        [Header("Model References (Optional / Inspector Binding)")]
        [SerializeField] private HealthSystem _healthSystem;
        [SerializeField] private PlayerExperience _playerExperience;

        private bool _isConstructed = false;

        private void Awake()
        {
            if (_view == null)
            {
                _view = GetComponent<OverheadStatusView>();
                if (_view == null) _view = GetComponentInChildren<OverheadStatusView>(true);
            }
        }

        private void Start()
        {
            // Tự động tìm kiếm nếu chưa được inject từ GameplayUIBinder / Factory
            if (!_isConstructed)
            {
                if (_healthSystem == null)
                {
                    _healthSystem = GetComponentInParent<HealthSystem>();
                }

                if (_playerExperience == null)
                {
                    _playerExperience = GetComponentInParent<PlayerExperience>();
                }

                if (_healthSystem != null)
                {
                    Construct(_healthSystem, _playerExperience);
                }
            }
        }

        /// <summary>
        /// Inject dependencies từ PlayerContext hoặc EnemySpawner.
        /// </summary>
        /// <param name="health">Bắt buộc: Hệ thống máu</param>
        /// <param name="experience">Tùy chọn: Kinh nghiệm/Level (chỉ dành cho Player)</param>
        public void Construct(HealthSystem health, PlayerExperience experience = null)
        {
            if (_isConstructed)
            {
                UnsubscribeEvents();
            }

            _healthSystem = health;
            _playerExperience = experience;

            SubscribeEvents();
            RefreshAll();

            _isConstructed = true;
        }

        private void OnEnable()
        {
            if (_isConstructed)
            {
                SubscribeEvents();
                RefreshAll();
            }
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (_healthSystem != null)
            {
                _healthSystem.OnHealthChanged += HandleHealthChanged;
                _healthSystem.OnDied += HandleDeath;
            }

            if (_playerExperience != null)
            {
                _playerExperience.OnLevelUp += HandleLevelUp;
                _playerExperience.OnExpChanged += HandleExpChanged;
            }
        }

        private void UnsubscribeEvents()
        {
            if (_healthSystem != null)
            {
                _healthSystem.OnHealthChanged -= HandleHealthChanged;
                _healthSystem.OnDied -= HandleDeath;
            }

            if (_playerExperience != null)
            {
                _playerExperience.OnLevelUp -= HandleLevelUp;
                _playerExperience.OnExpChanged -= HandleExpChanged;
            }
        }

        private void RefreshAll()
        {
            if (_view == null) return;

            if (_healthSystem != null)
            {
                _view.SetHealth(_healthSystem.CurrentHealth, _healthSystem.MaxHealth);
            }

            if (_playerExperience != null)
            {
                _view.SetLevelVisible(true);
                _view.SetLevel(_playerExperience.CurrentLevel);
                _view.SetExp(_playerExperience.CurrentExp, _playerExperience.MaxExp);
            }
            else
            {
                // Nếu không có PlayerExperience (vd: là Enemy/Boss), ẩn badge level
                _view.SetLevelVisible(false);
            }
        }

        private void HandleHealthChanged(float currentHealth, float maxHealth)
        {
            if (_view != null)
            {
                _view.SetHealth(currentHealth, maxHealth);
            }
        }

        private void HandleLevelUp(int newLevel)
        {
            if (_view != null)
            {
                _view.SetLevel(newLevel);
                if (_playerExperience != null)
                {
                    _view.SetExp(_playerExperience.CurrentExp, _playerExperience.MaxExp);
                }
            }
        }

        private void HandleExpChanged(float currentExp, float maxExp)
        {
            if (_view != null)
            {
                _view.SetExp(currentExp, maxExp);
            }
        }

        private void HandleDeath()
        {
            if (_view != null)
            {
                _view.SetHealth(0f, _healthSystem != null ? _healthSystem.MaxHealth : 1f);
            }
        }
    }
}
