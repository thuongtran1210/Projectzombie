using UnityEngine;

namespace ProjectZombie.Features.Weapons.Skills
{
    /// <summary>
    /// Điều khiển vòng đời và kích hoạt hiệu ứng kỹ năng IceBullet (Đạn Băng - 2D Top-down Anime URP).
    /// Quản lý hiệu ứng chớp nòng hàn khí, lõi đạn tinh thể băng, vệt đuôi tuyết lạnh, mảnh băng vỡ vụn và khói tàn dư.
    /// </summary>
    public class SK_IceBulletController : MonoBehaviour
    {
        [Header("VFX Particle Layers")]
        [SerializeField] private ParticleSystem _muzzleFlash;
        [SerializeField] private ParticleSystem _bulletCore;
        [SerializeField] private ParticleSystem _bulletTrail;
        [SerializeField] private ParticleSystem _sparksBurst;
        [SerializeField] private ParticleSystem _hitImpact;
        [SerializeField] private ParticleSystem _smokeResidual;

        [Header("Skill Configuration")]
        [SerializeField] private float _duration = 0.6f;
        [SerializeField] private bool _autoPlayOnEnable = true;

        private float _activeTimer;
        private bool _isPlaying;

        private void OnEnable()
        {
            if (_autoPlayOnEnable)
            {
                PlayIceBullet();
            }
        }

        private void OnDisable()
        {
            StopAllEffects();
        }

        /// <summary>
        /// Kích hoạt chuỗi Particle System cho hiệu ứng Viên Đạn Băng.
        /// </summary>
        public void PlayIceBullet()
        {
            _activeTimer = 0f;
            _isPlaying = true;

            if (_muzzleFlash != null) _muzzleFlash.Play();
            if (_bulletCore != null) _bulletCore.Play();
            if (_bulletTrail != null) _bulletTrail.Play();
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
        /// Reset và ngắt toàn bộ hiệu ứng khi hoàn tác về Object Pool.
        /// </summary>
        public void StopAllEffects()
        {
            _isPlaying = false;
            if (_muzzleFlash != null) _muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (_bulletCore != null) _bulletCore.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (_bulletTrail != null) _bulletTrail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (_sparksBurst != null) _sparksBurst.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (_hitImpact != null) _hitImpact.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (_smokeResidual != null) _smokeResidual.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
