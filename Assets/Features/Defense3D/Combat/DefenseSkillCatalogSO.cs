using System;
using System.Collections.Generic;
using UnityEngine;
namespace ProjectZombie.Features.Defense3D
{
    [CreateAssetMenu(menuName="Defense3D/Skills/Catalog")]
    public sealed class DefenseSkillCatalogSO : ScriptableObject
    {
        public const string ResourcePath="Defense3D/DefenseSkillCatalog";
        [Serializable] public struct RelicBinding { public string RelicId; public DefenseSkillDefinitionSO Skill; }
        [SerializeField] private DefenseSkillDefinitionSO[] _skills=Array.Empty<DefenseSkillDefinitionSO>();
        [SerializeField] private RelicBinding[] _relics=Array.Empty<RelicBinding>();
        public IReadOnlyList<DefenseSkillDefinitionSO> Skills => _skills;
        public IReadOnlyList<RelicBinding> Relics => _relics;
        public DefenseSkillDefinitionSO Find(string id)
        {foreach(var skill in _skills)if(skill!=null&&string.Equals(skill.SkillId,id,StringComparison.Ordinal))return skill;return null;}
        public DefenseSkillDefinitionSO FindRelic(string id)
        {foreach(var entry in _relics)if(string.Equals(entry.RelicId,id,StringComparison.OrdinalIgnoreCase))return entry.Skill;return null;}
        public static DefenseSkillCatalogSO Load()
        {var value=Resources.Load<DefenseSkillCatalogSO>(ResourcePath);if(value==null)throw new InvalidOperationException("Missing DefenseSkillCatalog. Run skill data migration.");return value;}
        public void Validate()
        {
            var ids=new HashSet<string>(StringComparer.Ordinal);
            foreach(var skill in _skills)
            {
                if(skill==null||string.IsNullOrWhiteSpace(skill.SkillId)||!ids.Add(skill.SkillId))throw new InvalidOperationException("Missing/duplicate skill ID.");
                skill.Capture();
                if(!Enum.IsDefined(typeof(DefenseSkillBehavior),skill.Behavior))throw new InvalidOperationException("Unregistered behavior: "+skill.SkillId);
                bool needsTuning=skill.Behavior!=DefenseSkillBehavior.Dash;
                if((needsTuning&&skill.Tuning==null)||(skill.Tuning!=null&&(!skill.Tuning.IsValid||skill.Tuning.Behavior!=skill.Behavior)))
                    throw new InvalidOperationException("Missing/invalid behavior tuning: "+skill.SkillId);
                if(skill.Presentation==null||!skill.Presentation.IsValid)throw new InvalidOperationException("Invalid presentation: "+skill.SkillId);
                bool valid=skill.Behavior==DefenseSkillBehavior.Dash?skill.Presentation is DefenseDashPresentationSO:
                    skill.Behavior==DefenseSkillBehavior.BasicAttack?skill.Presentation is DefenseAttackPresentationSO:
                    skill.Behavior==DefenseSkillBehavior.ScholarSignature||skill.Behavior==DefenseSkillBehavior.LightningSignature?skill.Presentation is DefenseSignaturePresentationSO:skill.Presentation is DefenseRelicPresentationSO;
                var kind=skill.Capture().Kind;
                var expected=skill.Behavior==DefenseSkillBehavior.Dash?DefenseSkillKind.Dash:skill.Behavior==DefenseSkillBehavior.BasicAttack?DefenseSkillKind.BasicAttack:
                    skill.Behavior==DefenseSkillBehavior.ScholarSignature||skill.Behavior==DefenseSkillBehavior.LightningSignature?DefenseSkillKind.Signature:DefenseSkillKind.Relic;
                if(!valid||kind!=expected)throw new InvalidOperationException("Skill behavior/slot/presentation mismatch: "+skill.SkillId);
            }
            var relicIds=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach(var entry in _relics)if(string.IsNullOrWhiteSpace(entry.RelicId)||!relicIds.Add(entry.RelicId)||entry.Skill==null||Find(entry.Skill.SkillId)!=entry.Skill||entry.Skill.Capture().Kind!=DefenseSkillKind.Relic)
                throw new InvalidOperationException("Invalid/duplicate relic binding.");
        }
    }
}
