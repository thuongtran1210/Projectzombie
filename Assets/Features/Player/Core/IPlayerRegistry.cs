using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Player.Core
{
    /// <summary>
    /// Hợp đồng quản lý tập trung toàn bộ người chơi (Solo & Multiplayer Co-op).
    /// Loại bỏ phụ thuộc cứng vào PlayerProvider static singleton (Quy tắc 3.2 & 3.3 AGENTS.md).
    /// </summary>
    public interface IPlayerRegistry
    {
        /// <summary>
        /// Danh sách toàn bộ người chơi đang hoạt động trên sàn đấu.
        /// </summary>
        IReadOnlyList<PlayerContext> ActivePlayers { get; }

        /// <summary>
        /// Thực thể người chơi cục bộ tại máy này (Local Player).
        /// </summary>
        PlayerContext LocalPlayer { get; }

        /// <summary>
        /// Có ít nhất một người chơi đang hoạt động hay không.
        /// </summary>
        bool HasAnyPlayer { get; }

        /// <summary>
        /// Tìm người chơi còn sống gần nhất so với tọa độ cho trước (0-GC Allocations).
        /// </summary>
        PlayerContext GetNearestLivingPlayer(Vector2 position);

        /// <summary>
        /// Tìm người chơi theo PlayerId.
        /// </summary>
        PlayerContext GetPlayerById(int playerId);

        /// <summary>
        /// Đăng ký người chơi mới vào hệ thống.
        /// </summary>
        void Register(PlayerContext player);

        /// <summary>
        /// Hủy đăng ký người chơi khỏi hệ thống.
        /// </summary>
        void Unregister(PlayerContext player);

        /// <summary>
        /// Xóa toàn bộ danh sách người chơi (khi đổi Scene/kết thúc trận).
        /// </summary>
        void Clear();

        /// <summary>
        /// Sự kiện phát ra khi có người chơi mới tham gia/sinh ra.
        /// </summary>
        event Action<PlayerContext> OnPlayerRegistered;

        /// <summary>
        /// Sự kiện phát ra khi người chơi rời đi/bị hủy.
        /// </summary>
        event Action<PlayerContext> OnPlayerUnregistered;
    }
}
