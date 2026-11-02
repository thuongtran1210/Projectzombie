using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Core.Save
{
    /// <summary>
    /// Hợp đồng trừu tượng quản lý lưu trữ và nạp tiến trình người chơi.
    /// Tuân thủ Dependency Inversion (Rule 3.2 & 3.3 AGENTS.md).
    /// </summary>
    public interface ISaveManager
    {
        /// <summary>
        /// Dữ liệu lưu trữ tiến trình meta hiện hành.
        /// </summary>
        MetaProgressionSaveData SaveData { get; }

        /// <summary>
        /// Đăng ký module nghiệp vụ tham gia vòng đời Save/Load.
        /// </summary>
        void RegisterModule(ISaveableModule module);

        /// <summary>
        /// Hủy đăng ký module.
        /// </summary>
        void UnregisterModule(ISaveableModule module);

        /// <summary>
        /// Khởi tạo lại tất cả các module từ SaveData hiện có.
        /// </summary>
        void InitializeAllManagers();

        /// <summary>
        /// Thu thập dữ liệu và ghi xuống đĩa lưu trữ.
        /// </summary>
        void SaveGame();

        /// <summary>
        /// Nạp lại dữ liệu từ đĩa lưu trữ.
        /// </summary>
        void LoadGame();
    }
}
