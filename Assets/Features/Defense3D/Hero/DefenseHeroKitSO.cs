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
        public DefenseSkillDefinitionSO Relic => _relic;

        /// <summary>Gán hoặc thay đổi trang bị Pháp Bảo (Relic) được chọn từ Loadout ngoài Sảnh.</summary>
        public void SetRelic(DefenseSkillDefinitionSO relic)
        {
            _relic = relic;
        }

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
            if (_attack == null || _attack.Capture().Kind != DefenseSkillKind.BasicAttack) throw new InvalidOperationException("Hero kit requires basic attack definition.");
            if (_dash == null || _dash.Capture().Kind != DefenseSkillKind.Dash) throw new InvalidOperationException("Hero kit requires dash definition.");
            if (_signature == null || _signature.Capture().Kind != DefenseSkillKind.ScholarSignature) throw new InvalidOperationException("Hero kit requires scholar signature definition.");
            if (_relic != null && _relic.Capture().Kind != DefenseSkillKind.DepToOng) throw new InvalidOperationException("Hero kit relic must match relic kind.");
        }
    }
}
