using UnityEngine;

namespace ProjectZombie.Features.Upgrades.Runtimes.Augments
{
    /// <summary>
    /// Runtime cho Lõi Kim Cương [Âm Dương Lưỡng Cực] (AUG_PRIS_LONGTIEN_YIN_YANG):
    /// Luân chuyển giữa Trạng Thái Thái Dương (+50% Sát Thương) và Thái Âm (Hồi máu liên tục) mỗi 8s.
    /// </summary>
    public class LongTienYinYangAugmentRuntime : AugmentRuntimeBase
    {
        [Header("Settings")]
        [SerializeField] private float _phaseDuration = 8.0f;
        [SerializeField] private float _yangDamageBonus = 0.50f;
        [SerializeField] private float _yinHealPerSec = 15f;
        [Header("VFX Settings")]
        [SerializeField] private GameObject _yangAuraVfxPrefab;
        [SerializeField] private GameObject _yinAuraVfxPrefab;

        private bool _isYangPhase = true; // True = Dương (Damage), False = Âm (Heal)
        private float _nextPhaseTime = 0f;

        protected override void OnAugmentInitialized()
        {
            _nextPhaseTime = Time.time + _phaseDuration;
            ApplyPhaseBuff();
        }

        private void Update()
        {
            if (Time.time >= _nextPhaseTime)
            {
                _nextPhaseTime = Time.time + _phaseDuration;
                _isYangPhase = !_isYangPhase;
                ApplyPhaseBuff();
            }

            // Nếu đang ở thể Âm -> Hồi máu theo thời gian
            if (!_isYangPhase && Context?.Health != null)
            {
                Context.Health.Heal(_yinHealPerSec * Time.deltaTime);
            }
        }

        private void ApplyPhaseBuff()
        {
            if (Context?.Stats == null) return;

            if (_isYangPhase)
            {
                Context.Stats.AddBaseDamage(_yangDamageBonus);

                if (_yangAuraVfxPrefab != null && ProjectZombie.Features.Shared.VFX.GlobalVFXPoolManager.Instance != null && Context?.Transform != null)
                {
                    ProjectZombie.Features.Shared.VFX.GlobalVFXPoolManager.Instance.PlayEffectAttached(_yangAuraVfxPrefab, Context.Transform, _phaseDuration);
                }

                Debug.Log("<color=#FF7700>[Âm Dương Lưỡng Cực] Chuyển thể THÁI DƯƠNG (+50% Sát thương)!</color>");
            }
            else
            {
                Context.Stats.AddBaseDamage(-_yangDamageBonus);

                if (_yinAuraVfxPrefab != null && ProjectZombie.Features.Shared.VFX.GlobalVFXPoolManager.Instance != null && Context?.Transform != null)
                {
                    ProjectZombie.Features.Shared.VFX.GlobalVFXPoolManager.Instance.PlayEffectAttached(_yinAuraVfxPrefab, Context.Transform, _phaseDuration);
                }

                Debug.Log("<color=#00E5FF>[Âm Dương Lưỡng Cực] Chuyển thể THÁI ÂM (Hào quang dưỡng sinh)!</color>");
            }
        }

        protected override void OnAugmentTeardown()
        {
            if (_isYangPhase && Context?.Stats != null)
            {
                Context.Stats.AddBaseDamage(-_yangDamageBonus);
            }
        }
    }
}
