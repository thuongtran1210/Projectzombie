using UnityEngine;

namespace ProjectZombie.Features.Multiplayer.Core.Commands
{
    /// <summary>
    /// Lệnh kích hoạt Kỹ năng Cổ vật / Pháp bảo (Relic Skill Action).
    /// </summary>
    public class RelicSkillActionCommand : INetworkActionCommand
    {
        public void Execute(GameObject playerRoot, Vector2 aimDirection)
        {
            // Trong chế độ Defense3D: Đồng bộ hóa kỹ năng qua hệ thống Defense3D
        }
    }
}
