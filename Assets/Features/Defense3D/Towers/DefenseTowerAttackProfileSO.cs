using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Immutable normal/special attack configuration. Cooldowns and counters belong to each tower.</summary>
    [CreateAssetMenu(menuName = "ProjectZombie/Defense3D/Tower Attack")]
    public sealed class DefenseTowerAttackProfileSO : ScriptableObject
    {
        [SerializeField] private DefenseProjectileDefinitionSO _projectile;
        [SerializeField] private SO_AttackEffect[] _effects = Array.Empty<SO_AttackEffect>();
        [SerializeField, Min(1)] private int _shots = 1;
        [SerializeField, Min(0f)] private float _damageMultiplier = 1f;
        [SerializeField, Min(0f)] private float _cooldown;
        [SerializeField, Min(1)] private int _unlockLevel = 1;
        public DefenseProjectileDefinitionSO Projectile => _projectile;
        public SO_AttackEffect[] Effects => _effects;
        public int Shots => Mathf.Max(1, _shots);
        public float DamageMultiplier => _damageMultiplier;
        public float Cooldown => _cooldown;
        public int UnlockLevel => _unlockLevel;
    }
}
