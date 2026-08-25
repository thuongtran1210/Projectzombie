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
        EvolutionUpgrade,
        // --- Nhóm Thẻ Action RPG Roguelite (GDD v5.0) ---
        ComboAugment,        // Bí kíp biến hóa chuỗi đòn chém (Combo 1-2-3)
        RelicAwakening,      // Thức tỉnh & cường hóa Pháp bảo hộ thân đã mang theo
        DashTrait,           // Cường hóa kỹ năng Lướt (Dash Cancel, Tàn ảnh, Parry)
        ConditionalPassive,  // Nội tại tình huống (Trảm hậu, Cuồng nộ, Hành quyết)
        BreakthroughUltimate // Bí tịch đột phá tuyệt kỹ (Mốc Level 5 & 10)
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
        [Tooltip("Mã định danh duy nhất (VD: P001, W001_Lv2, E001)")]
        public string id;
        public string upgradeName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Upgrade Settings")]
        [Tooltip("Type chỉ dùng cho mục đích phân loại hiển thị trên UI")]
        public UpgradeType upgradeType;
        [Tooltip("Trọng số xuất hiện (càng cao càng dễ ra)")]
        public float spawnWeight = 1f;

        [Tooltip("Cấp độ tối đa của nâng cấp này (0 = Không giới hạn cấp)")]
        public int maxLevel = 0;


        // TODO FIX: Trạng thái âm dương chỉ phụ thuộc vào nhân vật Đạo sĩ các nhân vật khác không cần                                                                                                             
        [Header("Vong Xuyen Requirements (v4.0)")]
        [Tooltip("Yêu cầu trạng thái Âm Dương để thẻ xuất hiện")]
        public ProjectZombie.Features.YinYang.YinYangState requiredYinYangState = ProjectZombie.Features.YinYang.YinYangState.Balanced;
        
        [Tooltip("Cờ bật bắt buộc phải thỏa mãn đúng YinYangState")]
        public bool checkYinYangState = false;

        [Tooltip("Hệ Ngũ Hành của thẻ nâng cấp này (nếu có)")]
        public ProjectZombie.Features.Shared.ElementType element = ProjectZombie.Features.Shared.ElementType.None;

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
