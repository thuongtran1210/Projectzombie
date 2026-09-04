namespace ProjectZombie.Core.Pooling
{
    /// <summary>
    /// Giao diện chuẩn cho tất cả các GameObject/Component được quản lý bởi Object Pool.
    /// Giúp reset state và khởi tạo lại logic một cách độc lập và đồng bộ.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Được gọi ngay khi Object được lấy ra khỏi Pool (Active).
        /// Dùng để khởi tạo lại máu, reset timer, bật lại colliders/renderers.
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// Được gọi ngay trước khi Object được trả về Pool (Deactive).
        /// Dùng để dừng coroutines, clear particle/trail effects, hủy đăng ký events.
        /// </summary>
        void OnDespawn();
    }
}
