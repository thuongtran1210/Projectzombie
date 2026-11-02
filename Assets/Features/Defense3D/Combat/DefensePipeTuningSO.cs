using UnityEngine;
namespace ProjectZombie.Features.Defense3D
{
    [CreateAssetMenu(menuName="Defense3D/Skills/Tuning/Pipe")]
    public sealed class DefensePipeTuningSO : DefenseSkillTuningSO
    {
        [SerializeField,Range(0,1)] private float _slowStrength=.6f;
        [SerializeField,Min(0)] private float _coughStunDuration=.6f;
        [SerializeField,Min(.01f)] private float _tickInterval=.5f;
        [SerializeField,Min(0)] private float _initialBurstRatio=.5f;
        [SerializeField,Min(0)] private float _tickDamageRatio=.15f;
        public override DefenseSkillBehavior Behavior=>DefenseSkillBehavior.Pipe;
        public override bool IsValid=>Ratio(_slowStrength)&&NonNegative(_coughStunDuration)&&Positive(_tickInterval)&&NonNegative(_initialBurstRatio)&&NonNegative(_tickDamageRatio);
        public override void Apply(DefenseSkillEffect3D effect)=>((DefensePipeRelicEffect3D)effect).ConfigureTuning(_slowStrength,_coughStunDuration,_tickInterval,_initialBurstRatio,_tickDamageRatio);
    }
}
