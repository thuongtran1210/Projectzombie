using UnityEngine;
namespace ProjectZombie.Features.Defense3D
{
    [CreateAssetMenu(menuName="Defense3D/Skills/Presentation/Signature")]
    public sealed class DefenseSignaturePresentationSO : DefenseSkillPresentationSO
    {
        [SerializeField] private DefenseSignatureEffectSetSO _effects;
        public DefenseSignatureEffectSetSO Effects => _effects;
        public override bool IsValid => base.IsValid && _effects != null && _effects.IsValid;
    }
}
