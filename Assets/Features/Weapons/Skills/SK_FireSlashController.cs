using UnityEngine;

namespace ProjectZombie.Features.Weapons.Skills
{
    /// <summary>
    /// Điều khiển vòng đời và kích hoạt hiệu ứng kỹ năng FireSlash (2D Top-down Anime URP).
    /// Quản lý thời lượng 0.4s, phát hiệu ứng âm thanh/VFX và trả về Pool sau khi hoàn tất.
    /// </summary>
    public class SK_FireSlashController : MonoBehaviour
    {
        [Header("VFX Particle Layers")]
        [SerializeField] private ParticleSystem _flashAnticipation;
        [SerializeField] private ParticleSystem _slashArc;
        [SerializeField] private ParticleSystem _slashGlow;
        [SerializeField] private ParticleSystem _sparksBurst;
        [SerializeField] private ParticleSystem _hitImpact;
        [SerializeField] private ParticleSystem _smokeResidual;

        [Header("Skill Configuration")]
        [SerializeField] private float _duration = 0.4f;
        [SerializeField] private bool _autoPlayOnEnable = true;

        private float _activeTimer;
        private bool _isPlaying;

        private void OnEnable()
        {
            if (_autoPlayOnEnable)
            {
                PlayFireSlash();
            }
        }

        private void OnDisable()
        {
            StopAllEffects();
        }

        /// <summary>
        /// Kích hoạt toàn bộ các Particle System layer theo chuỗi timing chuẩn 60 FPS.
        /// </summary>
        public void PlayFireSlash()
        {
            _activeTimer = 0f;
            _isPlaying = true;

            if (_flashAnticipation != null) _flashAnticipation.Play();
            if (_slashArc != null) _slashArc.Play();
            if (_slashGlow != null) _slashGlow.Play();
            if (_sparksBurst != null) _sparksBurst.Play();
            if (_hitImpact != null) _hitImpact.Play();
            if (_smokeResidual != null) _smokeResidual.Play();
        }

        private void Update()
        {
            if (!_isPlaying) return;

            _activeTimer += Time.deltaTime;
            if (_activeTimer >= _duration)
            {
                _isPlaying = false;
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Đảm bảo tất cả particle được ngắt và reset khi trả về pool.
        /// </summary>
        public void StopAllEffects()
        {
            _isPlaying = false;
            if (_flashAnticipation != null) _flashAnticipation.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (_slashArc != null) _slashArc.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (_slashGlow != null) _slashGlow.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (_sparksBurst != null) _sparksBurst.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (_hitImpact != null) _hitImpact.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (_smokeResidual != null) _smokeResidual.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
