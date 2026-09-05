using System;

namespace ProjectZombie.Features.Player.Stats
{
    public enum StatModType
    {
        Flat = 100,
        PercentAdd = 200,
        PercentMult = 300
    }

    /// <summary>
    /// Đại diện cho một hệ số thay đổi chỉ số (Buff, Debuff, Trang bị, Nâng cấp Meta).
    /// Hỗ trợ xóa theo Nguồn (Source) để quản lý hiệu ứng tạm thời sạch sẽ.
    /// </summary>
    [Serializable]
    public class StatModifier
    {
        public float Value { get; }
        public StatModType Type { get; }
        public int Order { get; }
        public object Source { get; }

        public StatModifier(float value, StatModType type, int order, object source)
        {
            Value = value;
            Type = type;
            Order = order;
            Source = source;
        }

        public StatModifier(float value, StatModType type) : this(value, type, (int)type, null) { }
        public StatModifier(float value, StatModType type, int order) : this(value, type, order, null) { }
        public StatModifier(float value, StatModType type, object source) : this(value, type, (int)type, source) { }
    }
}
