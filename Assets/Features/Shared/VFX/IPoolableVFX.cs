using ProjectZombie.Core.Pooling;

namespace ProjectZombie.Features.Shared.VFX
{
    /// <summary>
    /// Interface cho các đối tượng hoặc Modular VFX Prefab muốn nhận thông báo khi được lấy ra từ Pool
    /// hoặc khi được hoàn trả về Pool, giúp dọn dẹp trạng thái và triệt tiêu lỗi rác thị giác (Ghosting).
    /// Kế thừa IPoolable của Core.Pooling để đảm bảo tính đồng bộ kiến trúc toàn dự án.
    /// </summary>
    public interface IPoolableVFX : IPoolable
    {
        /// <summary>
        /// Được gọi ngay khi instance được lấy ra từ Object Pool.
        /// Thường dùng để Play các Particle System con và thiết lập trạng thái khởi tạo.
        /// </summary>
        void OnSpawnFromPool();

        /// <summary>
        /// Được gọi khi instance chuẩn bị được trả về Object Pool hoặc bị tắt.
        /// Thường dùng để Stop Particle, Clear TrailRenderer và xóa các hiệu ứng rác.
        /// </summary>
        void OnReturnToPool();
    }
}
