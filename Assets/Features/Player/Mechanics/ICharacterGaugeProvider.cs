namespace ProjectZombie.Features.Player.Mechanics
{
    /// <summary>
    /// Giao diện chuẩn cho các hệ thống theo dõi và cung cấp chỉ số thanh cơ chế đặc thù nhân vật
    /// (Ví dụ: Cán Cân Âm Dương của Đạo Sĩ, Nộ Khí của Võ Tăng, Bút Lực của Thư Sinh, ...).
    /// Áp dụng Open/Closed Principle (OCP) và Dependency Inversion Principle (DIP).
    /// </summary>
    public interface ICharacterGaugeProvider
    {
        /// <summary>
        /// Tiêu đề hoặc trạng thái hiện tại của cơ chế (vd: "<color=#FF4444>Dương Thịnh</color>", "Nộ Khí: 80%").
        /// </summary>
        string GaugeTitle { get; }

        /// <summary>
        /// Giá trị hiện tại của thanh cơ chế.
        /// </summary>
        float CurrentValue { get; }

        /// <summary>
        /// Giá trị nhỏ nhất (thường là 0).
        /// </summary>
        float MinValue { get; }

        /// <summary>
        /// Giá trị lớn nhất (thường là 100).
        /// </summary>
        float MaxValue { get; }

        /// <summary>
        /// Màu sắc nhận diện của thanh cơ chế theo trạng thái hiện tại.
        /// </summary>
        UnityEngine.Color GaugeColor { get; }

        /// <summary>
        /// Sự kiện phát ra mỗi khi giá trị hoặc trạng thái thanh cơ chế thay đổi.
        /// Tham số: (float currentValue, string formattedStateTitle)
        /// </summary>
        event System.Action<float, string> OnGaugeValueChanged;
    }
}
