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
            if (playerRoot.TryGetComponent<PlayerController>(out var controller))
            {
                controller.PerformDash();
            }
        }
    }
}
