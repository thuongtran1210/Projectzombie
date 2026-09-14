using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectZombie.Features.Upgrades
{
    /// <summary>
    /// Giao diện dịch vụ cung cấp và quản lý bể nâng cấp (Upgrade Pool).
    /// Cho phép các Presenter và Manager khác truy cập không phụ thuộc vào Singleton cụ thể.
    /// </summary>
    public interface IUpgradeService
    {
        List<UpgradeData> GetRandomUpgrades(int count, UnityEngine.GameObject player);
        List<UpgradeData> GetRandomUpgrades(int count);
        void BanUpgrade(UpgradeData upgrade);
        bool IsBanned(UpgradeData upgrade);
        void ResetBannedUpgrades();
        Task AutoPopulateUpgradesIfEmptyAsync();
    }
}
