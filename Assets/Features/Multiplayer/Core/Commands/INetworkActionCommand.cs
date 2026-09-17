using UnityEngine;

namespace ProjectZombie.Features.Multiplayer.Core.Commands
{
    /// <summary>
    /// Hợp đồng thực thi lệnh hành động mạng (Command Pattern & OCP).
    /// Cho phép mở rộng các hành động nút bấm mạng mà không phải sửa đổi NetworkPlayerCharacter.
    /// </summary>
    public interface INetworkActionCommand
    {
        /// <summary>
        /// Thực thi hành động tương ứng với cờ nút bấm mạng.
        /// </summary>
        void Execute(GameObject playerRoot, Vector2 aimDirection);
    }
}
