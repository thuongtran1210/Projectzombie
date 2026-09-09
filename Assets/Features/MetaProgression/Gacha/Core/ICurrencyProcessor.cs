namespace ProjectZombie.Features.MetaProgression.Gacha.Core
{
    /// <summary>
    /// Các loại đơn vị tiền tệ có thể sử dụng để quay Gacha.
    /// </summary>
    public enum GachaCurrencyType
    {
        CoTien = 0,         // Cổ Tiền (Tiền vĩnh viễn In-game)
        LinhThach = 1,      // Linh Thạch (Premium Currency)
        SummonTicket = 2,   // Vé Bùa Chiêu Hồn (Gacha Ticket)
        RewardedAd = 3      // Xem Quảng Cáo AdMob
    }

    /// <summary>
    /// Interface trừu tượng hóa xử lý thanh toán chi phí Gacha.
    /// Giúp hoán đổi giữa Cổ Tiền, Linh Thạch, Vé quay hoặc Xem Ads mà không sửa đổi RelicGachaManager.
    /// </summary>
    public interface ICurrencyProcessor
    {
        GachaCurrencyType CurrencyType { get; }
        bool HasEnough(int amount);
        bool TrySpend(int amount);
        int GetBalance();
    }
}
