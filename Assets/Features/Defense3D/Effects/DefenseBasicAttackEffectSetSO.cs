using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Bộ style đánh thường; thay profile để đổi mỹ thuật mà không sửa logic combat.</summary>
    [CreateAssetMenu(menuName = "Defense3D/Effects/Basic Attack Set")]
    public sealed class DefenseBasicAttackEffectSetSO : ScriptableObject
    {
        [SerializeField] private DefenseEffectProfileSO _swing;
        [SerializeField] private DefenseEffectProfileSO _hit;
        [SerializeField, Min(0f)] private float _swingHeight = .68f;
        [SerializeField, Min(0f)] private float _forwardOffset = .25f;
        [SerializeField, Min(.01f)] private float _hitRadius = .3f;
        public DefenseEffectProfileSO Swing => _swing;
        public DefenseEffectProfileSO Hit => _hit;
        public float SwingHeight => _swingHeight;
        public float ForwardOffset => _forwardOffset;
        public float HitRadius => _hitRadius;
        public bool IsValid => _swing != null && _swing.IsValid && !_swing.Sustained && _hit != null && _hit.IsValid && !_hit.Sustained &&
            DefenseDamageRules.IsFinite(_swingHeight) && _swingHeight >= 0f && DefenseDamageRules.IsFinite(_forwardOffset) && _forwardOffset >= 0f &&
            DefenseDamageRules.IsFinite(_hitRadius) && _hitRadius > 0f;
    }
}
