namespace ProjectZombie.Core.Save
{
    /// <summary>
    /// Giao diện dành cho các hệ thống / Module nghiệp vụ tham gia vào vòng đời Lưu và Nạp dữ liệu Save của trò chơi.
    /// Cho phép GameManager ở tầng Core tương tác với các hệ thống ở tầng Features mà không bị liên kết cứng (Tight Coupling).
    /// Tuân thủ nguyên tắc Dependency Inversion Principle (DIP).
    /// </summary>
    public interface ISaveableModule
    {
        /// <summary>
        /// Nạp và đồng bộ trạng thái runtime từ SaveData khi game khởi động hoặc load lại save.
        /// </summary>
        void InitializeFromSave(MetaProgressionSaveData data);

        /// <summary>
        /// Ghi nhận và đồng bộ các chỉ số runtime hiện tại vào SaveData trước khi lưu xuống đĩa.
        /// </summary>
        void PopulateSaveData(MetaProgressionSaveData data);
    }
}
