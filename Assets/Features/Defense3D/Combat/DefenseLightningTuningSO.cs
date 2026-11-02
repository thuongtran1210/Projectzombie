using UnityEngine;
namespace ProjectZombie.Features.Defense3D
{
    [CreateAssetMenu(menuName="Defense3D/Skills/Tuning/Lightning")]
    public sealed class DefenseLightningTuningSO : DefenseSkillTuningSO
    {
        [SerializeField,Min(0)] private float _stunDuration=1.5f;
        [SerializeField,Range(0,1)] private float _slowStrength=.4f;
        [SerializeField,Min(0)] private float _slowDuration=1f;
        public override DefenseSkillBehavior Behavior=>DefenseSkillBehavior.LightningSignature;
        public override bool IsValid=>NonNegative(_stunDuration)&&Ratio(_slowStrength)&&NonNegative(_slowDuration);
        public override void Apply(DefenseSkillEffect3D effect)=>((DefenseLightningSignatureEffect3D)effect).ConfigureTuning(_stunDuration,_slowStrength,_slowDuration);
    }
}
