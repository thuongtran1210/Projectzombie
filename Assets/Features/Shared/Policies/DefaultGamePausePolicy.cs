using UnityEngine;

namespace ProjectZombie.Features.Shared.Policies
{
    /// <summary>
    /// Chính sách tạm dừng mặc định cho chế độ chơi đơn (Single-player).
    /// </summary>
    public class DefaultGamePausePolicy : IGamePausePolicy
    {
        public static readonly DefaultGamePausePolicy Instance = new DefaultGamePausePolicy();

        public float GetTimeScaleForState(GameState state)
        {
            switch (state)
            {
                case GameState.Playing:
                    return 1f;
                case GameState.LevelUpSelection:
                case GameState.Paused:
                case GameState.GameOver:
                    return 0f;
                default:
                    return 1f;
            }
        }

        public bool IsCombatActive(GameState state)
        {
            return state == GameState.Playing && Time.timeScale > 0f;
        }
    }
}
