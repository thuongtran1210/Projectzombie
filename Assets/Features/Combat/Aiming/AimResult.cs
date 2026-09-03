using System;
using UnityEngine;

namespace ProjectZombie.Features.Combat.Aiming
{
    /// <summary>
    /// Cấu trúc dữ liệu đóng gói kết quả ngắm bắn định hướng hoàn chỉnh từ UI/Touch input đến Vũ khí/Pháp bảo.
    /// Triệt tiêu việc phân mảnh dữ liệu giữa vị trí chỉ dấu hiển thị và điểm xuất chiêu thực tế.
    /// </summary>
    [Serializable]
    public struct AimResult
    {
        /// <summary>
        /// Hướng ngắm bắn chuẩn hóa (Normalized Direction Vector).
        /// </summary>
        public Vector2 Direction;

        /// <summary>
        /// Khoảng cách từ điểm xuất phát đến điểm nhắm (World Units).
        /// </summary>
        public float Distance;

        /// <summary>
        /// Tọa độ điểm rơi / tâm chiêu thức trong không gian thế giới (World Position).
        /// </summary>
        public Vector3 TargetWorldPos;

        /// <summary>
        /// Tỉ lệ lực kéo của Joystick / Drag Button (0.0f đến 1.0f).
        /// </summary>
        public float PullPercent;

        /// <summary>
        /// True nếu là thao tác Chạm Nhanh (Quick Tap / Auto-Aim), False nếu là Kéo Nhắm thủ công (Explicit Drag).
        /// </summary>
        public bool IsQuickTap;

        public AimResult(Vector2 direction, float distance, Vector3 targetWorldPos, float pullPercent = 1.0f, bool isQuickTap = false)
        {
            Direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
            Distance = distance;
            TargetWorldPos = targetWorldPos;
            PullPercent = Mathf.Clamp01(pullPercent);
            IsQuickTap = isQuickTap;
        }

        /// <summary>
        /// Tạo nhanh AimResult từ hướng bắn đơn giản (tương thích ngược).
        /// </summary>
        public static AimResult FromDirection(Vector2 direction, Vector3 origin, float range = 5.0f, bool isQuickTap = false)
        {
            Vector2 dir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
            return new AimResult(dir, range, origin + (Vector3)(dir * range), 1.0f, isQuickTap);
        }

        public static AimResult QuickTap(Vector2 direction, Vector3 origin, float range)
        {
            return FromDirection(direction, origin, range, true);
        }
    }
}
