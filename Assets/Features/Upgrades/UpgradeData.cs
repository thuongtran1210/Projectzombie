using UnityEngine;

namespace ProjectZombie.Features.Upgrades
{
    public enum UpgradeType
    {
        WeaponUpgrade,
        SignatureSkillUpgrade,
        CommonUpgrade,
        FactionCounterUpgrade,
        RareUpgrade,
        EvolutionUpgrade
    }

    [System.Serializable]
    public struct WeaponStatModifier
    {
        public float damageBonus;
        public float attackSpeedBonus;
        public int projectileCountBonus;
        public int pierceBonus;
        public float scaleBonus;
        public float critChanceBonus;
        public float critDamageBonus;
        public float projectileSpeedBonus;
    }

    [System.Serializable]
    public struct PlayerStatModifier
    {
        public float maxHealthBonus;
        public float moveSpeedBonus;
        public float critChanceBonus;
        public float baseDamageBonus;
        public float pickupRangeBonus;
        public float expMultiplierBonus;
    }

    /// <summary>
    /// Base class cho tất cả các loại thẻ nâng cấp.
    /// </summary>
    public abstract class UpgradeData : ScriptableObject
    {
        [Header("Display Info")]
        public string upgradeName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Upgrade Settings")]
        [Tooltip("Type chỉ dùng cho mục đích phân loại hiển thị trên UI")]
        public UpgradeType upgradeType;
        [Tooltip("Trọng số xuất hiện (càng cao càng dễ ra)")]
        public float spawnWeight = 1f;

        /// <summary>
        /// Kiểm tra xem thẻ này có đủ điều kiện để xuất hiện trong lượt roll hiện tại không.
        /// </summary>
        public abstract bool IsAvailable(GameObject player);

        /// <summary>
        /// Thực thi hiệu ứng của thẻ khi người chơi chọn.
        /// </summary>
        public abstract void ApplyUpgrade(GameObject player);
    }
}
