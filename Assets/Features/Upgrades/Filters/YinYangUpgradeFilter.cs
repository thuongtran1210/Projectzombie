using UnityEngine;
using ProjectZombie.Features.YinYang;

namespace ProjectZombie.Features.Upgrades.Filters
{
    /// <summary>
    /// Bộ lọc kiểm tra tính tương thích giữa trạng thái Cán Cân Âm Dương hiện tại và yêu cầu của thẻ.
    /// Nếu tính năng Âm Dương không kích hoạt (chơi nhân vật không phải Đạo Sĩ), tự động loại trừ các thẻ đòi hỏi Âm/Dương đặc quyền.
    /// </summary>
    public class YinYangUpgradeFilter : IUpgradeFilter
    {
        public bool IsAllowed(UpgradeData upgrade, GameObject player)
        {
            if (upgrade == null) return false;

            // Nếu thẻ không yêu cầu kiểm tra Âm Dương -> Hợp lệ
            if (!upgrade.checkYinYangState) return true;

            var manager = YinYangManager.Instance;
            if (manager == null || !manager.IsTrackerActive)
            {
                // Nếu tính năng Âm Dương không kích hoạt cho nhân vật hiện tại -> Không xuất hiện thẻ này
                return false;
            }

            // Kiểm tra trạng thái hiện tại
            return upgrade.requiredYinYangState == manager.GetState();
        }
    }
}
