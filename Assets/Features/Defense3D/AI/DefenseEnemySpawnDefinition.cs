using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Scalar and prefab-reference snapshot captured when the stage is compiled.</summary>
    public sealed class DefenseEnemySpawnDefinition
    {
        public DefenseEnemyDataSO Source { get; }
        public GameObject Prefab { get; }
        public DefenseEnemyStats Stats { get; }
        public string EnemyId => Stats.EnemyId;

        public DefenseEnemySpawnDefinition(DefenseEnemyDataSO source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.Prefab == null) throw new ArgumentException("Enemy definition needs an authored prefab: " + source.EnemyId);
            Source = source;
            Prefab = source.Prefab;
            Stats = source.CreateStats();
            if (string.IsNullOrWhiteSpace(Stats.EnemyId) || !Positive(Stats.MaxHp) || !NonNegative(Stats.Speed) ||
                !NonNegative(Stats.Damage) || !Positive(Stats.AttackInterval) || !NonNegative(Stats.AttackRange) ||
                Stats.WoodReward < 0 || Stats.ExpReward < 0 || !Enum.IsDefined(typeof(DefenseEnemyLocomotionType), Stats.Locomotion))
                throw new ArgumentException("Enemy definition has invalid scalar stats: " + source.EnemyId);
        }

        private static bool Positive(float value) => DefenseDamageRules.IsFinite(value) && value > 0f;
        private static bool NonNegative(float value) => DefenseDamageRules.IsFinite(value) && value >= 0f;
    }
}
