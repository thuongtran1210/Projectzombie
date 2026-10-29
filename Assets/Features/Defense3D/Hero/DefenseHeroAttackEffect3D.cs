using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Nối thao tác cast thủ công với chủ sở hữu đòn đánh; không quản lý hình ảnh hoặc pool.</summary>
    public sealed class DefenseHeroAttackEffect3D : DefenseSkillEffect3D, IDefenseSharedAbilityEffect
    {
        [SerializeField] private DefenseHeroAutoAttack3D _attack;
        public override DefenseSkillKind Kind => DefenseSkillKind.BasicAttack;
        public override bool IsConfigured => _attack != null;
        public DefenseAbilityRuntime SharedAbility => _attack.Ability;
        public float CastCooldown => _attack.EffectiveAttackInterval;
        public float ConeAngle => _attack.ConeAngle;
        public bool TryResolveBasicAim(out DefenseResolvedAim aim) => _attack.TryResolveBasicAim(out aim);
        protected override void OnConfigured(DefenseSkillDefinition definition) => _attack.ConfigureAttack(definition);
        public override bool TryPrepare(DefenseResolvedAim requested, out DefenseResolvedAim prepared)
        {
            prepared = requested;
            return IsConfigured && Definition != null && _attack.CanPrepare && _attack.TryResolveBasicAim(out prepared);
        }
        public override void Execute(DefenseResolvedAim prepared) => _attack.QueueAttack(prepared);
    }
}
