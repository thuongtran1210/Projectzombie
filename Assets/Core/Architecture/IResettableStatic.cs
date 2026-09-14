namespace ProjectZombie.Core.Architecture
{
    /// <summary>
    /// Interface chuẩn cho tất cả các hệ thống hoặc service có sử dụng static collection,
    /// static event hoặc cache. Bắt buộc implement để dọn dẹp state khi chuyển Scene hoặc bắt đầu run mới.
    /// </summary>
    public interface IResettableStatic
    {
        /// <summary>
        /// Reset toàn bộ static cache, counter, and unbind static events.
        /// </summary>
        void ResetStaticState();
    }
}
