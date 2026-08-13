using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Core.Audio
{
    /// <summary>
    /// Manager quản lý tập trung toàn bộ âm thanh trong game (SFX, BGM, UI).
    /// Hỗ trợ Object Pooling, Audio Cooldown & Control Mixer Volume.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer _masterMixer;
        [SerializeField] private string _masterVolumeParam = "MasterVolume";
        [SerializeField] private string _sfxVolumeParam = "SFXVolume";
        [SerializeField] private string _bgmVolumeParam = "BGMVolume";

        [Header("Pool Settings")]
        [SerializeField] private int _initialPoolSize = 20;

        [Header("BGM Source")]
        [SerializeField] private AudioSource _bgmAudioSource;

        private AudioSourcePool _pool;
        private AudioCooldownTracker _cooldownTracker;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSystem();
        }

        private void InitializeAudioSystem()
        {
            var poolContainer = new GameObject("[AudioSourcePool]");
            poolContainer.transform.SetParent(transform);
            
            _pool = new AudioSourcePool(poolContainer.transform, _initialPoolSize);
            _cooldownTracker = new AudioCooldownTracker();

            if (_bgmAudioSource == null)
            {
                var bgmObj = new GameObject("[BGM_AudioSource]");
                bgmObj.transform.SetParent(transform);
                _bgmAudioSource = bgmObj.AddComponent<AudioSource>();
                _bgmAudioSource.loop = true;
                _bgmAudioSource.playOnAwake = false;
            }
        }

        /// <summary>
        /// Phát 1 hiệu ứng âm thanh SFX / UI dựa trên AudioConfigSO.
        /// </summary>
        public void PlaySound(AudioConfigSO config, Vector3 position = default)
        {
            if (config == null) return;
            if (!_cooldownTracker.CanPlay(config)) return;

            AudioClip clipToPlay = config.DirectClip;
            if (clipToPlay == null)
            {
                // Nếu dùng Addressables reference, nạp và phát (có thể mở rộng nạp async qua Addressables)
                Debug.LogWarning($"[{nameof(AudioManager)}] AudioConfig '{config.name}' không có DirectClip được gán.");
                return;
            }

            AudioSource source = _pool.Get();
            source.clip = clipToPlay;
            source.outputAudioMixerGroup = config.MixerGroup;
            source.volume = config.Volume;
            source.pitch = config.GetRandomPitch();

            if (position != default)
            {
                source.transform.position = position;
                source.spatialBlend = 1f; // 3D sound
            }
            else
            {
                source.spatialBlend = 0f; // 2D sound
            }

            _cooldownTracker.RecordPlay(config);
            source.Play();

            StartCoroutine(Routine_ReleaseSource(source, config, clipToPlay.length / Mathf.Abs(source.pitch)));
        }

        private IEnumerator Routine_ReleaseSource(AudioSource source, AudioConfigSO config, float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            _cooldownTracker.RecordStop(config);
            _pool.Release(source);
        }

        /// <summary>
        /// Phát Nhạc nền (BGM).
        /// </summary>
        public void PlayBGM(AudioConfigSO config, bool fade = true)
        {
            if (config == null || config.DirectClip == null) return;

            _bgmAudioSource.clip = config.DirectClip;
            _bgmAudioSource.outputAudioMixerGroup = config.MixerGroup;
            _bgmAudioSource.volume = config.Volume;
            _bgmAudioSource.Play();
        }

        public void StopBGM()
        {
            if (_bgmAudioSource != null && _bgmAudioSource.isPlaying)
            {
                _bgmAudioSource.Stop();
            }
        }

        #region Mixer Control Methods

        public void SetMasterVolume(float linearVolume)
        {
            SetMixerVolume(_masterVolumeParam, linearVolume);
        }

        public void SetSFXVolume(float linearVolume)
        {
            SetMixerVolume(_sfxVolumeParam, linearVolume);
        }

        public void SetBGMVolume(float linearVolume)
        {
            SetMixerVolume(_bgmVolumeParam, linearVolume);
        }

        private void SetMixerVolume(string parameterName, float linearVolume)
        {
            if (_masterMixer == null) return;
            // Chuyển đổi từ linear [0.0001, 1] sang dB [-80, 0]
            float dB = linearVolume > 0.0001f ? Mathf.Log10(linearVolume) * 20f : -80f;
            _masterMixer.SetFloat(parameterName, dB);
        }

        #endregion
    }
}
