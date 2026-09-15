using UnityEngine;

namespace ProjectZombie.Features.Upgrades
{
    public enum UpgradeType
    {
        // --- Nhóm Vũ Khí & Khí Vận (Khớp với Asset YAML 0, 1, 2) ---
        WeaponUpgrade = 0,        // Vũ khí cơ bản (W001-W012)
        SynergyTrait = 1,         // Thần Binh Thuật / Thẻ Nhánh Độc Quyền của Lõi (TRAIT_PD_...)
        CommonUpgrade = 2,        // Bổ trợ chỉ số cơ bản / Khí Vận (P001-P012)

        // --- Nhóm Thần Thoại & Tuyệt Kỹ ---
        MythicCore = 3,           // Đại Lõi Thần Thoại (Khởi đầu trận Level 1)
        RareUpgrade = 4,          // Bổ trợ hiếm / Chuyển đổi chỉ số
        EvolutionUpgrade = 5,     // Tiến hóa vũ khí thành Thần Binh Tối Thượng (E001-E012)
        RelicFusion = 6,          // Luyện hóa & Gộp thẻ tạo Pháp bảo Thần Binh
        BreakthroughUltimate = 7, // Bí tịch đột phá tuyệt kỹ (Mốc Level 6 & 12)

        // --- Nhóm Bổ Trợ & Thao Tác (Action RPG) ---
        ComboAugment = 8,         // Bí kíp biến hóa chuỗi đòn chém (Combo 1-2-3)
        DashTrait = 9,            // Cường hóa kỹ năng Lướt (Tàn ảnh, Đốt cháy, Kháng đòn)
        ConditionalPassive = 10,  // Nội tại tình huống (Trảm hậu, Cuồng nộ, Hành quyết)
        RelicAwakening = 11       // Thức tỉnh Pháp bảo hộ thân
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
        public float attackSpeedBonus;
        public float dashCooldownReduction;
        public float dashSpeedBonus;
        public float areaScaleBonus;
        public float fireDamageBonus;
    }

    /// <summary>
    /// Base class cho tất cả các loại thẻ nâng cấp.
    /// Hỗ trợ cả PlayerContext (chuẩn Clean Architecture mới) và GameObject (tương thích ngược).
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

        [Tooltip("Hệ Ngũ Hành của thẻ nâng cấp này (nếu có)")]
        public ProjectZombie.Features.Shared.ElementType element = ProjectZombie.Features.Shared.ElementType.None;

        /// <summary>
        /// Kiểm tra xem thẻ này có đủ điều kiện xuất hiện qua PlayerContext (Khuyên dùng - 0 GetComponent).
        /// </summary>
        public virtual bool IsAvailable(ProjectZombie.Features.Player.PlayerContext context)
        {
            return context != null && IsAvailable(context.GameObject);
        }

        /// <summary>
        /// Thực thi hiệu ứng thẻ qua PlayerContext (Khuyên dùng - 0 GetComponent).
        /// </summary>
        public virtual void ApplyUpgrade(ProjectZombie.Features.Player.PlayerContext context)
        {
            if (context != null)
            {
                ApplyUpgrade(context.GameObject);
            }
        }

        /// <summary>
        /// Kiểm tra tương thích ngược với GameObject thô.
        /// </summary>
        public virtual bool IsAvailable(GameObject player)
        {
            if (player == null) return false;
            return true;
        }

        /// <summary>
        /// Thực thi tương thích ngược với GameObject thô.
        /// </summary>
        public virtual void ApplyUpgrade(GameObject player)
        {
            if (player != null)
            {
                ApplyUpgrade(ProjectZombie.Features.Player.PlayerContext.Create(player));
            }
        }

        /// <summary>
        /// Trả về chuỗi hiển thị Thể Loại thẻ trên UI (VD: [THẦN PHÁP], [BÍ KÍP ĐÒN CHÉM],...)
        /// </summary>
        public virtual string GetCategoryDisplayName()
        {
            return "<color=#1B4D7E><b>[BỔ TRỢ KHÍ VẬN]</b></color>";
        }

        /// <summary>
        /// Trả về chuỗi hiển thị Cấp độ của thẻ dựa trên trạng thái hiện tại của Player (VD: Cấp 2/5, TIẾN HÓA, MỚI!)
        /// </summary>
        public virtual string GetLevelDisplayName(GameObject player)
        {
            return string.Empty;
        }

        /// <summary>
        /// Hệ số nhân trọng số xuất hiện động qua PlayerContext.
        /// </summary>
        public virtual float GetDynamicWeightMultiplier(ProjectZombie.Features.Player.PlayerContext context)
        {
            return context != null ? GetDynamicWeightMultiplier(context.GameObject) : 1.0f;
        }

        /// <summary>
        /// Hệ số nhân trọng số xuất hiện động (Dynamic Weight Multiplier) của thẻ này trong lượt roll.
        /// </summary>
        public virtual float GetDynamicWeightMultiplier(GameObject player)
        {
            return 1.0f;
        }
    }
}
