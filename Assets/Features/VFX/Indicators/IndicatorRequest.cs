using UnityEngine;

namespace ProjectZombie.Features.VFX.Indicators
{
    public enum IndicatorShape
    {
        Box,        // Vệt chữ nhật (Lao tông, đâm thẳng, laser)
        Circle,     // Vệt hình tròn (Giậm đất, mưa thiên thạch, nổ AoE)
        Cone        // Vệt hình quạt (Chém ngang, quạt quái)
    }

    /// <summary>
    /// Struct truyền thông số khi yêu cầu hiển thị vệt chỉ dấu nguy hiểm.
    /// </summary>
    public struct IndicatorRequest
    {
        public IndicatorShape Shape;
        public Vector3 Position;
        public Vector3 Direction;   // Hướng của vệt (dùng cho Box/Cone)
        public Vector2 Size;        // Box: (Rộng, Dài) | Circle: (Bán kính, Bán kính)
        public float Duration;      // Thời gian cảnh báo (chạy từ 0 -> 100%)
        public Color Color;         // Màu vệt chỉ dấu (mặc định Đỏ)

        public IndicatorRequest(IndicatorShape shape, Vector3 position, Vector3 direction, Vector2 size, float duration, Color color)
        {
            Shape = shape;
            Position = position;
            Direction = direction;
            Size = size;
            Duration = duration;
            Color = color;
        }

        public IndicatorRequest(IndicatorShape shape, Vector3 position, Vector3 direction, Vector2 size, float duration)
        {
            Shape = shape;
            Position = position;
            Direction = direction;
            Size = size;
            Duration = duration;
            Color = new Color(1f, 0f, 0f, 0.4f); // Màu đỏ mặc định với Alpha = 0.4
        }
    }
}
