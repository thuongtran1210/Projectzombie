using System;
using System.Threading.Tasks;
using ProjectZombie.Features.Defense3D;

namespace ProjectZombie.Core.Architecture
{
    /// <summary>
    /// Hợp đồng trừu tượng cho Scene Composition Root của mọi chế độ chơi trận đấu.
    /// Giúp AppFlowController khởi tạo màn chơi thông qua Interface (Dependency Inversion),
    /// không bị liên kết cứng (tightly coupled) vào component cụ thể của bất kỳ tính năng nào.
    /// </summary>
    public interface IMatchCompositionRoot
    {
        /// <summary>
        /// Cho biết kịch bản trận đấu và các đối tượng trong màn chơi đã sẵn sàng chiến đấu hay chưa.
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// Nạp các thành phần, đăng ký gameplay và cấu hình trận đấu bất đồng bộ.
        /// </summary>
        Task InitializeMatchAsync(MatchLaunchRequest request, Action<float, string> progress);
    }
}
