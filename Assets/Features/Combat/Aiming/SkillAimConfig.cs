using System;
using UnityEngine;

namespace ProjectZombie.Features.Combat.Aiming
{
    /// <summary>
    /// Các dạng hình thái của chỉ dấu ngắm chiêu (Skill Indicator Types).
    /// </summary>
    public enum SkillAimType
    {
        None,           // Tự kích hoạt quanh thân hoặc không cần ngắm
        LineArrow,      // Đường thẳng / Mũi tên định hướng (Nỏ Thần, Cung, Kiếm Khí)
        ConeSector,     // Hình quạt nón (Trống Đồng, Đao Cửu Vĩ, Vệt Chém)
        CircleReticle,  // Vòng tròn điểm rơi AOE (Lựu Đạn, Nước Thánh, Nồi Cơm)
        SelfAOE,        // Vòng tròn hào quang dính cố định quanh chân người chơi (Aura / Khiên hộ thể)
        DashLine        // Đường lướt né đòn: Chỉ hướng lướt + Vòng tròn báo điểm đáp
    }

    /// <summary>
    /// Giao diện dịch vụ điều phối chỉ dấu kỹ năng (Service Contract)
    /// </summary>
    public interface ISkillAimService
    {
        void StartAim(SkillAimConfig config);
        void UpdateAim(Vector2 aimDirection, float pullPercent, bool isCancelHovered = false);
        void StopAim();
        void HideAll();
    }

    /// <summary>
    /// Cấu hình thông số ngắm bắn cho từng kỹ năng / vũ khí / đòn đánh.
    /// </summary>
    [Serializable]
    public struct SkillAimConfig
    {
        public SkillAimType aimType;
        [Tooltip("Tầm với tối đa của kỹ năng (Độ dài mũi tên / Khoảng cách ném AOE / Khoảng cách lướt)")]
        public float range;
        [Tooltip("Bán kính vùng ảnh hưởng (Dành cho Circle AOE, SelfAOE hoặc độ rộng quạt/mũi tên)")]
        public float radius;
        [Tooltip("Góc quét hình quạt (Độ - dành cho ConeSector)")]
        public float sectorAngle;
        [Tooltip("Tự động khóa mục tiêu quái vật khi bấm nhanh (Quick Tap)")]
        public bool autoTargetOnTap;

        public SkillAimConfig(SkillAimType type, float range, float radius = 1.0f, float sectorAngle = 60f, bool autoTarget = true)
        {
            this.aimType = type;
            this.range = range;
            this.radius = radius;
            this.sectorAngle = sectorAngle;
            this.autoTargetOnTap = autoTarget;
        }

        public static SkillAimConfig DefaultMelee => new SkillAimConfig(SkillAimType.ConeSector, 2.5f, 1.8f, 90f, true);
        public static SkillAimConfig DefaultRanged => new SkillAimConfig(SkillAimType.LineArrow, 7.0f, 0.8f, 0f, true);
        public static SkillAimConfig DefaultAOE => new SkillAimConfig(SkillAimType.CircleReticle, 6.0f, 2.2f, 0f, true);
        public static SkillAimConfig DefaultSelfAOE => new SkillAimConfig(SkillAimType.SelfAOE, 0f, 2.5f, 0f, false);
        public static SkillAimConfig DefaultDash => new SkillAimConfig(SkillAimType.DashLine, 4.5f, 0.8f, 0f, false);
        public static SkillAimConfig DefaultInstant => new SkillAimConfig(SkillAimType.None, 0f, 0f, 0f, false);
    }

    /// <summary>
    /// Giao diện cho bất kỳ Skill/Weapon nào hỗ trợ ngắm bắn định hướng MOBA.
    /// </summary>
    public interface IAimableSkill
    {
        SkillAimConfig AimConfig { get; }
    }
}
