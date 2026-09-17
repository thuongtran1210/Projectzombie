using System;
using System.Threading.Tasks;

namespace ProjectZombie.Features.Multiplayer.Core
{
    /// <summary>
    /// Hợp đồng điều phối kết nối mạng và quản lý phòng chơi (Photon Host Mode / Relay).
    /// </summary>
    public interface INetworkSessionService
    {
        /// <summary>
        /// Thông tin phòng hiện tại. Null nếu chưa vào phòng.
        /// </summary>
        NetworkRoomInfo CurrentRoom { get; }

        /// <summary>
        /// Có đang là Host của phòng hay không.
        /// </summary>
        bool IsHost { get; }

        /// <summary>
        /// Đã kết nối vào phòng thành công hay chưa.
        /// </summary>
        bool IsInRoom { get; }

        /// <summary>
        /// Tạo phòng mới với vai trò Host và sinh mã Room Code 6 ký tự.
        /// </summary>
        Task<bool> CreateHostSessionAsync(string roomCode = null, int maxPlayers = 4);

        /// <summary>
        /// Tham gia vào phòng của Host thông qua Room Code 6 ký tự.
        /// </summary>
        Task<bool> JoinSessionAsync(string roomCode);

        /// <summary>
        /// Rời phòng hiện tại và ngắt kết nối Relay.
        /// </summary>
        Task LeaveSessionAsync();

        /// <summary>
        /// Chuyển đổi trạng thái Sẵn Sàng (Ready) của người chơi cục bộ.
        /// </summary>
        void SetLocalPlayerReady(bool isReady);

        /// <summary>
        /// Host bấm Bắt Đầu Trận Đấu (Load Scene Gameplay đồng bộ cho tất cả Client).
        /// </summary>
        Task StartGameMatchAsync();

        /// <summary>
        /// Sự kiện khi danh sách người chơi trong phòng thay đổi (Player Joined / Left).
        /// </summary>
        event Action<NetworkRoomInfo> OnRoomUpdated;

        /// <summary>
        /// Sự kiện khi trận đấu bắt đầu.
        /// </summary>
        event Action OnMatchStarted;

        /// <summary>
        /// Sự kiện khi bị ngắt kết nối hoặc lỗi mạng.
        /// </summary>
        event Action<string> OnConnectionError;
    }
}
