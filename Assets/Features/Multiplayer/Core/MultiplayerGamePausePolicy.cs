using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Shared.Policies;

namespace ProjectZombie.Features.Multiplayer.Core
{
    /// <summary>
    /// Chính sách tạm dừng cho chế độ Multiplayer Co-op.
    /// Giữ Time.timeScale = 1f khi người chơi mở menu LevelUpSelection hoặc Paused trong phòng mạng
    /// để đảm bảo tick rate và trạng thái mạng không bị đóng băng (AGENTS.md).
    /// </summary>
    public class MultiplayerGamePausePolicy : IGamePausePolicy
    {
        private readonly INetworkSessionService _sessionService;

        public MultiplayerGamePausePolicy(INetworkSessionService sessionService)
        {
            _sessionService = sessionService;
        }

        public float GetTimeScaleForState(GameState state)
        {
            bool inRoom = _sessionService != null && _sessionService.IsInRoom;
            switch (state)
            {
                case GameState.Playing:
                    return 1f;
                case GameState.LevelUpSelection:
                case GameState.Paused:
                    return inRoom ? 1f : 0f;
                case GameState.GameOver:
                    return 0f;
                default:
                    return 1f;
            }
        }

        public bool IsCombatActive(GameState state)
        {
            if (state == GameState.Playing) return Time.timeScale > 0f;
            if (state == GameState.LevelUpSelection || state == GameState.Paused)
            {
                bool inRoom = _sessionService != null && _sessionService.IsInRoom;
                return inRoom && Time.timeScale > 0f;
            }
            return false;
        }
    }
}
