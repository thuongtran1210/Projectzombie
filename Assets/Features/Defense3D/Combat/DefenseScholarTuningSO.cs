using UnityEngine;
namespace ProjectZombie.Features.Defense3D
{
    [CreateAssetMenu(menuName="Defense3D/Skills/Tuning/Scholar")]
    public sealed class DefenseScholarTuningSO : DefenseSkillTuningSO
    {
        [SerializeField,Range(0,1)] private float _relicCooldownReduction=.2f;
        public override DefenseSkillBehavior Behavior=>DefenseSkillBehavior.ScholarSignature;
        public override bool IsValid=>Ratio(_relicCooldownReduction);
        public override void Apply(DefenseSkillEffect3D effect)=>((DefenseScholarSignatureEffect3D)effect).ConfigureTuning(_relicCooldownReduction);
    }
}
