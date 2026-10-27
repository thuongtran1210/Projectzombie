using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    [CreateAssetMenu(menuName = "Defense3D/Hero Kit")]
    public sealed class DefenseHeroKitSO : ScriptableObject
    {
        [SerializeField] private string _heroId;
        [SerializeField] private string _displayName;
        [SerializeField] private DefenseSkillDefinitionSO _attack;
        [SerializeField] private DefenseSkillDefinitionSO _dash;
        [SerializeField] private DefenseSkillDefinitionSO _signature;
        [SerializeField] private DefenseSkillDefinitionSO _relic;
        public string HeroId => _heroId;
        public string DisplayName => _displayName;
        public DefenseSkillDefinitionSO DefinitionFor(DefenseSkillKind kind)
        {
            switch (kind)
            {
                case DefenseSkillKind.BasicAttack: return _attack;
                case DefenseSkillKind.Dash: return _dash;
                case DefenseSkillKind.ScholarSignature: return _signature;
                case DefenseSkillKind.DepToOng: return _relic;
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(_heroId) || string.IsNullOrWhiteSpace(_displayName)) throw new InvalidOperationException("Hero kit requires identity.");
            foreach (DefenseSkillKind kind in Enum.GetValues(typeof(DefenseSkillKind)))
                if (DefinitionFor(kind) == null || DefinitionFor(kind).Capture().Kind != kind) throw new InvalidOperationException("Hero kit requires four matching definitions.");
        }
    }
}
