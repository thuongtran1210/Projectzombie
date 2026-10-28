using System;
using System.Collections.Generic;

namespace ProjectZombie.Features.Defense3D.Loadout
{
    /// <summary>
    /// DTO lưu trữ dữ liệu lựa chọn Loadout xuống đĩa / save system.
    /// Có trường Version tường minh để hỗ trợ Schema Migration trong tương lai (Mục 3.2 & 3.3 AGENTS.md).
    /// </summary>
    [Serializable]
    public sealed class LoadoutSaveDto
    {
        public int Version = 1;
        public string HeroId = string.Empty;
        public string RelicId = string.Empty;
        public List<string> MinionIds = new List<string>();
        public List<string> TowerIds = new List<string>();

        public LoadoutSaveDto() { }

        public LoadoutSaveDto(string heroId, string relicId, IEnumerable<string> minionIds, IEnumerable<string> towerIds, int version = 1)
        {
            Version = version;
            HeroId = heroId ?? string.Empty;
            RelicId = relicId ?? string.Empty;
            if (minionIds != null) MinionIds.AddRange(minionIds);
            if (towerIds != null) TowerIds.AddRange(towerIds);
        }
    }

    /// <summary>
    /// Hợp đồng lưu trữ / nạp dữ liệu Loadout của người chơi.
    /// Cho phép inject Fake repository khi chạy Unit Test thuần C#.
    /// </summary>
    public interface ILoadoutRepository
    {
        LoadoutSaveDto Load();
        void Save(LoadoutSaveDto dto);
    }
}
