using UnityEngine;
using System;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Bộ quản lý trạng thái trò chơi (FSM Model).
    /// Quản lý việc chuyển trạng thái, điều phối Time.timeScale tương ứng và phát tín hiệu cho toàn hệ thống.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.MainMenu; // Mặc định là MainMenu khi mở game (Hướng A)

        /// <summary>
        /// Kích hoạt khi trạng thái trò chơi thay đổi.
        /// </summary>
        public event Action<GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // Nếu đây là game quản lý đa cảnh, giữ lại manager xuyên suốt các scene
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Chuyển đổi sang trạng thái mới.
        /// </summary>
        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;

            CurrentState = newState;

            // Đồng bộ hoá Time.timeScale theo trạng thái
            switch (newState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                case GameState.LevelUpSelection:
                case GameState.GameOver:
                    Time.timeScale = 0f;
                    break;
            }

            Debug.Log($"[GameStateManager] Game State changed to: {newState} (TimeScale set to {Time.timeScale})");
            OnStateChanged?.Invoke(CurrentState);
        }
    }
}
