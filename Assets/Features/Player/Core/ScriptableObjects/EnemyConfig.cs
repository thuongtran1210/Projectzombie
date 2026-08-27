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
        public float attackCooldown = 1.2f;
        public float attackRange = 2f;

        [Header("Animation & Hitbox Action Window (Đồng Bộ Nhịp Đánh)")]
        [Tooltip("Tỉ lệ thời gian vung đòn chuẩn bị trước khi Hitbox chạm vào Player (0.25 = 25% thời lượng đòn đánh)")]
        [Range(0.1f, 0.6f)] public float windupRatio = 0.25f;
        [Tooltip("Prefab VFX vệt chém/vồ/móng vuốt của quái")]
        public GameObject attackVfxPrefab;
        [Tooltip("Thời gian tồn tại của VFX quái (giây)")]
        public float vfxDuration = 0.35f;

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

        [Header("Status Effect Immunities (Miễn Kháng Hiệu Ứng)")]
        [Tooltip("Danh sách các hiệu ứng trạng thái bất lợi mà quái này hoàn toàn miễn nhiễm.")]
        public System.Collections.Generic.List<ProjectZombie.Features.Enemies.StatusEffectType> immuneStatuses = new System.Collections.Generic.List<ProjectZombie.Features.Enemies.StatusEffectType>();

        /// <summary>
        /// Kiểm tra xem quái có miễn nhiễm với loại hiệu ứng cụ thể nào không.
        /// </summary>
        public bool IsImmuneTo(ProjectZombie.Features.Enemies.StatusEffectType statusType)
        {
            if (immuneStatuses != null && immuneStatuses.Contains(statusType))
            {
                return true;
            }

            // Boss tự động miễn nhiễm với các hiệu ứng phá vỡ logic hành vi
            if (tier == EnemyTier.Boss)
            {
                if (statusType == ProjectZombie.Features.Enemies.StatusEffectType.RagdollFlight ||
                    statusType == ProjectZombie.Features.Enemies.StatusEffectType.Humiliated ||
                    statusType == ProjectZombie.Features.Enemies.StatusEffectType.Dancing)
                {
                    return true;
                }
            }

            return false;
        }

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
