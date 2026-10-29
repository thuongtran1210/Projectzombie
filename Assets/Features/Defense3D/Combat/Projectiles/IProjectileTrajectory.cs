using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    public enum DefenseTrajectoryType
    {
        ParabolicHoming = 0,    // Bay parabol bám đuổi mục tiêu di động (Tháp bắn tên)
        StraightDirectional = 1, // Bay thẳng theo hướng chỉ định / skillshot (Tướng bắn)
        BallisticGround = 2      // Rơi cầu vồng xuống mặt đất (Pháo cối nổ diện rộng)
    }

    /// <summary>
    /// Chiến lược tính toán quỹ đạo bay của đạn (Strategy Pattern).
    /// Đảm bảo logic toán học thuần túy, không cấp phát bộ nhớ rác (0 GC Allocations).
    /// </summary>
    public interface IProjectileTrajectory
    {
        DefenseTrajectoryType Type { get; }
        Vector3 EvaluatePosition(Vector3 start, Vector3 end, float progress, float arcHeight);
        Vector3 EvaluateDirection(Vector3 start, Vector3 end, float progress, float arcHeight);
    }

    /// <summary>
    /// Quỹ đạo bay parabol bám đuổi mục tiêu (dành cho Tháp canh bắn tên, mũi tên thần).
    /// </summary>
    public sealed class ParabolicHomingTrajectory : IProjectileTrajectory
    {
        public static readonly ParabolicHomingTrajectory Instance = new ParabolicHomingTrajectory();

        public DefenseTrajectoryType Type => DefenseTrajectoryType.ParabolicHoming;

        public Vector3 EvaluatePosition(Vector3 start, Vector3 end, float progress, float arcHeight)
        {
            float p = Mathf.Clamp01(progress);
            Vector3 linear = Vector3.Lerp(start, end, p);
            float heightOffset = 4f * arcHeight * p * (1f - p);
            linear.y += heightOffset;
            return linear;
        }

        public Vector3 EvaluateDirection(Vector3 start, Vector3 end, float progress, float arcHeight)
        {
            float p = Mathf.Clamp01(progress);
            Vector3 delta = end - start;
            delta.y += 4f * arcHeight * (1f - 2f * p);
            return delta.sqrMagnitude > 0.0001f ? delta.normalized : Vector3.forward;
        }
    }

    /// <summary>
    /// Quỹ đạo bay thẳng theo vector hướng ngắm (dành cho Kỹ năng tướng ném bùa, phi tiêu, đạn bắn thẳng).
    /// </summary>
    public sealed class StraightDirectionalTrajectory : IProjectileTrajectory
    {
        public static readonly StraightDirectionalTrajectory Instance = new StraightDirectionalTrajectory();

        public DefenseTrajectoryType Type => DefenseTrajectoryType.StraightDirectional;

        public Vector3 EvaluatePosition(Vector3 start, Vector3 end, float progress, float arcHeight)
        {
            return Vector3.Lerp(start, end, Mathf.Clamp01(progress));
        }

        public Vector3 EvaluateDirection(Vector3 start, Vector3 end, float progress, float arcHeight)
        {
            Vector3 delta = end - start;
            delta.y = 0f;
            return delta.sqrMagnitude > 0.0001f ? delta.normalized : Vector3.forward;
        }
    }

    /// <summary>
    /// Quỹ đạo đạn pháo cối cầu vồng rơi xuống tọa độ mặt đất.
    /// </summary>
    public sealed class BallisticGroundTrajectory : IProjectileTrajectory
    {
        public static readonly BallisticGroundTrajectory Instance = new BallisticGroundTrajectory();

        public DefenseTrajectoryType Type => DefenseTrajectoryType.BallisticGround;

        public Vector3 EvaluatePosition(Vector3 start, Vector3 end, float progress, float arcHeight)
        {
            float p = Mathf.Clamp01(progress);
            Vector3 linear = Vector3.Lerp(start, end, p);
            float heightOffset = 4f * Mathf.Max(arcHeight, 1.5f) * p * (1f - p);
            linear.y += heightOffset;
            return linear;
        }

        public Vector3 EvaluateDirection(Vector3 start, Vector3 end, float progress, float arcHeight)
        {
            float p = Mathf.Clamp01(progress);
            Vector3 delta = end - start;
            delta.y += 4f * Mathf.Max(arcHeight, 1.5f) * (1f - 2f * p);
            return delta.sqrMagnitude > 0.0001f ? delta.normalized : Vector3.down;
        }
    }
}
