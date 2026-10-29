using UnityEngine;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Multiplayer.Core.Commands
{
    /// <summary>
    /// Lệnh thực hiện Lướt né đòn (Dash Action).
    /// </summary>
    public class DashActionCommand : INetworkActionCommand
    {
        public void Execute(GameObject playerRoot, Vector2 aimDirection)
        {
            // Dự phòng cho mạng: Tương thích với hệ thống di chuyển 3D hoặc bộ điều khiển tương lai
        }
    }
}
