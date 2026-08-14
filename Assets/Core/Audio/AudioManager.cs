using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Core.Audio
{
    /// <summary>
    /// Manager quản lý tập trung toàn bộ âm thanh trong game (SFX, BGM, UI).
    /// Hỗ trợ Object Pooling, Audio Cooldown, Adaptive BGM Snapshots & Phase Stingers.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer _masterMixer;
        [SerializeField] private string _masterVolumeParam = "MasterVolume";
        [SerializeField] private string _bgmVolumeParam = "BGMVolume";
        [SerializeField] private string _sfxVolumeParam = "SFXVolume";
        [SerializeField] private string _uiVolumeParam = "UIVolume";

        [Header("Pool Settings")]
        [SerializeField] private int _initialPoolSize = 20;

        [Header("BGM Sources (Adaptive Layering)")]
        [SerializeField] private AudioSource _bgmAudioSource;
        [SerializeField] private AudioSource _stingerAudioSource;

        private const string PREFS_MASTER_VOL = "Setting_MasterVolume";
        private const string PREFS_BGM_VOL = "Setting_BGMVolume";
        private const string PREFS_SFX_VOL = "Setting_SFXVolume";
        private const string PREFS_UI_VOL = "Setting_UIVolume";

        private const float DEFAULT_MASTER_VOLUME = 1.0f;
        private const float DEFAULT_BGM_VOLUME = 0.4f;
        private const float DEFAULT_SFX_VOLUME = 0.9f;
        private const float DEFAULT_UI_VOLUME = 0.8f;

        private AudioSourcePool _pool;
        private AudioCooldownTracker _cooldownTracker;

        public AudioMixer MasterMixer => _masterMixer;
        public float MasterVolume { get; private set; } = DEFAULT_MASTER_VOLUME;
        public float BGMVolume { get; private set; } = DEFAULT_BGM_VOLUME;
        public float SFXVolume { get; private set; } = DEFAULT_SFX_VOLUME;
        public float UIVolume { get; private set; } = DEFAULT_UI_VOLUME;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
            DontDestroyOnLoad(gameObject);

            InitializeAudioSystem();
            LoadAndApplyVolumeSettings();
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

            if (_stingerAudioSource == null)
            {
                var stingerObj = new GameObject("[Stinger_AudioSource]");
                stingerObj.transform.SetParent(transform);
                _stingerAudioSource = stingerObj.AddComponent<AudioSource>();
                _stingerAudioSource.loop = false;
                _stingerAudioSource.playOnAwake = false;
            }
        }

        private void LoadAndApplyVolumeSettings()
        {
            MasterVolume = PlayerPrefs.GetFloat(PREFS_MASTER_VOL, DEFAULT_MASTER_VOLUME);
            BGMVolume = PlayerPrefs.GetFloat(PREFS_BGM_VOL, DEFAULT_BGM_VOLUME);
            SFXVolume = PlayerPrefs.GetFloat(PREFS_SFX_VOL, DEFAULT_SFX_VOLUME);
            UIVolume = PlayerPrefs.GetFloat(PREFS_UI_VOL, DEFAULT_UI_VOLUME);

            SetMasterVolume(MasterVolume, saveToPrefs: false);
            SetBGMVolume(BGMVolume, saveToPrefs: false);
            SetSFXVolume(SFXVolume, saveToPrefs: false);
            SetUIVolume(UIVolume, saveToPrefs: false);
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

        /// <summary>
        /// Phát Phase Transition Stinger (2-3s âm thanh báo hiệu chuyển Phase đồng bộ với Palette-Swap).
        /// </summary>
        public void PlayPhaseStinger(AudioConfigSO stingerConfig)
        {
            if (stingerConfig == null || stingerConfig.DirectClip == null) return;

            _stingerAudioSource.clip = stingerConfig.DirectClip;
            _stingerAudioSource.outputAudioMixerGroup = stingerConfig.MixerGroup;
            _stingerAudioSource.volume = stingerConfig.Volume;
            _stingerAudioSource.pitch = stingerConfig.GetRandomPitch();
            _stingerAudioSource.Play();
        }


        public void StopBGM()
        {
            if (_bgmAudioSource != null && _bgmAudioSource.isPlaying)
            {
                _bgmAudioSource.Stop();
            }
        }

        #region Mixer Control Methods (Settings Panel Ready)

        public void SetMasterVolume(float linearVolume, bool saveToPrefs = true)
        {
            MasterVolume = Mathf.Clamp01(linearVolume);
            SetMixerVolume(_masterVolumeParam, MasterVolume);
            if (saveToPrefs)
            {
                PlayerPrefs.SetFloat(PREFS_MASTER_VOL, MasterVolume);
                PlayerPrefs.Save();
            }
        }

        public void SetBGMVolume(float linearVolume, bool saveToPrefs = true)
        {
            BGMVolume = Mathf.Clamp01(linearVolume);
            SetMixerVolume(_bgmVolumeParam, BGMVolume);
            if (saveToPrefs)
            {
                PlayerPrefs.SetFloat(PREFS_BGM_VOL, BGMVolume);
                PlayerPrefs.Save();
            }
        }

        public void SetSFXVolume(float linearVolume, bool saveToPrefs = true)
        {
            SFXVolume = Mathf.Clamp01(linearVolume);
            SetMixerVolume(_sfxVolumeParam, SFXVolume);
            if (saveToPrefs)
            {
                PlayerPrefs.SetFloat(PREFS_SFX_VOL, SFXVolume);
                PlayerPrefs.Save();
            }
        }

        public void SetUIVolume(float linearVolume, bool saveToPrefs = true)
        {
            UIVolume = Mathf.Clamp01(linearVolume);
            SetMixerVolume(_uiVolumeParam, UIVolume);
            if (saveToPrefs)
            {
                PlayerPrefs.SetFloat(PREFS_UI_VOL, UIVolume);
                PlayerPrefs.Save();
            }
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

