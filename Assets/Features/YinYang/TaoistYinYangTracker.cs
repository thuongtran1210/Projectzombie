using UnityEngine;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.YinYang
{
    /// <summary>
    /// Component theo dõi di chuyển và hành vi chiến đấu của nhân vật Đạo Sĩ (Thanh Đồng)
    /// để điều chỉnh Cán Cân Âm Dương (Yin-Yang Balance).
    /// Tách rời hoàn toàn khỏi PlayerController để đảm bảo tính module hóa và không gây ảnh hưởng tới các class khác.
    /// </summary>
    public class TaoistYinYangTracker : MonoBehaviour
    {
        [Header("Tốc độ tích luỹ điểm")]
        [Tooltip("Điểm Dương nạp mỗi giây khi di chuyển")]
        [SerializeField] private float _yangMoveRate = 1.5f;

        [Tooltip("Điểm Âm nạp mỗi giây khi đứng yên/né tránh phạm vi hẹp")]
        [SerializeField] private float _yinIdleRate = -2.0f;

        [Tooltip("Điểm Dương cộng tức thì khi thực hiện kỹ năng Lướt (Dash)")]
        [SerializeField] private float _dashYangBonus = 3.0f;

        private PlayerController _playerController;

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

            // Kích hoạt YinYangManager khi Đạo Sĩ xuất hiện
            if (YinYangManager.Instance != null)
            {
                YinYangManager.Instance.SetTrackerActive(true);
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
    }
}
