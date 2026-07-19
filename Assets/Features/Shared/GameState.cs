namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Các trạng thái vòng đời của trò chơi.
    /// </summary>
    public enum GameState
    {
        MainMenu,          // Ngoài menu chính
        Playing,           // Đang trong trận đấu thường
        Paused,            // Game bị tạm dừng (người chơi chủ động nhấn Tab/Esc)
        LevelUpSelection,  // Màn hình chọn nâng cấp khi thăng cấp
        GameOver           // Kết thúc trận đấu (Thua trận hoặc chiến thắng)
    }
}
