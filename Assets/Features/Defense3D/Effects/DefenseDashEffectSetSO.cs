using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Bộ cấu hình dash: burst ngắn ở hai đầu và trail duy trì đến khi motor kết thúc.</summary>
    [CreateAssetMenu(menuName = "Defense3D/Effects/Dash Set")]
    public sealed class DefenseDashEffectSetSO : ScriptableObject
    {
        [SerializeField] private DefenseEffectProfileSO _start;
        [SerializeField] private DefenseEffectProfileSO _trail;
        [SerializeField] private DefenseEffectProfileSO _end;
        [SerializeField, Min(.01f)] private float _burstRadius = .42f;
        [SerializeField, Min(.01f)] private float _trailRadius = .28f;
        [SerializeField, Min(0f)] private float _lift = .04f;
        public DefenseEffectProfileSO Start => _start;
        public DefenseEffectProfileSO Trail => _trail;
        public DefenseEffectProfileSO End => _end;
        public float BurstRadius => _burstRadius;
        public float TrailRadius => _trailRadius;
        public float Lift => _lift;
        public bool IsValid => _start != null && _start.IsValid && !_start.Sustained && _trail != null && _trail.IsValid && _trail.Sustained &&
            _end != null && _end.IsValid && !_end.Sustained && DefenseDamageRules.IsFinite(_burstRadius) && _burstRadius > 0f &&
            DefenseDamageRules.IsFinite(_trailRadius) && _trailRadius > 0f && DefenseDamageRules.IsFinite(_lift) && _lift >= 0f;
    }
}
