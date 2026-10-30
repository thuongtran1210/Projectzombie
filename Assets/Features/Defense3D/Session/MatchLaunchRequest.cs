using System;
using ProjectZombie.Features.Maps;
using ProjectZombie.Features.Defense3D.Loadout;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Validated, immutable launch intent. Never contains a scene object reference.</summary>
    public sealed class MatchLaunchRequest
    {
        public StageDefinitionSO Stage { get; }
        public DefenseLoadoutSnapshot Loadout { get; }
        public string RunId { get; }
        public StageSpawnConfig ResolvedRules { get; }

        public MatchLaunchRequest(StageDefinitionSO stage, DefenseLoadoutSnapshot loadout)
        {
            if (stage == null || !stage.CanLaunchDefense)
                throw new ArgumentException("Ải chưa có map hoặc cấu hình Defense hợp lệ.", nameof(stage));
            Stage = stage;
            Loadout = loadout ?? throw new ArgumentNullException(nameof(loadout));
            RunId = Guid.NewGuid().ToString("N");
        }

        private MatchLaunchRequest(MatchLaunchRequest intent, StageSpawnConfig rules)
        {
            Stage = intent.Stage;
            Loadout = intent.Loadout;
            RunId = intent.RunId;
            ResolvedRules = rules;
        }

        public MatchLaunchRequest ResolveRules(StageSpawnConfig rules)
        {
            if (Stage.DefenseBlueprint == null && rules == null) throw new ArgumentNullException(nameof(rules));
            return new MatchLaunchRequest(this, rules);
        }
    }
}
