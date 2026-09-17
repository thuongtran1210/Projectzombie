using System;
using UnityEngine;

namespace ProjectZombie.Features.Multiplayer.Core
{
    /// <summary>
    /// Các cờ nút bấm điều khiển (Bitmask Flags) tối ưu 0-GC Allocations qua mạng.
    /// </summary>
    [Flags]
    public enum NetworkInputButtons : byte
    {
        None = 0,
        Attack = 1 << 0,
        Dash = 1 << 1,
        SignatureSkill = 1 << 2,
        RelicSkill = 1 << 3
    }

    /// <summary>
    /// Cấu trúc Input truyền nhận qua mạng (Blittable Struct, 0-GC Allocations).
    /// Chuẩn hóa sẵn sàng tương thích với Photon Fusion INetworkInput hoặc bất kỳ Network Transport nào.
    /// </summary>
    [System.Serializable]
    public struct NetworkInputData : Fusion.INetworkInput
    {
        /// <summary>
        /// Hướng di chuyển của người chơi (Normalized Vector2).
        /// </summary>
        public Vector2 MoveDirection;

        /// <summary>
        /// Hướng ngắm bắn chiêu thức MOBA (Aim Vector2).
        /// </summary>
        public Vector2 AimDirection;

        /// <summary>
        /// Tập hợp các nút bấm được nhấn trong Tick mạng này.
        /// </summary>
        public NetworkInputButtons Buttons;

        public bool IsButtonSet(NetworkInputButtons button) => (Buttons & button) == button;

        public void SetButton(NetworkInputButtons button, bool state)
        {
            if (state) Buttons |= button;
            else Buttons &= ~button;
        }
    }
}
