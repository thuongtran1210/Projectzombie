using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Player.Core;

namespace ProjectZombie.Features.Enemies
{
    /// <summary>
    /// Hợp đồng chiến lược lựa chọn mục tiêu cho quái vật (Strategy Pattern - Rule 11 AGENTS.md).
    /// Hỗ trợ chuyển đổi linh hoạt giữa các thuật toán: Chọn người gần nhất, chọn người có điểm cừu hận cao nhất, v.v.
    /// </summary>
    public interface ITargetSelector
    {
        /// <summary>
        /// Lựa chọn mục tiêu người chơi thích hợp dựa trên vị trí hiện tại của quái.
        /// </summary>
        PlayerContext SelectTarget(Vector2 enemyPosition, IPlayerRegistry registry);
    }
}
