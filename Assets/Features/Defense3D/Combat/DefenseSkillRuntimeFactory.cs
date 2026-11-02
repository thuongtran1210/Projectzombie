using System;
using UnityEngine;
namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Behavior registration only; catalog assets decide which behavior each skill uses.</summary>
    public static class DefenseSkillRuntimeFactory
    {
        public static DefenseSkillEffect3D Bind(DefenseSkillDefinitionSO definition,DefenseHeroHealth3D hero,
            DefenseHeroAutoAttack3D attack,DefenseHeroMotor3D motor,DefenseSession session)
        {
            if(definition.Behavior!=DefenseSkillBehavior.Dash&&definition.Tuning==null)throw new InvalidOperationException("Missing behavior tuning: "+definition.SkillId);
            Type type;
            switch(definition.Behavior)
            {
                case DefenseSkillBehavior.BasicAttack:type=typeof(DefenseHeroAttackEffect3D);break;
                case DefenseSkillBehavior.Dash:type=typeof(DefenseDashSkillEffect3D);break;
                case DefenseSkillBehavior.ScholarSignature:type=typeof(DefenseScholarSignatureEffect3D);break;
                case DefenseSkillBehavior.LightningSignature:type=typeof(DefenseLightningSignatureEffect3D);break;
                case DefenseSkillBehavior.Slipper:type=typeof(DefenseRelicSkillEffect3D);break;
                case DefenseSkillBehavior.Pipe:type=typeof(DefensePipeRelicEffect3D);break;
                default:throw new InvalidOperationException("Unregistered skill behavior: "+definition.Behavior);
            }
            var effect=hero.GetComponent(type) as DefenseSkillEffect3D;
            if(effect==null&&(type==typeof(DefenseLightningSignatureEffect3D)||type==typeof(DefensePipeRelicEffect3D)))
                effect=(DefenseSkillEffect3D)hero.gameObject.AddComponent(type);
            if(effect==null)throw new InvalidOperationException("Hero requires authored "+type.Name+" for "+definition.SkillId);
            if(effect is DefenseLightningSignatureEffect3D lightning)lightning.ConfigureComponents(hero,attack,motor);
            if(definition.Tuning!=null)
            {
                if(!definition.Tuning.IsValid||definition.Tuning.Behavior!=definition.Behavior)throw new InvalidOperationException("Invalid tuning for "+definition.SkillId);
                definition.Tuning.Apply(effect);
            }
            if(session!=null)effect.BindGameplay(session);
            return effect;
        }
    }
}
