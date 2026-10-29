using UnityEngine;

namespace ProjectZombie.Features.Multiplayer.Core.Commands
{
    /// <summary>
    /// Lệnh thực hiện Đòn đánh thường (Primary Attack Action).
    /// </summary>
    public class AttackActionCommand : INetworkActionCommand
    {
        public void Execute(GameObject playerRoot, Vector2 aimDirection)
        {
            // Trong chế độ Defense3D: Đồng bộ hóa chiến đấu qua hệ thống Defense3D
        }
    }
}
