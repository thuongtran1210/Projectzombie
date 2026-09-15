using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Upgrades.Weighting
{
    /// <summary>
    /// Hợp đồng cho các Strategy tính trọng số xuất hiện của thẻ nâng cấp.
    /// Cho phép mở rộng các quy tắc cộng hưởng (Synergy), bảo hiểm (Pity), hoặc tăng tỉ lệ ra đồ độc lập.
    /// </summary>
    public interface IUpgradeWeighter
    {
        /// <summary>
        /// Tính toán và trả về hệ số nhân trọng số (Multiplier). Mặc định là 1.0f (không đổi).
        /// </summary>
        /// <param name="upgrade">Thẻ nâng cấp đang được xét</param>
        /// <param name="context">Ngữ cảnh nhân vật (PlayerContext)</param>
        /// <returns>Hệ số nhân trọng số (vd: 1.5f = tăng 50% tỉ lệ xuất hiện)</returns>
        float CalculateMultiplier(UpgradeData upgrade, PlayerContext context);
    }
}
