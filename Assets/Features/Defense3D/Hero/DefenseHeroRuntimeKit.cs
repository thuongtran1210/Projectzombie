using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Runtime wrapper cho Hero Kit trong trận đánh.
    /// Kế thừa toàn bộ skill cơ bản (Attack, Dash, Signature) từ authored DefenseHeroKitSO
    /// nhưng liên kết động Relic được chọn từ Loadout mà KHÔNG cần ScriptableObject.Instantiate / clone SO.
    /// Giúp loại bỏ hoàn toàn GC allocation và rò rỉ bộ nhớ runtime.
    /// </summary>
    public sealed class DefenseHeroRuntimeKit : IDefenseHeroKit
    {
        private readonly DefenseHeroKitSO _authoredKit;
        private DefenseSkillDefinitionSO _relic;

        public DefenseHeroRuntimeKit(DefenseHeroKitSO authoredKit, DefenseSkillDefinitionSO relic = null)
        {
            _authoredKit = authoredKit ?? throw new ArgumentNullException(nameof(authoredKit));
            _relic = relic;
        }

        public string HeroId => _authoredKit.HeroId;
        public string DisplayName => _authoredKit.DisplayName;
        public DefenseSkillDefinitionSO Attack => _authoredKit.Attack;
        public DefenseSkillDefinitionSO Dash => _authoredKit.Dash;
        public DefenseSkillDefinitionSO Signature => _authoredKit.Signature;
        public DefenseSkillDefinitionSO Relic => _relic;
        public DefenseHeroKitSO AuthoredKit => _authoredKit;

        public void SetRelic(DefenseSkillDefinitionSO relic)
        {
            _relic = relic;
        }

        public DefenseSkillDefinitionSO DefinitionFor(DefenseSkillKind kind)
        {
            switch (kind)
            {
                case DefenseSkillKind.BasicAttack: return _authoredKit.Attack;
                case DefenseSkillKind.Dash: return _authoredKit.Dash;
                case DefenseSkillKind.Signature: return _authoredKit.Signature;
                case DefenseSkillKind.Relic: return _relic;
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        public void Validate()
        {
            _authoredKit.Validate();
            if (_relic != null && _relic.Capture().Kind != DefenseSkillKind.Relic && _relic.Capture().Kind != DefenseSkillKind.DepToOng)
            {
                throw new InvalidOperationException("Hero kit relic must match relic kind.");
            }
        }
    }
}
