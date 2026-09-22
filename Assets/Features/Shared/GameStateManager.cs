using UnityEngine;
using System;
using ProjectZombie.Core.Architecture;
using ProjectZombie.Features.Shared.Policies;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Bộ quản lý trạng thái trò chơi (FSM Model).
    /// Quản lý việc chuyển trạng thái, điều phối Time.timeScale tương ứng và phát tín hiệu cho toàn hệ thống.
    /// Tuân thủ Dependency Inversion: Sử dụng IGamePausePolicy để quyết định hành vi tạm dừng/chạy tiếp (Mục 3.2 AGENTS.md).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        private static IGamePausePolicy PausePolicy =>
            ServiceContext.Get<IGamePausePolicy>() ?? DefaultGamePausePolicy.Instance;

        /// <summary>
        /// Single Source of Truth kiểm tra xem trò chơi có đang trong trạng thái chiến đấu hoạt động hay không.
        /// Trả về false khi đang Pause, GameOver hoặc ở MainMenu.
        /// </summary>
        public static bool IsPlaying
        {
            get
            {
                if (Instance == null) return true;
                return PausePolicy.IsCombatActive(Instance.CurrentState);
            }
        }

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

            // Đồng bộ hoá Time.timeScale thông qua PausePolicy trừu tượng
            Time.timeScale = PausePolicy.GetTimeScaleForState(newState);

            Debug.Log($"[GameStateManager] Game State changed to: {newState} (TimeScale set to {Time.timeScale})");
            OnStateChanged?.Invoke(CurrentState);
        }
    }
}
