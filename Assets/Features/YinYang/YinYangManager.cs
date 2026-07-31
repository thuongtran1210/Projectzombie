using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.YinYang
{
    public enum YinYangState
    {
        YinDominant, // Âm thịnh (< 20 hoặc nghịch > 80)
        Balanced,    // Cân bằng (40 - 60)
        YangDominant // Dương thịnh (> 80)
    }

    /// <summary>
    /// Quản lý biến trạng thái Cán cân Âm Dương (Yin-Yang Balance).
    /// </summary>
    public class YinYangManager : MonoBehaviour
    {
        public static YinYangManager Instance { get; private set; }

        [SerializeField, Range(0, 100)] 
        private float _yinYangValue = 50f;

        public float CurrentValue => _yinYangValue;

        /// <summary>
        /// Sự kiện phát ra khi trạng thái Âm Dương thay đổi.
        /// </summary>
        public event System.Action<YinYangState> OnYinYangStateChanged;

        /// <summary>
        /// Sự kiện phát ra mỗi khi giá trị Cán cân Âm Dương thay đổi.
        /// </summary>
        public event System.Action<float, YinYangState> OnYinYangValueChanged;

        private YinYangState _currentState = YinYangState.Balanced;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Lấy trạng thái Âm Dương hiện tại theo quy tắc Revised (v6.1):
        /// - Dương thịnh: > 80
        /// - Âm thịnh: < 20 (Nghịch mốc 80)
        /// - Cân bằng: 40 - 60
        /// </summary>
        public YinYangState GetState()
        {
            if (_yinYangValue > 80f) return YinYangState.YangDominant;
            if (_yinYangValue < 20f) return YinYangState.YinDominant;
            if (_yinYangValue >= 40f && _yinYangValue <= 60f) return YinYangState.Balanced;
            return _currentState; // Giữ nguyên trạng thái trung gian nếu nằm trong khoảng 20-40 hoặc 60-80
        }


        private void OnValidate()
        {
            if (Application.isPlaying && Instance == this)
            {
                AdjustValue(0); // Trigger event refresh ngay khi kéo slider trong Inspector
            }
        }

        private bool _isLockedInOverride;
        private float _overrideEndTime;
        private float _overrideTargetValue = 50f;

        private void Update()
        {
            if (_isLockedInOverride)
            {
                if (Time.time < _overrideEndTime)
                {
                    _yinYangValue = Mathf.Lerp(_yinYangValue, _overrideTargetValue, Time.deltaTime * 5f);
                    OnYinYangValueChanged?.Invoke(_yinYangValue, GetState());
                }
                else
                {
                    _isLockedInOverride = false;
                }
            }
        }

        /// <summary>
        /// Ép và giữ giá trị Cán cân Âm Dương về Thái Cực Cân Bằng (50) trong khoảng thời gian duration (Bát Quái Trận Đồ Đạo Sĩ - GDD 3.1.2).
        /// </summary>
        public void SetTemporaryNeutralOverride(float duration, float targetValue = 50f)
        {
            _isLockedInOverride = true;
            _overrideTargetValue = targetValue;
            _overrideEndTime = Time.time + duration;
        }

        /// <summary>
        /// Gán trực tiếp giá trị Cán cân Âm Dương.
        /// </summary>
        public void SetValue(float newValue)
        {
            if (_isLockedInOverride) return;
            float delta = newValue - _yinYangValue;
            AdjustValue(delta);
        }

        /// <summary>
        /// Điều chỉnh giá trị Cán cân Âm Dương và kích hoạt các sự kiện tương ứng.
        /// </summary>
        /// <param name="delta">Lượng thay đổi (+ cho Dương, - cho Âm)</param>
        public void AdjustValue(float delta)
        {
            if (_isLockedInOverride) return;

            _yinYangValue = Mathf.Clamp(_yinYangValue + delta, 0f, 100f);
            
            YinYangState newState = GetState();
            if (newState != _currentState)
            {
                _currentState = newState;
                OnYinYangStateChanged?.Invoke(_currentState);
            }

            OnYinYangValueChanged?.Invoke(_yinYangValue, _currentState);
        }

    }
}
