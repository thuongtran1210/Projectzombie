using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    public enum DefenseMinionRole { Militia, Gatherer, Healer }
    public enum DefenseMinionOrder { Defend, Follow, Advance }

    /// <summary>Captured recruitment and role statistics from Vongxuyen's MinionFactory/MinionManager.</summary>
    [CreateAssetMenu(menuName = "TREBRAVE/Defense3D/Minion Definition")]
    public sealed class DefenseMinionDefinitionSO : ScriptableObject
    {
        [SerializeField] private string _id;
        public string Id => _id;
        [SerializeField] private string _displayName;
        public string DisplayName => _displayName;
        [SerializeField] private DefenseMinionRole _role;
        public DefenseMinionRole Role => _role;
        [Min(1)] [SerializeField] private int _woodCost;
        public int WoodCost => _woodCost;
        [Min(1)] [SerializeField] private float _maxHp;
        public float MaxHp => _maxHp;
        [Min(0.1f)] [SerializeField] private float _speed;
        public float Speed => _speed;
        [Min(0)] [SerializeField] private float _damage;
        public float Damage => _damage;
        [Min(0.1f)] [SerializeField] private float _attackRange;
        public float AttackRange => _attackRange;
        [Min(0.1f)] [SerializeField] private float _attackInterval = 1f;
        public float AttackInterval => _attackInterval;
        [Min(0.1f)] [SerializeField] private float _thinkInterval = 0.2f;
        public float ThinkInterval => _thinkInterval;
        [Min(1f)] [SerializeField] private float _searchRange = 8f;
        public float SearchRange => _searchRange;
        [Min(1f)] [SerializeField] private float _healAmount = 45f;
        public float HealAmount => _healAmount;
        [Min(1f)] [SerializeField] private float _heroHealAmount = 28f;
        public float HeroHealAmount => _heroHealAmount;
        [Min(0.1f)] [SerializeField] private float _healInterval = 2.6f;
        public float HealInterval => _healInterval;
        [Min(0)] [SerializeField] private int _carryCapacity = 60;
        public int CarryCapacity => _carryCapacity;
        [Min(0)] [SerializeField] private float _slowStrength = 0.5f;
        public float SlowStrength => _slowStrength;
        [Min(0)] [SerializeField] private float _slowDuration = 2.2f;
        public float SlowDuration => _slowDuration;
        [Min(0)] [SerializeField] private float _heavyStunDuration = 1.25f;
        public float HeavyStunDuration => _heavyStunDuration;
        [SerializeField, Min(0)] private float _heavyDamage = 14f;
        public float HeavyDamage => _heavyDamage;
        [SerializeField, Min(0)] private float _knockStrength = .325f;
        public float KnockStrength => _knockStrength;
        [SerializeField, Min(.01f)] private float _knockDuration = .2f;
        public float KnockDuration => _knockDuration;
        [SerializeField, Min(1f)] private float _repairAmount = 25f;
        public float RepairAmount => _repairAmount;
        [SerializeField, Min(1f)] private float _baseRepairAmount = 15f;
        public float BaseRepairAmount => _baseRepairAmount;
        [SerializeField, Min(.1f)] private float _repairInterval = .95f;
        public float RepairInterval => _repairInterval;
    }
}
