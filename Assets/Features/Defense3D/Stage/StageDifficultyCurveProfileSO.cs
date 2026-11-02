using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Phân loại tính chất nhịp độ của từng đợt sóng hoặc giai đoạn trong trận chiến.
    /// </summary>
    public enum StagePacingType
    {
        [InspectorName("Đợt Sóng Tiêu Chuẩn (Normal)")]
        Normal = 0,

        [InspectorName("Bầy Quái Tràn Ngập (Horde / Swarm Rush)")]
        HordeRush = 1,

        [InspectorName("Quái Tinh Anh Đột Kích (Elite Miniboss)")]
        EliteMiniboss = 2,

        [InspectorName("Khoảng Lặng Nghỉ Hơi (Breather / Recovery)")]
        Breather = 3,

        [InspectorName("Quyết Chiến Đại Boss (Climax Arena)")]
        ClimaxBoss = 4
    }

    /// <summary>
    /// Định nghĩa một mốc biến cố quan trọng trên dòng thời gian trận đấu.
    /// </summary>
    [Serializable]
    public sealed class StagePacingMilestone
    {
        [Tooltip("Mã định danh mốc thời gian (Ví dụ: horde-01, elite-01)")]
        public string milestoneId = "milestone-01";

        [Tooltip("Tên biến cố hiển thị cho Designer (Ví dụ: Đợt Thủy Tộc Đột Kích)")]
        public string displayName = "Đợt Biến Cố";

        [Tooltip("Thời điểm bắt đầu biến cố tính từ đầu trận (giây)")]
        [Min(0f)] public float triggerTime = 120f;

        [Tooltip("Thời lượng diễn ra biến cố này (giây)")]
        [Min(1f)] public float duration = 30f;

        [Tooltip("Phân loại nhịp độ biến cố")]
        public StagePacingType pacingType = StagePacingType.HordeRush;

        [Tooltip("Tăng thêm tỉ lệ spawn trong khoảng biến cố này (hệ số cộng dồn)")]
        [Range(0.5f, 3.0f)] public float intensityMultiplier = 1.5f;

        [Tooltip("Ghi chú định hướng thiết kế cho Designer")]
        [TextArea(1, 2)] public string designNote = "";
    }

    /// <summary>
    /// Hồ sơ cấu hình đường cong độ khó & nhịp độ trận đấu (Stage Difficulty Curve Profile).
    /// Cho phép Game Designer kiểm soát toàn bộ bài toán cân bằng game:
    /// - Nhịp độ xuất hiện quái theo thời gian (Spawn Rate Pacing)
    /// - Độ 'trâu' tăng dần của đàn quái (HP Scaling Curve)
    /// - Ngân sách kinh tế Cổ Mộc người chơi thu thập được (Economy Projection)
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultyCurve_Stage01", menuName = "ProjectZombie/Defense3D/Difficulty Curve Profile")]
    public sealed class StageDifficultyCurveProfileSO : ScriptableObject
    {
        [Header("── 1. ĐƯỜNG CONG MẬT ĐỘ QUÁI (Spawn Rate Curve) ──")]
        [Tooltip("Tốc độ sinh quái (quái / phút) từ 0:00 (0s) đến 10:00 (600s). Khuyên dùng EaseInOut hoặc S-Curve.")]
        [SerializeField]
        private AnimationCurve spawnRateCurve = new AnimationCurve(
            new Keyframe(0f, 12f, 0f, 0.05f),      // Phút 0: 12 quái/phút (làm quen)
            new Keyframe(180f, 28f, 0.1f, 0.1f),   // Phút 3: 28 quái/phút (tăng dần)
            new Keyframe(360f, 48f, 0.15f, 0.15f), // Phút 6: 48 quái/phút (cao điểm)
            new Keyframe(510f, 75f, 0.2f, 0f)      // Phút 8.5: 75 quái/phút (trước Boss)
        );

        [Header("── 2. ĐƯỜNG CONG TĂNG TRƯỞNG MÁU (HP Scaling Curve) ──")]
        [Tooltip("Hệ số nhân chỉ số Máu tối đa của quái vật theo thời gian (1.0x lúc đầu -> 3.5x cuối trận).")]
        [SerializeField]
        private AnimationCurve hpScalingCurve = new AnimationCurve(
            new Keyframe(0f, 1.0f, 0f, 0.002f),
            new Keyframe(300f, 1.8f, 0.003f, 0.003f),
            new Keyframe(510f, 3.2f, 0.005f, 0f)
        );

        [Header("── 3. DỰ PHÓNG KINH TẾ CỔ MỘC (Economy Curve) ──")]
        [Tooltip("Dự phóng tổng lượng Cổ Mộc tích lũy kỳ vọng mà người chơi kiếm được theo thời gian (phục vụ nâng trụ).")]
        [SerializeField]
        private AnimationCurve woodIncomeCurve = new AnimationCurve(
            new Keyframe(0f, 30f, 0f, 2f),
            new Keyframe(180f, 400f, 3f, 3f),
            new Keyframe(360f, 1100f, 4f, 4f),
            new Keyframe(510f, 2200f, 5f, 0f)
        );

        [Header("── 4. CÁC MỐC BIẾN CỐ CAO TRÀO & NGHỈ HƠI (Milestones) ──")]
        [Tooltip("Danh sách các sự kiện đặc biệt trên dòng thời gian (Horde quái tràn qua cầu, quái tinh anh đột kích...)")]
        [SerializeField]
        private List<StagePacingMilestone> pacingMilestones = new List<StagePacingMilestone>
        {
            new StagePacingMilestone
            {
                milestoneId = "horde-01",
                displayName = "Đợt Thủy Quái Tràn Kênh",
                triggerTime = 120f,
                duration = 25f,
                pacingType = StagePacingType.HordeRush,
                intensityMultiplier = 1.6f,
                designNote = "Thử thách phản xạ đầu tiên: Thủy Quái tràn qua Kênh giữa buộc người chơi dựng Tháp Nỏ."
            },
            new StagePacingMilestone
            {
                milestoneId = "breather-01",
                displayName = "Khoảng Lặng Sau Đợt Sóng",
                triggerTime = 150f,
                duration = 20f,
                pacingType = StagePacingType.Breather,
                intensityMultiplier = 0.5f,
                designNote = "Giảm nhịp để người chơi và Phu Tạp Dịch thu nhặt Cổ Mộc tu sửa căn cứ."
            },
            new StagePacingMilestone
            {
                milestoneId = "elite-01",
                displayName = "Ma Giáp Phi Cương Đột Kích",
                triggerTime = 270f,
                duration = 35f,
                pacingType = StagePacingType.EliteMiniboss,
                intensityMultiplier = 1.4f,
                designNote = "Quái Phi Cương dẫn đầu đàn tiến công bọc sườn từ Bờ Nam."
            },
            new StagePacingMilestone
            {
                milestoneId = "horde-02",
                displayName = "Tổng Lực Thủy Lục Tiến Công",
                triggerTime = 420f,
                duration = 45f,
                pacingType = StagePacingType.HordeRush,
                intensityMultiplier = 1.8f,
                designNote = "Áp lực cực đại trên cả 3 làn trước khi bước vào giai đoạn quyết chiến Đại Boss."
            }
        };

        public AnimationCurve SpawnRateCurve => spawnRateCurve;
        public AnimationCurve HpScalingCurve => hpScalingCurve;
        public AnimationCurve WoodIncomeCurve => woodIncomeCurve;
        public IReadOnlyList<StagePacingMilestone> PacingMilestones => pacingMilestones;

        /// <summary>
        /// Tính toán tốc độ sinh quái (quái / phút) tại thời điểm chỉ định, đã tính cả hệ số biến cố nếu có.
        /// </summary>
        public float EvaluateSpawnRate(float timeSeconds)
        {
            float baseRate = Mathf.Max(1f, spawnRateCurve.Evaluate(timeSeconds));
            var activeMilestone = GetActiveMilestone(timeSeconds);
            if (activeMilestone != null)
            {
                baseRate *= activeMilestone.intensityMultiplier;
            }
            return baseRate;
        }

        /// <summary>
        /// Đánh giá hệ số máu của quái tại thời điểm chỉ định.
        /// </summary>
        public float EvaluateHpScaling(float timeSeconds)
        {
            return Mathf.Max(0.5f, hpScalingCurve.Evaluate(timeSeconds));
        }

        /// <summary>
        /// Đánh giá lượng Cổ Mộc người chơi kỳ vọng đã kiếm được đến thời điểm chỉ định.
        /// </summary>
        public float EvaluateExpectedWood(float timeSeconds)
        {
            return Mathf.Max(0f, woodIncomeCurve.Evaluate(timeSeconds));
        }

        /// <summary>
        /// Tra cứu xem tại thời điểm này có đang diễn ra biến cố đặc biệt nào không.
        /// </summary>
        public StagePacingMilestone GetActiveMilestone(float timeSeconds)
        {
            if (pacingMilestones == null) return null;
            for (int i = 0; i < pacingMilestones.Count; i++)
            {
                var m = pacingMilestones[i];
                if (m != null && timeSeconds >= m.triggerTime && timeSeconds <= (m.triggerTime + m.duration))
                {
                    return m;
                }
            }
            return null;
        }
    }
}
