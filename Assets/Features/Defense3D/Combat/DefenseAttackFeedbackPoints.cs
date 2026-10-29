using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Chụp điểm hiển thị trước damage; chỉ dùng anchor có sẵn, không tìm child theo tên.</summary>
    internal static class DefenseAttackFeedbackPoints
    {
        public static DefensePoint3 Capture(Transform target, Vector3 fallback)
        {
            if (target != null && target.TryGetComponent<DefenseActorAnchors3D>(out var anchors) && anchors.Hit != null)
                fallback = anchors.Hit.position;
            return new DefensePoint3(fallback.x, fallback.y, fallback.z);
        }
        public static DefensePoint3 Point(Vector3 point) => new DefensePoint3(point.x, point.y, point.z);
    }
}
