using System;
using System.Threading.Tasks;

namespace ProjectZombie.Features.Multiplayer.Core
{
    /// <summary>
    /// Hợp đồng điều phối vòng đời trận đấu nhiều người chơi (ISP - Interface Segregation Principle).
    /// Đơn trách nhiệm: Chỉ quản lý sự kiện bắt đầu trận đấu, kết thúc trận đấu và thông báo cho gameplay.
    /// </summary>
    public interface IMatchLifecycleService
    {
        /// <summary>
        /// Host kích hoạt bắt đầu trận đấu Co-op (chuyển sang màn hình chơi và sinh nhân vật mạng).
        /// </summary>
        Task StartGameMatchAsync();

        /// <summary>
        /// Sự kiện khi trận đấu Co-op chính thức bắt đầu trên sàn đấu.
        /// </summary>
        event Action OnMatchStarted;

        /// <summary>
        /// Sự kiện khi trận đấu kết thúc hoặc phòng bị hủy.
        /// </summary>
        event Action OnMatchEnded;
    }
}
