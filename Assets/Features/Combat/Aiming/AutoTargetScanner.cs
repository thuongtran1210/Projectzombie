using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Combat.Aiming
{
    /// <summary>
    /// Service quét mục tiêu tự động thông minh cho các thao tác Chạm Nhanh (Quick Tap) chuẩn MOBA.
    /// Tính toán hướng bắn và vị trí rơi tối ưu, 0 GC allocation.
    /// </summary>
    public static class AutoTargetScanner
    {
        /// <summary>
        /// Quét tìm hướng ngắm tối ưu nhất về phía kẻ địch trong tầm.
        /// Trả về true nếu tìm thấy mục tiêu; false nếu không có quái xung quanh.
        /// </summary>
        public static bool TryGetAutoAimDirection(
            Vector3 origin, 
            SkillAimConfig config, 
            Vector2 fallbackDirection, 
            out Vector2 aimDirection, 
            out Vector3 targetPosition,
            TargetPriority priority = TargetPriority.Nearest)
        {
            float scanRadius = config.range > 0f ? config.range : 5.0f;
            Transform target = TargetingUtility.FindTarget(origin, scanRadius, priority);

            if (target != null)
            {
                targetPosition = target.position;
                Vector2 dir = (targetPosition - origin);
                aimDirection = dir.sqrMagnitude > 0.001f ? dir.normalized : fallbackDirection;
                return true;
            }

            // Fallback khi không có quái: ngắm theo hướng mặt / hướng di chuyển hiện tại
            aimDirection = fallbackDirection != Vector2.zero ? fallbackDirection.normalized : Vector2.right;
            targetPosition = origin + (Vector3)(aimDirection * Mathf.Max(1.5f, config.range * 0.7f));
            return false;
        }
    }
}
