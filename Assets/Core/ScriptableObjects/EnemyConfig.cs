using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Core.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewEnemyConfig", menuName = "ProjectZombie/Enemy Config", order = 1)]
    public class EnemyConfig : ScriptableObject, ICharacterStats
    {
        [Header("Movement")]
        public float moveSpeed = 3f;
        
        [Tooltip("Khoảng cách giữ cự ly với mục tiêu (dành cho Ranged)")]
        public float preferredDistance = 5f;
        
        [Tooltip("Khoảng cách tối thiểu trước khi lùi lại (dành cho Ranged)")]
        public float minDistance = 3f;

        [Header("Combat")]
        public float maxHealth = 50f;
        public float damageToPlayer = 10f;
        public float attackCooldown = 1f;
        public float attackRange = 2f;

        public float AttackSpeed => 1f / attackCooldown;
        public float CritChance => 0f;
        public float AttackRange => attackRange;
        public float GetTotalDamage() => damageToPlayer;
    }
}
