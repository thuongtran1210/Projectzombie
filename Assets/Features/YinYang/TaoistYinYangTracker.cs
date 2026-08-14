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
        public Color GaugeColor => GetStateColor(YinYangManager.Instance != null ? YinYangManager.Instance.GetState() : YinYangState.Balanced);

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
                YinYangState.YinDominant => "<color=#9B51E0><b>☯ ÂM THỊNH</b></color>",
                YinYangState.YangDominant => "<color=#FF8C00><b>☯ DƯƠNG THỊNH</b></color>",
                _ => "<color=#FFD700><b>☯ THÁI CỰC CÂN BẰNG</b></color>"
            };
        }

        private Color GetStateColor(YinYangState state)
        {
            return state switch
            {
                YinYangState.YinDominant => new Color(0.61f, 0.32f, 0.88f, 1f), // #9B51E0 (Tím Ma Mị)
                YinYangState.YangDominant => new Color(1.0f, 0.55f, 0.0f, 1f),   // #FF8C00 (Cam Thái Dương)
                _ => new Color(1.0f, 0.84f, 0.0f, 1f)                           // #FFD700 (Hoàng Kim)
            };
        }
    }
}
