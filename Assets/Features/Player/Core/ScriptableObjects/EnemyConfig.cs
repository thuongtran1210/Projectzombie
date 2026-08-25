using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Core.ScriptableObjects
{
    /// <summary>Phên loại cấp độ của kẻ địch.</summary>
    public enum EnemyTier { Common, Elite, Boss }

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

        [Header("Reward & Tier")]
        [Tooltip("Lượng EXP Gem rớt ra khi chết.")]
        public int expReward = 5;

        [Tooltip("Phân loại (dùng bởi Spawner để quản lý wave).")]
        public EnemyTier tier = EnemyTier.Common;

        [Header("Coin Loot Drop Settings")]
        [Tooltip("Tỉ lệ rơi Cổ Tiền khi quái chết (0.0 = 0%, 1.0 = 100%).")]
        [Range(0f, 1f)] public float coinDropRate = 0.25f;

        [Tooltip("Lượng Cổ Tiền tối thiểu khi rơi.")]
        public int minCoinDrop = 1;

        [Tooltip("Lượng Cổ Tiền tối đa khi rơi.")]
        public int maxCoinDrop = 3;

        [Header("Vong Xuyen Attributes (v4.0)")]
        [Tooltip("Thuộc tính Ngũ Hành của Yêu Ma")]
        public ElementType elementType = ElementType.None;

        [Header("Armor & Defense")]
        [Tooltip("Cơ chế Cản Đạn Xuyên (Heavy Armor Bullet Sponge) - Tiêu tốn 2 Pierce Count của đạn xuyên.")]
        public bool isHeavyArmor = false;

        public float AttackSpeed => 1f / attackCooldown;
        public float CritChance => 0f;
        public float AttackRange => attackRange;
        public float GetTotalDamage() => damageToPlayer;

        private void OnValidate()
        {
            // Với quái cận chiến (tầm đánh <= 2.5m), minDistance và preferredDistance không được vượt quá attackRange
            if (attackRange <= 2.5f && preferredDistance > attackRange)
            {
                preferredDistance = 0f;
                minDistance = 0f;
            }
        }
    }
}
