using System;

namespace ProjectZombie.Features.Defense3D.Loadout
{
    /// <summary>
    /// Bộ quy tắc ràng buộc cấu hình Loadout cho một trận chiến Defense 3D.
    /// Cho phép tùy biến số lượng và tính bắt buộc theo từng chế độ chơi (Campaign / Challenge).
    /// </summary>
    public sealed class LoadoutRules
    {
        public bool RequireHero { get; }
        public bool RequireRelic { get; }
        public int MaxHeroCount { get; }
        public int MaxRelicCount { get; }
        public int MinMinionCount { get; }
        public int MaxMinionCount { get; }
        public int MinTowerCount { get; }
        public int MaxTowerCount { get; }

        public LoadoutRules(
            bool requireHero = true,
            bool requireRelic = true,
            int maxHeroCount = 1,
            int maxRelicCount = 1,
            int minMinionCount = 1,
            int maxMinionCount = 3,
            int minTowerCount = 1,
            int maxTowerCount = 3)
        {
            if (maxHeroCount < (requireHero ? 1 : 0))
                throw new ArgumentException("MaxHeroCount không thể nhỏ hơn số lượng bắt buộc.", nameof(maxHeroCount));
            if (maxRelicCount < (requireRelic ? 1 : 0))
                throw new ArgumentException("MaxRelicCount không thể nhỏ hơn số lượng bắt buộc.", nameof(maxRelicCount));
            if (minMinionCount < 0 || maxMinionCount < minMinionCount)
                throw new ArgumentException("Giới hạn MinionCount không hợp lệ.", nameof(maxMinionCount));
            if (minTowerCount < 0 || maxTowerCount < minTowerCount)
                throw new ArgumentException("Giới hạn TowerCount không hợp lệ.", nameof(maxTowerCount));

            RequireHero = requireHero;
            RequireRelic = requireRelic;
            MaxHeroCount = maxHeroCount;
            MaxRelicCount = maxRelicCount;
            MinMinionCount = minMinionCount;
            MaxMinionCount = maxMinionCount;
            MinTowerCount = minTowerCount;
            MaxTowerCount = maxTowerCount;
        }

        public static LoadoutRules Default => new LoadoutRules();
    }
}
