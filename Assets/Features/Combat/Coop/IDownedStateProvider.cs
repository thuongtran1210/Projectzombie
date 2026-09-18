using System;

namespace ProjectZombie.Features.Combat.Coop
{
    /// <summary>
    /// Contract cung cấp trạng thái gục ngã và tiến độ giải cứu hồi sinh trong Co-op.
    /// Tuân thủ Interface Segregation Principle (ISP) và Dependency Inversion Principle (DIP).
    /// Cho phép tầng UI/HUD (View/Presenter) lắng nghe sự kiện mà không phụ thuộc cứng vào MonoBehaviour cụ thể.
    /// </summary>
    public interface IDownedStateProvider
    {
        /// <summary>
        /// Người chơi có đang ở trạng thái gục ngã (Downed) chờ cứu hay không.
        /// </summary>
        bool IsDowned { get; }

        /// <summary>
        /// Tiến độ giải cứu hồi sinh chuẩn hóa từ 0.0f đến 1.0f.
        /// </summary>
        float ReviveProgressNormalized { get; }

        /// <summary>
        /// Bán kính cần thiết để đồng đội đứng gần cứu (mét).
        /// </summary>
        float ReviveRadius { get; }

        /// <summary>
        /// Sự kiện phát ra khi trạng thái gục ngã thay đổi (true = bị gục ngã, false = đã hồi sinh/bình thường).
        /// </summary>
        event Action<bool> OnDownedStateChanged;

        /// <summary>
        /// Sự kiện phát ra khi tiến độ giải cứu hồi sinh thay đổi (0.0f -> 1.0f).
        /// </summary>
        event Action<float> OnReviveProgressChanged;
    }
}
