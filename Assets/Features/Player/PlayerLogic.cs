using System;
using System.Collections;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Arena;
using ProjectZombie.Features.Weapons;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// Quản lý logic vòng đời và chuỗi hiệu ứng tử trận (Cinematic Death Sequence) của người chơi.
    /// </summary>
    [RequireComponent(typeof(HealthSystem))]
    public class PlayerLogic : MonoBehaviour
    {
        [Header("Death Sequence Settings")]
        [Tooltip("Thời lượng chạy hoạt ảnh tử trận và slow-motion (giây thực tế - unscaled)")]
        [SerializeField] private float deathSequenceDuration = 1.8f;

        [Tooltip("Mức độ zoom của Camera vào nhân vật (Orthographic Size cận cảnh)")]
        [SerializeField] private float deathZoomOrthoSize = 2.5f;

        [Tooltip("Tốc độ thời gian thấp nhất trong lúc slow-motion")]
        [SerializeField] private float targetSlowMotionScale = 0.15f;

        [Header("Audio Settings")]
        [Tooltip("Âm thanh Chuông Chiêng Tử Trận ngân vang")]
        [SerializeField] private AudioClip deathGongClip;
        [SerializeField] private global::Core.Audio.AudioConfigSO deathAudioConfig;

        private HealthSystem _healthSystem;
        private PlayerAnimator _playerAnimator;
        private PlayerController _playerController;
        private WeaponManager _weaponManager;
        private Collider2D _collider;

        private bool _isDying = false;

        /// <summary>
        /// Sự kiện bắn ra khi chuỗi hoạt ảnh tử trận và zoom camera hoàn tất, sẵn sàng mở bảng Game Over.
        /// </summary>
        public static event Action OnPlayerDeathSequenceCompleted;

        private void Awake()
        {
            _healthSystem = GetComponent<HealthSystem>();
            _playerAnimator = GetComponentInChildren<PlayerAnimator>();
            _playerController = GetComponent<PlayerController>();
            _weaponManager = GetComponent<WeaponManager>();
            _collider = GetComponent<Collider2D>();

            if (_healthSystem != null)
            {
                // Ngăn HealthSystem ẩn GameObject để player kịp chạy animation tử trận
                _healthSystem.DisableGameObjectOnDeath = false;
                _healthSystem.OnDied += HandlePlayerDeath;
            }
        }

        private void OnDestroy()
        {
            if (_healthSystem != null)
            {
                _healthSystem.OnDied -= HandlePlayerDeath;
            }
        }

        private void HandlePlayerDeath()
        {
            if (_isDying) return;
            _isDying = true;

            Debug.Log("[PlayerLogic] Bắt đầu Cinematic Death Sequence...");

            // 1. Vô hiệu hóa điều khiển di chuyển
            if (_playerController != null)
            {
                _playerController.enabled = false;
            }

            // 2. Tắt hệ thống vũ khí để ngừng bắn tự động
            if (_weaponManager != null)
            {
                _weaponManager.enabled = false;
            }

            // 3. Tắt Collider để kẻ địch không tiếp tục va chạm/đẩy xác
            if (_collider != null)
            {
                _collider.enabled = false;
            }

            // 4. Kích hoạt hoạt ảnh gục ngã / tử trận
            if (_playerAnimator != null)
            {
                _playerAnimator.ChangeAnimationState(PlayerAnimationState.Dead);
            }

            // 5. Phát âm thanh Chuông Chiêng Tử Trận
            if (deathAudioConfig != null && global::Core.Audio.AudioManager.Instance != null)
            {
                global::Core.Audio.AudioManager.Instance.PlaySound(deathAudioConfig, transform.position);
            }
            else if (deathGongClip != null)
            {
                AudioSource.PlayClipAtPoint(deathGongClip, transform.position);
            }

            // 6. Khởi chạy Coroutine điều phối Slow-motion và Camera Zoom cận cảnh
            StartCoroutine(DeathSequenceCoroutine());
        }

        private IEnumerator DeathSequenceCoroutine()
        {
            // Yêu cầu Camera zoom vào vị trí nhân vật
            if (CameraFollow.Instance != null)
            {
                CameraFollow.Instance.ZoomTo(deathZoomOrthoSize, deathSequenceDuration);
            }

            float initialTimeScale = Time.timeScale;
            float elapsed = 0f;

            // Làm chậm thời gian mượt mà từ initialTimeScale về targetSlowMotionScale
            while (elapsed < deathSequenceDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / deathSequenceDuration);
                
                // Giảm dần timeScale theo đường cong mượt
                Time.timeScale = Mathf.Lerp(initialTimeScale, targetSlowMotionScale, progress);

                yield return null;
            }

            // Dừng hoàn toàn thời gian khi hoạt ảnh kết thúc
            Time.timeScale = 0f;

            Debug.Log("[PlayerLogic] Hoàn tất Death Sequence -> Thông báo mở Panel Game Over.");
            OnPlayerDeathSequenceCompleted?.Invoke();
        }
    }
}

