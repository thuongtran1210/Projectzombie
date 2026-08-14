using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Player.Mechanics;

namespace ProjectZombie.Features.YinYang
{
    /// <summary>
    /// Component theo dõi di chuyển và hành vi chiến đấu của nhân vật Đạo Sĩ (Thanh Đồng)
    /// để điều chỉnh Cán Cân Âm Dương (Yin-Yang Balance).
    /// Hiện thực ICharacterGaugeProvider để cung cấp dữ liệu cơ chế cho CharacterGaugeWidgetPresenter độc lập.
    /// </summary>
    public class TaoistYinYangTracker : MonoBehaviour, ICharacterGaugeProvider
    {
        [Header("Tốc độ tích luỹ điểm")]
        [Tooltip("Điểm Dương nạp mỗi giây khi di chuyển")]
        [SerializeField] private float _yangMoveRate = 1.5f;

        [Tooltip("Điểm Âm nạp mỗi giây khi đứng yên/né tránh phạm vi hẹp")]
        [SerializeField] private float _yinIdleRate = -2.0f;

        [Tooltip("Điểm Dương cộng tức thì khi thực hiện kỹ năng Lướt (Dash)")]
        [SerializeField] private float _dashYangBonus = 3.0f;

        private PlayerController _playerController;

        // ====================================================================
        // ICharacterGaugeProvider Implementation
        // ====================================================================

        public string GaugeTitle => GetFormattedStateTitle(YinYangManager.Instance != null ? YinYangManager.Instance.GetState() : YinYangState.Balanced);
        public float CurrentValue => YinYangManager.Instance != null ? YinYangManager.Instance.CurrentValue : 50f;
        public float MinValue => 0f;
        public float MaxValue => 100f;

        public event System.Action<float, string> OnGaugeValueChanged;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
        }

        private void OnEnable()
        {
            if (_playerController != null)
            {
                _playerController.OnDashed += HandlePlayerDashed;
            }

            if (YinYangManager.Instance != null)
            {
                YinYangManager.Instance.SetTrackerActive(true);
                YinYangManager.Instance.OnYinYangValueChanged += HandleYinYangValueChanged;
            }
        }

        private void Start()
        {
            if (YinYangManager.Instance != null)
            {
                YinYangManager.Instance.SetTrackerActive(true);
                YinYangManager.Instance.OnYinYangValueChanged -= HandleYinYangValueChanged;
                YinYangManager.Instance.OnYinYangValueChanged += HandleYinYangValueChanged;
                
                // Kích hoạt cập nhật ban đầu
                HandleYinYangValueChanged(YinYangManager.Instance.CurrentValue, YinYangManager.Instance.GetState());
            }
        }

        private void OnDisable()
        {
            if (_playerController != null)
            {
                _playerController.OnDashed -= HandlePlayerDashed;
            }

            if (YinYangManager.Instance != null)
            {
                YinYangManager.Instance.OnYinYangValueChanged -= HandleYinYangValueChanged;
                YinYangManager.Instance.SetTrackerActive(false);
            }
        }

        private void Update()
        {
            if (YinYangManager.Instance == null || _playerController == null) return;

            // Kiểm tra trạng thái di chuyển
            if (_playerController.MovementInput.sqrMagnitude > 0.01f || _playerController.IsDashing)
            {
                // Di chuyển liên tục -> Nghiêng Dương
                YinYangManager.Instance.AdjustValue(_yangMoveRate * Time.deltaTime);
            }
            else
            {
                // Đứng yên né tránh -> Nghiêng Âm
                YinYangManager.Instance.AdjustValue(_yinIdleRate * Time.deltaTime);
            }
        }

        private void HandlePlayerDashed()
        {
            if (YinYangManager.Instance != null)
            {
                YinYangManager.Instance.AdjustValue(_dashYangBonus);
            }
        }

        private void HandleYinYangValueChanged(float val, YinYangState state)
        {
            string stateTitle = GetFormattedStateTitle(state);
            OnGaugeValueChanged?.Invoke(val, stateTitle);
        }

        private string GetFormattedStateTitle(YinYangState state)
        {
            return state switch
            {
                YinYangState.YinDominant => "<color=#4A90E2>Âm Thịnh</color>",
                YinYangState.YangDominant => "<color=#FF4444>Dương Thịnh</color>",
                _ => "<color=#FFD700>Thái Cực Cân Bằng</color>"
            };
        }
    }
}
