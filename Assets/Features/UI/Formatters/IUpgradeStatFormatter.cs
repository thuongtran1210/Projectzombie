namespace ProjectZombie.Features.UI.Formatters
{
    using ProjectZombie.Features.Upgrades;

    /// <summary>
    /// Hợp đồng định dạng dữ liệu chỉ số thay đổi (Stat Diff) của thẻ nâng cấp sang chuỗi hiển thị TextMeshPro.
    /// </summary>
    public interface IUpgradeStatFormatter
    {
        /// <summary>
        /// Chuyển đổi dữ liệu nâng cấp sang chuỗi so sánh chỉ số trực quan kèm Rich Text tags.
        /// </summary>
        string FormatStatDiff(UpgradeData upgradeData);
    }
}
