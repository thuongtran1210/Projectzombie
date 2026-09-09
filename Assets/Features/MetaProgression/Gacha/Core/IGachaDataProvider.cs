using System;
using ProjectZombie.Features.MetaProgression.Gacha.Data;

namespace ProjectZombie.Features.MetaProgression.Gacha.Core
{
    /// <summary>
    /// Interface trừu tượng hóa việc nạp dữ liệu Banner Gacha.
    /// Giúp hoán đổi linh hoạt giữa đọc ScriptableObject cục bộ và fetch JSON từ Server/Firebase Remote Config.
    /// </summary>
    public interface IGachaDataProvider
    {
        GachaBannerConfigSO GetBanner(string bannerId);
        void RefreshBannerData(Action onCompleted);
    }
}
