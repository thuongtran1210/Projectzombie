using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.AddressableAssets;

namespace Core.Audio
{
    public enum SoundCategory
    {
        SFX,
        BGM,
        UI,
        Ambient
    }

    /// <summary>
    /// ScriptableObject định nghĩa cấu hình cho 1 âm thanh (SFX / BGM / UI).
    /// Giúp Game Designer dễ dàng điều chỉnh thông số âm thanh mà không cần sửa code.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioConfig_", menuName = "ProjectZombie/Audio/Audio Config")]
    public class AudioConfigSO : ScriptableObject
    {
        [Header("Asset Reference")]
        [SerializeField] private AssetReferenceT<AudioClip> _audioClipRef;
        [SerializeField] private AudioClip _directClip; // Cho phép gán trực tiếp nếu không dùng Addressable

        [Header("Audio Settings")]
        [SerializeField] private SoundCategory _category = SoundCategory.SFX;
        [SerializeField] private AudioMixerGroup _mixerGroup;
        
        [Range(0f, 1f)] 
        [SerializeField] private float _volume = 1f;

        [Tooltip("Biên độ thay đổi Pitch ngẫu nhiên (tránh cảm giác lặp lại âm thanh)")]
        [SerializeField] private Vector2 _pitchRange = new Vector2(0.95f, 1.05f);

        [Header("Cooldown & Limits (Android Optimization)")]
        [Tooltip("Khoảng thời gian tối thiểu (tính bằng giây) giữa 2 lần phát âm thanh này để tránh spam sound")]
        [SerializeField] private float _cooldownTime = 0.05f;

        [Tooltip("Số lượng voice phát đồng thời tối đa của loại sound này")]
        [SerializeField] private int _maxConcurrentVoices = 3;

        public AssetReferenceT<AudioClip> AudioClipRef => _audioClipRef;
        public AudioClip DirectClip
        {
            get
            {
                if (_directClip != null) return _directClip;
#if UNITY_EDITOR
                if (name.Contains("Exp")) _directClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Data/Audios/SFX_Exp_Gem_Pickup.wav");
                else if (name.Contains("Die")) _directClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Data/Audios/SFX_Enemy_Dissolve_Death.wav");
                else if (name.Contains("LevelUp")) _directClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Data/Audios/SFX_Player_LevelUp.wav");
#endif
                return _directClip;
            }
        }
        public SoundCategory Category => _category;
        public AudioMixerGroup MixerGroup => _mixerGroup;
        public float Volume => _volume;
        public float CooldownTime => _cooldownTime;
        public int MaxConcurrentVoices => _maxConcurrentVoices;

        /// <summary>
        /// Lấy giá trị Pitch ngẫu nhiên trong khoảng cấu hình.
        /// </summary>
        public float GetRandomPitch()
        {
            if (Mathf.Approximately(_pitchRange.x, _pitchRange.y))
                return _pitchRange.x;
            return Random.Range(_pitchRange.x, _pitchRange.y);
        }
    }
}
