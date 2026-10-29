using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Dữ liệu yêu cầu phóng một viên đạn / mũi tên trong chiến trường Defense 3D.
    /// Dùng struct thuần để triệt tiêu 100% GC Allocations trong vòng lặp chiến đấu.
    /// </summary>
    public struct DefenseProjectileLaunchRequest
    {
        public Component Source;
        public Vector3 Origin;
        public IDefenseEnemyTarget3D Target;
        public Vector3 TargetPosition;
        public float Damage;
        public float Speed;
        public float ArcHeight;
        public int RunGeneration;
        public object SessionIdentity;

        public DefenseProjectileLaunchRequest(
            Component source,
            Vector3 origin,
            IDefenseEnemyTarget3D target,
            float damage,
            int runGeneration,
            object sessionIdentity,
            float speed = 25f,
            float arcHeight = 0.5f)
        {
            Source = source;
            Origin = origin;
            Target = target;
            TargetPosition = target != null && target.TargetTransform != null ? target.TargetTransform.position : origin;
            Damage = Mathf.Max(0f, damage);
            Speed = Mathf.Max(0.1f, speed);
            ArcHeight = Mathf.Max(0f, arcHeight);
            RunGeneration = runGeneration;
            SessionIdentity = sessionIdentity;
        }

        public DefenseProjectileLaunchRequest(
            Component source,
            Vector3 origin,
            Vector3 targetPosition,
            float damage,
            int runGeneration,
            object sessionIdentity,
            float speed = 25f,
            float arcHeight = 0.5f)
        {
            Source = source;
            Origin = origin;
            Target = null;
            TargetPosition = targetPosition;
            Damage = Mathf.Max(0f, damage);
            Speed = Mathf.Max(0.1f, speed);
            ArcHeight = Mathf.Max(0f, arcHeight);
            RunGeneration = runGeneration;
            SessionIdentity = sessionIdentity;
        }
    }

    /// <summary>
    /// Giao diện trừu tượng cho bất kỳ hệ thống hoặc đối tượng nào có khả năng phóng đạn trong Defense 3D
    /// (Tháp bắn tên, Pháo nổ, Tướng đánh xa, Quái vật bắn gai).
    /// Tuân thủ nguyên tắc Dependency Inversion (DIP) và Open/Closed (OCP).
    /// </summary>
    public interface IDefenseProjectileLauncher
    {
        bool TryLaunch(DefenseProjectileLaunchRequest request);
    }
}
