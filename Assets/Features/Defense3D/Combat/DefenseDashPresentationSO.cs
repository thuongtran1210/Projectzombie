using UnityEngine;
namespace ProjectZombie.Features.Defense3D
{
    [CreateAssetMenu(menuName="Defense3D/Skills/Presentation/Dash")]
    public sealed class DefenseDashPresentationSO : DefenseSkillPresentationSO
    {
        [SerializeField] private DefenseDashEffectSetSO _effects;
        public DefenseDashEffectSetSO Effects => _effects;
        public override bool IsValid => base.IsValid && _effects != null && _effects.IsValid;
    }
}
