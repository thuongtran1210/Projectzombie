using UnityEngine;
namespace ProjectZombie.Features.Defense3D
{
    [CreateAssetMenu(menuName="Defense3D/Skills/Tuning/Slipper")]
    public sealed class DefenseSlipperTuningSO : DefenseSkillTuningSO
    {
        [SerializeField,Min(.01f)] private float _recastWindow=4f;
        [SerializeField,Min(.01f)] private float _recastDashDuration=.3f;
        [SerializeField] private DefenseHitStatusProfileSO _hitStatuses;
        public override DefenseSkillBehavior Behavior=>DefenseSkillBehavior.Slipper;
        public override bool IsValid=>Positive(_recastWindow)&&Positive(_recastDashDuration)&&(_hitStatuses==null||_hitStatuses.IsValid);
        public override void Apply(DefenseSkillEffect3D effect)=>((DefenseRelicSkillEffect3D)effect).ConfigureTuning(_recastWindow,_recastDashDuration,_hitStatuses);
    }
}
