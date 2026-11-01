using System;

namespace ProjectZombie.Features.MetaProgression
{
    /// <summary>
    /// Giao diện trừu tượng hóa cho dịch vụ Quản lý Kho Pháp Bảo & Thần Khí (SOLID: Interface Segregation & Dependency Inversion).
    /// </summary>
    public interface IRelicInventoryService
    {
        RelicStarProgressionSO ProgressionConfig { get; }

        event Action<string, int> OnRelicStarUpgraded;
        event Action<string, int> OnRelicShardsChanged;

        int GetRelicStarLevel(string relicId);
        int GetRelicShardCount(string relicId);
        bool IsRelicUnlocked(string relicId);
        bool CanFuse(string relicId, out string reason);
        bool TryFuseRelic(string relicId);
    }
}

