using UnityEngine;
namespace ProjectZombie.Features.Defense3D
{
    [CreateAssetMenu(menuName="Defense3D/Skills/Presentation/Relic")]
    public sealed class DefenseRelicPresentationSO : DefenseSkillPresentationSO
    {
        [SerializeField] private DefenseEffectProfileSO _profile;
        [SerializeField] private Sprite _projectileSprite;
        public DefenseEffectProfileSO Profile => _profile;
        public Sprite ProjectileSprite => _projectileSprite;
        public override bool IsValid => base.IsValid && _profile != null && _profile.IsValid;
    }
}
