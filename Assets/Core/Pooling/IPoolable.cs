namespace ProjectZombie.Core.Pooling
{
    /// <summary>
    /// Contract cho các GameObject được quản lý bởi Object Pool.
    /// Giúp reset trạng thái chuẩn xác khi tái sử dụng mà không gây cấp phát rác (0 GC).
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Được gọi khi object được lấy ra từ Pool.
        /// </summary>
        void OnSpawnFromPool();

        /// <summary>
        /// Được gọi ngay trước khi object được trả về Pool (dọn dẹp event, cancel tween...).
        /// </summary>
        void OnReturnToPool();
    }
}
