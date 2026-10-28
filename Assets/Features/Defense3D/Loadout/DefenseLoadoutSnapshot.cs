using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectZombie.Features.Defense3D.Loadout
{
    /// <summary>
    /// Snapshot bất biến (Immutable Snapshot) đóng băng cấu hình Loadout khi trận đấu bắt đầu.
    /// Đảm bảo trong suốt trận chiến, việc sửa đổi bản nháp LoadoutSelection ngoài Sảnh không ảnh hưởng đến trận đang chơi.
    /// </summary>
    public sealed class DefenseLoadoutSnapshot
    {
        public string SnapshotId { get; }
        public string HeroId { get; }
        public string RelicId { get; }
        public IReadOnlyList<string> MinionIds { get; }
        public IReadOnlyList<string> TowerIds { get; }

        public DefenseLoadoutSnapshot(
            string heroId,
            string relicId,
            IEnumerable<string> minionIds,
            IEnumerable<string> towerIds,
            string snapshotId = null)
        {
            SnapshotId = snapshotId ?? Guid.NewGuid().ToString("N");
            HeroId = heroId ?? string.Empty;
            RelicId = relicId ?? string.Empty;

            var minions = minionIds?.Where(id => !string.IsNullOrWhiteSpace(id)).ToArray() ?? Array.Empty<string>();
            MinionIds = Array.AsReadOnly(minions);

            var towers = towerIds?.Where(id => !string.IsNullOrWhiteSpace(id)).ToArray() ?? Array.Empty<string>();
            TowerIds = Array.AsReadOnly(towers);
        }

        public static DefenseLoadoutSnapshot Create(LoadoutSelection selection)
        {
            if (selection == null) return CreateDefault();

            return new DefenseLoadoutSnapshot(
                selection.HeroId,
                selection.RelicId,
                selection.MinionIds,
                selection.TowerIds);
        }

        /// <summary>
        /// Tạo cấu hình mặc định tương thích hoàn toàn với baseline Defense 3D hiện tại:
        /// Tướng Thư Sinh, Pháp Bảo Dép Tổ Ong, 3 loại lính làng (Dân binh, Phu tạp dịch, Thầy lang) và Trụ Tiễn cơ bản.
        /// </summary>
        public static DefenseLoadoutSnapshot CreateDefault()
        {
            return new DefenseLoadoutSnapshot(
                heroId: "hero_thusinh",
                relicId: "relic_deptoong",
                minionIds: new[] { "minion_danbinh", "minion_phutapdich", "minion_thaylang" },
                towerIds: new[] { "tower_cungtien" });
        }
    }
}
