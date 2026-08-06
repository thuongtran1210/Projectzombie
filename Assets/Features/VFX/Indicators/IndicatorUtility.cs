using UnityEngine;

namespace ProjectZombie.Features.VFX.Indicators
{
    /// <summary>
    /// Class tiện ích tính toán khoảng cách và kích thước vệt chỉ báo đồng bộ với Hitbox thực tế.
    /// </summary>
    public static class IndicatorUtility
    {
        /// <summary>
        /// Tính toán kích thước (Rộng, Dài) chuẩn mét thế giới thực cho vệt báo Dash/Lao Tông.
        /// </summary>
        public static Vector2 CalculateDashSize(float baseMoveSpeed, float speedMultiplier, float duration, float dashWidth = 1.5f)
        {
            float totalDistance = baseMoveSpeed * speedMultiplier * duration;
            return new Vector2(dashWidth, totalDistance);
        }

        /// <summary>
        /// Tính toán đường kính cho vệt báo hình tròn AoE từ bán kính.
        /// </summary>
        public static Vector2 CalculateAoESize(float radius)
        {
            return new Vector2(radius, radius);
        }
    }
}
