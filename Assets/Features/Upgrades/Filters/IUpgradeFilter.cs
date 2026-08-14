using UnityEngine;

namespace ProjectZombie.Features.Upgrades.Filters
{
    /// <summary>
    /// Interface chiến lược lọc thẻ nâng cấp (Filter Strategy Pattern).
    /// Cho phép mở rộng thêm các điều kiện lọc thẻ mới mà không cần chỉnh sửa UpgradeManager.
    /// </summary>
    public interface IUpgradeFilter
    {
        /// <summary>
        /// Kiểm tra xem thẻ nâng cấp có thỏa mãn điều kiện để xuất hiện trong pool Gacha hay không.
        /// </summary>
        /// <param name="upgrade">Dữ liệu thẻ nâng cấp</param>
        /// <param name="player">GameObject của người chơi</param>
        /// <returns>True nếu được phép xuất hiện, False nếu bị lọc bỏ</returns>
        bool IsAllowed(UpgradeData upgrade, GameObject player);
    }
}
