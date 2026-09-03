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
        DashLine,       // Đường lướt né đòn: Chỉ hướng lướt + Vòng tròn báo điểm đáp
        VectorWall,     // Chỉ dấu dựng tường chắn / vạch đường ngăn cách (Nước Thánh, Điếu Cày)
        CurvedTrajectory, // Chỉ dấu quỹ đạo ném cong Boomerang/Parabol (Dép Tổ Ong, Phi Tiêu)
        RhythmPulse     // Vòng tròn co giãn theo nhịp QTE (Trống Đồng Đông Sơn)
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
        [Tooltip("Bán kính vùng ảnh hưởng (Dành cho Circle AOE, SelfAOE hoặc độ rộng quạt/mũi tên/tường)")]
        public float radius;
        [Tooltip("Góc quét hình quạt (Độ - dành cho ConeSector) hoặc độ cong quỹ đạo (CurvedTrajectory)")]
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

        // Semantic Helpers giải nghĩa trường dữ liệu rõ ràng theo từng loại hình
        public float WallLength => radius > 0.1f ? radius : 5.2f;
        public float WallThickness => sectorAngle > 0.1f ? sectorAngle : 1.8f;
        public float AOERadius => radius > 0.1f ? radius : 2.0f;
        public float ConeAngle => sectorAngle > 0.1f ? sectorAngle : 90f;
        public float ConeReach => range > 0.1f ? range : 2.5f;

        public static SkillAimConfig DefaultMelee => new SkillAimConfig(SkillAimType.ConeSector, 2.5f, 1.8f, 90f, true);
        public static SkillAimConfig DefaultRanged => new SkillAimConfig(SkillAimType.LineArrow, 7.0f, 0.8f, 0f, true);
        public static SkillAimConfig DefaultAOE => new SkillAimConfig(SkillAimType.CircleReticle, 6.0f, 2.2f, 0f, true);
        public static SkillAimConfig DefaultSelfAOE => new SkillAimConfig(SkillAimType.SelfAOE, 0f, 2.5f, 0f, false);
        public static SkillAimConfig DefaultDash => new SkillAimConfig(SkillAimType.DashLine, 4.5f, 0.8f, 0f, false);
        public static SkillAimConfig DefaultVectorWall => new SkillAimConfig(SkillAimType.VectorWall, 6.0f, 4.5f, 1.8f, true);
        public static SkillAimConfig DefaultCurvedTrajectory => new SkillAimConfig(SkillAimType.CurvedTrajectory, 6.5f, 1.5f, 45f, true);
        public static SkillAimConfig DefaultRhythmPulse => new SkillAimConfig(SkillAimType.RhythmPulse, 0f, 5.0f, 0f, false);
        public static SkillAimConfig DefaultInstant => new SkillAimConfig(SkillAimType.None, 0f, 0f, 0f, false);

        // Factory Methods tạo cấu hình trực quan chuẩn mực
        public static SkillAimConfig CreateVectorWall(float spawnDistance, float wallLength, float wallThickness = 1.8f, bool autoTarget = true)
        {
            return new SkillAimConfig(SkillAimType.VectorWall, spawnDistance, wallLength, wallThickness, autoTarget);
        }

        public static SkillAimConfig CreateCircleReticle(float maxCastRange, float aoeRadius, bool autoTarget = true)
        {
            return new SkillAimConfig(SkillAimType.CircleReticle, maxCastRange, aoeRadius, 0f, autoTarget);
        }

        public static SkillAimConfig CreateCone(float reach, float width, float arcAngle = 90f, bool autoTarget = true)
        {
            return new SkillAimConfig(SkillAimType.ConeSector, reach, width, arcAngle, autoTarget);
        }

        public static SkillAimConfig CreateLine(float length, float width = 1.0f, bool autoTarget = true)
        {
            return new SkillAimConfig(SkillAimType.LineArrow, length, width, 0f, autoTarget);
        }
    }

    /// <summary>
    /// Giao diện cho bất kỳ Skill/Weapon nào hỗ trợ ngắm bắn định hướng MOBA.
    /// </summary>
    public interface IAimableSkill
    {
        SkillAimConfig AimConfig { get; }
    }
}
