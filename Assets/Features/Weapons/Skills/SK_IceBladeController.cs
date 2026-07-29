using UnityEngine;

namespace ProjectZombie.Features.Weapons.Skills
{
    /// <summary>
    /// Điều khiển vòng đời và kích hoạt hiệu ứng kỹ năng IceBlade (2D Top-down Anime URP).
    /// Quản lý hiệu ứng đóng băng, tinh thể băng vỡ vụn và sóng bóp méo không gian (Frost Distortion).
    /// </summary>
    public class SK_IceBladeController : MonoBehaviour
    {
        [Header("VFX Particle Layers")]
        [SerializeField] private ParticleSystem _flashAnticipation;
        [SerializeField] private ParticleSystem _slashArc;
        [SerializeField] private ParticleSystem _slashGlow;
        [SerializeField] private ParticleSystem _sparksBurst;
        [SerializeField] private ParticleSystem _hitImpact;
        [SerializeField] private ParticleSystem _smokeResidual;
        [SerializeField] private ParticleSystem _distortionHeat;

        [Header("Skill Configuration")]
        [SerializeField] private float _duration = 0.45f;
        [SerializeField] private bool _autoPlayOnEnable = true;

        private float _activeTimer;
        private bool _isPlaying;

        private void OnEnable()
        {
            if (_autoPlayOnEnable)
            {
                PlayIceBlade();
            }
        }

        private void OnDisable()
        {
            StopAllEffects();
        }

        /// <summary>
        /// Kích hoạt chuỗi Particle System cho hiệu ứng chém Băng Kiếm.
        /// </summary>
        public void PlayIceBlade()
        {
            _activeTimer = 0f;
            _isPlaying = true;

            if (_flashAnticipation != null) _flashAnticipation.Play();
            if (_slashArc != null) _slashArc.Play();
            if (_slashGlow != null) _slashGlow.Play();
            if (_sparksBurst != null) _sparksBurst.Play();
            if (_hitImpact != null) _hitImpact.Play();
            if (_smokeResidual != null) _smokeResidual.Play();
            if (_distortionHeat != null) _distortionHeat.Play();
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
        /// Reset và dùng lại trong Object Pool.
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
            if (_distortionHeat != null) _distortionHeat.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
