using System;
using ProjectZombie.Features.Defense3D.Loadout;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Vùng lưu trữ ngữ cảnh chuyển tiếp (Session Carrier) giữa Sảnh Menu và Scene Gameplay Defense 3D.
    /// Giữ snapshot đội hình xuất trận khi chuyển Scene mà không tạo liên kết cứng (tight coupling).
    /// </summary>
    public static class DefenseSessionContext
    {
        private static DefenseLoadoutSnapshot _pendingSnapshot;

        /// <summary>
        /// Snapshot đội hình đang chờ chuyển giao cho trận đấu sắp tới.
        /// </summary>
        public static DefenseLoadoutSnapshot PendingSnapshot
        {
            get => _pendingSnapshot;
            set => _pendingSnapshot = value;
        }

        /// <summary>
        /// Lấy snapshot đội hình đã chuẩn bị và dọn dẹp bộ nhớ tạm để tránh tái sử dụng sai ở các lượt sau.
        /// Nếu không có snapshot nào được chuẩn bị trước (ví dụ mở trực tiếp scene trong Editor), tự động trả về cấu hình mặc định an toàn.
        /// </summary>
        public static DefenseLoadoutSnapshot ConsumeSnapshot()
        {
            var snapshot = _pendingSnapshot;
            _pendingSnapshot = null;

            if (snapshot == null)
            {
                snapshot = DefenseLoadoutSnapshot.CreateDefault();
            }

            return snapshot;
        }

        /// <summary>
        /// Xóa dữ liệu phiên đang chờ (dùng khi hủy xuất trận hoặc thoát về sảnh).
        /// </summary>
        public static void Clear()
        {
            _pendingSnapshot = null;
        }
    }
}
