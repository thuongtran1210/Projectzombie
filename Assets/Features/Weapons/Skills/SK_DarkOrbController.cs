using UnityEngine;

namespace ProjectZombie.Features.Weapons.Skills
{
    /// <summary>
    /// Điều khiển vòng đời và kích hoạt hiệu ứng kỹ năng DarkOrb (Quả Cầu Hắc Ám - 2D Top-down Anime URP).
    /// Quản lý hiệu ứng hội tụ năng lượng hư không, quả cầu bóng tối xoay tròn, tia sét tím nổ tung và sóng méo không gian.
    /// </summary>
    public class SK_DarkOrbController : MonoBehaviour
    {
        [Header("VFX Particle Layers")]
        [SerializeField] private ParticleSystem _flashAnticipation;
        [SerializeField] private ParticleSystem _orbCore;
        [SerializeField] private ParticleSystem _orbGlow;
        [SerializeField] private ParticleSystem _sparksBurst;
        [SerializeField] private ParticleSystem _hitImpact;
        [SerializeField] private ParticleSystem _smokeResidual;
        [SerializeField] private ParticleSystem _distortionHeat;

        [Header("Skill Configuration")]
        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private bool _autoPlayOnEnable = true;

        private float _activeTimer;
        private bool _isPlaying;

        private void OnEnable()
        {
            if (_autoPlayOnEnable)
            {
                PlayDarkOrb();
            }
        }

        private void OnDisable()
        {
            StopAllEffects();
        }

        /// <summary>
        /// Kích hoạt chuỗi Particle System cho hiệu ứng Quả Cầu Hắc Ám.
        /// </summary>
        public void PlayDarkOrb()
        {
            _activeTimer = 0f;
            _isPlaying = true;

            if (_flashAnticipation != null) _flashAnticipation.Play();
            if (_orbCore != null) _orbCore.Play();
            if (_orbGlow != null) _orbGlow.Play();
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
        /// Reset và ngắt hiệu ứng khi hoàn tác về Object Pool.
        /// </summary>
        public void StopAllEffects()
        {
            _isPlaying = false;
            if (_flashAnticipation != null) _flashAnticipation.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (_orbCore != null) _orbCore.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (_orbGlow != null) _orbGlow.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (_sparksBurst != null) _sparksBurst.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (_hitImpact != null) _hitImpact.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (_smokeResidual != null) _smokeResidual.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (_distortionHeat != null) _distortionHeat.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
