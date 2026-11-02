using UnityEngine;
namespace ProjectZombie.Features.Defense3D
{
    [CreateAssetMenu(menuName="Defense3D/Skills/Tuning/Basic Attack")]
    public sealed class DefenseAttackTuningSO : DefenseSkillTuningSO
    {
        [SerializeField,Range(1,360)] private float _coneAngle=DefenseBasicAttackRules.ConeAngle;
        [SerializeField,Range(0,1)] private float _knockbackStrength=.275f;
        [SerializeField,Min(.01f)] private float _knockbackDuration=.2f;
        [SerializeField] private DefenseAttackTargetingProfileSO _targeting;
        public override DefenseSkillBehavior Behavior=>DefenseSkillBehavior.BasicAttack;
        public override bool IsValid=>Positive(_coneAngle)&&_coneAngle<=360&&Ratio(_knockbackStrength)&&Positive(_knockbackDuration)&&_targeting!=null;
        public override void Apply(DefenseSkillEffect3D effect)=>effect.GetComponent<DefenseHeroAutoAttack3D>().ConfigureTuning(_coneAngle,_knockbackStrength,_knockbackDuration,_targeting);
    }
}
