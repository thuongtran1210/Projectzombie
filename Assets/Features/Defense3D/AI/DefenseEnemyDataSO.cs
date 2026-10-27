using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    public enum DefenseEnemyLocomotionType
    {
        GroundNavMesh,
        AmphibianWaterToShore,
        AirFlight,
        ParatrooperLanding
    }

    /// <summary>
    /// U3D-006: Data-driven definition for Enemies in TREBRAVE Defense 3D.
    /// Preserves exact source IDs and benchmark attributes from Stage01 & Vongxuyen contracts.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "TREBRAVE/Defense3D/EnemyData")]
    public sealed class DefenseEnemyDataSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique ID matching Vongxuyen: magiap, thuyquai, huyetdoi, quaidu, phicuong, magiap_boss, yaksha_boss")]
        [SerializeField] private string enemyId = "magiap";
        [SerializeField] private string displayName = "Ma Giáp";
        [SerializeField] private DefenseEnemyLocomotionType locomotionType = DefenseEnemyLocomotionType.GroundNavMesh;
        [SerializeField] private bool isBossOrMiniboss;
        [SerializeField] private GameObject prefab;

        [Header("Base Stats (Unscaled)")]
        [SerializeField, Min(1f)] private float maxHP = 130f;
        [SerializeField, Min(0.1f)] private float moveSpeed = 1.45f;
        [SerializeField, Min(0f)] private float attackDamage = 16f;
        [SerializeField, Min(0.1f)] private float attackInterval = 2f;
        [SerializeField, Min(0.1f)] private float attackRange = 1.2f;

        [Header("Economy Rewards")]
        [SerializeField, Min(0)] private int woodDrop = 10;
        [SerializeField, Min(0)] private int expDrop = 5;

        public string EnemyId => enemyId;
        public string DisplayName => displayName;
        public DefenseEnemyLocomotionType LocomotionType => locomotionType;
        public bool IsBossOrMiniboss => isBossOrMiniboss;
        public GameObject Prefab => prefab;
        public float MaxHP => maxHP;
        public float MoveSpeed => moveSpeed;
        public float AttackDamage => attackDamage;
        public float AttackInterval => attackInterval;
        public float AttackRange => attackRange;
        public int WoodDrop => woodDrop;
        public int ExpDrop => expDrop;

        // Copy values per spawn; damage and scaling never mutate the shared asset.
        public DefenseEnemyStats CreateStats()
        {
            return new DefenseEnemyStats(enemyId, displayName, locomotionType, maxHP,
                moveSpeed, attackDamage, woodDrop, expDrop, isBossOrMiniboss)
            {
                AttackInterval = attackInterval,
                AttackRange = attackRange
            };
        }

        /// <summary>
        /// Calculate scaled stats according to wave/phase multiplier.
        /// Benchmark formula from Web runtime: 1 + (phase - 1) * 0.18f
        /// </summary>
        public float GetScaledHP(int phase)
        {
            float mult = 1f + Mathf.Max(0, phase - 1) * 0.18f;
            return maxHP * mult;
        }
    }
}
