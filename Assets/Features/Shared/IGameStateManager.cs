using System;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Hợp đồng trừu tượng quản lý vòng đời và trạng thái trò chơi (FSM Model).
    /// Tuân thủ Dependency Inversion (Rule 3.2 & 3.3 AGENTS.md).
    /// </summary>
    public interface IGameStateManager
    {
        /// <summary>
        /// Trạng thái hiện tại của trò chơi.
        /// </summary>
        GameState CurrentState { get; }

        /// <summary>
        /// Kích hoạt khi trạng thái trò chơi thay đổi.
        /// </summary>
        event Action<GameState> OnStateChanged;

        /// <summary>
        /// Yêu cầu chuyển đổi sang trạng thái mới.
        /// </summary>
        void ChangeState(GameState newState);

        /// <summary>
        /// Chiếu trạng thái tương thích từ DefenseSession mà không làm gián đoạn clock.
        /// </summary>
        void ProjectDefenseState(GameState state);
    }
}
