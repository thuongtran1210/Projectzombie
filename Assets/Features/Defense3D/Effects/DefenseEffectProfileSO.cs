using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Cấu hình trình diễn; không chứa damage, cooldown hoặc trạng thái của từng lượt phát.</summary>
    [CreateAssetMenu(menuName = "Defense3D/Effects/Profile")]
    public sealed class DefenseEffectProfileSO : ScriptableObject
    {
        [SerializeField] private DefenseEffectInstance3D _prefab;
        [SerializeField, Min(.01f)] private float _duration = .45f;
        [SerializeField] private bool _sustained;
        [SerializeField] private bool _animateSustained;
        [SerializeField] private Color _color = new Color(1f, .82f, .42f, .9f);
        [SerializeField, Min(.01f)] private float _scale = 1f;
        [SerializeField, Range(0, 10)] private int _priority;
        public DefenseEffectInstance3D Prefab => _prefab;
        public float Duration => _duration;
        public bool Sustained => _sustained;
        public bool AnimateSustained => _animateSustained;
        public Color Color => _color;
        public float Scale => _scale;
        public int Priority => _priority;
        public bool IsValid => _prefab != null && _prefab.IsConfigured && DefenseDamageRules.IsFinite(_duration) && _duration > 0f &&
            DefenseDamageRules.IsFinite(_scale) && _scale > 0f && _priority >= 0 && _priority <= 10 &&
            DefenseDamageRules.IsFinite(_color.r) && DefenseDamageRules.IsFinite(_color.g) &&
            DefenseDamageRules.IsFinite(_color.b) && DefenseDamageRules.IsFinite(_color.a);
    }

    public readonly struct DefenseEffectHandle
    {
        internal readonly object Owner;
        internal readonly long Token;
        internal DefenseEffectHandle(object owner, long token) { Owner = owner; Token = token; }
        public bool IsAssigned => Owner != null && Token > 0;
    }
    public interface IDefenseEffectService
    {
        DefenseEffectHandle Play(in DefenseEffectRequest request);
        DefenseEffectHandle Play(DefenseEffectProfileSO profile, Vector3 position, float radius = 1f,
            Component owner = null, Transform anchor = null);
        bool Stop(DefenseEffectHandle handle);
        void StopOwner(Component owner);
    }

    /// <summary>Đồng hồ C# thuần; hiệu ứng duy trì chỉ hết hạn khi nhận lệnh dừng.</summary>
    public readonly struct DefenseEffectLifetime
    {
        private readonly float _start, _duration;
        private readonly bool _sustained;
        private readonly bool _animateSustained;
        public DefenseEffectLifetime(float start, float duration, bool sustained, bool animateSustained = false)
        {
            if (!DefenseDamageRules.IsFinite(start) || !DefenseDamageRules.IsFinite(duration) || duration <= 0f)
                throw new System.ArgumentOutOfRangeException(nameof(duration));
            _start = start; _duration = duration; _sustained = sustained; _animateSustained = animateSustained;
        }
        public bool IsExpired(float gameplayTime) => !_sustained && gameplayTime - _start >= _duration;
        public float Progress(float gameplayTime) => _sustained && !_animateSustained ? 0f : Mathf.Clamp01((gameplayTime - _start) / _duration);
    }
}
