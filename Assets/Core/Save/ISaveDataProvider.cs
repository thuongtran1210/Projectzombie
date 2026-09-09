using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Core.Save
{
    /// <summary>
    /// Interface trừu tượng hóa dịch vụ Lưu / Nạp dữ liệu.
    /// Giúp tách rời hoàn toàn Persistence Layer khỏi Gameplay & UI (hỗ trợ Local JSON, Firebase, PlayFab, Cloud Save).
    /// </summary>
    public interface ISaveDataProvider
    {
        /// <summary>
        /// Tên định danh của Provider (VD: "Local JSON", "Firebase Firestore").
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Lưu dữ liệu tiến trình game.
        /// </summary>
        bool Save(MetaProgressionSaveData saveData);

        /// <summary>
        /// Nạp dữ liệu tiến trình game.
        /// </summary>
        MetaProgressionSaveData Load();

        /// <summary>
        /// Xóa dữ liệu tiến trình (Reset).
        /// </summary>
        void DeleteSave();

        /// <summary>
        /// Kiểm tra xem file/bản ghi lưu trữ có tồn tại hay không.
        /// </summary>
        bool SaveExists();
    }
}
