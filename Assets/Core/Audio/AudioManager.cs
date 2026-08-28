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
        private static AudioManager _instance;
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AudioManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("AudioManager");
                        _instance = go.AddComponent<AudioManager>();
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

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

        [Header("Core UI Sound Effects (Tinh Gọn & Trầm Ấm)")]
        [SerializeField] private AudioClip _uiClickClip;
        [SerializeField] private AudioClip _uiConfirmClip;
        [SerializeField] private AudioClip _uiWeaponEquipClip;
        [SerializeField] private AudioClip _uiErrorClip;
        [SerializeField] private AudioClip _uiCoinClip;

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

#if UNITY_EDITOR
            if (_uiClickClip == null) _uiClickClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Data/Audios/SFX_UI_Wooden_Click.wav");
            if (_uiConfirmClip == null) _uiConfirmClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Data/Audios/SFX_UI_Confirm.wav");
            if (_uiConfirmClip == null) _uiConfirmClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Data/Audios/SFX_UI_Card_Select.wav");
            if (_uiWeaponEquipClip == null) _uiWeaponEquipClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Data/Audios/SFX_UI_Weapon_Equip.wav");
            if (_uiErrorClip == null) _uiErrorClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Data/Audios/SFX_UI_Error.wav");
            if (_uiCoinClip == null) _uiCoinClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Data/Audios/SFX_Coin_Tick.wav");
#endif
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
        /// Phát 1 hiệu ứng âm thanh SFX / UI dựa trên AudioConfigSO (hỗ trợ custom pitch cho combo).
        /// </summary>
        public void PlaySound(AudioConfigSO config, Vector3 position = default, float customPitch = 0f)
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
            source.pitch = customPitch > 0f ? customPitch : config.GetRandomPitch();

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
        /// Phát Nhạc nền (BGM) với cơ chế Fade mượt mà tránh ngắt âm đột ngột.
        /// </summary>
        public void PlayBGM(AudioConfigSO config, bool fade = true)
        {
            if (config == null || config.DirectClip == null) return;
            PlayBGM(config.DirectClip, config.Volume, fade ? 0.75f : 0f);
        }

        public void PlayBGM(AudioClip clip, float targetVolume = 0.4f, float fadeDuration = 0.75f)
        {
            if (clip == null || _bgmAudioSource == null) return;
            if (_bgmAudioSource.clip == clip && _bgmAudioSource.isPlaying) return;

            StopAllCoroutines();
            if (fadeDuration > 0f && _bgmAudioSource.isPlaying)
            {
                StartCoroutine(Routine_FadeBGM(clip, targetVolume, fadeDuration));
            }
            else
            {
                _bgmAudioSource.clip = clip;
                _bgmAudioSource.volume = targetVolume;
                _bgmAudioSource.Play();
            }
        }

        private IEnumerator Routine_FadeBGM(AudioClip newClip, float targetVolume, float duration)
        {
            float startVol = _bgmAudioSource.volume;
            float halfDuration = duration * 0.5f;

            // Fade Out
            for (float t = 0; t < halfDuration; t += Time.unscaledDeltaTime)
            {
                _bgmAudioSource.volume = Mathf.Lerp(startVol, 0f, t / halfDuration);
                yield return null;
            }

            _bgmAudioSource.Stop();
            _bgmAudioSource.clip = newClip;
            _bgmAudioSource.Play();

            // Fade In
            for (float t = 0; t < halfDuration; t += Time.unscaledDeltaTime)
            {
                _bgmAudioSource.volume = Mathf.Lerp(0f, targetVolume, t / halfDuration);
                yield return null;
            }
            _bgmAudioSource.volume = targetVolume;
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

        #region Core UI Sound Playback (Tiện ích Giao Diện Linh Hoạt)

        public void PlayUIClick(float pitch = 1f)
        {
            // Thêm độ lệch pitch ngẫu nhiên nhẹ (±3%) giúp âm thanh tự nhiên khi click liên tục
            float naturalPitch = pitch * Random.Range(0.97f, 1.03f);
            if (_uiClickClip != null) PlaySound(_uiClickClip, default, 0.9f, naturalPitch);
        }

        public void PlayUIConfirm(float pitch = 1f)
        {
            float naturalPitch = pitch * Random.Range(0.98f, 1.02f);
            AudioClip clip = _uiConfirmClip != null ? _uiConfirmClip : _uiClickClip;
            if (clip != null) PlaySound(clip, default, 1f, naturalPitch);
        }

        public void PlayWeaponEquip(float pitch = 1f)
        {
            float naturalPitch = pitch * Random.Range(0.97f, 1.03f);
            AudioClip clip = _uiWeaponEquipClip != null ? _uiWeaponEquipClip : (_uiConfirmClip != null ? _uiConfirmClip : _uiClickClip);
            if (clip != null) PlaySound(clip, default, 0.95f, naturalPitch);
        }

        public void PlayUIError(float pitch = 1f)
        {
            if (_uiErrorClip != null)
            {
                PlaySound(_uiErrorClip, default, 0.9f, pitch);
            }
            else if (_uiClickClip != null)
            {
                PlaySound(_uiClickClip, default, 0.7f, 0.65f); // Low pitch error fallback
            }
        }

        public void PlayCoinTick(float pitch = 1f)
        {
            // Pitch ngẫu nhiên linh hoạt từ 0.95 đến 1.10 mô phỏng tiếng đồng xu rơi đa dạng
            float coinPitch = pitch * Random.Range(0.95f, 1.10f);
            if (_uiCoinClip != null) PlaySound(_uiCoinClip, default, 0.85f, coinPitch);
        }

        /// <summary>
        /// Phát trực tiếp 1 AudioClip không cần qua AudioConfigSO (tiện lợi cho UI & Event).
        /// </summary>
        public void PlaySound(AudioClip clip, Vector3 position = default, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return;

            AudioSource source = _pool.Get();
            source.clip = clip;
            source.outputAudioMixerGroup = null;
            source.volume = volume * UIVolume;
            source.pitch = pitch;

            if (position != default)
            {
                source.transform.position = position;
                source.spatialBlend = 1f;
            }
            else
            {
                source.spatialBlend = 0f;
            }

            source.Play();
            // Tối ưu: Giới hạn thời gian giữ AudioSource tối đa 3.5s để hoàn trả về Pool nhanh chóng
            float playDuration = Mathf.Min(clip.length / Mathf.Max(0.01f, Mathf.Abs(pitch)), 3.5f);
            StartCoroutine(Routine_ReleaseClipSource(source, playDuration));
        }

        private IEnumerator Routine_ReleaseClipSource(AudioSource source, float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            _pool.Release(source);
        }

        #endregion

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

namespace ProjectZombie.Core.Audio
{
    // Alias tương thích ngược để gọi được từ cả ProjectZombie.Core.Audio lẫn Core.Audio
    public static class AudioHelper
    {
        public static global::Core.Audio.AudioManager Manager => global::Core.Audio.AudioManager.Instance;
    }
}

