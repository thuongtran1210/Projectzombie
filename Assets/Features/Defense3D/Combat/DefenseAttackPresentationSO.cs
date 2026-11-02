using UnityEngine;
namespace ProjectZombie.Features.Defense3D
{
    [CreateAssetMenu(menuName="Defense3D/Skills/Presentation/Attack")]
    public sealed class DefenseAttackPresentationSO : DefenseSkillPresentationSO
    {
        [SerializeField] private DefenseBasicAttackEffectSetSO _effects;
        public DefenseBasicAttackEffectSetSO Effects => _effects;
        public override bool IsValid => base.IsValid && _effects != null && _effects.IsValid;
    }
}
