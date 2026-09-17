using System;
using System.Threading.Tasks;

namespace ProjectZombie.Features.Multiplayer.Core
{
    /// <summary>
    /// Hợp đồng điều phối kết nối mạng và quản lý phòng chơi (Photon Host Mode / Relay).
    /// Kế thừa Composite Interface (ILobbySessionService & IMatchLifecycleService) để bảo toàn 100% tương thích ngược
    /// đồng thời tuân thủ chuẩn Interface Segregation Principle (ISP) của AGENTS.md.
    /// </summary>
    public interface INetworkSessionService : ILobbySessionService, IMatchLifecycleService
    {
    }
}
