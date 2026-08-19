using ProjectZombie.Features.Shared;
using UnityEngine;

namespace ProjectZombie.Features.Enemies.Visuals
{
    /// <summary>
    /// Component quản lý hiệu ứng nháy sáng (Hit Flash) cho quái vật và Boss khi nhận sát thương.
    /// Hoạt động trực tiếp với Shader "ProjectZombie/Sprite_HitFlash" qua MaterialPropertyBlock.
    /// Đảm bảo Zero-GC Allocation và tối ưu CPU (tự động tắt Update khi hết nháy).
    /// </summary>
    [DisallowMultipleComponent]
    public class HitFlashFeedback : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Thời gian nháy sáng mỗi lần trúng đòn (giây)")]
        [SerializeField] private float _flashDuration = 0.06f;

        [Header("Flash Colors")]
        [SerializeField] private Color _normalFlashColor = Color.white; // #FFFFFF
        [SerializeField] private Color _counterFlashColor = new Color(1f, 0.84f, 0f, 1f); // #FFD700 (Vàng Kim Tương Khắc)
        [SerializeField] private Color _critFlashColor = new Color(1f, 0.35f, 0.1f, 1f); // #FF591A (Đỏ Cam Chí Mạng)

        [Header("Target Renderers")]
        [SerializeField] private SpriteRenderer[] _spriteRenderers;

        private HealthSystem _healthSystem;
        private MaterialPropertyBlock _propBlock;
        private float _flashEndTime;
        private bool _isFlashing;

        private static readonly int _FlashColorId = Shader.PropertyToID("_FlashColor");
        private static readonly int _FlashAmountId = Shader.PropertyToID("_FlashAmount");

        private void Awake()
        {
            _healthSystem = GetComponent<HealthSystem>();
            if (_healthSystem == null)
            {
                _healthSystem = GetComponentInParent<HealthSystem>();
            }

            if (_spriteRenderers == null || _spriteRenderers.Length == 0)
            {
                _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            }

            _propBlock = new MaterialPropertyBlock();

            // Tắt Update mặc định để tiết kiệm CPU
            enabled = false;
        }

        private void OnEnable()
        {
            if (_healthSystem != null)
            {
                _healthSystem.OnDamageTaken += HandleDamageTaken;
            }
        }

        private void OnDisable()
        {
            if (_healthSystem != null)
            {
                _healthSystem.OnDamageTaken -= HandleDamageTaken;
            }

            ResetFlash();
        }

        /// <summary>
        /// Xử lý sự kiện nhận sát thương từ HealthSystem và chọn màu Flash phù hợp.
        /// </summary>
        private void HandleDamageTaken(DamageData damageData)
        {
            Color flashColor = _normalFlashColor;

            if (damageData.IsCounter)
            {
                flashColor = _counterFlashColor;
            }
            else if (damageData.IsCritical)
            {
                flashColor = _critFlashColor;
            }

            TriggerFlash(flashColor);
        }

        /// <summary>
        /// Kích hoạt nháy sáng với màu sắc tùy chọn.
        /// </summary>
        public void TriggerFlash(Color color)
        {
            if (_spriteRenderers == null || _spriteRenderers.Length == 0) return;

            _flashEndTime = Time.time + _flashDuration;
            _isFlashing = true;

            ApplyFlashProperties(1.0f, color);

            // Bật Update để theo dõi đếm ngược thời gian
            enabled = true;
        }

        private void Update()
        {
            if (!_isFlashing)
            {
                enabled = false;
                return;
            }

            if (Time.time >= _flashEndTime)
            {
                ResetFlash();
                enabled = false;
            }
        }

        private void ApplyFlashProperties(float amount, Color color)
        {
            if (_spriteRenderers == null) return;

            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                var sr = _spriteRenderers[i];
                if (sr == null) continue;

                sr.GetPropertyBlock(_propBlock);
                _propBlock.SetFloat(_FlashAmountId, amount);
                _propBlock.SetColor(_FlashColorId, color);
                sr.SetPropertyBlock(_propBlock);
            }
        }

        private void ResetFlash()
        {
            _isFlashing = false;

            if (_spriteRenderers == null) return;

            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                var sr = _spriteRenderers[i];
                if (sr == null) continue;

                sr.GetPropertyBlock(_propBlock);
                _propBlock.SetFloat(_FlashAmountId, 0.0f);
                sr.SetPropertyBlock(_propBlock);
            }
        }
    }
}
