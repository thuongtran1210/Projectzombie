namespace ProjectZombie.Features.Shared.Policies
{
    /// <summary>
    /// Contract trừu tượng hóa chính sách dừng/chạy mô phỏng (Time.timeScale) và trạng thái chiến đấu cho GameStateManager.
    /// Khử bỏ hoàn toàn phụ thuộc trực tiếp từ Shared sang Multiplayer (Mục 3.2 & 3.3 AGENTS.md).
    /// </summary>
    public interface IGamePausePolicy
    {
        /// <summary>
        /// Xác định giá trị Time.timeScale tương ứng khi GameState thay đổi.
        /// </summary>
        float GetTimeScaleForState(GameState state);

        /// <summary>
        /// Xác định xem trận đấu có đang hoạt động (active combat simulation) hay không.
        /// </summary>
        bool IsCombatActive(GameState state);
    }
}
